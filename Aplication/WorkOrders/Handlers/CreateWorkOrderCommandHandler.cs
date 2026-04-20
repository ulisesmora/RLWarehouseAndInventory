using Inventory.Application.WorkOrders.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inventory.Application.WorkOrders.Handlers
{
    public class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, Guid>
    {
        private readonly InventoryDbContext _context;

        public CreateWorkOrderCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"[WO] INICIO CreateWorkOrder");
            Console.WriteLine($"[WO]   RecipeId      = {request.ProductRecipeId}");
            Console.WriteLine($"[WO]   PlannedQty    = {request.PlannedQuantity}");
            Console.WriteLine($"[WO]   TenantId      = {_context.CurrentTenantId}");

            var recipe = await _context.ProductRecipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == request.ProductRecipeId, cancellationToken);

            if (recipe == null) throw new Exception("Receta no encontrada.");

            Console.WriteLine($"[WO]   Receta        = '{recipe.Name}'");
            Console.WriteLine($"[WO]   Ingredientes  = {recipe.Ingredients.Count}");

            var workOrder = new WorkOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"WO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                ProductRecipeId = request.ProductRecipeId,
                FinishedGoodId = recipe.FinishedGoodId,
                PlannedQuantity = request.PlannedQuantity,
                PlannedStartDate = request.PlannedStartDate,
                Notes = request.Notes,
                Status = WorkOrderStatus.Allocated
            };

            _context.WorkOrder.Add(workOrder);

            // 🚀 Algoritmo FIFO / Allocation
            foreach (var ingredient in recipe.Ingredients)
            {
                decimal totalRequired = ingredient.QuantityRequired * request.PlannedQuantity;
                decimal quantityAllocated = 0;

                Console.WriteLine($"--------------------------------------------------");
                Console.WriteLine($"[WO] Ingrediente MaterialId = {ingredient.MaterialId}");
                Console.WriteLine($"[WO]   QuantityRequired (x1 unidad) = {ingredient.QuantityRequired}");
                Console.WriteLine($"[WO]   Total requerido              = {ingredient.QuantityRequired} x {request.PlannedQuantity} = {totalRequired}");

                // --- DIAGNÓSTICO: buscar TODOS los stock items para ese material sin filtro de qty ---
                var allForMaterial = await _context.StockItems
                    .Where(s => s.MaterialId == ingredient.MaterialId)
                    .ToListAsync(cancellationToken);

                Console.WriteLine($"[WO]   StockItems encontrados (sin filtro qty): {allForMaterial.Count}");
                if (allForMaterial.Count == 0)
                {
                    Console.WriteLine($"[WO]   *** ATENCIÓN: No hay NINGÚN StockItem para ese MaterialId con el TenantId actual ***");
                }
                foreach (var si in allForMaterial)
                {
                    decimal libre = si.QuantityOnHand - si.QuantityReserved - si.AllocatedQuantity;
                    Console.WriteLine($"[WO]     ID={si.Id} | OrgId={si.OrganizationId} | OnHand={si.QuantityOnHand} | Reserved={si.QuantityReserved} | Allocated={si.AllocatedQuantity} | LIBRE={libre}");
                }

                // --- Query real con filtro de cantidad libre ---
                var availableStock = await _context.StockItems
                    .Include(s => s.Lot)
                    .Where(s => s.MaterialId == ingredient.MaterialId
                             && (s.QuantityOnHand - s.QuantityReserved - s.AllocatedQuantity) > 0)
                    .OrderBy(s => s.Lot != null ? s.Lot.ExpirationDate : DateTime.MaxValue)
                    .ThenBy(s => s.CreatedAt)
                    .ToListAsync(cancellationToken);

                decimal totalOnHand = availableStock.Sum(s => s.QuantityOnHand);
                decimal totalFree   = availableStock.Sum(s => s.QuantityOnHand - s.QuantityReserved - s.AllocatedQuantity);

                Console.WriteLine($"[WO]   StockItems con cantidad libre > 0: {availableStock.Count}");
                Console.WriteLine($"[WO]   Suma OnHand={totalOnHand} | Suma Libre={totalFree}");

                foreach (var stock in availableStock)
                {
                    if (quantityAllocated >= totalRequired) break;

                    decimal freeInThisItem = stock.QuantityOnHand - stock.QuantityReserved - stock.AllocatedQuantity;
                    decimal missingQuantity = totalRequired - quantityAllocated;
                    decimal quantityToTake = Math.Min(freeInThisItem, missingQuantity);

                    Console.WriteLine($"[WO]   -> Stock {stock.Id}: libre={freeInThisItem} | faltan={missingQuantity} | toTake={quantityToTake}");

                    if (quantityToTake <= 0)
                    {
                        Console.WriteLine($"[WO]      SKIP (toTake <= 0)");
                        continue;
                    }

                    var pickTask = new ProductionPickTask
                    {
                        Id = Guid.NewGuid(),
                        WorkOrderId = workOrder.Id,
                        MaterialId = ingredient.MaterialId,
                        SourceStockItemId = stock.Id,
                        RequiredQuantity = quantityToTake,
                        Status = PickTaskStatus.Pending
                    };

                    _context.ProductionPickTask.Add(pickTask);
                    stock.AllocatedQuantity += quantityToTake;
                    quantityAllocated += quantityToTake;

                    Console.WriteLine($"[WO]      PickTask OK. Acumulado={quantityAllocated}/{totalRequired}");
                }

                Console.WriteLine($"[WO]   RESULTADO FINAL: {quantityAllocated} / {totalRequired}");

                if (quantityAllocated < totalRequired)
                {
                    Console.WriteLine($"[WO] *** FALLO: Stock insuficiente para MaterialId={ingredient.MaterialId} ***");
                    Console.WriteLine("==================================================");
                    throw new Exception(
                        $"Stock insuficiente para MaterialId={ingredient.MaterialId}. " +
                        $"Requerido: {totalRequired:F4} | Libre: {totalFree:F4} | Físico total: {totalOnHand:F4}. " +
                        $"Revisa si hay órdenes previas que bloquean el inventario o si el material " +
                        $"del stock coincide con el de la receta.");
                }
            }

            Console.WriteLine($"[WO] Todos los ingredientes asignados. Guardando WO...");
            Console.WriteLine("==================================================");

            await _context.SaveChangesAsync(cancellationToken);
            return workOrder.Id;
        }
    }
}
