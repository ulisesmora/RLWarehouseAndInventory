using Inventory.Application.Integrations.Commands;
using Inventory.Application.Integrations.Queries;
using Inventory.Application.Integrations.Services;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Handlers
{
    public class ImportOrdersCommandHandler
        : IRequestHandler<ImportOrdersCommand, ImportResultDto>
    {
        private readonly InventoryDbContext    _context;
        private readonly WooCommerceApiService _wcService;
        private readonly ShopifyApiService     _shService;

        public ImportOrdersCommandHandler(
            InventoryDbContext context,
            WooCommerceApiService wcService,
            ShopifyApiService     shService)
        {
            _context   = context;
            _wcService = wcService;
            _shService = shService;
        }

        public async Task<ImportResultDto> Handle(
            ImportOrdersCommand request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"[IMPORT] Canal={request.Channel}");

            // 1. Cargar la configuración del canal
            var config = await _context.ChannelConfigs
                .FirstOrDefaultAsync(c => c.Channel == request.Channel, cancellationToken);

            if (config == null || !config.IsConnected)
                throw new InvalidOperationException(
                    $"El canal '{request.Channel}' no está conectado. Configura las credenciales primero.");

            // 2. Cargar todos los materiales del tenant para auto-match
            var materials = await _context.Materials
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // 3. Cargar mappings existentes (para no volver a procesar SKUs conocidos)
            var existingMappings = await _context.ChannelProductMappings
                .Where(m => m.Channel == request.Channel)
                .ToListAsync(cancellationToken);

            // 4. Obtener pedidos ya importados (para evitar duplicados)
            var existingRefs = await _context.SalesOrders
                .Where(o => o.SourceChannel == request.Channel && o.ExternalReference != null)
                .Select(o => o.ExternalReference!)
                .ToHashSetAsync(cancellationToken);

            // 5. Fetch pedidos del canal externo
            List<(string ExtOrderId, string ExtOrderNum, string CustomerName, string CustomerEmail,
                  string ShippingAddress, DateTime CreatedAt,
                  List<(string Sku, string ProductId, string Name, decimal Qty, decimal Price)> Lines)>
                externalOrders = new();

            try
            {
                if (request.Channel == SalesChannel.WooCommerce)
                {
                    var wcOrders = await _wcService.GetOrdersAsync(
                        config.StoreUrl, config.ApiKey, config.ApiSecret,
                        perPage: request.MaxOrders, ct: cancellationToken);

                    foreach (var o in wcOrders)
                    {
                        var customer = $"{o.Billing.FirstName} {o.Billing.LastName}".Trim();
                        var address  = $"{o.Shipping.Address1}, {o.Shipping.City}, {o.Shipping.Country}".Trim(',', ' ');
                        externalOrders.Add((
                            ExtOrderId:      o.Id.ToString(),
                            ExtOrderNum:     $"WC-{o.Number}",
                            CustomerName:    string.IsNullOrEmpty(customer) ? "Cliente WooCommerce" : customer,
                            CustomerEmail:   o.Billing.Email,
                            ShippingAddress: address,
                            CreatedAt:       o.DateCreated,
                            Lines: o.LineItems.Select(l => (l.Sku, l.ProductId.ToString(), l.Name, l.Quantity, l.PriceDecimal)).ToList()
                        ));
                    }
                }
                else if (request.Channel == SalesChannel.Shopify)
                {
                    var shOrders = await _shService.GetOrdersAsync(
                        config.StoreUrl, config.ApiSecret,
                        limit: request.MaxOrders, ct: cancellationToken);

                    foreach (var o in shOrders)
                    {
                        var customer = o.Customer != null
                            ? $"{o.Customer.FirstName} {o.Customer.LastName}".Trim()
                            : o.Email;
                        var address = o.ShippingAddress != null
                            ? $"{o.ShippingAddress.Address1}, {o.ShippingAddress.City}, {o.ShippingAddress.Country}".Trim(',', ' ')
                            : string.Empty;

                        externalOrders.Add((
                            ExtOrderId:      o.Id.ToString(),
                            ExtOrderNum:     o.Name,
                            CustomerName:    string.IsNullOrEmpty(customer) ? "Cliente Shopify" : customer,
                            CustomerEmail:   o.Email,
                            ShippingAddress: address,
                            CreatedAt:       o.CreatedAt,
                            Lines: o.LineItems.Select(l => (l.Sku, l.ProductId.ToString(), l.Title, l.Quantity, l.PriceDecimal)).ToList()
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                config.LastError = ex.Message;
                await _context.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException($"Error al obtener pedidos de {request.Channel}: {ex.Message}");
            }

            Console.WriteLine($"[IMPORT] Pedidos recibidos del canal: {externalOrders.Count}");

            // 6. Procesar cada pedido
            int imported = 0, skipped = 0, unmappedCount = 0;
            var errors = new List<string>();

            foreach (var (extOrderId, extOrderNum, customerName, customerEmail, shippingAddress, createdAt, lines)
                     in externalOrders)
            {
                // Saltar duplicados
                if (existingRefs.Contains(extOrderId))
                {
                    skipped++;
                    continue;
                }

                // ── Resolver líneas con auto-match ──────────────────────────
                bool anyUnmapped = false;
                var resolvedLines = new List<(Guid? MaterialId, decimal Qty, decimal Price, string SkuName)>();

                foreach (var (sku, productId, name, qty, price) in lines)
                {
                    var (materialId, matchMethod) = ResolveSkuToMaterial(
                        sku, name, productId, request.Channel,
                        materials, existingMappings);

                    // Upsert del mapping
                    var mapping = existingMappings
                        .FirstOrDefault(m => m.ExternalSku == sku && m.Channel == request.Channel);

                    if (mapping == null)
                    {
                        mapping = new ChannelProductMapping
                        {
                            Id                  = Guid.NewGuid(),
                            Channel             = request.Channel,
                            ExternalSku         = sku,
                            ExternalProductName = name,
                            ExternalProductId   = productId,
                            MaterialId          = materialId,
                            IsAutoMapped        = materialId.HasValue,
                            MatchMethod         = matchMethod
                        };
                        _context.ChannelProductMappings.Add(mapping);
                        existingMappings.Add(mapping);
                    }
                    else if (!mapping.MaterialId.HasValue && materialId.HasValue)
                    {
                        // Si antes no había match y ahora sí, actualizar
                        mapping.MaterialId   = materialId;
                        mapping.IsAutoMapped = true;
                        mapping.MatchMethod  = matchMethod;
                    }

                    if (!materialId.HasValue) anyUnmapped = true;
                    resolvedLines.Add((materialId, qty, price, $"{name} (SKU: {sku})"));
                }

                // ── Crear SalesOrder ─────────────────────────────────────────
                try
                {
                    var salesOrder = new SalesOrder
                    {
                        Id                = Guid.NewGuid(),
                        OrderNumber       = $"{request.Channel.ToString().ToUpper()}-{extOrderNum}",
                        CustomerName      = customerName,
                        CustomerEmail     = customerEmail,
                        ShippingAddress   = shippingAddress,
                        SourceChannel     = request.Channel,
                        ExternalReference = extOrderId,
                        Status            = anyUnmapped
                            ? SalesOrderStatus.Draft      // tiene líneas sin material → Draft
                            : SalesOrderStatus.Confirmed  // todo mapeado → directo a Confirmed
                    };
                    _context.SalesOrders.Add(salesOrder);

                    foreach (var (matId, qty, price, skuName) in resolvedLines)
                    {
                        if (!matId.HasValue)
                        {
                            unmappedCount++;
                            Console.WriteLine($"[IMPORT]   ⚠ Línea sin mapeo: {skuName}");
                            continue; // línea ignorada hasta que el usuario la mapee
                        }

                        var line = new SalesOrderLine
                        {
                            Id              = Guid.NewGuid(),
                            SalesOrderId    = salesOrder.Id,
                            MaterialId      = matId.Value,
                            OrderedQuantity = qty,
                            UnitPrice       = price,
                            PickedQuantity  = 0,
                            Status          = SalesOrderLineStatus.Pending
                        };
                        _context.SalesOrderLines.Add(line);

                        // ── FEFO allocation solo si el pedido está Confirmed ───
                        if (salesOrder.Status == SalesOrderStatus.Confirmed)
                        {
                            await AllocateStockFefoAsync(
                                salesOrder, line, matId.Value, qty, cancellationToken);
                        }
                    }

                    if (salesOrder.Status == SalesOrderStatus.Confirmed)
                        salesOrder.Status = SalesOrderStatus.Picking;

                    existingRefs.Add(extOrderId);
                    imported++;

                    Console.WriteLine($"[IMPORT] ✓ Pedido {salesOrder.OrderNumber} importado. Unmapped={anyUnmapped}");
                }
                catch (Exception ex)
                {
                    errors.Add($"{extOrderNum}: {ex.Message}");
                    Console.WriteLine($"[IMPORT] ✗ Error en {extOrderNum}: {ex.Message}");
                }
            }

            // 7. Actualizar metadata del config
            config.LastSyncAt    = DateTime.UtcNow;
            config.TotalImported += imported;
            config.LastError      = errors.Count > 0
                ? $"{errors.Count} pedido(s) fallaron durante la importación."
                : null;

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[IMPORT] Resumen: imported={imported} skipped={skipped} unmapped={unmappedCount} errors={errors.Count}");

            return new ImportResultDto
            {
                Imported  = imported,
                Skipped   = skipped,
                Unmapped  = unmappedCount,
                Message   = $"Importación completada: {imported} nuevo(s), {skipped} duplicado(s), {unmappedCount} línea(s) sin material.",
                Errors    = errors
            };
        }

        // ── Auto-match SKU → Material ─────────────────────────────────────────
        private static (Guid? MaterialId, string MatchMethod) ResolveSkuToMaterial(
            string externalSku,
            string externalName,
            string externalProductId,
            SalesChannel channel,
            List<Material> materials,
            List<ChannelProductMapping> existingMappings)
        {
            // 0. ¿Ya existe un mapping previo confirmado?
            var existing = existingMappings.FirstOrDefault(
                m => m.Channel == channel
                  && m.ExternalSku == externalSku
                  && m.MaterialId.HasValue);
            if (existing != null)
                return (existing.MaterialId, existing.MatchMethod);

            if (string.IsNullOrWhiteSpace(externalSku) && string.IsNullOrWhiteSpace(externalName))
                return (null, "None");

            // 1. Coincidencia exacta por SKU interno
            if (!string.IsNullOrWhiteSpace(externalSku))
            {
                var bySkuExact = materials.FirstOrDefault(m =>
                    string.Equals(m.SKU, externalSku, StringComparison.OrdinalIgnoreCase));
                if (bySkuExact != null) return (bySkuExact.Id, "ExactSku");

                // 2. SKU externo coincide con barcode del material
                var byBarcode = materials.FirstOrDefault(m =>
                    !string.IsNullOrEmpty(m.BarCode) &&
                    string.Equals(m.BarCode, externalSku, StringComparison.OrdinalIgnoreCase));
                if (byBarcode != null) return (byBarcode.Id, "Barcode");
            }

            // 3. Coincidencia exacta por nombre
            if (!string.IsNullOrWhiteSpace(externalName))
            {
                var byNameExact = materials.FirstOrDefault(m =>
                    string.Equals(m.Name, externalName, StringComparison.OrdinalIgnoreCase));
                if (byNameExact != null) return (byNameExact.Id, "ExactName");

                // 4. Fuzzy: nombre del material contenido en el nombre externo (o viceversa)
                var byNameContains = materials.FirstOrDefault(m =>
                    externalName.Contains(m.Name, StringComparison.OrdinalIgnoreCase) ||
                    m.Name.Contains(externalName, StringComparison.OrdinalIgnoreCase));
                if (byNameContains != null) return (byNameContains.Id, "FuzzyName");
            }

            // Sin match
            return (null, "None");
        }

        // ── FEFO allocation (reutiliza la lógica del CreateSalesOrder) ─────────
        private async Task AllocateStockFefoAsync(
            SalesOrder salesOrder, SalesOrderLine line,
            Guid materialId, decimal totalRequired,
            CancellationToken ct)
        {
            var availableStock = await _context.StockItems
                .Include(s => s.Lot)
                .Where(s => s.MaterialId == materialId
                         && (s.QuantityOnHand - s.QuantityReserved - s.AllocatedQuantity) > 0)
                .OrderBy(s => s.Lot != null ? s.Lot.ExpirationDate : DateTime.MaxValue)
                .ThenBy(s => s.CreatedAt)
                .ToListAsync(ct);

            decimal totalAllocated = 0;

            foreach (var stock in availableStock)
            {
                if (totalAllocated >= totalRequired) break;

                decimal free   = stock.QuantityOnHand - stock.QuantityReserved - stock.AllocatedQuantity;
                decimal toTake = Math.Min(free, totalRequired - totalAllocated);
                if (toTake <= 0) continue;

                _context.OutboundPickTasks.Add(new OutboundPickTask
                {
                    Id               = Guid.NewGuid(),
                    SalesOrderId     = salesOrder.Id,
                    SalesOrderLineId = line.Id,
                    MaterialId       = materialId,
                    SourceStockItemId = stock.Id,
                    RequiredQuantity  = toTake,
                    PickedQuantity    = 0,
                    Status            = PickTaskStatus.Pending
                });

                stock.AllocatedQuantity += toTake;
                totalAllocated          += toTake;
            }

            // Si no hay stock suficiente, el pedido puede crearse igualmente (Draft o Confirmed),
            // pero sin tarea de picking completa — el operador lo verá en el monitor.
            if (totalAllocated < totalRequired)
                Console.WriteLine($"[IMPORT] ⚠ Stock insuficiente para MaterialId={materialId}. " +
                    $"Requerido={totalRequired} | Asignado={totalAllocated}");
        }
    }
}
