using Inventory.Application.WorkOrders.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.WorkOrders.Handlers;

public class DeleteWorkOrderCommandHandler : IRequestHandler<DeleteWorkOrderCommand, Unit>
{
    private readonly InventoryDbContext _context;

    public DeleteWorkOrderCommandHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrder
            .Include(w => w.PickTasks)
                .ThenInclude(p => p.SourceStockItem)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (workOrder == null)
            throw new Exception($"No se encontró la orden de trabajo con ID {request.Id}");

        // Solo podemos cancelar si no se ha completado
        if (workOrder.Status == WorkOrderStatus.Completed)
            throw new Exception("No se puede eliminar una orden de trabajo que ya fue completada.");

        // Liberar el inventario reservado (Rollback del Allocation)
        foreach (var task in workOrder.PickTasks)
        {
            if (task.SourceStockItem != null)
            {
                // Devolvemos la cantidad que esta orden tenía apartada
                task.SourceStockItem.AllocatedQuantity -= task.RequiredQuantity;

                // Evitamos que quede en negativo por seguridad
                if (task.SourceStockItem.AllocatedQuantity < 0)
                    task.SourceStockItem.AllocatedQuantity = 0;
            }
        }

        workOrder.Status = WorkOrderStatus.Canceled;

        // Ejecutamos el Soft Delete heredado
        _context.WorkOrder.Remove(workOrder);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}