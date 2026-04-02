using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Persistence.EntityConfigurations
{
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasKey(x => x.Id);

            // Regla de Oro: El SKU debe ser ÚNICO en toda la base de datos
            builder.HasIndex(x => new { x.SKU, x.OrganizationId }).IsUnique();

            // Configuración de decimales (Postgres necesita saber la precisión)
            // (18, 2) para dinero, (18, 4) para medidas físicas (ej: 0.0001 gramos)
            builder.Property(x => x.StandardCost).HasPrecision(18, 2);
            builder.Property(x => x.SalesPrice).HasPrecision(18, 2);

            builder.Property(x => x.Weight).HasPrecision(18, 4);
            builder.Property(x => x.Volume).HasPrecision(18, 4);
            builder.Property(x => x.ReorderPoint).HasPrecision(18, 4);
            builder.Property(x => x.TargetStock).HasPrecision(18, 4);

            // Relaciones: Un Material tiene muchos Lotes
            builder.HasMany(m => m.Lots)
                   .WithOne(l => l.Material)
                   .HasForeignKey(l => l.MaterialId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
