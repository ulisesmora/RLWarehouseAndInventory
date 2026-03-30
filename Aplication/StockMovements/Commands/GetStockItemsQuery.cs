using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.StockMovements.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commands
{
    public record GetStockItemsQuery : IRequest<PaginatedList<StockItemDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;

        public string? SearchTerm { get; init; } // Buscar por LPN (ReferenceNumber)

        // Filtros del WMS
        public Guid? WarehouseId { get; init; }
        public Guid? MaterialId { get; init; }
        public Guid? LotId { get; init; }
        public Guid? StorageBinId { get; init; }

        // 🔥 EL FILTRO DE ORO PARA TU PANTALLA DE PUTAWAY
        // Si es true, trae los que NO tienen StorageBin asignado
        public bool? IsPendingPutaway { get; init; }
    }
}
