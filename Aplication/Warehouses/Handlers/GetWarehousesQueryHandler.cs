using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Warehouses.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Handlers
{
    public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, List<WarehouseDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetWarehousesQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
        {
            // Usamos ProjectTo para optimizar y calcular el ZonesCount automáticamente
            return await _context.Warehouses
                .AsNoTracking()
                .ProjectTo<WarehouseDto>(_mapper.ConfigurationProvider)
                .OrderBy(w => w.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
