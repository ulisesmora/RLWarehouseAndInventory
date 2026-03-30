using Inventory.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commands
{
    public record ReceiveStockBatchItem(
        Guid MaterialId,
        decimal Quantity,
        Guid WarehouseId,
        Guid StatusId,
        Guid? StorageBinId,
        Guid? LotId,

        decimal? LengthCm,
        decimal? WidthCm,
        decimal? HeightCm,
        decimal? WeightKg,
        bool IsStackable,
        ContainerType containerType,

        // --- Auditoría (Nombres alineados a tu BD) ---
        string? ReferenceNumber, // Ej: "PO-2024-001"
        string? Comments        // Notas del bodeguero
  
    ) : IRequest<Guid>;

    public record ReceiveStockCommand(
        List<ReceiveStockBatchItem> Items,
        Guid? UserId // El usuario es el mismo para toda la operación
    ) : IRequest<bool>;
}
