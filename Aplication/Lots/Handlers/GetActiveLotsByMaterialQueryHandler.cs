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
    public class GetActiveLotsByMaterialQueryHandler : IRequestHandler<GetActiveLotsByMaterialQuery, List<LotDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetActiveLotsByMaterialQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<LotDto>> Handle(GetActiveLotsByMaterialQuery request, CancellationToken cancellationToken)
        {
            return await _context.Lots
         .AsNoTracking()
         .Where(l => l.MaterialId == request.MaterialId)
            //      && l.StockItems.Sum(s => s.QuantityOnHand) > 0)

         // 🔥 ARREGLO FEFO: Los lotes sin fecha de caducidad (NULL) se van al final de la cola
         // simulando que caducan en el fin de los tiempos (DateTime.MaxValue)
         .OrderBy(l => l.ExpirationDate ?? DateTime.MaxValue)
         .ProjectTo<LotDto>(_mapper.ConfigurationProvider)
         .ToListAsync(cancellationToken);
        }
    }
}
