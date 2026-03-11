using Inventory.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commands
{
    public record IssueStockCommand(
         Guid MaterialId,
         decimal Quantity,
         Guid WarehouseId,
         Guid StatusId,       // De qué estado lo estamos sacando (ej. "Disponible")
         Guid? StorageBinId,
         Guid? LotId,

         MovementType Type,   // ¿Es por venta (SalesShipment) o para fabricar (ManufacturingUse)?

         // --- Auditoría ---
         string? ReferenceNumber, // Ej: "FACTURA-2026-999"
         string? Comments,
         Guid UserId
     ) : IRequest<Guid>;
}
