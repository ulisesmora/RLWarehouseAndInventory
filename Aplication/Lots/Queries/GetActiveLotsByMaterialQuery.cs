using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Queries
{
    public record GetActiveLotsByMaterialQuery(Guid MaterialId) : IRequest<List<LotDto>>;
}
