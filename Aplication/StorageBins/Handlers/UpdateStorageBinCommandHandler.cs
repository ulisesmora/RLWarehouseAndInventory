using AutoMapper;
using Inventory.Application.StorageBins.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Handlers
{
    public class UpdateStorageBinCommandHandler : IRequestHandler<UpdateStorageBinCommand>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public UpdateStorageBinCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task Handle(UpdateStorageBinCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.StorageBins
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"La ubicación {request.Id} no existe.");
            }

            // Mapeo Mágico: Actualiza solo las propiedades que coinciden
            _mapper.Map(request, entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
