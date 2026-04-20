using Inventory.Domain;
using MediatR;
using System;
using System.Collections.Generic;

namespace Inventory.Application.SalesOrders.Commands
{
    public record CreateSalesOrderCommand(
        string              CustomerName,
        string?             CustomerEmail,
        string?             ShippingAddress,
        SalesChannel        SourceChannel,
        DateTime?           ShipByDate,
        string?             Notes,
        string?             ExternalReference,
        List<SalesOrderLineInput> Lines
    ) : IRequest<Guid>;

    /// <summary>Un ítem del pedido (material + cantidad + precio)</summary>
    public record SalesOrderLineInput(
        Guid    MaterialId,
        decimal OrderedQuantity,
        decimal UnitPrice
    );
}
