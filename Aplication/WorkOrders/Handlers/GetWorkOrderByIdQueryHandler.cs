using AutoMapper;
using Inventory.Application.WorkOrders.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Handlers
{
    public class GetWorkOrderByIdQueryHandler : IRequestHandler<GetWorkOrderByIdQuery, WorkOrderDto>
    {
        private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetWorkOrderByIdQueryHandler(InventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkOrderDto> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrder
            .Include(w => w.ProductRecipe)
            .Include(w => w.FinishedGood)
            .Include(w => w.PickTasks)
                .ThenInclude(p => p.Material)
            .Include(w => w.PickTasks)
                .ThenInclude(p => p.SourceStockItem)
                    .ThenInclude(s => s.Lot)           // Para LotNumber en las tarjetas de picking
            .Include(w => w.PickTasks)
                .ThenInclude(p => p.SourceStockItem)
                    .ThenInclude(s => s.StorageBin)
                        .ThenInclude(b => b.Zone)      // Vital para el Gemelo 3D y la ruta de picking
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (workOrder == null)
            throw new KeyNotFoundException($"No se encontró la orden de trabajo {request.Id}");

        return _mapper.Map<WorkOrderDto>(workOrder);
    }
}
}
