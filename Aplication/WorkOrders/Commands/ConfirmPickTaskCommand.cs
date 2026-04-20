using MediatR;
using System;

namespace Inventory.Application.WorkOrders.Commands
{
    public record ConfirmPickTaskCommand(
        Guid WorkOrderId,
        Guid TaskId,
        string ScannedLpn,      // LPN escaneado por el operador (debe coincidir con SourceStockItem.ReferenceNumber)
        decimal PickedQuantity  // Cantidad que el operador realmente tomó
    ) : IRequest<ConfirmPickTaskResult>;

    public record ConfirmPickTaskResult(
        bool Success,
        string Message,
        bool WorkOrderInProgress,   // true si todas las tareas de la orden ya se confirmaron
        Guid? NextTaskId            // ID de la siguiente tarea pendiente (null si no hay más)
    );
}
