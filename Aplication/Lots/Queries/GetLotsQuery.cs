using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.Materials.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Queries
{

   
       public record GetLotsQuery : IRequest<PaginatedList<LotDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;

        // Opcional: Filtros de búsqueda
        public string? SearchTerm { get; init; }

        public Guid? MaterialId { get; init; }
        public Guid? SupplierId { get; init; }
        public bool? IsBlocked { get; init; }
        public bool? OnlyAvailable { get; init; }
    }
}
