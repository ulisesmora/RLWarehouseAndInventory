using AutoMapper;
using Inventory.Application.Warehouses.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Handlers
{
    public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper; // Inyectamos el Mapper

        public UpdateWarehouseCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Warehouses
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"El almacén con ID {request.Id} no existe.");
            }

            // Mapeo Automático: Actualiza 'entity' con los datos de 'request'
            // Esto sobreescribe Name y Location automáticamente
            _mapper.Map(request, entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
