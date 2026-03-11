using AutoMapper;
using Inventory.Application.Materials.Commands;
using Inventory.Application.Warehouses.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Handlers
{   
        public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Guid>
        {
            private readonly InventoryDbContext _context;
            private readonly IMapper _mapper;

            public CreateWarehouseCommandHandler(InventoryDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Guid> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
            {
                // 1. Convertir DTO (Command) a Entidad de Dominio
                // Nota: Aquí podrías usar AutoMapper/Mapster, pero manual es más explícito y rápido.
                var entity = _mapper.Map<Warehouse>(request);

                // 2. Agregar al contexto
                _context.Warehouses.Add(entity);

                // 3. Guardar cambios (Dispara el SQL INSERT)
                await _context.SaveChangesAsync(cancellationToken);

                // 4. Retornar el ID generado
                return entity.Id;
            }

        
    }
    
}
