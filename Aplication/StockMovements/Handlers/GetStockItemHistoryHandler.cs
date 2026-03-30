using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.StockMovements.Commands;
using Inventory.Application.StockMovements.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Handlers
{
    public class GetStockItemHistoryHandler : IRequestHandler<GetStockItemHistoryQuery, List<StockMovementDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetStockItemHistoryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<StockMovementDto>> Handle(GetStockItemHistoryQuery request, CancellationToken cancellationToken)
        {
            // Traemos la línea de tiempo de ESA CAJA específica
            return await _context.StockMovements
                .AsNoTracking()
                .Where(m => m.StockItemId == request.StockItemId)
                .OrderByDescending(m => m.MovementDate) // Del más reciente al más antiguo
                .ProjectTo<StockMovementDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
