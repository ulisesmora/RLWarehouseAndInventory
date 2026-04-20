using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.SalesOrders.Queries;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.SalesOrders.Handlers
{
    public class GetSalesOrdersWithPaginationHandler
        : IRequestHandler<GetSalesOrdersWithPaginationQuery, PaginatedList<SalesOrderDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetSalesOrdersWithPaginationHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<PaginatedList<SalesOrderDto>> Handle(
            GetSalesOrdersWithPaginationQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.SalesOrders.AsNoTracking();

            // Filtro por texto libre (número de orden, cliente, referencia externa)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                query = query.Where(x =>
                    x.OrderNumber.Contains(term) ||
                    x.CustomerName.Contains(term) ||
                    (x.ExternalReference != null && x.ExternalReference.Contains(term)));
            }

            // Filtro por estado
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<SalesOrderStatus>(request.Status, ignoreCase: true, out var statusEnum))
            {
                query = query.Where(x => x.Status == statusEnum);
            }

            // Filtro por canal
            if (!string.IsNullOrWhiteSpace(request.Channel) &&
                Enum.TryParse<SalesChannel>(request.Channel, ignoreCase: true, out var channelEnum))
            {
                query = query.Where(x => x.SourceChannel == channelEnum);
            }

            // Más recientes primero
            query = query.OrderByDescending(x => x.CreatedAt);

            return await PaginatedList<SalesOrderDto>.CreateAsync(
                query.ProjectTo<SalesOrderDto>(_mapper.ConfigurationProvider),
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
