using Inventory.Application.Integrations.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Handlers
{
    public class MapProductCommandHandler
        : IRequestHandler<MapProductCommand, Unit>
    {
        private readonly InventoryDbContext _context;

        public MapProductCommandHandler(InventoryDbContext context)
            => _context = context;

        public async Task<Unit> Handle(
            MapProductCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Cargar el mapping
            var mapping = await _context.ChannelProductMappings
                .FirstOrDefaultAsync(m => m.Id == request.MappingId, cancellationToken);

            if (mapping == null)
                throw new KeyNotFoundException(
                    $"No se encontró el mapping con Id '{request.MappingId}'.");

            // 2. Verificar que el material existe
            var materialExists = await _context.Materials
                .AnyAsync(m => m.Id == request.MaterialId, cancellationToken);

            if (!materialExists)
                throw new KeyNotFoundException(
                    $"No se encontró el material con Id '{request.MaterialId}'.");

            // 3. Actualizar mapping → asignación manual
            mapping.MaterialId   = request.MaterialId;
            mapping.IsAutoMapped = false;
            mapping.MatchMethod  = "Manual";

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine(
                $"[MAP] Mapping {request.MappingId} → Material {request.MaterialId} (Manual)");

            return Unit.Value;
        }
    }
}
