using Inventory.Application.Materials.Commons.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Queries
{
    public record GetCategoriesWithPaginationQuery : IRequest<PaginatedList<CategoryDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? SearchTerm { get; init; }
    }
}
