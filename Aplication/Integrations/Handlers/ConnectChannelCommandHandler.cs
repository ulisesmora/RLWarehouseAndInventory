using Inventory.Application.Integrations.Commands;
using Inventory.Application.Integrations.Queries;
using Inventory.Application.Integrations.Services;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Handlers
{
    public class ConnectChannelCommandHandler
        : IRequestHandler<ConnectChannelCommand, ChannelStatusDto>
    {
        private readonly InventoryDbContext  _context;
        private readonly WooCommerceApiService _wcService;
        private readonly ShopifyApiService     _shService;

        public ConnectChannelCommandHandler(
            InventoryDbContext context,
            WooCommerceApiService wcService,
            ShopifyApiService     shService)
        {
            _context   = context;
            _wcService = wcService;
            _shService = shService;
        }

        public async Task<ChannelStatusDto> Handle(
            ConnectChannelCommand request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"[CONNECT] Canal={request.Channel} | Store={request.StoreUrl}");

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

            Console.WriteLine($"[CONNECT] Canal {request.Channel} conectado. ConfigId={config.Id}");

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
