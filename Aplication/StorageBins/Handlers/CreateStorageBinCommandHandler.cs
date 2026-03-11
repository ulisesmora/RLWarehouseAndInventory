using AutoMapper;
using Inventory.Application.StorageBins.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Handlers
{
    public class CreateStorageBinCommandHandler : IRequestHandler<CreateStorageBinCommand, Guid>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public CreateStorageBinCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateStorageBinCommand request, CancellationToken cancellationToken)
        {
            // Mapeo automático Command -> Entity
            var entity = _mapper.Map<StorageBin>(request);

            // Importante: Asignamos el WarehouseId automáticamente basado en la Zona.
            // Para evitar consultar la BD extra, podemos confiar en que ZoneId es correcto,
            // pero si queremos llenar el campo 'WarehouseId' de StorageBin (desnormalización),
            // lo ideal es buscar la zona. 

            // OPCIÓN A (Rápida): Guardar y dejar que EF resuelva relaciones.
            // OPCIÓN B (Segura): Buscar la Zona para obtener el WarehouseId y guardarlo también en el Bin.

            // Vamos con la Opción B para mantener la consistencia del dato WarehouseId en la tabla Bins
            var zone = await _context.Zones.FindAsync(new object[] { request.ZoneId }, cancellationToken);
            if (zone == null) throw new Exception("La zona especificada no existe.");

            entity.WarehouseId = zone.WarehouseId; // Heredamos el ID del edificio

            _context.StorageBins.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
