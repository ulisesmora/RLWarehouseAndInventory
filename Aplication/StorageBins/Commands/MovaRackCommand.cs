using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Commands
{
    public record MoveRackCommand(
         Guid ZoneId,
         string RackPrefix, // Ej: "RACK-8F2A1"
         double PhysicalOffsetX,
         double PhysicalOffsetZ,
         int Rotation 
    ) : IRequest<bool>;
}
