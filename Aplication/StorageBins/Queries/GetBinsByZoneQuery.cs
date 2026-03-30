using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Queries
{
    public record GetBinsByZoneQuery(Guid ZoneId, bool? IncludeItems) : IRequest<List<StorageBinDto>>;
}
