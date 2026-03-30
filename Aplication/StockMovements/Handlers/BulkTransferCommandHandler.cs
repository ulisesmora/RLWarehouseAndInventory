using Inventory.Application.StockMovements.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Handlers
{
        public class BulkTransferCommandHandler(InventoryDbContext context)
        : IRequestHandler<BulkTransferCommand, BulkTransferResult>
        {
            public async Task<BulkTransferResult> Handle(BulkTransferCommand request, CancellationToken cancellationToken)
            {
                int successfulCount = 0;
                List<string> failedItemReferences = [];

                // 1. Traemos TODOS los estantes involucrados de un solo golpe
                var binIds = request.ItemsToMove.Select(i => i.DestinationStorageBinId).Distinct().ToList();
                var destinationBins = await context.StorageBins
                    .Include(b => b.StockItems)
                    .Where(b => binIds.Contains(b.Id))
                    .ToDictionaryAsync(b => b.Id, cancellationToken);

                // 2. Traemos TODAS las cajas involucradas
                var itemIds = request.ItemsToMove.Select(i => i.StockItemId).ToList();
                var stockItems = await context.StockItems
                    .Where(s => itemIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, cancellationToken);

                var dateNow = DateTime.UtcNow;

                // 3. Iteramos las peticiones en memoria
                foreach (var move in request.ItemsToMove)
                {
                    if (!stockItems.TryGetValue(move.StockItemId, out var item) ||
                        !destinationBins.TryGetValue(move.DestinationStorageBinId, out var bin))
                    {
                        failedItemReferences.Add($"Error ID: {move.StockItemId} (No encontrado)");
                        continue;
                    }

                    // 🔥 1. VALIDACIÓN GEOMÉTRICA (Faltaba en tu código)
                    var itemWidthM = Convert.ToDouble((item.WidthCm ?? 0) / 100m);
                    var itemHeightM = Convert.ToDouble((item.HeightCm ?? 0) / 100m);
                    var itemDepthM = Convert.ToDouble((item.LengthCm ?? 0) / 100m);

                    if (itemWidthM > bin.Width || itemHeightM > bin.Height || itemDepthM > bin.Depth)
                    {
                        failedItemReferences.Add(item.ReferenceNumber); // No cabe físicamente
                        continue;
                    }

                    // 🔥 2. VALIDACIÓN DE ESPACIO Y PESO (Con la corrección de M3)
                    var currentWeight = bin.StockItems.Sum(s => s.WeightKg ?? 0);
                    var currentVolume = bin.StockItems.Sum(s => ((s.LengthCm ?? 0) * (s.WidthCm ?? 0) * (s.HeightCm ?? 0)) / 1000000m);

                    var itemVolume = ((item.LengthCm ?? 0) * (item.WidthCm ?? 0) * (item.HeightCm ?? 0)) / 1000000m;
                    var itemWeight = item.WeightKg ?? 0;

                    var binMaxVolumeM3 = bin.MaxVolume / 1000000m; // Convertimos cm3 a m3

                    if ((currentWeight + itemWeight) > bin.MaxWeight || (currentVolume + itemVolume) > binMaxVolumeM3)
                    {
                        failedItemReferences.Add(item.ReferenceNumber); // Excede peso/volumen
                        continue;
                    }

                    // ==========================================================
                    // 🔥 3. TRASLADO FÍSICO Y CREACIÓN DEL HISTORIAL DE AUDITORÍA
                    // ==========================================================
                    var sourceWarehouseId = item.WarehouseId;
                    var sourceBinId = item.StorageBinId;

                    // Actualizamos la caja
                    item.StorageBinId = bin.Id;
                    item.WarehouseId = bin.WarehouseId;

                    // Movimiento de Salida (Negativo)
                    context.StockMovements.Add(new StockMovement
                    {
                        StockItemId = item.Id,
                        MaterialId = item.MaterialId,
                        WarehouseId = sourceWarehouseId,
                        StorageBinId = sourceBinId,
                        LotId = item.LotId,
                        Type = MovementType.Transfer,
                        Quantity = -item.QuantityOnHand,
                        MovementDate = dateNow,
                        ReferenceNumber = item.ReferenceNumber,
                        Comments = "Salida por auto-asignación masiva (WMS AI)",
                        UserId = request.UserId
                    });

                    // Movimiento de Entrada (Positivo)
                    context.StockMovements.Add(new StockMovement
                    {
                        StockItemId = item.Id,
                        MaterialId = item.MaterialId,
                        WarehouseId = bin.WarehouseId,
                        StorageBinId = bin.Id,
                        LotId = item.LotId,
                        Type = MovementType.Transfer,
                        Quantity = item.QuantityOnHand,
                        MovementDate = dateNow,
                        ReferenceNumber = item.ReferenceNumber,
                        Comments = "Entrada por auto-asignación masiva (WMS AI)",
                        UserId = request.UserId
                    });

                    // Añadimos la caja a la colección en memoria del Bin para la siguiente iteración
                    bin.StockItems.Add(item);

                    // Optmistic Locking
                    context.Entry(bin).State = EntityState.Modified;
                    successfulCount++;
                }

                // 4. Guardamos TODAS las exitosas + su historial en bloque
                try
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException("Hubo un conflicto de concurrencia en la zona. Algunos estantes fueron modificados por otro usuario. Por favor, recarga y reintenta.");
                }

                // 5. Retornamos
                return new BulkTransferResult(successfulCount, failedItemReferences);
            }
        }
}
