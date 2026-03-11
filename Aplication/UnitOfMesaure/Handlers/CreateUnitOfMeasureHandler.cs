using AutoMapper;
using Inventory.Application.UnitOfMesaure.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Handlers
{
    public class CreateUnitOfMeasureHandler : IRequestHandler<CreateUnitOfMeasureCommand, Guid>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public CreateUnitOfMeasureHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateUnitOfMeasureCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<UnitOfMeasure>(request);

            _context.UnitOfMeasures.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
