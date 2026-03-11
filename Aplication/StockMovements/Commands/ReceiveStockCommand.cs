using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commands
{
    public record ReceiveStockCommand(
        Guid MaterialId,
        decimal Quantity,
        Guid WarehouseId,
        Guid StatusId,
        Guid? StorageBinId,
        Guid? LotId,

        // --- Auditoría (Nombres alineados a tu BD) ---
        string? ReferenceNumber, // Ej: "PO-2024-001"
        string? Comments,        // Notas del bodeguero
        Guid UserId              // Quién está recibiendo esto
    ) : IRequest<Guid>;
}
