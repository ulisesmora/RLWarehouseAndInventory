using Inventory.Application.ProductRecipes.Commands;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Handlers
{
    public class DeleteProductRecipeCommandHandler : IRequestHandler<DeleteProductRecipeCommand, Unit>
    {
        private readonly InventoryDbContext _context;

        public DeleteProductRecipeCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteProductRecipeCommand request, CancellationToken cancellationToken)
        {
            var recipe = await _context.ProductRecipes
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (recipe == null)
            {
                throw new Exception($"No se encontró la receta con ID {request.Id}");
            }

            // Esto disparará el Soft Delete por la configuración que ya tienes
            _context.ProductRecipes.Remove(recipe);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
