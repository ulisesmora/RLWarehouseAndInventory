using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Commands
{
    public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Description,
    Guid? ParentCategoryId
) : IRequest<Unit>;
}
