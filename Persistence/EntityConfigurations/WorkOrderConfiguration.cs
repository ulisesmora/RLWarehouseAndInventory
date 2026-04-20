using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Persistence.EntityConfigurations
{
    public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            builder.ToTable("WorkOrders");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.OrderNumber).IsRequired().HasMaxLength(50);
            builder.Property(w => w.ProducedQuantity).HasColumnType("decimal(18,4)");

            builder.HasOne(w => w.ProductRecipe)
                   .WithMany()
                   .HasForeignKey(w => w.ProductRecipeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(w => w.PickTasks)
                   .WithOne(p => p.WorkOrder)
                   .HasForeignKey(p => p.WorkOrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
