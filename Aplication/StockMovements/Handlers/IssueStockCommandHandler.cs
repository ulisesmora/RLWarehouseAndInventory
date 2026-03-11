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
    public class IssueStockCommandHandler : IRequestHandler<IssueStockCommand, Guid>
    {
        private readonly InventoryDbContext _context;

        public IssueStockCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(IssueStockCommand request, CancellationToken cancellationToken)
        {
            // 1. BUSCAR EXACTAMENTE DE DÓNDE VAMOS A SACAR LA MERCANCÍA
            var stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s =>
                    s.MaterialId == request.MaterialId &&
                    s.WarehouseId == request.WarehouseId &&
                    s.StorageBinId == request.StorageBinId &&
                    s.LotId == request.LotId &&
                    s.StatusId == request.StatusId,
                    cancellationToken);

            // 2. REGLA DE NEGOCIO: ¿Existe el registro?
            if (stockItem == null)
            {
                throw new InvalidOperationException("No se encontró inventario con las características especificadas en esta ubicación.");
            }

            // 3. REGLA DE ORO: ¿Hay suficiente cantidad disponible?
            // Aquí usamos tu genial propiedad QuantityAvailable (Físico - Reservado)
            if (stockItem.QuantityAvailable < request.Quantity)
            {
                throw new InvalidOperationException($"Stock insuficiente. Solicitado: {request.Quantity}, Disponible: {stockItem.QuantityAvailable}.");
            }

            // 4. DESCONTAR EL INVENTARIO (Solo afectamos lo físico, lo disponible se calcula solo)
            stockItem.QuantityOnHand -= request.Quantity;

            // 5. CREAR EL HISTORIAL DE SALIDA
            var movement = new StockMovement
            {
                MaterialId = request.MaterialId,
                WarehouseId = request.WarehouseId,
                StorageBinId = request.StorageBinId,
                LotId = request.LotId,

                Type = request.Type, // Ej: SalesShipment

                // TU REGLA DE NEGOCIO: "Negativo = Salida"
                Quantity = -request.Quantity,

                MovementDate = DateTime.UtcNow,
                ReferenceNumber = request.ReferenceNumber ?? string.Empty,
                Comments = request.Comments ?? string.Empty,
                UserId = request.UserId
            };

            _context.StockMovements.Add(movement);

            // 6. LIMPIEZA OPICIONAL (Opcional pero recomendado)
            // Si el estante quedó en cero absoluto (físico y reservado), 
            // a veces es mejor borrar el registro de StockItem para no llenar la BD de ceros.
            if (stockItem.QuantityOnHand == 0 && stockItem.QuantityReserved == 0)
            {
                _context.StockItems.Remove(stockItem);
            }

            // 7. GUARDAR TRANSACCIÓN
            await _context.SaveChangesAsync(cancellationToken);

            return movement.Id;
        }
    }
}
