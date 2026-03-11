using Inventory.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductsRecipes.Commands
{
    public record CreateProductRecipeCommand(
        string Name,
        string? Instructions,
        Guid FinishedGoodId,
        decimal EstimatedMachineHours,
        decimal EstimatedLaborHours,

        // Listas de detalles
        List<IngredientCommandDto> Ingredients,
        List<CostCommandDto> AdditionalCosts
    ) : IRequest<Guid>;

    // Sub-registros para el comando
    public record IngredientCommandDto(Guid MaterialId, decimal QuantityRequired);
    public record CostCommandDto(CostType CostType, string Description, decimal EstimatedCost);
}
