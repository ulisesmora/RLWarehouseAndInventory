using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.SupplierMaterials.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Handlers
{
    public class GetSuppliersByMaterialQueryHandler : IRequestHandler<GetSuppliersByMaterialQuery, List<SupplierMaterialDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetSuppliersByMaterialQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SupplierMaterialDto>> Handle(GetSuppliersByMaterialQuery request, CancellationToken cancellationToken)
        {
            return await _context.SupplierMaterials
                .AsNoTracking()
                .Where(sm => sm.MaterialId == request.MaterialId)
                // Ordenamos: Primero el favorito, luego por precio más barato
                .OrderByDescending(sm => sm.IsPreferred)
                .ThenBy(sm => sm.UnitCost)
                .ProjectTo<SupplierMaterialDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
