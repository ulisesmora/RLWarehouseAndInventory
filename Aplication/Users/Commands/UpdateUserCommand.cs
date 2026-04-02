using Inventory.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Commands
{
    public record UpdateUserCommand(
     Guid Id,
     string FullName,
     SystemRole Role,
     Guid? RestrictedWarehouseId,
     bool IsActive
 ) : IRequest<Unit>;
}
