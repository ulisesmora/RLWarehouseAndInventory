using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Commands
{
    public record UpdateZoneCommand(
        Guid Id,
        string Name,
        string? Description,
        double Width,
        double Depth,
        double Height
    ) : IRequest;
}
