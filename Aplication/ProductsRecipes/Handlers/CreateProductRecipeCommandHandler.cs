using Inventory.Application.ProductsRecipes.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductsRecipes.Handlers
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
            // 1. Crear la cabecera
            var recipe = new ProductRecipe
            {
                Name = request.Name,
                Instructions = request.Instructions,
                FinishedGoodId = request.FinishedGoodId,
                EstimatedMachineHours = request.EstimatedMachineHours,
                EstimatedLaborHours = request.EstimatedLaborHours
            };

            // 2. Agregar los Ingredientes
            if (request.Ingredients != null)
            {
                foreach (var ing in request.Ingredients)
                {
                    if (ing.QuantityRequired <= 0) throw new ArgumentException("La cantidad del ingrediente debe ser mayor a cero.");

                    recipe.Ingredients.Add(new RecipeIngredient
                    {
                        MaterialId = ing.MaterialId,
                        QuantityRequired = ing.QuantityRequired
                    });
                }
            }

            // 3. Agregar los Costos Adicionales
            if (request.AdditionalCosts != null)
            {
                foreach (var cost in request.AdditionalCosts)
                {
                    if (cost.EstimatedCost < 0) throw new ArgumentException("El costo estimado no puede ser negativo.");

                    recipe.AdditionalCosts.Add(new RecipeCost
                    {
                        CostType = cost.CostType,
                        Description = cost.Description,
                        EstimatedCost = cost.EstimatedCost
                    });
                }
            }

            // 4. Guardar todo el árbol
            _context.ProductRecipes.Add(recipe);
            await _context.SaveChangesAsync(cancellationToken);

            return recipe.Id;
        }
    }
}
