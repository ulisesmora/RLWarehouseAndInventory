using System;

namespace Inventory.Application.WorkOrders.Queries
{
    public class ProductionPickTaskDto
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public Guid SourceStockItemId { get; set; }

        // Trazabilidad exacta de la caja que el operador debe tomar
        public string LpnCode { get; set; } = string.Empty;   // ReferenceNumber del StockItem (el código de la caja)
        public string LotNumber { get; set; } = string.Empty;  // Número de lote
        public string BinCode { get; set; } = string.Empty;    // Código del bin (P.ej. ZA-R01-N02-P03)

        // Etiqueta legible completa: "Zona A - ZA-R01-N02-P03"
        public string LocationLabel { get; set; } = string.Empty;

        // Para el Gemelo Digital 3D y la Ruta de Picking
        public Guid? SourceBinId { get; set; }
        public Guid? SourceZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;

        public decimal RequiredQuantity { get; set; }
        public decimal PickedQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
