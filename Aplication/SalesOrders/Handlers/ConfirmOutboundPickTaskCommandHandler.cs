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
    public class ConfirmOutboundPickTaskCommandHandler
        : IRequestHandler<ConfirmOutboundPickTaskCommand, ConfirmOutboundPickResult>
    {
        private readonly InventoryDbContext _context;

        public ConfirmOutboundPickTaskCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<ConfirmOutboundPickResult> Handle(
            ConfirmOutboundPickTaskCommand request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"[OPICK] ConfirmOutboundPickTask: SO={request.SalesOrderId} Task={request.TaskId}");
            Console.WriteLine($"[OPICK] ScannedLpn={request.ScannedLpn} | Qty={request.PickedQuantity}");

            // 1. Cargar la tarea con sus relaciones
            var task = await _context.OutboundPickTasks
                .Include(t => t.SourceStockItem)
                    .ThenInclude(s => s.Lot)
                .Include(t => t.SalesOrder)
                    .ThenInclude(o => o.PickTasks)
                .Include(t => t.SalesOrderLine)
                .FirstOrDefaultAsync(t =>
                    t.Id == request.TaskId && t.SalesOrderId == request.SalesOrderId,
                    cancellationToken);

            if (task == null)
                throw new KeyNotFoundException(
                    $"Tarea {request.TaskId} no encontrada en el pedido {request.SalesOrderId}.");

            if (task.Status == PickTaskStatus.Completed)
                throw new InvalidOperationException("Esta tarea ya fue confirmada anteriormente.");

            // 2. Validar LPN escaneado
            var stockItem = task.SourceStockItem;
            if (!string.Equals(stockItem.ReferenceNumber, request.ScannedLpn, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[OPICK] LPN MISMATCH: esperado={stockItem.ReferenceNumber} | escaneado={request.ScannedLpn}");
                throw new InvalidOperationException(
                    $"LPN incorrecto. Esperado: '{stockItem.ReferenceNumber}' | Escaneado: '{request.ScannedLpn}'. " +
                    $"Verifica que estás tomando el contenedor correcto.");
            }

            // 3. Confirmar la tarea
            task.Status        = PickTaskStatus.Completed;
            task.PickedQuantity = request.PickedQuantity;

            // 4. Actualizar cantidad recogida en la línea del pedido
            var line = task.SalesOrderLine;
            line.PickedQuantity += request.PickedQuantity;

            // ¿Línea completamente surtida?
            if (line.PickedQuantity >= line.OrderedQuantity)
                line.Status = SalesOrderLineStatus.Fulfilled;
            else if (line.PickedQuantity > 0)
                line.Status = SalesOrderLineStatus.PartiallyPicked;

            // 5. Revisar el estado global del pedido
            var salesOrder  = task.SalesOrder;
            var allTasks    = salesOrder.PickTasks.ToList();
            var pendingTasks = allTasks
                .Where(t => t.Id != task.Id && t.Status != PickTaskStatus.Completed)
                .ToList();

            bool allDone   = !pendingTasks.Any();
            Guid? nextTask = pendingTasks.FirstOrDefault()?.Id;

            if (allDone)
            {
                salesOrder.Status = SalesOrderStatus.ReadyToShip;
                Console.WriteLine($"[OPICK] Todas las tareas completadas. Pedido {salesOrder.Id} → ReadyToShip");
            }
            else if (salesOrder.Status == SalesOrderStatus.Confirmed)
            {
                salesOrder.Status = SalesOrderStatus.Picking;
            }

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[OPICK] Guardado OK. AllDone={allDone} | NextTask={nextTask}");
            Console.WriteLine("==================================================");

            return new ConfirmOutboundPickResult(
                Success:          true,
                Message:          allDone
                    ? "Todas las tareas completadas. El pedido está listo para embarcar."
                    : $"Tarea confirmada. Quedan {pendingTasks.Count} tarea(s) pendiente(s).",
                OrderReadyToShip: allDone,
                NextTaskId:       nextTask
            );
        }
    }
}
