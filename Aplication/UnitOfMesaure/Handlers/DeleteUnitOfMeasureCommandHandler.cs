using Inventory.Application.UnitOfMesaure.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Handlers
{
    public class DeleteUnitOfMeasureCommandHandler : IRequestHandler<DeleteUnitOfMeasureCommand>
    {
        private readonly InventoryDbContext _context;

        public DeleteUnitOfMeasureCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteUnitOfMeasureCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.UnitOfMeasures.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new Exception($"La Unidad de Medida con ID {request.Id} no fue encontrada.");
            }

            _context.UnitOfMeasures.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
