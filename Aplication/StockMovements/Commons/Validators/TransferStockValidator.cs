using FluentValidation;
using Inventory.Application.StockMovements.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commons.Validators
{
    public class TransferStockValidator : AbstractValidator<TransferStockCommand>
    {
        public TransferStockValidator()
        {
            RuleFor(v => v.Quantity).GreaterThan(0).WithMessage("La cantidad a transferir debe ser mayor a cero.");
            RuleFor(v => v.SourceWarehouseId).NotEmpty();
            RuleFor(v => v.DestinationWarehouseId).NotEmpty();

            // Regla de negocio: El origen y destino no pueden ser exactamente iguales
            RuleFor(v => v)
                .Must(v => !(v.SourceWarehouseId == v.DestinationWarehouseId && v.SourceStorageBinId == v.DestinationStorageBinId))
                .WithMessage("La ubicación de origen y destino no pueden ser la misma.");
        }
    }
}
