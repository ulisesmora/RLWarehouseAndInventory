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
    public class ReceiveStockCommandHandler : IRequestHandler<ReceiveStockCommand, Guid>
    {
        private readonly InventoryDbContext _context;

        public ReceiveStockCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(ReceiveStockCommand request, CancellationToken cancellationToken)
        {
            // 1. BUSCAR LA FOTO ACTUAL (Misma lógica que definimos antes)
            var stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s =>
                    s.MaterialId == request.MaterialId &&
                    s.WarehouseId == request.WarehouseId &&
                    s.StorageBinId == request.StorageBinId &&
                    s.LotId == request.LotId &&
                    s.StatusId == request.StatusId,
                    cancellationToken);

            // 2. ACTUALIZAR O CREAR EN STOCK ITEMS
            if (stockItem != null)
            {
                stockItem.QuantityOnHand += request.Quantity;
                // QuantityAvailable se calcula solo en tu dominio 🚀
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
                    QuantityOnHand = request.Quantity,
                    QuantityReserved = 0
                };
                _context.StockItems.Add(stockItem);
            }

            // 3. CREAR EL HISTORIAL (Compatible 100% con tu Entidad)
            var movement = new StockMovement
            {
                MaterialId = request.MaterialId,
                WarehouseId = request.WarehouseId,
                StorageBinId = request.StorageBinId, // Ya agregado a tu entidad
                LotId = request.LotId,

                Type = MovementType.PurchaseReception, // Tu Enum
                Quantity = request.Quantity,           // Positivo

                MovementDate = DateTime.UtcNow,        // Tu nombre de propiedad
                ReferenceNumber = request.ReferenceNumber ?? string.Empty,
                Comments = request.Comments ?? string.Empty,
                UserId = request.UserId                // Tu auditoría
            };

            _context.StockMovements.Add(movement);

            // 4. GUARDAR TRANSACCIÓN
            await _context.SaveChangesAsync(cancellationToken);

            return movement.Id;
        }
    }
}
