using AutoMapper;
using Inventory.Application.StatusCatalogs.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StatusCatalogs.Handlers
{
    public class CreateStatusCatalogCommandHandler : IRequestHandler<CreateStatusCatalogCommand, Guid>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public CreateStatusCatalogCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateStatusCatalogCommand request, CancellationToken cancellationToken)
        {
            // Mapeo automático del Comando a la Entidad
            var entity = _mapper.Map<StatusCatalog>(request);

            _context.Statuses.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
