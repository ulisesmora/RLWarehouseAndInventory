using Inventory.Application.Integrations.Commands;
using Inventory.Application.Integrations.Queries;
using Inventory.Application.Integrations.Services;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Handlers
{
    public class ConnectChannelCommandHandler
        : IRequestHandler<ConnectChannelCommand, ChannelStatusDto>
    {
        private readonly InventoryDbContext    _context;
        private readonly WooCommerceApiService _wcService;
        private readonly ShopifyApiService     _shService;
        private readonly IConfiguration        _cfg;
        private readonly ILogger<ConnectChannelCommandHandler> _logger;

        public ConnectChannelCommandHandler(
            InventoryDbContext context,
            WooCommerceApiService wcService,
            ShopifyApiService     shService,
            IConfiguration        cfg,
            ILogger<ConnectChannelCommandHandler> logger)
        {
            _context   = context;
            _wcService = wcService;
            _shService = shService;
            _cfg       = cfg;
            _logger    = logger;
        }

        public async Task<ChannelStatusDto> Handle(
            ConnectChannelCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[CONNECT] Canal={Channel} | Store={Store}", request.Channel, request.StoreUrl);

            // 1. Verificar conexión con el canal externo
            bool ok = request.Channel switch
            {
                SalesChannel.WooCommerce => await _wcService.TestConnectionAsync(
                    request.StoreUrl, request.ApiKey, request.ApiSecret, cancellationToken),
                SalesChannel.Shopify => await _shService.TestConnectionAsync(
                    request.StoreUrl, request.ApiSecret, cancellationToken),
                _ => throw new InvalidOperationException(
                    $"Canal '{request.Channel}' no soporta conexión directa todavía.")
            };

            if (!ok)
                throw new InvalidOperationException(
                    "No se pudo conectar al canal externo. Verifica la URL y las credenciales.");

            // 2. Buscar config existente o crear una nueva
            var config = await _context.ChannelConfigs
                .FirstOrDefaultAsync(c => c.Channel == request.Channel, cancellationToken);

            if (config == null)
            {
                config = new ChannelConfig { Id = Guid.NewGuid(), Channel = request.Channel };
                _context.ChannelConfigs.Add(config);
            }

            config.StoreUrl    = request.StoreUrl.TrimEnd('/');
            config.ApiKey      = request.ApiKey;
            config.ApiSecret   = request.ApiSecret;
            config.IsConnected = true;
            config.LastError   = null;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[CONNECT] Canal {Channel} conectado. ConfigId={ConfigId}", request.Channel, config.Id);

            // ── Registro automático de webhooks para WooCommerce ──────────────
            if (request.Channel == SalesChannel.WooCommerce)
            {
                try
                {
                    // Derivar la URL del webhook desde la config (igual que el controller)
                    var backendBase = _cfg["Shopify:CallbackUrl"]?
                        .Replace("/api/integrations/shopify/oauth/callback", "")
                        ?? string.Empty;

                    // Si hay una entrada explícita la usamos; si no, construimos desde el base
                    var webhookUrl  = _cfg["WooCommerce:WebhookCallbackUrl"]
                        ?? $"{backendBase}/api/webhooks/woocommerce";

                    if (!string.IsNullOrWhiteSpace(webhookUrl))
                    {
                        var registered = await _wcService.RegisterWebhooksAsync(
                            config.StoreUrl, config.ApiKey, config.ApiSecret, webhookUrl, cancellationToken);

                        if (registered.Count > 0)
                            _logger.LogInformation("[CONNECT WC] ✓ Webhooks registrados: {Count} nuevos ({Ids})",
                                registered.Count, string.Join(", ", registered.ConvertAll(w => w.Topic)));
                        else
                            _logger.LogInformation("[CONNECT WC] Webhooks ya existían — nada que crear.");
                    }
                    else
                    {
                        _logger.LogWarning("[CONNECT WC] No se pudo determinar la URL del webhook — omitiendo registro.");
                    }
                }
                catch (Exception ex)
                {
                    // No fatal: la conexión quedó guardada; el webhook se puede registrar después
                    _logger.LogWarning(ex, "[CONNECT WC] Error registrando webhooks (no fatal). La conexión queda activa.");
                }
            }

            return new ChannelStatusDto
            {
                Channel       = config.Channel.ToString(),
                IsConnected   = true,
                StoreUrl      = config.StoreUrl,
                LastSyncAt    = config.LastSyncAt,
                TotalImported = config.TotalImported,
                LastError     = null
            };
        }
    }
}
