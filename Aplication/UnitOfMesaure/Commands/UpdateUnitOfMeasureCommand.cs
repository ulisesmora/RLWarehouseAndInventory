using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Commands
{
    public record UpdateUnitOfMeasureCommand(
    Guid Id,
    string Name,
    string Abbreviation,
    bool IsBaseUnit,
    decimal ConversionFactor
) : IRequest<Unit>;
}
