using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Zones.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Handlers
{
    public class GetZonesByWarehouseQueryHandler : IRequestHandler<GetZonesByWarehouseQuery, List<ZoneDetailDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetZonesByWarehouseQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ZoneDetailDto>> Handle(GetZonesByWarehouseQuery request, CancellationToken cancellationToken)
        {
            return await _context.Zones
                .AsNoTracking()
                .Where(z => z.WarehouseId == request.WarehouseId)
                .ProjectTo<ZoneDetailDto>(_mapper.ConfigurationProvider) // Mapeo optimizado SQL
                .OrderBy(z => z.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
