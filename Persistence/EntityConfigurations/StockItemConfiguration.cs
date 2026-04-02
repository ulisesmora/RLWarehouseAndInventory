using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Persistence.EntityConfigurations
{
    public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> builder)
        {
            // Precisión para cantidades
            builder.Property(x => x.QuantityOnHand).HasPrecision(18, 4);
            builder.Property(x => x.QuantityReserved).HasPrecision(18, 4);

            // ÍNDICE COMPUESTO ÚNICO (La clave del éxito)
            // Evita duplicados: Solo una fila por Almacén + Material + Lote + Estado
            
            builder.HasIndex(x => new { x.WarehouseId, x.MaterialId, x.LotId, x.StatusId });
            builder.HasIndex(x => new { x.ReferenceNumber, x.OrganizationId }).IsUnique();
            
        }
    }
}
