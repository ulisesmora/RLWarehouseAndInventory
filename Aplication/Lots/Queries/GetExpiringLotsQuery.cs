using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Queries
{
    public record GetExpiringLotsQuery(int DaysThreshold = 30) : IRequest<List<LotDto>>;

}
