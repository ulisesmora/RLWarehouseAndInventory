using Inventory.Application.WorkOrders.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.WorkOrders.Handlers
{
    public class SendToQualityControlCommandHandler : IRequestHandler<SendToQualityControlCommand, Unit>
    {
        private readonly InventoryDbContext _context;

        public SendToQualityControlCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(SendToQualityControlCommand request, CancellationToken cancellationToken)
        {
            var workOrder = await _context.WorkOrder
                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

            if (workOrder == null)
                throw new KeyNotFoundException($"Orden de trabajo {request.WorkOrderId} no encontrada.");

            if (workOrder.Status != WorkOrderStatus.InProgress)
                throw new InvalidOperationException(
                    $"Solo se puede enviar a Control de Calidad una orden en estado 'InProgress'. Estado actual: {workOrder.Status}.");

            workOrder.Status = WorkOrderStatus.QualityControl;

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[QC] Orden {workOrder.OrderNumber} enviada a Control de Calidad.");

            return Unit.Value;
        }
    }
}
