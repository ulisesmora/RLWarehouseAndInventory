using AutoMapper;
using Inventory.Application.Lots.Commands;
using Inventory.Application.Materials.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Handlers
{
    public class UpdateLotHandler : IRequestHandler<UpdateLotCommand, Unit>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public UpdateLotHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateLotCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscar la entidad existente
            var entity = await _context.Lots
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                // En un proyecto real, lanzarías una excepción personalizada NotFoundException
                throw new Exception($"Lot con ID {request.Id} no encontrado.");
            }

            // 2. Mapear los cambios DEL comando A la entidad existente
            // AutoMapper sobreescribe las propiedades de 'entity' con las de 'request'
            _mapper.Map(request, entity);

            // 3. Guardar (EF Core detecta que 'entity' cambió y genera el UPDATE SQL)
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
