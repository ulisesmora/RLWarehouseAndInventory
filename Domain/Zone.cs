using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class Zone : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Ej: "Recepción", "Pasillo A", "Cuarto Frío"
        public string? Description { get; set; }

        // --- Dimensiones Físicas (Para el renderizado 3D) ---
        // Usamos double para compatibilidad con motores como Three.js
        public double Width { get; set; }  // Ancho
        public double Depth { get; set; }  // Profundidad
        public double Height { get; set; } // Altura

        public bool? AllowMixedLots { get; set; } = false;

        // --- 2. Condiciones Ambientales de la Zona ---
        public decimal? MinTemperatureCelsius { get; set; }
        public decimal? MaxTemperatureCelsius { get; set; }
        public List<string> AllowedHazmatTags { get; set; } = new List<string>();

        // Relación: Pertenece a un Warehouse
        public Guid WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }

        // Relación: Contiene muchas ubicaciones (Bins/Estantes)
        public virtual ICollection<StorageBin> Bins { get; set; } = new List<StorageBin>();
    }

}
