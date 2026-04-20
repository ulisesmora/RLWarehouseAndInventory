using MediatR;
using System;

namespace Inventory.Application.WorkOrders.Commands
{
    /// <summary>
    /// Transiciona una orden de InProgress → QualityControl.
    /// El supervisor la envía cuando la producción ha finalizado físicamente.
    /// </summary>
    public record SendToQualityControlCommand(Guid WorkOrderId) : IRequest<Unit>;
}
