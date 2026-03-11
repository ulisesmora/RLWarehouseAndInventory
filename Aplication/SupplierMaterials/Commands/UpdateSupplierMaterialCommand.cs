using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Commands
{
    public record UpdateSupplierMaterialCommand(
        Guid Id,
        string VendorSKU,
        string VendorMaterialName,
        decimal UnitCost,
        string Currency,
        decimal MinimumOrderQuantity,
        int LeadTimeDays,
        bool IsPreferred
    ) : IRequest;
}
