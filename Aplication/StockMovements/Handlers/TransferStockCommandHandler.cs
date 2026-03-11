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
    public class TransferStockCommandHandler : IRequestHandler<TransferStockCommand>
    {
        private readonly InventoryDbContext _context;

        public TransferStockCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(TransferStockCommand request, CancellationToken cancellationToken)
        {
            // 1. OBTENER Y VALIDAR ORIGEN
            var sourceItem = await _context.StockItems
                .FirstOrDefaultAsync(s =>
                    s.MaterialId == request.MaterialId &&
                    s.WarehouseId == request.SourceWarehouseId &&
                    s.StorageBinId == request.SourceStorageBinId &&
                    s.LotId == request.LotId &&
                    s.StatusId == request.StatusId,
                    cancellationToken);

            if (sourceItem == null || sourceItem.QuantityAvailable < request.Quantity)
            {
                throw new InvalidOperationException("Stock insuficiente en la ubicación de origen para realizar la transferencia.");
            }

            // 2. OBTENER O CREAR DESTINO
            var destItem = await _context.StockItems
                .FirstOrDefaultAsync(s =>
                    s.MaterialId == request.MaterialId &&
                    s.WarehouseId == request.DestinationWarehouseId &&
                    s.StorageBinId == request.DestinationStorageBinId &&
                    s.LotId == request.LotId &&
                    s.StatusId == request.StatusId,
                    cancellationToken);

            if (destItem != null)
            {
                destItem.QuantityOnHand += request.Quantity;
            }
            else
            {
                destItem = new StockItem
                {
                    MaterialId = request.MaterialId,
                    WarehouseId = request.DestinationWarehouseId,
                    StorageBinId = request.DestinationStorageBinId,
                    LotId = request.LotId,
                    StatusId = request.StatusId,
                    QuantityOnHand = request.Quantity,
                    QuantityReserved = 0
                };
                _context.StockItems.Add(destItem);
            }

            // 3. DESCONTAR DEL ORIGEN
            sourceItem.QuantityOnHand -= request.Quantity;
            if (sourceItem.QuantityOnHand == 0 && sourceItem.QuantityReserved == 0)
            {
                _context.StockItems.Remove(sourceItem); // Limpiamos registros vacíos
            }

            // 4. CREAR HISTORIAL (2 Movimientos)
            var dateNow = DateTime.UtcNow;

            // Movimiento de Salida (Negativo)
            var outMovement = new StockMovement
            {
                MaterialId = request.MaterialId,
                WarehouseId = request.SourceWarehouseId,
                StorageBinId = request.SourceStorageBinId,
                LotId = request.LotId,
                Type = MovementType.Transfer,
                Quantity = -request.Quantity, // NEGATIVO
                MovementDate = dateNow,
                ReferenceNumber = request.ReferenceNumber ?? string.Empty,
                Comments = request.Comments ?? "Salida por traslado",
                UserId = request.UserId
            };

            // Movimiento de Entrada (Positivo)
            var inMovement = new StockMovement
            {
                MaterialId = request.MaterialId,
                WarehouseId = request.DestinationWarehouseId,
                StorageBinId = request.DestinationStorageBinId,
                LotId = request.LotId,
                Type = MovementType.Transfer,
                Quantity = request.Quantity, // POSITIVO
                MovementDate = dateNow,
                ReferenceNumber = request.ReferenceNumber ?? string.Empty,
                Comments = request.Comments ?? "Entrada por traslado",
                UserId = request.UserId
            };

            _context.StockMovements.Add(outMovement);
            _context.StockMovements.Add(inMovement);

            // 5. GUARDAR TRANSACCIÓN
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
