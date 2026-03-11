using Inventory.Application.Zones.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Handlers
{
    public class DeleteZoneCommandHandler : IRequestHandler<DeleteZoneCommand>
    {
        private readonly InventoryDbContext _context;

        public DeleteZoneCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteZoneCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Zones
                .Include(z => z.Bins) // Incluimos para validar
                .FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken);

            if (entity == null) throw new KeyNotFoundException($"Zona {request.Id} no encontrada.");

            // Validación: No borrar si tiene bins
            if (entity.Bins.Any())
            {
                throw new InvalidOperationException($"No se puede eliminar la zona '{entity.Name}' porque contiene {entity.Bins.Count} ubicaciones (Bins). Elimina las ubicaciones primero.");
            }

            _context.Zones.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
