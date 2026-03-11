using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Materials.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Handlers
{
    public class GetMaterialByIdHandler : IRequestHandler<GetMaterialByIdQuery, MaterialDto>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetMaterialByIdHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MaterialDto> Handle(GetMaterialByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _context.Materials
                .Where(m => m.Id == request.Id)
                // ProjectTo hace el SELECT name, sku... FROM Materials (SQL optimizado)
                .ProjectTo<MaterialDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null) throw new Exception("Material no encontrado"); // O NotFoundException

            return dto;
        }
    }
}
