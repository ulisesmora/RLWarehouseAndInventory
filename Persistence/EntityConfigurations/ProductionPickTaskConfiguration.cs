using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Persistence.EntityConfigurations
{
    public class ProductionPickTaskConfiguration : IEntityTypeConfiguration<ProductionPickTask>
    {
        public void Configure(EntityTypeBuilder<ProductionPickTask> builder)
        {
            builder.ToTable("ProductionPickTasks");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.RequiredQuantity).HasColumnType("decimal(18,4)");
            builder.Property(p => p.PickedQuantity).HasColumnType("decimal(18,4)");

            builder.HasOne(p => p.Material)
                   .WithMany()
                   .HasForeignKey(p => p.MaterialId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relación vital con el ítem de inventario específico
            builder.HasOne(p => p.SourceStockItem)
                   .WithMany()
                   .HasForeignKey(p => p.SourceStockItemId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
