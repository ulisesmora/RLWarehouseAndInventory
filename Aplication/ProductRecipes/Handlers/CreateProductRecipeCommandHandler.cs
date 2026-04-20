using Inventory.Application.ProductRecipes.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Handlers
{
    public class CreateProductRecipeCommandHandler : IRequestHandler<CreateProductRecipeCommand, Guid>
    {
        private readonly InventoryDbContext _context;

        public CreateProductRecipeCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateProductRecipeCommand request, CancellationToken cancellationToken)
        {
            // 1. Crear la Entidad Principal
            var recipe = new ProductRecipe
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Instructions = request.Instructions,
                YieldQuantity = request.YieldQuantity,
                FinishedGoodId = request.FinishedGoodId,
                EstimatedMachineHours = request.EstimatedMachineHours,
                EstimatedLaborHours = request.EstimatedLaborHours,

                // 2. Mapear Ingredientes
                Ingredients = request.Ingredients.Select(i => new RecipeIngredient
                {
                    Id = Guid.NewGuid(),
                    MaterialId = i.MaterialId,
                    QuantityRequired = i.QuantityRequired
                }).ToList(),

                // 3. Mapear Costos Adicionales
                AdditionalCosts = request.AdditionalCosts.Select(c => new RecipeCost
                {
                    Id = Guid.NewGuid(),
                    Description = c.Description,
                    EstimatedCost = c.EstimatedCost
                }).ToList()
            };

            // 4. Guardar en Base de Datos
            _context.ProductRecipes.Add(recipe);
            await _context.SaveChangesAsync(cancellationToken);

            return recipe.Id;
        }
    }
}
