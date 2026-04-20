using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Persistence.EntityConfigurations
{
    public class ProductRecipeConfiguration : IEntityTypeConfiguration<ProductRecipe>
    {
        public void Configure(EntityTypeBuilder<ProductRecipe> builder)
        {
            builder.ToTable("ProductRecipes");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
            builder.Property(r => r.EstimatedMachineHours).HasColumnType("decimal(18,4)");
            builder.Property(r => r.EstimatedLaborHours).HasColumnType("decimal(18,4)");

            // Relación 1 a 1: El producto final que resulta de esta receta
            builder.HasOne(r => r.FinishedGood)
                   .WithMany()
                   .HasForeignKey(r => r.FinishedGoodId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relación 1 a Muchos: Ingredientes
            builder.HasMany(r => r.Ingredients)
                   .WithOne(i => i.ProductRecipe)
                   .HasForeignKey(i => i.ProductRecipeId)
                   .OnDelete(DeleteBehavior.Cascade); // 🔥 Si se borra la receta, se borran sus ingredientes

            // Relación 1 a Muchos: Costos Adicionales
            builder.HasMany(r => r.AdditionalCosts)
                   .WithOne(c => c.ProductRecipe)
                   .HasForeignKey(c => c.ProductRecipeId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
