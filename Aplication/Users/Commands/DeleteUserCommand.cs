using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Commands
{
    public record DeleteUserCommand(Guid Id) : IRequest<Unit>;
}
