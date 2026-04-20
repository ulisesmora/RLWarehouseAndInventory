using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Persistence.EntityConfigurations
{
    public class RecipeCostConfiguration : IEntityTypeConfiguration<RecipeCost>
    {
        public void Configure(EntityTypeBuilder<RecipeCost> builder)
        {
            builder.ToTable("RecipeCosts");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Description).IsRequired().HasMaxLength(200);
            builder.Property(c => c.EstimatedCost).HasColumnType("decimal(18,4)");
        }
    }
}
