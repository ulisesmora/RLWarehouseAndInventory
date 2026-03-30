using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.StockMovements.Commands;
using Inventory.Application.StockMovements.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Handlers
{
    public class GetStockItemsHandler : IRequestHandler<GetStockItemsQuery, PaginatedList<StockItemDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetStockItemsHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<StockItemDto>> Handle(GetStockItemsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.StockItems.AsNoTracking();

            // 1. Filtro global (Buscar por código de barras / LPN)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(x => x.ReferenceNumber.ToLower().Contains(term));
            }

            // 2. Filtros exactos
            if (request.WarehouseId.HasValue)
                query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);

            if (request.MaterialId.HasValue)
                query = query.Where(x => x.MaterialId == request.MaterialId.Value);

            if (request.LotId.HasValue)
                query = query.Where(x => x.LotId == request.LotId.Value);

            if (request.StorageBinId.HasValue)
                query = query.Where(x => x.StorageBinId == request.StorageBinId.Value);

            // 3. 🔥 Filtro para Putaway (Cajas sin ubicación física)
            if (request.IsPendingPutaway.HasValue)
            {
                if (request.IsPendingPutaway.Value)
                    query = query.Where(x => x.StorageBinId == null);
                else
                    query = query.Where(x => x.StorageBinId != null);
            }

            // 4. Ordenar y Paginar
            query = query.OrderBy(x => x.Lot != null && x.Lot.ExpirationDate.HasValue
                            ? x.Lot.ExpirationDate.Value
                            : DateTime.MaxValue)
             .ThenBy(x => x.CreatedAt);

            return await PaginatedList<StockItemDto>.CreateAsync(
                query.ProjectTo<StockItemDto>(_mapper.ConfigurationProvider),
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
