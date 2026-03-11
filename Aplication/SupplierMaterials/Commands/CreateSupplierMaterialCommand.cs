using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Commands
{
    public record CreateSupplierMaterialCommand(
         Guid SupplierId,
         Guid MaterialId,
         string VendorSKU,
         string VendorMaterialName,
         decimal UnitCost,
         string Currency,
         decimal MinimumOrderQuantity,
         int LeadTimeDays,
         bool IsPreferred
     ) : IRequest<Guid>;
}
