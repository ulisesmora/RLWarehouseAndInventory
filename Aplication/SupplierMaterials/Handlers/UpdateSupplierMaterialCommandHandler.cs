using AutoMapper;
using Inventory.Application.SupplierMaterials.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Handlers
{
    public class UpdateSupplierMaterialCommandHandler : IRequestHandler<UpdateSupplierMaterialCommand>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public UpdateSupplierMaterialCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task Handle(UpdateSupplierMaterialCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.SupplierMaterials
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"El registro con ID {request.Id} no existe.");
            }

            // REGLA DE NEGOCIO: Gestionar el favorito (IsPreferred)
            // Si el usuario lo está marcando como favorito ahora mismo:
            if (request.IsPreferred && !entity.IsPreferred)
            {
                // Buscamos si hay OTRO proveedor para ESTE material que sea el favorito actual
                var existingPreferred = await _context.SupplierMaterials
                    .Where(sm => sm.MaterialId == entity.MaterialId && sm.IsPreferred && sm.Id != entity.Id)
                    .ToListAsync(cancellationToken);

                // Les quitamos la corona
                foreach (var item in existingPreferred)
                {
                    item.IsPreferred = false;
                }
            }

            // Mapeo Automático: Sobreescribe los valores del entity con los del request
            _mapper.Map(request, entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
