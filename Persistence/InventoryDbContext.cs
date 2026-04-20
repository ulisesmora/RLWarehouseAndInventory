using Domain;
using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text; 
namespace Inventory.Persistence
{
    public class InventoryDbContext : DbContext
    {
        private readonly ICurrentUserService? _currentUserService;
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Material> Materials => Set<Material>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<SupplierMaterial> SupplierMaterials => Set<SupplierMaterial>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<StockItem> StockItems => Set<StockItem>();
        public DbSet<Lot> Lots => Set<Lot>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<StatusCatalog> Statuses => Set<StatusCatalog>();

        public DbSet<ProductRecipe> ProductRecipes => Set<ProductRecipe>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
        public DbSet<RecipeCost> RecipeCosts => Set<RecipeCost>();

        public DbSet<Zone> Zones => Set<Zone>();
        public DbSet<StorageBin> StorageBins => Set<StorageBin>();
        public DbSet<WorkOrder> WorkOrder => Set<WorkOrder>();
        public DbSet<Organization> Organization => Set<Organization>();
        public DbSet<User> User => Set<User>();
        public DbSet<WorkOrderConsumption> WorkOrderConsumption => Set<WorkOrderConsumption>();
        public DbSet<ProductionPickTask> ProductionPickTask => Set<ProductionPickTask>();

        // ── Outbound / Ventas ──────────────────────────────────────────────────
        public DbSet<SalesOrder>      SalesOrders      => Set<SalesOrder>();
        public DbSet<SalesOrderLine>  SalesOrderLines  => Set<SalesOrderLine>();
        public DbSet<OutboundPickTask> OutboundPickTasks => Set<OutboundPickTask>();

        // ── Integraciones de canales externos ─────────────────────────────────
        public DbSet<ChannelConfig>         ChannelConfigs         => Set<ChannelConfig>();
        public DbSet<ChannelProductMapping> ChannelProductMappings => Set<ChannelProductMapping>();


        public Guid? CurrentTenantId => _currentUserService?.GetTenantId();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Aplicar todas las configuraciones IEntityTypeConfiguration que creamos arriba
            // (Escanea el ensamblado actual y las carga solas)
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<Material>()
          .Property(m => m.HazmatTags)
          .HasConversion(
              v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
              // Si la base de datos manda un texto vacío, devolvemos una lista vacía en lugar de quebrar
              v => string.IsNullOrWhiteSpace(v)
                   ? new List<string>()
                   : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null)
          );

            modelBuilder.Entity<Zone>()
                .Property(z => z.AllowedHazmatTags)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    // Lo mismo aquí
                    v => string.IsNullOrWhiteSpace(v)
                         ? new List<string>()
                         : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null)
                );

            // 2. Configuración Global para Soft Delete (Query Filter)
            // Esto hace que EF Core ignore automáticamente cualquier registro con IsDeleted = true
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                bool isBaseEntity = typeof(BaseEntity).IsAssignableFrom(clrType);
                bool isTenantEntity = typeof(ITenantEntity).IsAssignableFrom(clrType);

                if (isBaseEntity || isTenantEntity)
                {
                    var parameter = Expression.Parameter(clrType, "e");
                    Expression? filterExpression = null;

                    if (isBaseEntity)
                    {
                        var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                        filterExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                    }

                    if (isTenantEntity)
                    {
                        var tenantIdProperty = Expression.Property(parameter, nameof(ITenantEntity.OrganizationId));
                        var currentTenantIdProp = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
                        var tenantFilter = Expression.Equal(tenantIdProperty, Expression.Convert(currentTenantIdProp, typeof(Guid)));

                        filterExpression = filterExpression == null ? tenantFilter : Expression.AndAlso(filterExpression, tenantFilter);
                    }

                    if (filterExpression != null)
                    {
                        var lambda = Expression.Lambda(filterExpression, parameter);
                        modelBuilder.Entity(clrType).HasQueryFilter(lambda);
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        // Pequeño hack para aplicar el filtro genérico dinámicamente
        private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression<TInterface>(
            System.Linq.Expressions.Expression<Func<TInterface, bool>> filterExpression,
            Type entityType)
        {
            var newParam = System.Linq.Expressions.Expression.Parameter(entityType);
            var newBody = ReplacingExpressionVisitor.Replace(filterExpression.Parameters.Single(), newParam, filterExpression.Body);
            return System.Linq.Expressions.Expression.Lambda(newBody, newParam);
        }

        // Sobreescribir SaveChanges para llenar CreatedAt y UpdatedAt automáticamente
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = _currentUserService?.GetTenantId();
            var userId = _currentUserService?.GetUserId();

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Deleted:
                        // Convertir Delete físico en Soft Delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        break;
                }
            }

            if (tenantId.HasValue)
            {
                foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.OrganizationId = tenantId.Value;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
