using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StatusCatalogs.Commands
{
    public record UpdateStatusCatalogCommand(
         Guid Id,
         string Name,
         string? Description
     ) : IRequest;
}
