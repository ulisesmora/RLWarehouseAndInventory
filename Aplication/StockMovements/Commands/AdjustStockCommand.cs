using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commands
{
    public record AdjustStockCommand(
        Guid StockItemId,
        decimal PhysicalCount, // Lo que realmente contó el operador
        string Reason, // Obligatorio para ajustes (Ej: "Mercancía dañada", "Error de conteo anterior")
        Guid UserId
    ) : IRequest;
}
