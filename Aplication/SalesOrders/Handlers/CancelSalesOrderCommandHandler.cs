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
    public class CancelSalesOrderCommandHandler : IRequestHandler<CancelSalesOrderCommand, Unit>
    {
        private readonly InventoryDbContext _context;

        public CancelSalesOrderCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CancelSalesOrderCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"[CANCEL-SO] SalesOrderId={request.SalesOrderId} | Reason={request.Reason}");

            // 1. Cargar el pedido con todas sus tareas de picking
            var order = await _context.SalesOrders
                .Include(o => o.PickTasks)
                    .ThenInclude(t => t.SourceStockItem)
                .FirstOrDefaultAsync(o => o.Id == request.SalesOrderId, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException($"Pedido de venta {request.SalesOrderId} no encontrado.");

            var nonCancellable = new[] { SalesOrderStatus.Shipped, SalesOrderStatus.Delivered, SalesOrderStatus.Cancelled };
            if (Array.Exists(nonCancellable, s => s == order.Status))
                throw new InvalidOperationException(
                    $"No se puede cancelar un pedido en estado '{order.Status}'.");

            // 2. Liberar AllocatedQuantity en todos los StockItems reservados
            foreach (var task in order.PickTasks.Where(t => t.Status != PickTaskStatus.Completed))
            {
                var stockItem = task.SourceStockItem;
                if (stockItem == null) continue;

                Console.WriteLine($"[CANCEL-SO] Liberando StockItem={stockItem.Id} | Allocated-={task.RequiredQuantity}");

                stockItem.AllocatedQuantity -= task.RequiredQuantity;
                if (stockItem.AllocatedQuantity < 0) stockItem.AllocatedQuantity = 0;

                task.Status = PickTaskStatus.Cancelled;
            }

            // 3. Cancelar el pedido
            order.Status = SalesOrderStatus.Cancelled;

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                var cancelNote = $"[Cancelación] {request.Reason}";
                order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                    ? cancelNote
                    : order.Notes + "\n" + cancelNote;
            }

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[CANCEL-SO] Pedido {order.OrderNumber} cancelado.");
            Console.WriteLine("==================================================");

            return Unit.Value;
        }
    }
}
