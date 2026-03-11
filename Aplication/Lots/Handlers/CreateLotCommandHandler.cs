using AutoMapper;
using Inventory.Application.Lots.Commands;
using Inventory.Application.Materials.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Handlers
{
    public class CreateLotCommandHandler : IRequestHandler<CreateLotCommand, Guid>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public CreateLotCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateLotCommand request, CancellationToken cancellationToken)
        {
            // 1. Convertir DTO (Command) a Entidad de Dominio
            // Nota: Aquí podrías usar AutoMapper/Mapster, pero manual es más explícito y rápido.
            var entity = _mapper.Map<Lot>(request);

            // 2. Agregar al contexto
            _context.Lots.Add(entity);

            // 3. Guardar cambios (Dispara el SQL INSERT)
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Retornar el ID generado
            return entity.Id;
        }
    }
}
