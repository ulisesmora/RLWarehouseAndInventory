using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class StorageBin : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // Ej: "A-01-01" (Pasillo-Rack-Nivel)
        public string? Description { get; set; }

        // --- Coordenadas Relativas a la ZONA ---
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }

        // --- Capacidades ---
        public decimal MaxWeight { get; set; } // Capacidad de carga
        public decimal MaxVolume { get; set; } // Capacidad volumétrica

        public double Width { get; set; }  // Ancho
        public double Depth { get; set; }  // Profundidad
        public double Height { get; set; } // Altura

        public double physicalOffsetX { get; set; }
        public double physicalOffsetZ { get; set; }
        public double rotation { get; set; }

        // Relación: Pertenece a una Zona
        public Guid ZoneId { get; set; }
        public virtual Zone? Zone { get; set; }

        // Relación: Atajo al Warehouse (Útil para queries rápidos)
        public Guid WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }

        // Relación: ¿Qué stock hay aquí?
        public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
    }
}
