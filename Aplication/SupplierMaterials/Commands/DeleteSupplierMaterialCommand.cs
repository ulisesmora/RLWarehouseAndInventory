using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Commands
{
    public record DeleteSupplierMaterialCommand(Guid Id) : IRequest;
}
