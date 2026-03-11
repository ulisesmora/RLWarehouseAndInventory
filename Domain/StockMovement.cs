using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public enum MovementType
    {
        PurchaseReception,  // Entrada por compra
        SalesShipment,      // Salida por venta
        ManufacturingUse,   // Salida para fabricar
        ManufacturingOutput,// Entrada de producto terminado
        Adjustment,         // Ajuste de inventario (robo, pérdida)
        Transfer            // Movimiento entre bodegas
    }

    public class StockMovement : BaseEntity
    {
        public DateTime MovementDate { get; set; }
        public MovementType Type { get; set; }

        // --- Relación con el StockItem afectado ---
        public Guid MaterialId { get; set; }
        public virtual Material Material { get; set; }

        public Guid WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; }

        public Guid? LotId { get; set; }
        public virtual Lot Lot { get; set; }

        // --- Cantidad Movida ---
        // Positivo = Entrada, Negativo = Salida
        public decimal Quantity { get; set; }

        // --- Auditoría ---
        public string ReferenceNumber { get; set; } // Ej: "PO-2024-001" (Orden de compra)
        public Guid UserId { get; set; }            // Quién hizo el movimiento
        public string Comments { get; set; }
        public Guid? StorageBinId { get; set; }
        public virtual StorageBin? StorageBin { get; set; }
    }
}
