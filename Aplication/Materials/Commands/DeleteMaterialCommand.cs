using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Commands
{
    public record DeleteMaterialCommand(Guid Id) : IRequest<Guid>;
}
