using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Queries
{
    public record GetSuppliersByMaterialQuery(Guid MaterialId) : IRequest<List<SupplierMaterialDto>>;
}
