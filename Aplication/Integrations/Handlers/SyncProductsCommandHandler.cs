using Inventory.Application.Integrations.Commands;
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
    public class SyncProductsCommandHandler
        : IRequestHandler<SyncProductsCommand, SyncProductsResultDto>
    {
        private readonly InventoryDbContext    _context;
        private readonly WooCommerceApiService _wcService;
        private readonly ShopifyApiService     _shService;

        public SyncProductsCommandHandler(
            InventoryDbContext context,
            WooCommerceApiService wcService,
            ShopifyApiService shService)
        {
            _context   = context;
            _wcService = wcService;
            _shService = shService;
        }

        public async Task<SyncProductsResultDto> Handle(
            SyncProductsCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Verificar que el canal esté conectado
            var config = await _context.ChannelConfigs
                .FirstOrDefaultAsync(c => c.Channel == request.Channel, cancellationToken);

            if (config == null || !config.IsConnected)
                throw new InvalidOperationException(
                    $"El canal '{request.Channel}' no está conectado.");

            // 2. Cargar materiales y mappings existentes
            var materials = await _context.Materials
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var existingMappings = await _context.ChannelProductMappings
                .Where(m => m.Channel == request.Channel)
                .ToListAsync(cancellationToken);

            // 3. Obtener productos del canal externo (aplanar a lista de SKUs)
            var externalProducts = new List<(string Sku, string ProductId, string Name)>();

            if (request.Channel == SalesChannel.WooCommerce)
            {
                var wcProducts = await _wcService.GetProductsAsync(
                    config.StoreUrl, config.ApiKey, config.ApiSecret,
                    ct: cancellationToken);

                foreach (var p in wcProducts)
                {
                    // Producto simple
                    if (!string.IsNullOrEmpty(p.Sku))
                        externalProducts.Add((p.Sku, p.Id.ToString(), p.Name));
                    // Productos variables sin SKU propio → se mapean por nombre
                    else
                        externalProducts.Add((string.Empty, p.Id.ToString(), p.Name));
                }
            }
            else if (request.Channel == SalesChannel.Shopify)
            {
                var shProducts = await _shService.GetProductsAsync(
                    config.StoreUrl, config.ApiSecret,
                    ct: cancellationToken);

                foreach (var p in shProducts)
                {
                    foreach (var v in p.Variants)
                    {
                        var name = p.Variants.Count == 1
                            ? p.Title
                            : $"{p.Title} - {v.Title}";

                        externalProducts.Add((v.Sku, v.ProductId.ToString(), name));
                    }
                }
            }

            // 4. Procesar cada producto
            int created = 0, updated = 0, matched = 0;

            foreach (var (sku, productId, name) in externalProducts)
            {
                var existing = existingMappings
                    .FirstOrDefault(m => m.ExternalSku == sku
                        || (string.IsNullOrEmpty(sku) && m.ExternalProductId == productId));

                if (existing != null)
                {
                    // Actualizar nombre si cambió
                    if (existing.ExternalProductName != name)
                    {
                        existing.ExternalProductName = name;
                        updated++;
                    }

                    // Intentar auto-match si aún no tiene material
                    if (!existing.MaterialId.HasValue)
                    {
                        var (matId, method) = TryAutoMatch(sku, name, materials);
                        if (matId.HasValue)
                        {
                            existing.MaterialId   = matId;
                            existing.IsAutoMapped = true;
                            existing.MatchMethod  = method;
                            matched++;
                        }
                    }
                }
                else
                {
                    var (matId, method) = TryAutoMatch(sku, name, materials);

                    var mapping = new ChannelProductMapping
                    {
                        Id                  = Guid.NewGuid(),
                        Channel             = request.Channel,
                        ExternalSku         = sku,
                        ExternalProductName = name,
                        ExternalProductId   = productId,
                        MaterialId          = matId,
                        IsAutoMapped        = matId.HasValue,
                        MatchMethod         = method
                    };

                    _context.ChannelProductMappings.Add(mapping);
                    existingMappings.Add(mapping);
                    created++;
                    if (matId.HasValue) matched++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[SYNC PRODUCTS] {request.Channel}: created={created} updated={updated} matched={matched}");

            return new SyncProductsResultDto
            {
                Created = created,
                Updated = updated,
                Matched = matched,
                Message = $"Sincronización completada: {created} nuevo(s), {updated} actualizado(s), {matched} con material asignado."
            };
        }

        private static (Guid? MaterialId, string MatchMethod) TryAutoMatch(
            string sku, string name, List<Material> materials)
        {
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

                var byNameContains = materials.FirstOrDefault(m =>
                    name.Contains(m.Name, StringComparison.OrdinalIgnoreCase) ||
                    m.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
                if (byNameContains != null) return (byNameContains.Id, "FuzzyName");
            }

            return (null, "None");
        }
    }
}
