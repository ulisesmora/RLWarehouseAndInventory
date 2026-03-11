using FluentValidation;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commons.Validators
{
    public class IssueStockValidator : AbstractValidator<IssueStockCommand>
    {
        public IssueStockValidator()
        {
            RuleFor(v => v.MaterialId).NotEmpty();
            RuleFor(v => v.WarehouseId).NotEmpty();
            RuleFor(v => v.StatusId).NotEmpty();
            RuleFor(v => v.UserId).NotEmpty();

            // La cantidad a sacar siempre debe venir como número positivo en el request
            RuleFor(v => v.Quantity)
                .GreaterThan(0).WithMessage("La cantidad a despachar debe ser mayor a cero.");

            // Solo permitimos tipos de movimiento que sean de SALIDA
            RuleFor(v => v.Type)
                .Must(t => t == MovementType.SalesShipment || t == MovementType.ManufacturingUse)
                .WithMessage("El tipo de movimiento debe ser una salida válida (Venta o Manufactura).");
        }
    }
}
