using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class WorkOrder : BaseTenantEntity
    {
        // Ej: "WO-2026-0001" (Para que el operario la busque fácilmente)
        public string OrderNumber { get; set; } = string.Empty;

        // ¿Qué receta vamos a usar?
        public Guid ProductRecipeId { get; set; }
        public virtual ProductRecipe ProductRecipe { get; set; }

        // ¿Qué material final vamos a obtener? (Redundante pero útil para queries rápidos)
        public Guid FinishedGoodId { get; set; }
        public virtual Material FinishedGood { get; set; }

        // --- Cantidades ---
        public decimal PlannedQuantity { get; set; } // Lo que el jefe pidió (Ej: 100)

        // Lo que realmente salió (A veces piden 100, pero se echa a perder tela y salen 98)
        public decimal ProducedQuantity { get; set; }

        // --- Tiempos ---
        public DateTime PlannedStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        // Estado actual de la orden
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Pending;

        public ICollection<ProductionPickTask> PickTasks { get; set; } = new List<ProductionPickTask>();

        // Notas del supervisor (Ej: "Urge para el pedido VIP del e-commerce")
        public string? Notes { get; set; }
    }
}
