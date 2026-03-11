using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductsRecipes.Queries
{
    public class ProductRecipeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Instructions { get; set; }

        public Guid FinishedGoodId { get; set; }
        public string FinishedGoodName { get; set; } // Aplanado desde la relación

        public decimal EstimatedMachineHours { get; set; }
        public decimal EstimatedLaborHours { get; set; }

        // Colecciones anidadas
        public List<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
        public List<RecipeCostDto> AdditionalCosts { get; set; } = new List<RecipeCostDto>();
    }

    public class RecipeIngredientDto
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; } // Aplanado
        public decimal QuantityRequired { get; set; }
    }

    public class RecipeCostDto
    {
        public Guid Id { get; set; }
        public string CostType { get; set; } // Enum convertido a string
        public string Description { get; set; }
        public decimal EstimatedCost { get; set; }
    }
}
