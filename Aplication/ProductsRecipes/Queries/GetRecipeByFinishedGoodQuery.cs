using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductsRecipes.Queries
{
    public record GetRecipeByFinishedGoodQuery(Guid FinishedGoodId) : IRequest<ProductRecipeDto>;
}
