using Inventory.Application.StorageBins.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Handlers
{
    public class DeleteStorageBinCommandHandler : IRequestHandler<DeleteStorageBinCommand>
    {
        private readonly InventoryDbContext _context;

        public DeleteStorageBinCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteStorageBinCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscamos la ubicación incluyendo su stock
            var entity = await _context.StorageBins
                .Include(b => b.StockItems)
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"La ubicación {request.Id} no existe.");
            }

            // 2. REGLA DE SEGURIDAD: ¿Hay stock físico aquí?
            // Sumamos todo el stock que haya en este bin
            var totalStockInBin = entity.StockItems.Sum(s => s.QuantityAvailable);

            if (totalStockInBin > 0)
            {
                throw new InvalidOperationException($"No se puede eliminar la ubicación '{entity.Code}' porque contiene {totalStockInBin} unidades de producto. Mueve el stock a otro lugar primero.");
            }

            // 3. Borrar
            _context.StorageBins.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
