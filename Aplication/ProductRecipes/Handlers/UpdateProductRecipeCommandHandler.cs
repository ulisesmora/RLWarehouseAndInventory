using Inventory.Application.ProductRecipes.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Handlers
{
    public class UpdateProductRecipeCommandHandler : IRequestHandler<UpdateProductRecipeCommand, Unit>
    {
        private readonly InventoryDbContext _context;

        public UpdateProductRecipeCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateProductRecipeCommand request, CancellationToken cancellationToken)
        {
            var recipe = await _context.ProductRecipes
                .Include(r => r.Ingredients)
                .Include(r => r.AdditionalCosts)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (recipe == null)
            {
                throw new Exception($"No se encontró la receta con ID {request.Id}");
            }

            // 1. Actualizar campos simples
            recipe.Name = request.Name;
            recipe.Instructions = request.Instructions;
            recipe.FinishedGoodId = request.FinishedGoodId;
            recipe.EstimatedMachineHours = request.EstimatedMachineHours;
            recipe.EstimatedLaborHours = request.EstimatedLaborHours;
            recipe.YieldQuantity = request.YieldQuantity;
            recipe.UpdatedAt = DateTime.UtcNow;

            // 2. Reemplazar ingredientes: eliminar los viejos y agregar los nuevos.
            // IMPORTANTE: se copia OrganizationId del padre para mantener aislamiento multi-tenant.
            _context.RecipeIngredients.RemoveRange(recipe.Ingredients);
            recipe.Ingredients = request.Ingredients.Select(i => new RecipeIngredient
            {
                Id = Guid.NewGuid(),
                OrganizationId = recipe.OrganizationId,
                ProductRecipeId = recipe.Id,
                MaterialId = i.MaterialId,
                QuantityRequired = i.QuantityRequired,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            // 3. Reemplazar costos (mismo patrón, misma razón de OrganizationId)
            _context.RecipeCosts.RemoveRange(recipe.AdditionalCosts);
            recipe.AdditionalCosts = request.AdditionalCosts.Select(c => new RecipeCost
            {
                Id = Guid.NewGuid(),
                OrganizationId = recipe.OrganizationId,
                ProductRecipeId = recipe.Id,
                Description = c.Description,
                EstimatedCost = c.EstimatedCost,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
