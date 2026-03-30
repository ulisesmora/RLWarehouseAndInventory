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
            // 1. BUSCAR EXACTAMENTE LA CAJA (LPN) DE DONDE VAMOS A SACAR LA MERCANCÍA
            var stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s => s.Id == request.StockItemId, cancellationToken);

            // 2. REGLA DE NEGOCIO: ¿Existe la caja?
            if (stockItem == null)
            {
                throw new InvalidOperationException("No se encontró el contenedor/LPN especificado para la salida.");
            }

            // 3. REGLA DE ORO: ¿Hay suficiente cantidad disponible en ESTA caja?
            // Usamos QuantityAvailable (Físico - Reservado) para no robarle mercancía a otro pedido.
            if (stockItem.QuantityAvailable < request.Quantity)
            {
                throw new InvalidOperationException($"Stock insuficiente en este contenedor. Solicitado: {request.Quantity}, Disponible: {stockItem.QuantityAvailable}.");
            }

            // 4. DESCONTAR EL INVENTARIO FÍSICO
            stockItem.QuantityOnHand -= request.Quantity;

            // 5. CREAR EL HISTORIAL DE SALIDA (El Ticket)
            var movement = new StockMovement
            {
                StockItemId = stockItem.Id,
                MaterialId = stockItem.MaterialId,
                WarehouseId = stockItem.WarehouseId,
                StorageBinId = stockItem.StorageBinId,
                LotId = stockItem.LotId,

                Type = request.Type, // Ej: SalesShipment o ManufacturingUse

                // REGLA WMS: "Negativo = Salida del almacén"
                Quantity = -request.Quantity,

                MovementDate = DateTime.UtcNow,
                ReferenceNumber = request.ReferenceNumber ?? string.Empty,
                Comments = string.IsNullOrWhiteSpace(request.Comments) ? "Salida de inventario" : request.Comments,
                UserId = request.UserId
            };

            _context.StockMovements.Add(movement);

            // 6. LIMPIEZA DE CAJAS VACÍAS (Destrucción del LPN)
            // Si el bodeguero sacó todo lo que quedaba en la caja, destruimos el registro físico 
            // para liberar el espacio (StorageBin) en el mapa 3D.
            if (stockItem.QuantityOnHand == 0 && stockItem.QuantityReserved == 0)
            {
                _context.StockItems.Remove(stockItem);
                movement.Comments += " (El contenedor quedó vacío y fue desechado/eliminado del sistema).";
            }

            // 7. GUARDAR TRANSACCIÓN (ACID)
            await _context.SaveChangesAsync(cancellationToken);

            return movement.Id;
        }
    }
}
