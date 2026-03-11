using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StatusCatalogs.Commands
{
    public record CreateStatusCatalogCommand(
         string Name,
         string? Description
     ) : IRequest<Guid>;
}
