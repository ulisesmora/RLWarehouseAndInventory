using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.UnitOfMesaure.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Handlers
{
    public class GetUnitsOfMeasureHandler : IRequestHandler<GetUnitsOfMeasureQuery, List<UnitOfMeasureDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetUnitsOfMeasureHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<UnitOfMeasureDto>> Handle(GetUnitsOfMeasureQuery request, CancellationToken cancellationToken)
        {
            return await _context.UnitOfMeasures
                .AsNoTracking()
                .ProjectTo<UnitOfMeasureDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
