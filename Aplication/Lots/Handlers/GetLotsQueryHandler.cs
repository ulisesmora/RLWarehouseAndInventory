using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Lots.Queries;
using Inventory.Application.Materials.Commons.Models;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Handlers
{
    public class GetLotsHandler : IRequestHandler<GetLotsQuery, PaginatedList<LotDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetLotsHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<LotDto>> Handle(GetLotsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Lots
                .AsNoTracking(); // Velocidad pura

            // 1. Filtro Global de Búsqueda (SearchTerm)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                // Buscamos en el Lote, o navegamos al Material y Proveedor
                query = query.Where(x =>
                    x.LotNumber.ToLower().Contains(term) ||
                    (x.Material != null && x.Material.Name.ToLower().Contains(term)) ||
                    (x.Material != null && x.Material.SKU.ToLower().Contains(term)) ||
                    (x.Supplier != null && x.Supplier.Name.ToLower().Contains(term))
                );
            }

            // 2. Filtros Específicos
            if (request.MaterialId.HasValue)
                query = query.Where(x => x.MaterialId == request.MaterialId.Value);

            if (request.SupplierId.HasValue)
                query = query.Where(x => x.SupplierId == request.SupplierId.Value);

            if (request.IsBlocked.HasValue)
                query = query.Where(x => x.IsBlocked == request.IsBlocked.Value);

            // 3. Ordenamiento (Cambiado a Descendente para ver los más nuevos primero)
            query = query.OrderByDescending(x => x.CreatedAt);

            // 4. Proyección y Ejecución Final
            // ProjectTo hace que EF Core solo haga un SELECT de las columnas necesarias
            // en lugar de traerse toda la fila y luego mapearla.
            return await PaginatedList<LotDto>.CreateAsync(
                query.ProjectTo<LotDto>(_mapper.ConfigurationProvider),
                request.PageNumber,
                request.PageSize
            );
        }
    }
}

