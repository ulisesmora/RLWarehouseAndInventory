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
            if (request.PhysicalCount < 0) throw new ArgumentException("El conteo físico no puede ser negativo.");

            var stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s =>
                    s.MaterialId == request.MaterialId &&
                    s.WarehouseId == request.WarehouseId &&
                    s.StorageBinId == request.StorageBinId &&
                    s.LotId == request.LotId &&
                    s.StatusId == request.StatusId,
                    cancellationToken);

            decimal currentSystemCount = stockItem?.QuantityOnHand ?? 0;

            // Si el conteo físico es igual al del sistema, no hay nada que ajustar.
            if (currentSystemCount == request.PhysicalCount) return;

            // Calculamos la diferencia (Positiva = Encontramos más; Negativa = Se perdió mercancía)
            decimal difference = request.PhysicalCount - currentSystemCount;

            // Actualizamos o creamos el StockItem
            if (stockItem != null)
            {
                // Validación opcional: No puedes ajustar por debajo de lo que ya tienes reservado.
                if (request.PhysicalCount < stockItem.QuantityReserved)
                {
                    throw new InvalidOperationException($"No puedes ajustar el inventario a {request.PhysicalCount} porque ya tienes {stockItem.QuantityReserved} unidades reservadas para clientes.");
                }
                stockItem.QuantityOnHand = request.PhysicalCount;
            }
            else
            {
                stockItem = new StockItem
                {
                    MaterialId = request.MaterialId,
                    WarehouseId = request.WarehouseId,
                    StorageBinId = request.StorageBinId,
                    LotId = request.LotId,
                    StatusId = request.StatusId,
                    QuantityOnHand = request.PhysicalCount,
                    QuantityReserved = 0
                };
                _context.StockItems.Add(stockItem);
            }

            // Si el ajuste lo dejó en cero, limpiamos
            if (stockItem.QuantityOnHand == 0 && stockItem.QuantityReserved == 0)
            {
                _context.StockItems.Remove(stockItem);
            }

            // Generamos el historial del ajuste
            var movement = new StockMovement
            {
                MaterialId = request.MaterialId,
                WarehouseId = request.WarehouseId,
                StorageBinId = request.StorageBinId,
                LotId = request.LotId,

                Type = MovementType.Adjustment,
                Quantity = difference, // Se guardará en negativo si se perdió, en positivo si sobró

                MovementDate = DateTime.UtcNow,
                ReferenceNumber = "AJUSTE-FÍSICO",
                Comments = request.Reason,
                UserId = request.UserId
            };

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
