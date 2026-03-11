using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Suppliers.Commands
{
    public record DeleteSupplierCommand(Guid Id) : IRequest;
}
