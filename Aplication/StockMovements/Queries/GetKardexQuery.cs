using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Queries
{
    public record GetKardexQuery(
         Guid? MaterialId,        // Opcional
         Guid? WarehouseId,       // Opcional
         Guid? LotId,             // Opcional
         DateTime? StartDate,     // Opcional
         DateTime? EndDate        // Opcional
     ) : IRequest<List<StockMovementDto>>;
}
