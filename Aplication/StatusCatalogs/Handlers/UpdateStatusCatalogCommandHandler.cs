using AutoMapper;
using Inventory.Application.StatusCatalogs.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StatusCatalogs.Handlers
{
    public class UpdateStatusCatalogCommandHandler : IRequestHandler<UpdateStatusCatalogCommand>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public UpdateStatusCatalogCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task Handle(UpdateStatusCatalogCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscamos el estado existente en la base de datos
            var entity = await _context.Statuses
                .FindAsync(new object[] { request.Id }, cancellationToken);

            // 2. Validamos que exista
            if (entity == null)
            {
                throw new KeyNotFoundException($"El estado de catálogo con ID {request.Id} no existe.");
            }

            // 3. Mapeo Mágico: Sobreescribe las propiedades Name y Description 
            // del 'entity' usando los valores que vienen en el 'request'
            _mapper.Map(request, entity);

            // 4. Guardamos los cambios
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
