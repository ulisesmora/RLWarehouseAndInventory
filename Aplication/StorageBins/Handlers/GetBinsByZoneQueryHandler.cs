using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.StorageBins.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Handlers
{
    public class GetBinsByZoneQueryHandler : IRequestHandler<GetBinsByZoneQuery, List<StorageBinDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetBinsByZoneQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<StorageBinDto>> Handle(GetBinsByZoneQuery request, CancellationToken cancellationToken)
        {
            return await _context.StorageBins
                .AsNoTracking()
                .Where(b => b.ZoneId == request.ZoneId)
                .ProjectTo<StorageBinDto>(_mapper.ConfigurationProvider)
                .OrderBy(b => b.Code) // Ordenamos alfabéticamente (A-01, A-02...)
                .ToListAsync(cancellationToken);
        }
    }
}
