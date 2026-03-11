using AutoMapper;
using Inventory.Application.Suppliers.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Suppliers.Handlers
{
    public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public UpdateSupplierCommandHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Suppliers.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null) throw new KeyNotFoundException($"Proveedor {request.Id} no encontrado.");

            _mapper.Map(request, entity); // Actualiza automáticamente

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
