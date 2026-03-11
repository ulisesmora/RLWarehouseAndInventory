using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Queries
{
    public record GetUnitsOfMeasureQuery : IRequest<List<UnitOfMeasureDto>>;
}
