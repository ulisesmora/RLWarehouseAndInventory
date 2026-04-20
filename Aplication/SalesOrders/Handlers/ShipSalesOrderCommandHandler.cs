using Inventory.Application.SalesOrders.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.SalesOrders.Handlers
{
    public class ShipSalesOrderCommandHandler : IRequestHandler<ShipSalesOrderCommand, Unit>
    {
        private readonly InventoryDbContext _context;

        public ShipSalesOrderCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ShipSalesOrderCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"[SHIP] SalesOrderId={request.SalesOrderId}");
            Console.WriteLine($"[SHIP] Carrier={request.CarrierName} | Tracking={request.TrackingNumber}");

            // 1. Cargar el pedido con tareas y stock
            var order = await _context.SalesOrders
                .Include(o => o.PickTasks)
                    .ThenInclude(t => t.SourceStockItem)
                        .ThenInclude(s => s.StorageBin)
                .FirstOrDefaultAsync(o => o.Id == request.SalesOrderId, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException($"Pedido de venta {request.SalesOrderId} no encontrado.");

            var allowedStatuses = new[] { SalesOrderStatus.ReadyToShip, SalesOrderStatus.Picking };
            if (!Array.Exists(allowedStatuses, s => s == order.Status))
                throw new InvalidOperationException(
                    $"Solo se puede embarcar un pedido en estado 'ReadyToShip' o 'Picking'. Estado actual: '{order.Status}'.");

            // 2. Para cada tarea: crear movimiento, descontar inventario y liberar reserva
            foreach (var task in order.PickTasks)
            {
                var stockItem = task.SourceStockItem;
                if (stockItem == null) continue;

                var consumed = task.PickedQuantity > 0 ? task.PickedQuantity : task.RequiredQuantity;

                Console.WriteLine($"[SHIP] Tarea {task.Id}: StockItem={stockItem.Id} | Consumed={consumed}");

                // A. Movimiento de inventario tipo SalesShipment
                var shipNotes = $"Embarque SO {order.OrderNumber}";
                if (!string.IsNullOrWhiteSpace(request.TrackingNumber))
                    shipNotes += $" | Tracking: {request.TrackingNumber}";
                if (!string.IsNullOrWhiteSpace(request.CarrierName))
                    shipNotes += $" | Carrier: {request.CarrierName}";

                var movement = new StockMovement
                {
                    Id              = Guid.NewGuid(),
                    OrganizationId  = order.OrganizationId,
                    MovementDate    = DateTime.UtcNow,
                    Type            = MovementType.SalesShipment,
                    MaterialId      = task.MaterialId,
                    StockItemId     = stockItem.Id,
                    StorageBinId    = stockItem.StorageBinId,
                    WarehouseId     = stockItem.StorageBin != null
                                        ? stockItem.StorageBin.WarehouseId
                                        : Guid.Empty,           // fallback — idealmente nunca Empty
                    Quantity        = -consumed,
                    ReferenceNumber = order.OrderNumber,
                    Comments        = shipNotes
                };
                _context.StockMovements.Add(movement);

                // B. Descontar físico y liberar reserva
                stockItem.QuantityOnHand    -= consumed;
                stockItem.AllocatedQuantity -= task.RequiredQuantity;

                if (stockItem.QuantityOnHand    < 0) stockItem.QuantityOnHand    = 0;
                if (stockItem.AllocatedQuantity < 0) stockItem.AllocatedQuantity = 0;

                Console.WriteLine($"[SHIP]   StockItem actualizado: OnHand={stockItem.QuantityOnHand} | Allocated={stockItem.AllocatedQuantity}");
            }

            // 3. Guardar datos de envío en Notes y cerrar el pedido
            if (!string.IsNullOrWhiteSpace(request.TrackingNumber) || !string.IsNullOrWhiteSpace(request.CarrierName))
            {
                var shipInfo = $"[Embarque] Carrier: {request.CarrierName ?? "N/A"} | Tracking: {request.TrackingNumber ?? "N/A"}";
                order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                    ? shipInfo
                    : order.Notes + "\n" + shipInfo;
            }

            order.Status = SalesOrderStatus.Shipped;

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[SHIP] Pedido {order.OrderNumber} embarcado. Status=Shipped");
            Console.WriteLine("==================================================");

            return Unit.Value;
        }
    }
}
