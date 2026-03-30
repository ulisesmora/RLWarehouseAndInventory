using Inventory.Application.StockMovements.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commands
{
    public record GetStockItemHistoryQuery(Guid StockItemId) : IRequest<List<StockMovementDto>>;
}
