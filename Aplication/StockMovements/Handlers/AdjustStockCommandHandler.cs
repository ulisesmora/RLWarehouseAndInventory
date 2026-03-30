using Inventory.Application.StockMovements.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Handlers
{
    public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand>
    {
        private readonly InventoryDbContext _context;

        public AdjustStockCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(AdjustStockCommand request, CancellationToken cancellationToken)
        {
            if (request.PhysicalCount < 0)
                throw new ArgumentException("El conteo físico no puede ser negativo.");

            // 1. BUSCAMOS LA CAJA ESPECÍFICA (LPN)
            var stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s => s.Id == request.StockItemId, cancellationToken);

            if (stockItem == null)
                throw new InvalidOperationException("No se encontró el contenedor/LPN en el sistema para ajustar.");

            decimal currentSystemCount = stockItem.QuantityOnHand;

            // Si el conteo físico es exactamente igual, no hay nada que ajustar.
            if (currentSystemCount == request.PhysicalCount) return;

            // 2. REGLA DE NEGOCIO: Proteger inventario comprometido
            if (request.PhysicalCount < stockItem.QuantityReserved)
            {
                throw new InvalidOperationException($"No puedes ajustar esta caja a {request.PhysicalCount} porque ya tiene {stockItem.QuantityReserved} unidades reservadas para un pedido.");
            }

            // Calculamos la diferencia (Positiva = Sobró mercancía; Negativa = Faltó mercancía)
            decimal difference = request.PhysicalCount - currentSystemCount;

            // 3. ACTUALIZAMOS LA CAJA
            stockItem.QuantityOnHand = request.PhysicalCount;

            // 4. GENERAMOS EL HISTORIAL DEL AJUSTE
            var movement = new StockMovement
            {
                StockItemId = stockItem.Id,
                MaterialId = stockItem.MaterialId,
                WarehouseId = stockItem.WarehouseId,
                StorageBinId = stockItem.StorageBinId,
                LotId = stockItem.LotId,

                Type = MovementType.Adjustment,
                Quantity = difference, // Guardamos la diferencia exacta del ajuste

                MovementDate = DateTime.UtcNow,
                ReferenceNumber = stockItem.ReferenceNumber, // Mantenemos la matrícula
                Comments = $"AJUSTE FÍSICO: {request.Reason}. (Sistema tenía: {currentSystemCount}, Físico: {request.PhysicalCount})",
                UserId = request.UserId
            };

            _context.StockMovements.Add(movement);

            if (stockItem.QuantityOnHand == 0 && stockItem.QuantityReserved == 0)
            {
                _context.StockItems.Remove(stockItem);
            }

            // 6. GUARDAR TRANSACCIÓN (ACID)
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
