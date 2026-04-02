using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Auth.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<AuthResponses>;
}
