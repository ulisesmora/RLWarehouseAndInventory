using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commands
{
    public record TransferStockCommand(
        Guid StockItemId,
         Guid MaterialId,
         decimal Quantity,

         // Origen
         Guid SourceWarehouseId,
         Guid? SourceStorageBinId,

         // Destino
         Guid DestinationWarehouseId,
         Guid? DestinationStorageBinId,

         // Detalles del producto
         Guid StatusId,
         Guid? LotId,

         // Auditoría
         string? ReferenceNumber, // Ej: "TRASLADO-001"
         string? Comments,
         Guid UserId
     ) : IRequest;
}
