using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Queries
{
    public record GetMaterialByIdQuery(Guid Id) : IRequest<MaterialDto>;
}
