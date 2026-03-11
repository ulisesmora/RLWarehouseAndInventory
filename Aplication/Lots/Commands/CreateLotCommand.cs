using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Commands
{
    public record CreateLotCommand(
        Guid MaterialId,
        string LotNumber,
        string? VendorBatchNumber, 
        Guid? SupplierId,         
        decimal InitialReceivedQuantity, 
        bool IsBlocked,
        DateTime? ManufacturingDate,
        DateTime? ExpirationDate
    ) : IRequest<Guid>;
}
