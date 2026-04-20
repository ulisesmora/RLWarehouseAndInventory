using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Queries
{
    public class RecipeIngredientDto
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public decimal QuantityRequired { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
    }

    public class RecipeCostDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
    }

    public class ProductRecipeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Instructions { get; set; }
        public Guid FinishedGoodId { get; set; }
        public string FinishedGoodName { get; set; } = string.Empty;

        public decimal EstimatedMachineHours { get; set; }
        public decimal EstimatedLaborHours { get; set; }

        public decimal YieldQuantity { get; set; }

        public List<RecipeIngredientDto> Ingredients { get; set; } = new();
        public List<RecipeCostDto> AdditionalCosts { get; set; } = new();
    }

}
