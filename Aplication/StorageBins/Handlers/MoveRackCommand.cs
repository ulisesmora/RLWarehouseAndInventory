using Inventory.Application.StorageBins.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Handlers
{
    public class MoveRackCommandHandler : IRequestHandler<MoveRackCommand, bool>
    {
        private readonly InventoryDbContext _context; // Cambia esto por el nombre de tu DbContext

        public MoveRackCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(MoveRackCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscamos TODOS los cajones que pertenezcan a esa Zona y a ese Mueble (Prefijo)
            var binsToUpdate = await _context.StorageBins
                .Where(b => b.ZoneId == request.ZoneId && b.Code.StartsWith(request.RackPrefix))
                .ToListAsync(cancellationToken);

            if (!binsToUpdate.Any())
                return false; // No se encontró el Rack

            // 2. Actualizamos las coordenadas físicas de todos los huecos de ese mueble
            foreach (var bin in binsToUpdate)
            {
                bin.physicalOffsetX = request.PhysicalOffsetX;
                bin.physicalOffsetZ = request.PhysicalOffsetZ;
                bin.rotation = request.Rotation;
            }

            // 3. Guardamos los cambios en la BD de un solo golpe
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
