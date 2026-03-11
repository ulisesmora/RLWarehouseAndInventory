using Inventory.Application.Materials.Commons.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Queries
{
    public record GetMaterialsWithPaginationQuery : IRequest<PaginatedList<MaterialDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;

        // Opcional: Filtros de búsqueda
        public string? SearchTerm { get; init; }
    }
}
