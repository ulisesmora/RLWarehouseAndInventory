using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class Lot : BaseTenantEntity
    {
        public string LotNumber { get; set; }        // Generado por tu sistema
        public string? VendorBatchNumber { get; set; } // El que viene impreso en la caja

        // --- Fechas Críticas (FIFO/FEFO) ---
        public DateTime ManufacturingDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        // --- Relaciones ---
        public Guid MaterialId { get; set; }
        public virtual Material Material { get; set; }

        // [Nuevo] Origen: ¿Qué proveedor nos mandó este lote?
        // Es nullable porque a veces es producción interna (manufactura propia)
        public Guid? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        public decimal InitialReceivedQuantity { get; set; }

        public bool IsBlocked { get; set; }


        public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();

        // 2. ¿Qué historial de movimientos tiene este lote? (Entradas, salidas, ajustes)
        // Vital para auditar y para evitar borrar un lote que ya se usó.
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }
}
