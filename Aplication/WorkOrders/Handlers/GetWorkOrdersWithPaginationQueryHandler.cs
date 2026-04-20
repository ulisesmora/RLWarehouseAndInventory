using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.WorkOrders.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Handlers
{
    public class GetWorkOrdersWithPaginationQueryHandler : IRequestHandler<GetWorkOrdersWithPaginationQuery, PaginatedList<WorkOrderDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetWorkOrdersWithPaginationQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<WorkOrderDto>> Handle(GetWorkOrdersWithPaginationQuery request, CancellationToken cancellationToken)
        {
            // 1. Preparamos la consulta
            var query = _context.WorkOrder
                .AsNoTracking();

            // 2. Filtros dinámicos
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                // Buscamos por número de orden o por notas
                query = query.Where(x => x.OrderNumber.Contains(term) ||
                                         (x.Notes != null && x.Notes.Contains(term)));
            }

            // 3. Ordenamiento (Para las órdenes de trabajo suele ser mejor ver las más nuevas primero)
            query = query.OrderByDescending(x => x.CreatedAt);

            // 4. Proyección y Ejecución Final (AutoMapper hace los JOINs automáticos con Recipe y FinishedGood)
            return await PaginatedList<WorkOrderDto>.CreateAsync(
                query.ProjectTo<WorkOrderDto>(_mapper.ConfigurationProvider),
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
