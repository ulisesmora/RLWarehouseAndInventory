using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Commands
{
    public record DeleteUnitOfMeasureCommand(Guid Id) : IRequest;
}
