using AutoMapper;
using Inventory.Application.Suppliers.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Suppliers.Handlers
{
    public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Guid>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public CreateSupplierCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Supplier>(request);

            _context.Suppliers.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
