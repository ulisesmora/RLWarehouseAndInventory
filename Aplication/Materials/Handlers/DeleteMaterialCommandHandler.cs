using Inventory.Application.Materials.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Handlers
{
    public class DeleteMaterialCommandHandler : IRequestHandler<DeleteMaterialCommand, Guid>
    {
        private readonly InventoryDbContext _context;

        public DeleteMaterialCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(DeleteMaterialCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscar la entidad (incluyendo stock para verificar)
            var entity = await _context.Materials
                .Include(m => m.StockItems) // Traemos el stock para contar
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                // Puedes lanzar una excepción personalizada "NotFoundException"
                throw new KeyNotFoundException($"El material con ID {request.Id} no existe.");
            }

            // 2. REGLA DE NEGOCIO: No borrar si hay stock físico
            var totalStock = entity.StockItems.Sum(s => s.QuantityOnHand);

            if (totalStock > 0)
            {
                throw new InvalidOperationException($"No puedes eliminar el material '{entity.Name}' porque tiene {totalStock} unidades en existencia. Realiza un ajuste de salida primero.");
            }

            // 3. REGLA DE SEGURIDAD (Opcional): 
            // Si ya tiene historial de movimientos (aunque ahora stock sea 0), 
            // lo mejor es hacer un "Soft Delete" (IsDeleted = true) en lugar de borrarlo.
            // Si tu entidad BaseEntity tiene SoftDelete, úsalo. Si no, hacemos Hard Delete:

            // Verificamos si hay historial (si tienes DbSet<StockMovement>)
            bool hasHistory = await _context.StockMovements
                .AnyAsync(sm => sm.MaterialId == request.Id, cancellationToken);

            if (hasHistory)
            {
                // Opción A: Bloquear borrado
                throw new InvalidOperationException("No se puede eliminar este material porque tiene historial de movimientos. Considere desactivarlo.");

                // Opción B: Si tuvieras Soft Delete implementado:
                // entity.IsDeleted = true;
            }
            else
            {
                // 4. Si está limpio (nuevo, sin stock, sin historial), lo borramos de raíz
                _context.Materials.Remove(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
