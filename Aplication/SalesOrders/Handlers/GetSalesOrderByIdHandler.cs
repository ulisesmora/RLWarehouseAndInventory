using AutoMapper;
using Inventory.Application.SalesOrders.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.SalesOrders.Handlers
{
    public class GetSalesOrderByIdHandler : IRequestHandler<GetSalesOrderByIdQuery, SalesOrderDto>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetSalesOrderByIdHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<SalesOrderDto> Handle(GetSalesOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _context.SalesOrders
                .Include(o => o.Lines)
                    .ThenInclude(l => l.Material)
                .Include(o => o.PickTasks)
                    .ThenInclude(t => t.Material)
                .Include(o => o.PickTasks)
                    .ThenInclude(t => t.SourceStockItem)
                        .ThenInclude(s => s.Lot)
                .Include(o => o.PickTasks)
                    .ThenInclude(t => t.SourceStockItem)
                        .ThenInclude(s => s.StorageBin)
                            .ThenInclude(b => b.Zone)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException($"Pedido de venta {request.Id} no encontrado.");

            return _mapper.Map<SalesOrderDto>(order);
        }
    }
}
