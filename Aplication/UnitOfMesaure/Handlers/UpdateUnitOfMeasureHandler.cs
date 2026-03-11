using AutoMapper;
using Inventory.Application.UnitOfMesaure.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Handlers
{
    public class UpdateUnitOfMeasureHandler : IRequestHandler<UpdateUnitOfMeasureCommand, Unit>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public UpdateUnitOfMeasureHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateUnitOfMeasureCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.UnitOfMeasures
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
                throw new Exception($"Unidad de Medida {request.Id} no encontrada.");

            _mapper.Map(request, entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
