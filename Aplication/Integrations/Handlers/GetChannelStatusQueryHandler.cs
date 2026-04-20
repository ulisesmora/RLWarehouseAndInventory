using Inventory.Application.Integrations.Queries;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Handlers
{
    public class GetChannelStatusQueryHandler
        : IRequestHandler<GetChannelStatusQuery, List<ChannelStatusDto>>
    {
        private readonly InventoryDbContext _context;

        public GetChannelStatusQueryHandler(InventoryDbContext context)
            => _context = context;

        public async Task<List<ChannelStatusDto>> Handle(
            GetChannelStatusQuery request,
            CancellationToken cancellationToken)
        {
            // Obtener configs persistidas en BD
            var configs = await _context.ChannelConfigs
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Construir respuesta: incluimos todos los canales conocidos,
            // mostrando "no configurado" cuando no hay registro en BD.
            var allChannels = new[]
            {
                SalesChannel.WooCommerce,
                SalesChannel.Shopify
            };

            var result = new List<ChannelStatusDto>();

            foreach (var channel in allChannels)
            {
                var cfg = configs.FirstOrDefault(c => c.Channel == channel);

                result.Add(new ChannelStatusDto
                {
                    Channel       = channel.ToString(),
                    IsConnected   = cfg?.IsConnected ?? false,
                    StoreUrl      = cfg?.StoreUrl    ?? string.Empty,
                    LastSyncAt    = cfg?.LastSyncAt,
                    TotalImported = cfg?.TotalImported ?? 0,
                    LastError     = cfg?.LastError
                });
            }

            return result;
        }
    }
}
