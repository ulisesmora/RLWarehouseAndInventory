using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.StockMovements.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Handlers
{
    public class GetKardexQueryHandler : IRequestHandler<GetKardexQuery, List<StockMovementDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetKardexQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<StockMovementDto>> Handle(GetKardexQuery request, CancellationToken cancellationToken)
        {
            var query = _context.StockMovements.AsNoTracking();

            // Vamos aplicando filtros condicionalmente
            if (request.MaterialId.HasValue)
            {
                query = query.Where(m => m.MaterialId == request.MaterialId.Value);
            }

            if (request.WarehouseId.HasValue)
            {
                query = query.Where(m => m.WarehouseId == request.WarehouseId.Value);
            }

            if (request.LotId.HasValue)
            {
                query = query.Where(m => m.LotId == request.LotId.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(m => m.MovementDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(m => m.MovementDate <= request.EndDate.Value);
            }

            // Ejecutamos y mapeamos
            return await query
                .OrderByDescending(m => m.MovementDate) // Siempre lo más nuevo primero
                .ProjectTo<StockMovementDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
