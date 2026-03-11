using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Queries
{
    public record GetUnitOfMeasureByIdQuery(Guid Id) : IRequest<UnitOfMeasureDto>;
}
