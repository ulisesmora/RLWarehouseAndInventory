using AutoMapper;
using Inventory.Application.Categories.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Handlers
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Guid>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public CreateCategoryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken token)
        {
            // Validación lógica: si tiene padre, que exista
            if (request.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories.AnyAsync(x => x.Id == request.ParentCategoryId, token);
                if (!parentExists) throw new Exception("La categoría padre no existe.");
            }

            var entity = _mapper.Map<Category>(request);
            _context.Categories.Add(entity);
            await _context.SaveChangesAsync(token);

            return entity.Id;
        }
    }
}
