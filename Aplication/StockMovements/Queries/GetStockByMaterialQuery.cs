using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Queries
{
    public record GetStockByMaterialQuery(Guid MaterialId) : IRequest<List<StockItemDto>>;
}
