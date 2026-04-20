using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Persistence.EntityConfigurations
{
    public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
        {
            builder.ToTable("RecipeIngredients");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.QuantityRequired).HasColumnType("decimal(18,4)");

            // Aseguramos que no se pueda borrar un Material si está siendo usado en una Receta
            builder.HasOne(i => i.Material)
                   .WithMany()
                   .HasForeignKey(i => i.MaterialId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
