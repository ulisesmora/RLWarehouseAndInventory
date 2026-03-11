using Inventory.Application.Suppliers.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Suppliers.Handlers
{
    public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand>
    {
        private readonly InventoryDbContext _context;

        public DeleteSupplierCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Suppliers.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null) throw new KeyNotFoundException($"Proveedor {request.Id} no encontrado.");

            // REGLA DE NEGOCIO: ¿Nos ha surtido lotes?
            bool hasLots = await _context.Lots.AnyAsync(l => l.SupplierId == request.Id, cancellationToken);
            if (hasLots)
            {
                throw new InvalidOperationException("No se puede eliminar el proveedor porque tiene lotes de inventario asociados. Considere desactivarlo en su lugar.");
            }

            _context.Suppliers.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
