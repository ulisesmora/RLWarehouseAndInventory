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
    public class GetZoneByIdQueryHandler : IRequestHandler<GetZoneByIdQuery, ZoneDetailDto>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetZoneByIdQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ZoneDetailDto> Handle(GetZoneByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _context.Zones
                .AsNoTracking()
                .Where(z => z.Id == request.Id)
                .ProjectTo<ZoneDetailDto>(_mapper.ConfigurationProvider) // <-- Hace el JOIN con StorageBins automáticamente
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null) throw new KeyNotFoundException($"La zona con ID {request.Id} no existe.");

            return dto;
        }
    }
}
