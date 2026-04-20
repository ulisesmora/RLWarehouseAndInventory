using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Commands
{
    public class CreateProductRecipeCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Instructions { get; set; }
        public Guid FinishedGoodId { get; set; }
        public decimal EstimatedMachineHours { get; set; }
        public decimal EstimatedLaborHours { get; set; }
        public decimal YieldQuantity { get; set; } = 1m;

        public List<CreateRecipeIngredientDto> Ingredients { get; set; } = new();
        public List<CreateRecipeCostDto> AdditionalCosts { get; set; } = new();
    }

    public class CreateRecipeIngredientDto
    {
        public Guid MaterialId { get; set; }
        public decimal QuantityRequired { get; set; }
    }

    public class CreateRecipeCostDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
    }
}
