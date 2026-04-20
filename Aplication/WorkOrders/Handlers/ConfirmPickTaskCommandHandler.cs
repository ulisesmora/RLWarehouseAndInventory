using Inventory.Application.WorkOrders.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.WorkOrders.Handlers
{
    public class ConfirmPickTaskCommandHandler : IRequestHandler<ConfirmPickTaskCommand, ConfirmPickTaskResult>
    {
        private readonly InventoryDbContext _context;

        public ConfirmPickTaskCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<ConfirmPickTaskResult> Handle(ConfirmPickTaskCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"[PICK] ConfirmPickTask: WO={request.WorkOrderId} Task={request.TaskId}");
            Console.WriteLine($"[PICK] ScannedLpn={request.ScannedLpn} | Qty={request.PickedQuantity}");

            // 1. Cargar la tarea con todo lo necesario para validar
            var task = await _context.ProductionPickTask
                .Include(t => t.SourceStockItem)
                    .ThenInclude(s => s.Lot)
                .Include(t => t.WorkOrder)
                    .ThenInclude(w => w.PickTasks)
                .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.WorkOrderId == request.WorkOrderId, cancellationToken);

            if (task == null)
                throw new KeyNotFoundException($"Tarea {request.TaskId} no encontrada en la orden {request.WorkOrderId}.");

            if (task.Status == PickTaskStatus.Completed)
                throw new InvalidOperationException("Esta tarea ya fue confirmada previamente.");

            // 2. Validar que el LPN escaneado coincide con el stock item asignado
            var stockItem = task.SourceStockItem;
            if (!string.Equals(stockItem.ReferenceNumber, request.ScannedLpn, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[PICK] LPN MISMATCH: esperado={stockItem.ReferenceNumber} | escaneado={request.ScannedLpn}");
                throw new InvalidOperationException(
                    $"LPN incorrecto. El sistema espera '{stockItem.ReferenceNumber}' pero escaneaste '{request.ScannedLpn}'. " +
                    $"Verifica que estás tomando el contenedor correcto.");
            }

            // 3. Validación FIFO: verificar que no existe un lote más antiguo del mismo material
            //    disponible en stock que debería haberse tomado antes.
            if (stockItem.LotId.HasValue && stockItem.Lot != null)
            {
                var olderLotExists = await _context.StockItems
                    .Include(s => s.Lot)
                    .Where(s => s.MaterialId == task.MaterialId
                             && s.Id != stockItem.Id
                             && s.LotId.HasValue
                             && (s.QuantityOnHand - s.QuantityReserved - s.AllocatedQuantity) > 0
                             && s.Lot != null
                             && s.Lot.ExpirationDate < stockItem.Lot.ExpirationDate)
                    .AnyAsync(cancellationToken);

                if (olderLotExists)
                {
                    Console.WriteLine($"[PICK] FIFO VIOLATION detectada para MaterialId={task.MaterialId}");
                    // No bloqueamos — solo advertencia en el log, el WMS ya hizo FIFO en la creación.
                    // Si quisieras bloquear, cambiarías esto a throw.
                }
            }

            // 4. Confirmar la tarea
            task.Status = PickTaskStatus.Completed;
            task.PickedQuantity = request.PickedQuantity;

            Console.WriteLine($"[PICK] Tarea marcada como Completed. PickedQty={request.PickedQuantity}");

            // 5. Verificar si todas las tareas de la orden están completadas
            var workOrder = task.WorkOrder;
            var allTasks = workOrder.PickTasks.ToList();
            var pendingTasks = allTasks
                .Where(t => t.Id != task.Id && t.Status != PickTaskStatus.Completed)
                .ToList();

            bool allDone = !pendingTasks.Any();
            Guid? nextTaskId = pendingTasks.FirstOrDefault()?.Id;

            if (allDone && workOrder.Status == WorkOrderStatus.Allocated)
            {
                workOrder.Status = WorkOrderStatus.InProgress;
                Console.WriteLine($"[PICK] Todas las tareas completadas. WO {workOrder.Id} → InProgress");
            }

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[PICK] Guardado OK. AllDone={allDone} | NextTask={nextTaskId}");
            Console.WriteLine("==================================================");

            return new ConfirmPickTaskResult(
                Success: true,
                Message: allDone
                    ? "Todas las tareas de picking completadas. La orden está lista para producción."
                    : $"Tarea confirmada. Quedan {pendingTasks.Count} tarea(s) pendiente(s).",
                WorkOrderInProgress: allDone,
                NextTaskId: nextTaskId
            );
        }
    }
}
