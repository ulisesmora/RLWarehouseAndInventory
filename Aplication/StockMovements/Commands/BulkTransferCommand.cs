using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commands
{
    public record BulkTransferCommand(
         List<TransferItem> ItemsToMove,
         Guid UserId
     ) : IRequest<BulkTransferResult>;

    public record TransferItem(
        Guid StockItemId,
        Guid DestinationStorageBinId
    );

    public record BulkTransferResult(
        int SuccessfulCount,
        List<string> FailedItemReferences
    );
}
