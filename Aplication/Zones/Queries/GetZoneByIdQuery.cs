using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Queries
{
    public record GetZoneByIdQuery(Guid Id) : IRequest<ZoneDetailDto>;
}
