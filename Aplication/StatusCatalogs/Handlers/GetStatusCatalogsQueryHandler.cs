using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.StatusCatalogs.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StatusCatalogs.Handlers
{
    public class GetStatusCatalogsQueryHandler : IRequestHandler<GetStatusCatalogsQuery, List<StatusCatalogDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetStatusCatalogsQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<StatusCatalogDto>> Handle(GetStatusCatalogsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Statuses
                .AsNoTracking()
                .OrderBy(s => s.Name) // Orden alfabético
                .ProjectTo<StatusCatalogDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
