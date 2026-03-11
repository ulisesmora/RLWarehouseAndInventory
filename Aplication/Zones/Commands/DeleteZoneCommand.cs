using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Commands
{
    public record DeleteZoneCommand(Guid Id) : IRequest;
}
