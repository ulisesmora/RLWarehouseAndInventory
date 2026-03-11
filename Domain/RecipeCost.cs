using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class RecipeCost : BaseEntity
    {
        public Guid ProductRecipeId { get; set; }
        public ProductRecipe? ProductRecipe { get; set; }

        public CostType CostType { get; set; }
        public string Description { get; set; } = string.Empty; // Ej: "Costurera", "Electricidad Plancha"

        // Costo estimado en dinero por hacer 1 unidad
        public decimal EstimatedCost { get; set; }
    }
}
