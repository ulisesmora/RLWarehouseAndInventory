using Inventory.Application.Materials.Commons.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Queries
{
    public class GetProductRecipesWithPaginationQuery : IRequest<PaginatedList<ProductRecipeDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }

    public class GetProductRecipeByIdQuery : IRequest<ProductRecipeDto>
    {
        public Guid Id { get; set; }

        public GetProductRecipeByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
