using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class ProductRecipe : BaseTenantEntity
    {
        public string Name { get; set; } = string.Empty; // Ej: "Receta Camisa Verano Talla M"
        public string? Instructions { get; set; } // Instrucciones de corte/costura

        // ¿Qué producto terminado resulta de esta receta?
        public Guid FinishedGoodId { get; set; }
        public Material? FinishedGood { get; set; }

        // Tiempos estimados para calcular productividad
        public decimal EstimatedMachineHours { get; set; }
        public decimal EstimatedLaborHours { get; set; }

        // Relaciones: Sus ingredientes y sus costos
        public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<RecipeCost> AdditionalCosts { get; set; } = new List<RecipeCost>();
    }
}
