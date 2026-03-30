using Inventory.Application.StockMovements.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Queries
{
    public class StorageBinDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } // "A-01-01"
        public string Description { get; set; }

        // Coordenadas relativas a la Zona
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }

        // Capacidades
        public decimal MaxWeight { get; set; }
        public decimal MaxVolume { get; set; }

        public double Width { get; set; }  // Ancho
        public double Depth { get; set; }  // Profundidad
        public double Height { get; set; } // Altura

        public double physicalOffsetX { get; set; }
        public double physicalOffsetZ { get; set; }
        public double rotation { get; set; }

        public decimal CurrentWeight { get; set; }
        public decimal CurrentVolumeM3 { get; set; }

        public decimal AvailableWeight => MaxWeight - CurrentWeight;
        public decimal AvailableVolume => (MaxVolume/ 1000000m) - CurrentVolumeM3;

        public List<BinItemDto> ExistingItems { get; set; } = new List<BinItemDto>();

        // Contexto
        public Guid ZoneId { get; set; }
        public string ZoneName { get; set; }
    }

    public class BinItemDto
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; }
        public string ContainerType { get; set; }
        public decimal? VolumeM3 { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? DepthCm { get; set; }

    }
}
