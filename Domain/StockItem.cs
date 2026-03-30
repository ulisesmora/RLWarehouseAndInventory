using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Inventory.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContainerType
    {
        Box,        // Caja de Cartón estándar (Cubo)
        Pallet,     // Tarima de madera con mercancía encima
        Drum,       // Tambo / Barril (Cilindro, típico para líquidos/químicos)
        Tote,       // Cubeta o Gaveta de plástico (Cajas reutilizables WMS)
        IBC,        // Contenedor IBC (Esos tanques cuadrados blancos con reja de metal para 1000L)
        Bottle,     // Botella / Frasco
        Bag,        // Saco / Costal (Ej: Cemento, Azúcar)
        Other       // Geometría irregular
    }
    public class StockItem : BaseEntity
    {
        public Guid WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; }

        public Guid MaterialId { get; set; }
        public virtual Material Material { get; set; }

        // --- Trazabilidad: ¿De qué lote específico? ---
        public Guid? LotId { get; set; }
        public virtual Lot Lot { get; set; }

        // --- Estado: ¿Está disponible o en cuarentena? ---
        public Guid StatusId { get; set; }
        public virtual StatusCatalog Status { get; set; }

        public Guid? StorageBinId { get; set; }
        public virtual StorageBin? StorageBin { get; set; }

        public string ReferenceNumber { get; set; }

        public string? Comments { get; set; }

        public ContainerType ContainerType { get; set; } = ContainerType.Box; // Por defecto es una caja

        // 2. Dimensiones Reales (El Bounding Box)
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }

        // 3. Propiedad calculada: El motor WMS la usará para saber cuánto espacio real roba en el estante
        public decimal? VolumeCm3 => (LengthCm.HasValue && WidthCm.HasValue && HeightCm.HasValue)
                                     ? (LengthCm * WidthCm * HeightCm)
                                     : null;

        // (Opcional, pero súper WMS Tier 1): 
        // Si el LPN es un Pallet, ¿Es un pallet apilable encima de otro?
        public bool IsStackable { get; set; } = true;
        // --- Cantidades ---
        public decimal QuantityOnHand { get; set; }   // Lo que cuentas físicamente en el estante
        public decimal QuantityReserved { get; set; } // Lo que ya vendiste pero no has enviado

        // Propiedad calculada (útil para lógica, no se mapea necesariamente a BD)
        public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    }
}
