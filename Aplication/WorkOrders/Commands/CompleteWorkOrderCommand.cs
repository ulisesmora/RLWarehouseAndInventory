using MediatR;
using System;

namespace Inventory.Application.WorkOrders.Commands
{
    /// <summary>
    /// Cierra una orden de trabajo:
    ///   1. Lee las PickTasks confirmadas → genera registros de consumo automáticamente.
    ///   2. Descuenta el stock físico de cada StockItem involucrado.
    ///   3. Crea el Lote del producto terminado (listo para acomodarse con el flujo de Putaway existente).
    ///   4. Marca la WO como Completada.
    /// No requiere DestinationBin ni una lista de consumos manual.
    /// </summary>
    public record CompleteWorkOrderCommand(
        Guid WorkOrderId,
        decimal ActualFinishedGoodQuantity, // Unidades que realmente salieron de producción
        string? LotNumber                   // Número de lote del producto terminado (se auto-genera si es null)
    ) : IRequest<Guid>; // Devuelve el ID del nuevo Lote creado
}
