using Inventory.Application.Dashboard.Queries;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Dashboard.Handlers
{
    public class GetDashboardSummaryQueryHandler
        : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
    {
        private readonly InventoryDbContext _context;

        public GetDashboardSummaryQueryHandler(InventoryDbContext context)
            => _context = context;

        public async Task<DashboardSummaryDto> Handle(
            GetDashboardSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var now      = DateTime.UtcNow;
            var today    = now.Date;
            var in7Days  = today.AddDays(7);
            var in30Days = today.AddDays(30);
            var sevenDaysAgo = today.AddDays(-6);

            // ── Inventario ───────────────────────────────────────────────────
            var totalMaterials = await _context.Materials
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var lowStockItems = await _context.Materials
                .AsNoTracking()
                .Where(m => m.Type != MaterialType.Service)
                .Select(m => new
                {
                    m.Id, m.Name, m.SKU, m.ReorderPoint,
                    OnHand = _context.StockItems
                        .Where(s => s.MaterialId == m.Id)
                        .Sum(s => (decimal?)s.QuantityOnHand) ?? 0m
                })
                .Where(x => x.OnHand <= x.ReorderPoint && x.ReorderPoint > 0)
                .OrderBy(x => x.OnHand - x.ReorderPoint)
                .Take(8)
                .ToListAsync(cancellationToken);

            var totalStockValue = await _context.StockItems
                .AsNoTracking()
                .Include(s => s.Material)
                .SumAsync(s => s.QuantityOnHand * s.Material.StandardCost, cancellationToken);

            var totalStockUnits = await _context.StockItems
                .AsNoTracking()
                .SumAsync(s => (decimal?)s.QuantityOnHand ?? 0m, cancellationToken);

            // ── Lotes ────────────────────────────────────────────────────────
            var expiringIn7 = await _context.Lots
                .AsNoTracking()
                .CountAsync(l => l.ExpirationDate.HasValue
                    && l.ExpirationDate.Value.Date >= today
                    && l.ExpirationDate.Value.Date <= in7Days, cancellationToken);

            var expiringIn30 = await _context.Lots
                .AsNoTracking()
                .CountAsync(l => l.ExpirationDate.HasValue
                    && l.ExpirationDate.Value.Date >= today
                    && l.ExpirationDate.Value.Date <= in30Days, cancellationToken);

            var expiringLotsList = await _context.Lots
                .AsNoTracking()
                .Include(l => l.Material)
                .Where(l => l.ExpirationDate.HasValue
                    && l.ExpirationDate.Value.Date >= today
                    && l.ExpirationDate.Value.Date <= in30Days)
                .OrderBy(l => l.ExpirationDate)
                .Take(5)
                .ToListAsync(cancellationToken);

            // ── Órdenes de venta ─────────────────────────────────────────────
            var ordersDraft        = await _context.SalesOrders.AsNoTracking().CountAsync(o => o.Status == SalesOrderStatus.Draft,       cancellationToken);
            var ordersPicking      = await _context.SalesOrders.AsNoTracking().CountAsync(o => o.Status == SalesOrderStatus.Picking,     cancellationToken);
            var ordersReady        = await _context.SalesOrders.AsNoTracking().CountAsync(o => o.Status == SalesOrderStatus.ReadyToShip, cancellationToken);
            var ordersShippedToday = await _context.SalesOrders.AsNoTracking().CountAsync(o => o.Status == SalesOrderStatus.Shipped && o.CreatedAt.Date == today, cancellationToken);

            var recentOrders = await _context.SalesOrders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Take(6)
                .Select(o => new RecentOrderDto
                {
                    OrderNumber  = o.OrderNumber,
                    CustomerName = o.CustomerName,
                    Status       = o.Status.ToString(),
                    Channel      = o.SourceChannel.ToString(),
                    CreatedAt    = o.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var trendRaw = await _context.SalesOrders
                .AsNoTracking()
                .Where(o => o.CreatedAt.Date >= sevenDaysAgo && o.CreatedAt.Date <= today)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            // ── Producción ───────────────────────────────────────────────────
            var woPending  = await _context.WorkOrder.AsNoTracking().CountAsync(w => w.Status == WorkOrderStatus.Pending,        cancellationToken);
            var woProgress = await _context.WorkOrder.AsNoTracking().CountAsync(w => w.Status == WorkOrderStatus.InProgress,     cancellationToken);
            var woQc       = await _context.WorkOrder.AsNoTracking().CountAsync(w => w.Status == WorkOrderStatus.QualityControl, cancellationToken);

            // ── Stock por zona ───────────────────────────────────────────────
            var stockByZone = await _context.StockItems
                .AsNoTracking()
                .Include(s => s.StorageBin).ThenInclude(b => b!.Zone)
                .Where(s => s.StorageBin != null && s.StorageBin.Zone != null)
                .GroupBy(s => s.StorageBin!.Zone!.Name)
                .Select(g => new StockByZoneDto
                {
                    ZoneName = g.Key,
                    Units    = g.Sum(s => s.QuantityOnHand)
                })
                .OrderByDescending(x => x.Units)
                .Take(6)
                .ToListAsync(cancellationToken);

            // ── Integraciones ────────────────────────────────────────────────
            var unmapped  = await _context.ChannelProductMappings.AsNoTracking().CountAsync(m => !m.MaterialId.HasValue, cancellationToken);
            var connected = await _context.ChannelConfigs.AsNoTracking().CountAsync(c => c.IsConnected, cancellationToken);

            // ── Post-proceso: trend (rellenar días sin pedidos con 0) ─────────
            var trend   = new List<DailyOrderCountDto>();
            var culture = new CultureInfo("es-MX");
            for (int i = 6; i >= 0; i--)
            {
                var day   = today.AddDays(-i);
                var count = trendRaw.FirstOrDefault(x => x.Date == day)?.Count ?? 0;
                trend.Add(new DailyOrderCountDto
                {
                    Label = day.ToString("ddd", culture),
                    Count = count
                });
            }

            // ── Post-proceso: alertas ─────────────────────────────────────────
            var lowStockAlerts = lowStockItems.Select(x => new LowStockAlertDto
            {
                MaterialName = x.Name,
                Sku          = x.SKU,
                OnHand       = x.OnHand,
                ReorderPoint = x.ReorderPoint
            }).ToList();

            var expiringAlerts = expiringLotsList.Select(l => new ExpiringLotAlertDto
            {
                LotNumber      = l.LotNumber,
                MaterialName   = l.Material?.Name ?? "—",
                ExpirationDate = l.ExpirationDate!.Value,
                DaysRemaining  = (int)(l.ExpirationDate!.Value.Date - today).TotalDays
            }).ToList();

            return new DashboardSummaryDto
            {
                // Inventario
                TotalMaterials  = totalMaterials,
                LowStockCount   = lowStockAlerts.Count,
                TotalStockValue = totalStockValue,
                TotalStockUnits = totalStockUnits,

                // Lotes
                ExpiringIn7Days  = expiringIn7,
                ExpiringIn30Days = expiringIn30,

                // Órdenes de venta
                OrdersDraft        = ordersDraft,
                OrdersPicking      = ordersPicking,
                OrdersReadyToShip  = ordersReady,
                OrdersShippedToday = ordersShippedToday,
                OrdersTotalActive  = ordersDraft + ordersPicking + ordersReady,

                // Producción
                WorkOrdersPending        = woPending,
                WorkOrdersInProgress     = woProgress,
                WorkOrdersQualityControl = woQc,

                // Integraciones
                UnmappedProducts  = unmapped,
                ConnectedChannels = connected,

                // Listas
                LowStockAlerts = lowStockAlerts,
                ExpiringLots   = expiringAlerts,
                RecentOrders   = recentOrders,

                // Gráficas
                OrderTrend  = trend,
                StockByZone = stockByZone
            };
        }
    }
}
