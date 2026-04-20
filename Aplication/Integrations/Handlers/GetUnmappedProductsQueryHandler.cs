using Inventory.Application.Integrations.Queries;
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
    public class GetUnmappedProductsQueryHandler
        : IRequestHandler<GetUnmappedProductsQuery, List<UnmappedProductDto>>
    {
        private readonly InventoryDbContext _context;

        public GetUnmappedProductsQueryHandler(InventoryDbContext context)
            => _context = context;

        public async Task<List<UnmappedProductDto>> Handle(
            GetUnmappedProductsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.ChannelProductMappings
                .AsNoTracking()
                .Where(m => !m.MaterialId.HasValue);  // solo los no mapeados

            // Filtro opcional por canal
            if (!string.IsNullOrWhiteSpace(request.Channel) &&
                Enum.TryParse<SalesChannel>(request.Channel, ignoreCase: true, out var channel))
            {
                query = query.Where(m => m.Channel == channel);
            }

            var mappings = await query
                .OrderBy(m => m.Channel)
                .ThenBy(m => m.ExternalProductName)
                .ToListAsync(cancellationToken);

            return mappings.Select(m => new UnmappedProductDto
            {
                Id                  = m.Id,
                Channel             = m.Channel.ToString(),
                ExternalSku         = m.ExternalSku,
                ExternalProductName = m.ExternalProductName,
                ExternalProductId   = m.ExternalProductId,
                MatchMethod         = m.MatchMethod
            }).ToList();
        }
    }
}
