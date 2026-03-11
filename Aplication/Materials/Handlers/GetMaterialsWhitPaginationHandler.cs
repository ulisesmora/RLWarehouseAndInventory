using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.Materials.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Handlers
{
    public class GetMaterialsWithPaginationHandler : IRequestHandler<GetMaterialsWithPaginationQuery, PaginatedList<MaterialDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetMaterialsWithPaginationHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

		public async Task<PaginatedList<MaterialDto>> Handle(GetMaterialsWithPaginationQuery request, CancellationToken cancellationToken)
		{
			// 1. Preparamos la consulta (IQueryable), pero NO la ejecutamos todavía
			var query = _context.Materials
				.AsNoTracking(); // Velocidad para lectura

			// 2. Aplicamos filtros SI existen (seguimos construyendo el SQL dinámicamente)
			if (!string.IsNullOrWhiteSpace(request.SearchTerm))
			{
				// Convertimos a minúsculas para búsqueda insensible (opcional según config de BD)
				var term = request.SearchTerm.Trim();
				query = query.Where(x => x.Name.Contains(term) || x.SKU.Contains(term));
			}

			// 3. Ordenamiento (OBLIGATORIO para poder paginar)
			// Sin un orden, SQL no sabe cuáles son los "primeros 10"
			query = query.OrderBy(x => x.CreatedAt);

			// 4. Proyección y Ejecución Final
			// Aquí usamos ProjectTo. Automáticamente hace los Includes necesarios 
			// y solo trae las columnas que pide el DTO (SELECT Name, Sku...)
			return await PaginatedList<MaterialDto>.CreateAsync(
				query.ProjectTo<MaterialDto>(_mapper.ConfigurationProvider),
				request.PageNumber,
				request.PageSize
			);
		}
	}
}
