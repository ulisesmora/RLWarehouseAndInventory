using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StatusCatalogs.Queries
{
    public record GetStatusCatalogsQuery : IRequest<List<StatusCatalogDto>>;
}
