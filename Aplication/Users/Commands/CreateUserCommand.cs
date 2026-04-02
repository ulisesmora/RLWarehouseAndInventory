using Inventory.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Commands
{
    public record CreateUserCommand(
    string FullName,
    string Email,
    string Password, // Se encriptará en el handler
    SystemRole Role,
    Guid? RestrictedWarehouseId
) : IRequest<Guid>;
}
