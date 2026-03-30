using Inventory.Application.StockMovements.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static iTextSharp.text.pdf.AcroFields;

namespace Inventory.Application.StockMovements.Handlers
{
    public class TransferStockCommandHandler : IRequestHandler<TransferStockCommand>
    {
        private readonly InventoryDbContext _context;

        public TransferStockCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(TransferStockCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscamos la caja
            var stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s => s.Id == request.StockItemId, cancellationToken);

            if (stockItem == null) throw new InvalidOperationException("No se encontró el contenedor.");

            // 2. 🔥 Buscamos el estante destino CON SUS CAJAS ACTUALES para verificar el espacio
            var destinationBin = await _context.StorageBins
                .Include(b => b.StockItems)
                .FirstOrDefaultAsync(b => b.Id == request.DestinationStorageBinId, cancellationToken);

            if (destinationBin == null) throw new InvalidOperationException("Estante destino no encontrado.");


            var itemWidthM = Convert.ToDouble((stockItem.WidthCm) / 100m);
            var itemHeightM = Convert.ToDouble((stockItem.HeightCm) / 100m);
            var itemDepthM = Convert.ToDouble((stockItem.LengthCm) / 100m);

            if (itemWidthM > (destinationBin.Width) || itemHeightM > destinationBin.Height || itemDepthM > destinationBin.Depth)
            {
                // OJO: Aquí podrías agregar lógica para ver si rotando la caja (Width x Depth) sí entra, 
                // pero por ahora hacemos validación estricta.
                throw new InvalidOperationException($"La caja {stockItem.ReferenceNumber} es demasiado grande físicamente para el estante {destinationBin.Code}.");
                // (En el BulkTransfer, esto se iría a la lista de failedItemReferences)
            }

            // 3. 🔥 VALIDACIÓN MATEMÁTICA ATÓMICA
            var currentWeight = destinationBin.StockItems.Sum(s => s.WeightKg ?? 0);
            var currentVolumeM3 = destinationBin.StockItems.Sum(s => ((s.LengthCm ?? 0) * (s.WidthCm ?? 0) * (s.HeightCm ?? 0)) / 1000000m);

            var itemVolumeM3 = ((stockItem.LengthCm ?? 0) * (stockItem.WidthCm ?? 0) * (stockItem.HeightCm ?? 0)) / 1000000m;
            var itemWeight = stockItem.WeightKg ?? 0;

            var binMaxVolumeM3 = destinationBin.MaxVolume / 1000000m;

            if ((currentWeight + itemWeight) > destinationBin.MaxWeight)
                throw new InvalidOperationException($"Excede el peso máximo del estante {destinationBin.Code}.");

            if ((currentVolumeM3 + itemVolumeM3) > binMaxVolumeM3)
                throw new InvalidOperationException($"Excede el volumen máximo del estante {destinationBin.Code}.");

            // 4. TRASLADO FÍSICO
            var sourceWarehouseId = stockItem.WarehouseId;
            var sourceBinId = stockItem.StorageBinId;

            stockItem.WarehouseId = request.DestinationWarehouseId;
            stockItem.StorageBinId = destinationBin.Id;


            var dateNow = DateTime.UtcNow;

            // Historial: Movimiento de Salida (De donde estaba)
            var outMovement = new StockMovement
            {
                StockItemId = stockItem.Id,
                MaterialId = stockItem.MaterialId,
                WarehouseId = sourceWarehouseId,
                StorageBinId = sourceBinId,
                LotId = stockItem.LotId,

                Type = MovementType.Transfer,
                Quantity = -stockItem.QuantityOnHand, // Negativo en el origen

                MovementDate = dateNow,
                ReferenceNumber = stockItem.ReferenceNumber,
                Comments = "Salida por reubicación manual (3D)",
                UserId = request.UserId
            };

            // Historial: Movimiento de Entrada (A donde llegó)
            var inMovement = new StockMovement
            {
                StockItemId = stockItem.Id,
                MaterialId = stockItem.MaterialId,
                WarehouseId = request.DestinationWarehouseId,
                StorageBinId = destinationBin.Id,
                LotId = stockItem.LotId,

                Type = MovementType.Transfer,
                Quantity = stockItem.QuantityOnHand, // Positivo en destino

                MovementDate = dateNow,
                ReferenceNumber = stockItem.ReferenceNumber,
                Comments = request.Comments ?? "Entrada por reubicación manual (3D)",
                UserId = request.UserId
            };

            _context.StockMovements.Add(outMovement);
            _context.StockMovements.Add(inMovement);


            // 5. 🔥 EL TRUCO DEL OPTIMISTIC LOCKING 🔥
            // Como solo modificamos StockItem, EF Core no sabe que el StorageBin cambió lógicamente.
            // Forzamos a EF Core a verificar el RowVersion del estante destino:
            _context.Entry(destinationBin).State = EntityState.Modified;

            // 6. GUARDAR CON PROTECCIÓN DE CONCURRENCIA
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // 🚨 CONDICIÓN DE CARRERA DETECTADA 🚨
                // Otro manager metió una caja milisegundos antes que nosotros y cambió el RowVersion.
                throw new InvalidOperationException("El estante acaba de ser modificado por otro usuario. Por favor, recarga el mapa de la zona y reintenta la asignación.");
            }
        }
    }
}
