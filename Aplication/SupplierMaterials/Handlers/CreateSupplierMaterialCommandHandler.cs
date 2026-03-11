using AutoMapper;
using Inventory.Application.SupplierMaterials.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Handlers
{
    public class CreateSupplierMaterialCommandHandler : IRequestHandler<CreateSupplierMaterialCommand, Guid>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public CreateSupplierMaterialCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateSupplierMaterialCommand request, CancellationToken cancellationToken)
        {
            // 1. REGLA: Prevenir duplicados
            bool exists = await _context.SupplierMaterials
                .AnyAsync(sm => sm.SupplierId == request.SupplierId && sm.MaterialId == request.MaterialId, cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("Este proveedor ya tiene configurado este material. Por favor, actualice el registro existente.");
            }

            // 2. REGLA (Opcional pero recomendada): Si este es 'Preferred', quitar el 'Preferred' a los demás de este Material
            if (request.IsPreferred)
            {
                var existingPreferred = await _context.SupplierMaterials
                    .Where(sm => sm.MaterialId == request.MaterialId && sm.IsPreferred)
                    .ToListAsync(cancellationToken);

                foreach (var item in existingPreferred)
                {
                    item.IsPreferred = false;
                }
            }

            var entity = _mapper.Map<SupplierMaterial>(request);

            _context.SupplierMaterials.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
