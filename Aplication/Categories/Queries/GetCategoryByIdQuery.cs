using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Queries
{
    public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;
}
