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
    public class ReceiveStockCommandHandler : IRequestHandler<ReceiveStockCommand, bool>
    {
        private readonly InventoryDbContext _context;

        public ReceiveStockCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ReceiveStockCommand request, CancellationToken cancellationToken)
        {
            //  1. OPTIMIZACIÓN: Extraemos los IDs únicos para no consultar la BD 100 veces
            var materialIds = request.Items.Select(i => i.MaterialId).Distinct().ToList();
            var lotIds = request.Items.Where(i => i.LotId.HasValue).Select(i => i.LotId.Value).Distinct().ToList();

            //  2. DICCIONARIOS EN MEMORIA: Traemos los nombres reales
            var materialsCache = await _context.Materials
                .Where(m => materialIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.Name, cancellationToken);

            var lotsCache = await _context.Lots
                .Where(l => lotIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.LotNumber, cancellationToken);

            foreach (var item in request.Items)
            {
                var newStockItemId = Guid.NewGuid();

                // ------------------------------------------------------------------
                // 🎨 3. FABRICACIÓN DE LA MATRÍCULA SEMÁNTICA (LPN)
                // ------------------------------------------------------------------

                // A) Extraer las primeras 3 letras del Material (Ej: "Aceite" -> "ACE")
                var matName = materialsCache.ContainsKey(item.MaterialId) ? materialsCache[item.MaterialId] : "MAT";
                // Limpiamos espacios y tomamos 3 letras (si es muy corto, rellenamos con X)
                var matPrefix = matName.Trim().Length >= 3
                    ? matName.Trim().Substring(0, 3).ToUpper()
                    : matName.Trim().ToUpper().PadRight(3, 'X');

                // B) Extraer el Número de Lote (Ej: "L260310" -> "L260310")
                var lotNumber = "NOLOT";
                if (item.LotId.HasValue && lotsCache.ContainsKey(item.LotId.Value))
                {
                    lotNumber = lotsCache[item.LotId.Value];
                }

                // C) El sufijo único de seguridad (4 caracteres aleatorios del GUID)
                var uniqueSuffix = newStockItemId.ToString().Substring(0, 4).ToUpper();

                // FORMATO FINAL: LPN-[MATERIAL]-[LOTE]-[ID_UNICO]
                // Ejemplo Real: LPN-ACE-L260310-A9F2
                var generatedLpn = $"LPN-{matPrefix}-{lotNumber}-{uniqueSuffix}";

                // Si el Frontend mandó un temporal o viene vacío, usamos el nuestro.
                // Si mandaron un código real escaneado manualmente, lo respetamos.
                var finalLpn = string.IsNullOrWhiteSpace(item.ReferenceNumber) || item.ReferenceNumber.Contains("TEMP")
                    ? generatedLpn
                    : item.ReferenceNumber;

                // ------------------------------------------------------------------

                // 📦 4. CREAMOS LA CAJA FÍSICA (StockItem)
                var stockItem = new StockItem
                {
                    Id = newStockItemId,
                    MaterialId = item.MaterialId,
                    WarehouseId = item.WarehouseId,
                    StorageBinId = item.StorageBinId,
                    LotId = item.LotId,
                    StatusId = item.StatusId,
                    QuantityOnHand = item.Quantity,
                    QuantityReserved = 0,
                    ReferenceNumber = finalLpn, // <-- Matrícula Semántica asignada
                    Comments = item.Comments ?? string.Empty,
                    ContainerType = item.containerType,
                    LengthCm = item.LengthCm,
                    WidthCm = item.WidthCm,
                    HeightCm = item.HeightCm,
                    WeightKg = item.WeightKg,
                    IsStackable = item.IsStackable
                };

                _context.StockItems.Add(stockItem);

                // 📜 5. TABLA  DE MOVIMIENTOS  (StockMovement)
                var movement = new StockMovement
                {
                    StockItemId = newStockItemId,
                    MaterialId = item.MaterialId,
                    WarehouseId = item.WarehouseId,
                    StorageBinId = item.StorageBinId,
                    LotId = item.LotId,

                    Type = MovementType.PurchaseReception,
                    Quantity = item.Quantity,

                    MovementDate = DateTime.UtcNow,
                    ReferenceNumber = finalLpn, // <-- Guardamos la misma matrícula
                    Comments = string.IsNullOrWhiteSpace(item.Comments) ? "Generación de LPN" : item.Comments,
                    UserId = request.UserId
                };

                _context.StockMovements.Add(movement);
            }

            // 🛡️ 6. PRINCIPIO ACID: Todo o Nada
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
