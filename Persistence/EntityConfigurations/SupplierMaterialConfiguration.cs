using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Persistence.EntityConfigurations
{
    public class SupplierMaterialConfiguration : IEntityTypeConfiguration<SupplierMaterial>
    {
        public void Configure(EntityTypeBuilder<SupplierMaterial> builder)
        {
            builder.Property(x => x.UnitCost).HasPrecision(18, 2);
            builder.Property(x => x.MinimumOrderQuantity).HasPrecision(18, 4);

            // Un proveedor solo puede estar listado una vez para un material específico
            builder.HasIndex(x => new { x.SupplierId, x.MaterialId, x.OrganizationId }).IsUnique();
        }
    }
}
