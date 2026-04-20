using Inventory.Application.SalesOrders.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.SalesOrders.Handlers
{
    public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, Guid>
    {
        private readonly InventoryDbContext _context;

        public CreateSalesOrderCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"[SO] INICIO CreateSalesOrder");
            Console.WriteLine($"[SO]   Cliente     = {request.CustomerName}");
            Console.WriteLine($"[SO]   Canal       = {request.SourceChannel}");
            Console.WriteLine($"[SO]   Líneas      = {request.Lines.Count}");
            Console.WriteLine($"[SO]   TenantId    = {_context.CurrentTenantId}");

            // 1. Crear la cabecera del pedido
            var salesOrder = new SalesOrder
            {
                Id                = Guid.NewGuid(),
                OrderNumber       = $"SO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                CustomerName      = request.CustomerName,
                CustomerEmail     = request.CustomerEmail,
                ShippingAddress   = request.ShippingAddress,
                SourceChannel     = request.SourceChannel,
                ShipByDate        = request.ShipByDate,
                Notes             = request.Notes,
                ExternalReference = request.ExternalReference,
                Status            = SalesOrderStatus.Confirmed
            };

            _context.SalesOrders.Add(salesOrder);

            // 2. Por cada línea del pedido: crear la línea + asignar stock FEFO
            foreach (var lineInput in request.Lines)
            {
                var line = new SalesOrderLine
                {
                    Id               = Guid.NewGuid(),
                    SalesOrderId     = salesOrder.Id,
                    MaterialId       = lineInput.MaterialId,
                    OrderedQuantity  = lineInput.OrderedQuantity,
                    UnitPrice        = lineInput.UnitPrice,
                    PickedQuantity   = 0,
                    Status           = SalesOrderLineStatus.Pending
                };

                _context.SalesOrderLines.Add(line);

                Console.WriteLine($"--------------------------------------------------");
                Console.WriteLine($"[SO] Línea MaterialId={lineInput.MaterialId} | Qty={lineInput.OrderedQuantity}");

                // --- Diagnóstico: todos los StockItems del material ---
                var allForMaterial = await _context.StockItems
                    .Where(s => s.MaterialId == lineInput.MaterialId)
                    .ToListAsync(cancellationToken);

                Console.WriteLine($"[SO]   StockItems totales (sin filtro): {allForMaterial.Count}");
                foreach (var si in allForMaterial)
                {
                    decimal libre = si.QuantityOnHand - si.QuantityReserved - si.AllocatedQuantity;
                    Console.WriteLine($"[SO]     ID={si.Id} | OnHand={si.QuantityOnHand} | Allocated={si.AllocatedQuantity} | LIBRE={libre}");
                }

                // --- FEFO: stock libre, ordenado por ExpirationDate ASC, luego CreatedAt ASC ---
                var availableStock = await _context.StockItems
                    .Include(s => s.Lot)
                    .Where(s => s.MaterialId == lineInput.MaterialId
                             && (s.QuantityOnHand - s.QuantityReserved - s.AllocatedQuantity) > 0)
                    .OrderBy(s => s.Lot != null ? s.Lot.ExpirationDate : DateTime.MaxValue)
                    .ThenBy(s => s.CreatedAt)
                    .ToListAsync(cancellationToken);

                decimal totalRequired  = lineInput.OrderedQuantity;
                decimal totalAllocated = 0;

                decimal totalFree = availableStock.Sum(s => s.QuantityOnHand - s.QuantityReserved - s.AllocatedQuantity);
                Console.WriteLine($"[SO]   Stock disponible (FEFO): {availableStock.Count} items | Libre total={totalFree}");

                foreach (var stock in availableStock)
                {
                    if (totalAllocated >= totalRequired) break;

                    decimal freeInItem  = stock.QuantityOnHand - stock.QuantityReserved - stock.AllocatedQuantity;
                    decimal missing     = totalRequired - totalAllocated;
                    decimal toTake      = Math.Min(freeInItem, missing);

                    if (toTake <= 0) continue;

                    Console.WriteLine($"[SO]   -> StockItem {stock.Id}: libre={freeInItem} | tomar={toTake}");

                    var pickTask = new OutboundPickTask
                    {
                        Id                 = Guid.NewGuid(),
                        SalesOrderId       = salesOrder.Id,
                        SalesOrderLineId   = line.Id,
                        MaterialId         = lineInput.MaterialId,
                        SourceStockItemId  = stock.Id,
                        RequiredQuantity   = toTake,
                        PickedQuantity     = 0,
                        Status             = PickTaskStatus.Pending
                    };

                    _context.OutboundPickTasks.Add(pickTask);
                    stock.AllocatedQuantity += toTake;
                    totalAllocated += toTake;
                }

                Console.WriteLine($"[SO]   RESULTADO: {totalAllocated} / {totalRequired}");

                if (totalAllocated < totalRequired)
                {
                    Console.WriteLine($"[SO] *** FALLO: Stock insuficiente para MaterialId={lineInput.MaterialId} ***");
                    Console.WriteLine("==================================================");
                    throw new InvalidOperationException(
                        $"Stock insuficiente para MaterialId={lineInput.MaterialId}. " +
                        $"Requerido: {totalRequired:F4} | Disponible (FEFO): {totalFree:F4}. " +
                        $"Verifica el inventario o reduce la cantidad pedida.");
                }
            }

            // 3. El pedido inicia directamente en Picking (ya tiene tareas asignadas)
            salesOrder.Status = SalesOrderStatus.Picking;

            Console.WriteLine($"[SO] Pedido {salesOrder.OrderNumber} creado. Guardando...");
            Console.WriteLine("==================================================");

            await _context.SaveChangesAsync(cancellationToken);
            return salesOrder.Id;
        }
    }
}
