using Inventory.Application.SupplierMaterials.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Handlers
{
    public class DeleteSupplierMaterialCommandHandler : IRequestHandler<DeleteSupplierMaterialCommand>
    {
        private readonly InventoryDbContext _context;

        public DeleteSupplierMaterialCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteSupplierMaterialCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.SupplierMaterials
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"El registro con ID {request.Id} no existe.");
            }

            // Borrado físico sin miedo, es un catálogo de condiciones de compra
            _context.SupplierMaterials.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
