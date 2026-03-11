using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application
{
    public interface IApplicationDbContext
    {
        DbSet<Material> Materials { get; }
        DbSet<Category> Categories { get; }
        DbSet<UnitOfMeasure> UnitOfMeasures { get; }
        DbSet<Supplier> Suppliers { get; }

        // Agrega los que faltaban para que coincidan con tu DbContext real:
        DbSet<SupplierMaterial> SupplierMaterials { get; }
        DbSet<Warehouse> Warehouses { get; }
        DbSet<StockItem> StockItems { get; }
        DbSet<Lot> Lots { get; }
        DbSet<StockMovement> StockMovements { get; }
        DbSet<StatusCatalog> Statuses { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
