using AutoMapper;
using Inventory.Application.Zones.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Handlers
{
    public class CreateZoneCommandHandler : IRequestHandler<CreateZoneCommand, Guid>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public CreateZoneCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateZoneCommand request, CancellationToken cancellationToken)
        {
            // 1. Mapeo Automático (Command -> Entity)
            var entity = _mapper.Map<Zone>(request);

            // 2. Guardar
            _context.Zones.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
