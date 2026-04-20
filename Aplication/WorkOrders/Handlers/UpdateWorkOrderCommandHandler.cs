using Inventory.Application.WorkOrders.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Handlers
{
    public class UpdateWorkOrderCommandHandler : IRequestHandler<UpdateWorkOrderCommand, Unit>
    {
        private readonly InventoryDbContext _context;

        public UpdateWorkOrderCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateWorkOrderCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.WorkOrder
                .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException($"No se encontró la orden {request.Id}");

            // Actualizamos metadatos permitidos
            entity.PlannedStartDate = request.PlannedStartDate;
            entity.Notes = request.Notes;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
