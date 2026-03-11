using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class RecipeIngredient : BaseEntity
    {
        public Guid ProductRecipeId { get; set; }
        public ProductRecipe? ProductRecipe { get; set; }

        // Qué materia prima se necesita (Ej: Tela Algodón)
        public Guid MaterialId { get; set; }
        public Material? Material { get; set; }

        // Cuánto se necesita para hacer 1 unidad del producto terminado
        public decimal QuantityRequired { get; set; }
    }
}
