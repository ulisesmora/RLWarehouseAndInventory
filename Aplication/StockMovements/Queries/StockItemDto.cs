using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Queries
{
    public class StockItemDto
    {
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; }

        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string? MaterialSKU { get; set; }
        public string WarehouseName { get; set; }

        public Guid? StorageBinId { get; set; }
        public string? StorageBinCode { get; set; }

        public Guid? LotId { get; set; }
        public string? LotNumber { get; set; }

        public Guid StatusId { get; set; }
        public string StatusName { get; set; } // Asumiendo que tu StatusCatalog tiene un Name

        public string ReferenceNumber { get; set; }

        public string Comments { get; set; }

        public string ContainerType { get; set; } // Viaja como texto "Drum", "Pallet", "Box"
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? VolumeCm3 { get; set; }   // AutoMapper lo calculará de la propiedad =>
        public bool IsStackable { get; set; }

        // Las cantidades clave
        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal QuantityAvailable { get; set; }

        public Guid? ZoneId { get; set; }
        public string? ZoneName { get; set; }

        public DateTime? ExpirationDate { get; set; } // Fecha de caducidad (viene del Lote)
        public string UnitOfMeasureName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
