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
    public class GetStockByMaterialQueryHandler : IRequestHandler<GetStockByMaterialQuery, List<StockItemDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetStockByMaterialQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<StockItemDto>> Handle(GetStockByMaterialQuery request, CancellationToken cancellationToken)
        {
            return await _context.StockItems
                .AsNoTracking() // Vital para consultas de solo lectura
                .Where(s => s.MaterialId == request.MaterialId)
                .ProjectTo<StockItemDto>(_mapper.ConfigurationProvider)
                // Ordenamos para que el Frontend lo vea limpio: Primero por bodega, luego por estante
                .OrderBy(s => s.WarehouseName)
                .ThenBy(s => s.StorageBinCode)
                .ToListAsync(cancellationToken);
        }
    }
}
