using Inventory.Application.Warehouses.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Handlers
{
    public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand>
    {
        private readonly InventoryDbContext _context;

        public DeleteWarehouseCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Warehouses
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"El almacén con ID {request.Id} no existe.");
            }

            // 1. REGLA: No borrar si tiene Stock
            bool hasStock = await _context.StockItems
                .AnyAsync(s => s.WarehouseId == request.Id && s.QuantityAvailable > 0, cancellationToken);

            if (hasStock)
            {
                throw new InvalidOperationException("No se puede eliminar el almacén porque contiene stock. Realiza una transferencia de salida primero.");
            }

            // 2. REGLA: Advertencia si tiene Zonas definidas
            // (Opcional: Podrías permitir borrar si borras las zonas en cascada, 
            // pero es más seguro obligar al usuario a borrar las zonas primero).
            bool hasZones = await _context.Zones
                .AnyAsync(z => z.WarehouseId == request.Id, cancellationToken);

            if (hasZones)
            {
                throw new InvalidOperationException("El almacén tiene zonas/habitaciones configuradas. Elimínalas primero.");
            }

            _context.Warehouses.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
