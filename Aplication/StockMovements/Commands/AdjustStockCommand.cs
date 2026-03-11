using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commands
{
    public record AdjustStockCommand(
        Guid MaterialId,
        decimal PhysicalCount, // Lo que realmente contó el operador

        Guid WarehouseId,
        Guid? StorageBinId,
        Guid StatusId,
        Guid? LotId,

        string Reason, // Obligatorio para ajustes (Ej: "Mercancía dañada", "Error de conteo anterior")
        Guid UserId
    ) : IRequest;
}
