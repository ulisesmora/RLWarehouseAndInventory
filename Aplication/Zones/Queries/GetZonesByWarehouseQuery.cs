using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Queries
{
    public record GetZonesByWarehouseQuery(Guid WarehouseId) : IRequest<List<ZoneDetailDto>>;
}
