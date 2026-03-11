using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Suppliers.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Suppliers.Queries
{
    public record GetSuppliersQuery() : IRequest<List<SupplierDto>>;

    public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, List<SupplierDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetSuppliersQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ProjectTo<SupplierDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
