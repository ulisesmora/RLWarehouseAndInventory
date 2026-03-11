using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Lots.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Handlers
{
    public class GetExpiringLotsQueryHandler : IRequestHandler<GetExpiringLotsQuery, List<LotDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;
        public GetExpiringLotsQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<LotDto>> Handle(GetExpiringLotsQuery request, CancellationToken cancellationToken)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(request.DaysThreshold);

            return await _context.Lots
                .AsNoTracking()
                .Where(l => l.ExpirationDate != null
                         && l.ExpirationDate <= thresholdDate
                         // 🔥 NO ocultamos los que YA vencieron. ¡El gerente debe verlos en rojo!

                         // 🔥 Solo alertamos si TODAVÍA TENEMOS CAJAS FÍSICAS de ese lote
                         && l.StockItems.Sum(s => s.QuantityOnHand) > 0)
                .OrderBy(l => l.ExpirationDate)
                .ProjectTo<LotDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
