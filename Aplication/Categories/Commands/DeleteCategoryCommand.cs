using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Commands
{
    public record DeleteCategoryCommand(Guid Id) : IRequest;
}
