using AutoMapper;
using Inventory.Application.Zones.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Handlers
{
    public class UpdateZoneCommandHandler : IRequestHandler<UpdateZoneCommand>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public UpdateZoneCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task Handle(UpdateZoneCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Zones.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null) throw new KeyNotFoundException($"Zona {request.Id} no encontrada.");

            // Mapeo automático de propiedades (Name, Width, etc.)
            _mapper.Map(request, entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
