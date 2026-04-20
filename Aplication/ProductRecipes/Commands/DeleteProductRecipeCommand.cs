using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Commands
{
    public class DeleteProductRecipeCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }

        public DeleteProductRecipeCommand(Guid id)
        {
            Id = id;
        }
    }
}
