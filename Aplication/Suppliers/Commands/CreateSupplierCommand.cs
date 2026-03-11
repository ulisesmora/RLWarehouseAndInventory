using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Suppliers.Commands
{
    public record CreateSupplierCommand(
         string Name,
         string? TaxId,
         string? ContactName,
         string? Email,
         string? PhoneNumber,
         string? Address
     ) : IRequest<Guid>;
}
