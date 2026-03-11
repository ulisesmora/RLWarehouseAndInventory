using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Queries
{
    public class MaterialDto
    {
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string SKU { get; set; } = string.Empty;
		public string? BarCode { get; set; }        // Nuevo
		public string? Description { get; set; }

		// Es vital que el frontend sepa qué tipo es (Materia Prima o Producto)
		// Gracias a tu configuración de JsonStringEnumConverter, esto viajará como texto "RawMaterial"
		public MaterialType Type { get; set; }

		// --- Logística ---
		public decimal? Weight { get; set; }
		public decimal? Volume { get; set; }
		public bool IsStockable { get; set; }

		// --- Planificación y Costos ---
		public decimal ReorderPoint { get; set; }
		public decimal TargetStock { get; set; }    // Nuevo
		public decimal StandardCost { get; set; }   // Nuevo
		public decimal SalesPrice { get; set; }     // Nuevo

		// --- Campos Aplanados (Flattening) ---
		public string UnitOfMeasureName { get; set; } = string.Empty;
		public string CategoryName { get; set; } = string.Empty;
		public string CategoryDescription { get; set; } = string.Empty; // Útil a veces

		// --- Campo Calculado (Stock Total) ---
		// Este no viene de la tabla Material, lo calcularemos en el Query
		public decimal TotalStock { get; set; }
	}
}
