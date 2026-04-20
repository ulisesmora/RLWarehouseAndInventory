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
    public class CompleteWorkOrderCommandHandler : IRequestHandler<CompleteWorkOrderCommand, Guid>
    {
        private readonly InventoryDbContext _context;

        public CompleteWorkOrderCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"[COMPLETE] WorkOrderId={request.WorkOrderId}");

            // 1. Cargar la orden con todas sus tareas de picking
            var workOrder = await _context.WorkOrder
                .Include(w => w.ProductRecipe)
                .Include(w => w.PickTasks)
                    .ThenInclude(t => t.SourceStockItem)
                        .ThenInclude(s => s.Lot)
                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

            if (workOrder == null)
                throw new KeyNotFoundException($"Orden de trabajo {request.WorkOrderId} no encontrada.");

            if (workOrder.Status == WorkOrderStatus.Completed)
                throw new InvalidOperationException("La orden ya fue completada anteriormente.");

            if (workOrder.Status == WorkOrderStatus.Canceled)
                throw new InvalidOperationException("No se puede completar una orden cancelada.");

            // Permitir completar desde InProgress, QualityControl o Allocated (flujo flexible)
            var allowedStatuses = new[] { WorkOrderStatus.InProgress, WorkOrderStatus.QualityControl, WorkOrderStatus.Allocated };
            if (!Array.Exists(allowedStatuses, s => s == workOrder.Status))
                throw new InvalidOperationException(
                    $"No se puede completar una orden en estado '{workOrder.Status}'. Debe estar en InProgress o QualityControl.");

            // 2. Generar consumos automáticamente desde las PickTasks
            //    - Usamos PickedQuantity si fue confirmada por el operador (> 0),
            //      o RequiredQuantity como respaldo si aún estaba pendiente.
            foreach (var task in workOrder.PickTasks)
            {
                var consumed = task.PickedQuantity > 0 ? task.PickedQuantity : task.RequiredQuantity;
                var stockItem = task.SourceStockItem;

                Console.WriteLine($"[COMPLETE] Tarea {task.Id}: MaterialId={task.MaterialId} | Required={task.RequiredQuantity} | Consumed={consumed}");

                // A. Registro de trazabilidad
                var consumptionRecord = new WorkOrderConsumption
                {
                    Id                    = Guid.NewGuid(),
                    OrganizationId        = workOrder.OrganizationId,
                    WorkOrderId           = workOrder.Id,
                    MaterialId            = task.MaterialId,
                    LotId                 = stockItem?.LotId,
                    SourceStorageBinId    = stockItem?.StorageBinId,
                    PlannedQuantity       = task.RequiredQuantity,
                    ActualConsumedQuantity = consumed
                };
                _context.WorkOrderConsumption.Add(consumptionRecord);

                // B. Descontar inventario físico
                if (stockItem != null)
                {
                    stockItem.QuantityOnHand    -= consumed;
                    stockItem.AllocatedQuantity -= task.RequiredQuantity; // Liberar lo que se había reservado
                    if (stockItem.QuantityOnHand    < 0) stockItem.QuantityOnHand    = 0;
                    if (stockItem.AllocatedQuantity < 0) stockItem.AllocatedQuantity = 0;

                    Console.WriteLine($"[COMPLETE]   StockItem {stockItem.Id}: OnHand={stockItem.QuantityOnHand} | Allocated={stockItem.AllocatedQuantity}");
                }

                // Marcar tarea como completada si aún no lo estaba
                if (task.Status != PickTaskStatus.Completed)
                    task.Status = PickTaskStatus.Completed;
            }

            // 3. Crear el Lote del Producto Terminado
            //    El lote queda "en espera de acomodo" — el operador lo ubica
            //    usando el flujo de Putaway ya existente.
            var newLot = new Lot
            {
                Id                     = Guid.NewGuid(),
                OrganizationId         = workOrder.OrganizationId,
                MaterialId             = workOrder.ProductRecipe.FinishedGoodId,
                LotNumber              = request.LotNumber ?? $"PROD-{workOrder.OrderNumber}",
                VendorBatchNumber      = null,
                SupplierId             = null,
                InitialReceivedQuantity = request.ActualFinishedGoodQuantity,
                IsBlocked              = false,
                ManufacturingDate      = DateTime.UtcNow,
                ExpirationDate         = DateTime.UtcNow.AddYears(1)
            };

            _context.Lots.Add(newLot);

            Console.WriteLine($"[COMPLETE] Lote creado: {newLot.LotNumber} | Qty={newLot.InitialReceivedQuantity}");

            // 4. Cerrar la orden
            workOrder.Status               = WorkOrderStatus.Completed;
            workOrder.ProducedQuantity     = request.ActualFinishedGoodQuantity;
            workOrder.ActualCompletionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[COMPLETE] Orden cerrada. LoteId={newLot.Id}");
            Console.WriteLine("==================================================");

            return newLot.Id;
        }
    }
}
