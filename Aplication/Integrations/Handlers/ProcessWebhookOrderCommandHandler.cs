using Inventory.Application.Integrations.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Handlers
{
    /// <summary>
    /// Procesa un pedido recibido en tiempo real vía webhook.
    /// Reutiliza la misma lógica de auto-match y FEFO que ImportOrdersCommandHandler,
    /// pero opera sobre un único pedido y sin contexto de usuario (webhook público).
    /// </summary>
    public class ProcessWebhookOrderCommandHandler
        : IRequestHandler<ProcessWebhookOrderCommand, Unit>
    {
        private readonly InventoryDbContext _context;

        public ProcessWebhookOrderCommandHandler(InventoryDbContext context)
            => _context = context;

        public async Task<Unit> Handle(
            ProcessWebhookOrderCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Buscar el ChannelConfig por dominio de tienda (sin filtro de tenant)
            //    para obtener el OrganizationId correcto.
            var config = await _context.ChannelConfigs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c =>
                    c.Channel == request.Channel &&
                    c.StoreUrl.Contains(request.StoreIdentifier.Replace("https://", "").Replace("http://", "")),
                    cancellationToken);

            if (config == null || !config.IsConnected)
            {
                Console.WriteLine($"[WEBHOOK] Canal {request.Channel} no configurado para '{request.StoreIdentifier}'. Ignorando.");
                return Unit.Value;
            }

            var orgId = config.OrganizationId;

            // 2. Evitar duplicados
            var alreadyExists = await _context.SalesOrders
                .IgnoreQueryFilters()
                .AnyAsync(o =>
                    o.OrganizationId    == orgId &&
                    o.SourceChannel     == request.Channel &&
                    o.ExternalReference == request.ExtOrderId,
                    cancellationToken);

            if (alreadyExists)
            {
                Console.WriteLine($"[WEBHOOK] Pedido {request.ExtOrderId} ya existe. Ignorando.");
                return Unit.Value;
            }

            // 3. Cargar materiales y mappings del tenant
            var materials = await _context.Materials
                .IgnoreQueryFilters()
                .Where(m => m.OrganizationId == orgId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var existingMappings = await _context.ChannelProductMappings
                .IgnoreQueryFilters()
                .Where(m => m.OrganizationId == orgId && m.Channel == request.Channel)
                .ToListAsync(cancellationToken);

            // 4. Deserializar líneas del JSON
            var rawLines = JsonSerializer.Deserialize<List<WebhookLineItem>>(
                request.LineItemsJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    // WooCommerce/Shopify pueden enviar price como número o como string
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                }) ?? new();

            // 5. Resolver líneas con auto-match
            bool anyUnmapped = false;
            var resolvedLines = new List<(Guid? MaterialId, decimal Qty, decimal Price, string SkuName)>();

            foreach (var line in rawLines)
            {
                var (materialId, matchMethod) = ResolveSkuToMaterial(
                    line.Sku, line.Name, line.ProductId.ToString(),
                    request.Channel, materials, existingMappings);

                // Upsert mapping
                var mapping = existingMappings
                    .FirstOrDefault(m => m.ExternalSku == line.Sku && m.Channel == request.Channel);

                if (mapping == null)
                {
                    mapping = new ChannelProductMapping
                    {
                        Id                  = Guid.NewGuid(),
                        OrganizationId      = orgId,
                        Channel             = request.Channel,
                        ExternalSku         = line.Sku,
                        ExternalProductName = line.Name,
                        ExternalProductId   = line.ProductId.ToString(),
                        MaterialId          = materialId,
                        IsAutoMapped        = materialId.HasValue,
                        MatchMethod         = matchMethod
                    };
                    _context.ChannelProductMappings.Add(mapping);
                    existingMappings.Add(mapping);
                }
                else if (!mapping.MaterialId.HasValue && materialId.HasValue)
                {
                    mapping.MaterialId   = materialId;
                    mapping.IsAutoMapped = true;
                    mapping.MatchMethod  = matchMethod;
                }

                if (!materialId.HasValue) anyUnmapped = true;
                resolvedLines.Add((materialId, line.Quantity, line.Price, $"{line.Name} (SKU: {line.Sku})"));
            }

            // 6. Crear SalesOrder
            var salesOrder = new SalesOrder
            {
                Id                = Guid.NewGuid(),
                OrganizationId    = orgId,
                OrderNumber       = $"{request.Channel.ToString().ToUpper()}-{request.ExtOrderNum}",
                CustomerName      = request.CustomerName,
                CustomerEmail     = request.CustomerEmail,
                ShippingAddress   = request.ShippingAddress,
                SourceChannel     = request.Channel,
                ExternalReference = request.ExtOrderId,
                Status            = anyUnmapped
                    ? SalesOrderStatus.Draft
                    : SalesOrderStatus.Confirmed
            };
            _context.SalesOrders.Add(salesOrder);

            foreach (var (matId, qty, price, skuName) in resolvedLines)
            {
                if (!matId.HasValue) continue;

                var line = new SalesOrderLine
                {
                    Id              = Guid.NewGuid(),
                    OrganizationId  = orgId,
                    SalesOrderId    = salesOrder.Id,
                    MaterialId      = matId.Value,
                    OrderedQuantity = qty,
                    UnitPrice       = price,
                    PickedQuantity  = 0,
                    Status          = SalesOrderLineStatus.Pending
                };
                _context.SalesOrderLines.Add(line);

                if (salesOrder.Status == SalesOrderStatus.Confirmed)
                    await AllocateStockFefoAsync(salesOrder, line, matId.Value, qty, orgId, cancellationToken);
            }

            if (salesOrder.Status == SalesOrderStatus.Confirmed)
                salesOrder.Status = SalesOrderStatus.Picking;

            // 7. Actualizar stats del canal
            config.LastSyncAt     = DateTime.UtcNow;
            config.TotalImported += 1;

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[WEBHOOK] ✓ Pedido {salesOrder.OrderNumber} creado vía webhook. Unmapped={anyUnmapped}");
            return Unit.Value;
        }

        // ── FEFO allocation ───────────────────────────────────────────────────
        private async Task AllocateStockFefoAsync(
            SalesOrder salesOrder, SalesOrderLine line,
            Guid materialId, decimal totalRequired,
            Guid orgId, CancellationToken ct)
        {
            var availableStock = await _context.StockItems
                .IgnoreQueryFilters()
                .Include(s => s.Lot)
                .Where(s => s.OrganizationId == orgId
                         && s.MaterialId     == materialId
                         && (s.QuantityOnHand - s.QuantityReserved - s.AllocatedQuantity) > 0)
                .OrderBy(s => s.Lot != null ? s.Lot.ExpirationDate : DateTime.MaxValue)
                .ThenBy(s => s.CreatedAt)
                .ToListAsync(ct);

            decimal allocated = 0;

            foreach (var stock in availableStock)
            {
                if (allocated >= totalRequired) break;

                decimal free   = stock.QuantityOnHand - stock.QuantityReserved - stock.AllocatedQuantity;
                decimal toTake = Math.Min(free, totalRequired - allocated);
                if (toTake <= 0) continue;

                _context.OutboundPickTasks.Add(new OutboundPickTask
                {
                    Id                = Guid.NewGuid(),
                    OrganizationId    = orgId,
                    SalesOrderId      = salesOrder.Id,
                    SalesOrderLineId  = line.Id,
                    MaterialId        = materialId,
                    SourceStockItemId = stock.Id,
                    RequiredQuantity  = toTake,
                    PickedQuantity    = 0,
                    Status            = PickTaskStatus.Pending
                });

                stock.AllocatedQuantity += toTake;
                allocated               += toTake;
            }
        }

        // ── Auto-match SKU → Material ─────────────────────────────────────────
        private static (Guid? MaterialId, string MatchMethod) ResolveSkuToMaterial(
            string sku, string name, string productId,
            SalesChannel channel,
            List<Material> materials,
            List<ChannelProductMapping> mappings)
        {
            var existing = mappings.FirstOrDefault(
                m => m.Channel == channel && m.ExternalSku == sku && m.MaterialId.HasValue);
            if (existing != null) return (existing.MaterialId, existing.MatchMethod);

            if (!string.IsNullOrWhiteSpace(sku))
            {
                var bySkuExact = materials.FirstOrDefault(m =>
                    string.Equals(m.SKU, sku, StringComparison.OrdinalIgnoreCase));
                if (bySkuExact != null) return (bySkuExact.Id, "ExactSku");

                var byBarcode = materials.FirstOrDefault(m =>
                    !string.IsNullOrEmpty(m.BarCode) &&
                    string.Equals(m.BarCode, sku, StringComparison.OrdinalIgnoreCase));
                if (byBarcode != null) return (byBarcode.Id, "Barcode");
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var byNameExact = materials.FirstOrDefault(m =>
                    string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
                if (byNameExact != null) return (byNameExact.Id, "ExactName");

                var byFuzzy = materials.FirstOrDefault(m =>
                    name.Contains(m.Name, StringComparison.OrdinalIgnoreCase) ||
                    m.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
                if (byFuzzy != null) return (byFuzzy.Id, "FuzzyName");
            }

            return (null, "None");
        }
    }

    // ── DTO interno para deserializar las líneas del JSON ──────────────────────
    internal class WebhookLineItem
    {
        public long    ProductId { get; set; }
        public string  Name      { get; set; } = string.Empty;
        public string  Sku       { get; set; } = string.Empty;
        public decimal Quantity  { get; set; }
        // WooCommerce envía price como número JSON — AllowReadingFromString en las opts lo cubre igual
        public decimal Price     { get; set; }
    }
}
