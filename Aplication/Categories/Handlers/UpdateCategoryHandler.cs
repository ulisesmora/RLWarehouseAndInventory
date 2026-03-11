using AutoMapper;
using Inventory.Application.Categories.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Handlers
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Unit>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public UpdateCategoryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken token)
        {
            var entity = await _context.Categories.FindAsync(new object[] { request.Id }, token);
            if (entity == null) throw new Exception("Categoría no encontrada.");

            // Evitar redundancia cíclica (no ser su propio padre)
            if (request.ParentCategoryId == request.Id)
                throw new Exception("Una categoría no puede ser su propio padre.");

            _mapper.Map(request, entity);
            await _context.SaveChangesAsync(token);

            return Unit.Value;
        }
    }
}
