using FluentValidation;
using Inventory.Application.StockMovements.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commons.Validators
{
    public class ReceiveStockValidator : AbstractValidator<ReceiveStockCommand>
    {
        public ReceiveStockValidator()
        {
            // 1. Validar GUIDs obligatorios (Que no vengan vacíos ni como "00000000-0000-0000-0000-000000000000")
            RuleFor(v => v.MaterialId).NotEmpty().WithMessage("El Material es obligatorio.");
            RuleFor(v => v.WarehouseId).NotEmpty().WithMessage("El Almacén destino es obligatorio.");
            RuleFor(v => v.StatusId).NotEmpty().WithMessage("Debe asignar un Estado (Status) a la mercancía.");
            RuleFor(v => v.UserId).NotEmpty().WithMessage("El ID del Usuario es obligatorio para la auditoría.");

            // 2. Regla de negocio: Entradas siempre > 0
            RuleFor(v => v.Quantity)
                .GreaterThan(0).WithMessage("La cantidad a recibir debe ser mayor a cero. No se permiten entradas negativas o en cero.");

            // 3. Proteger la Base de Datos (Longitudes de texto)
            RuleFor(v => v.ReferenceNumber)
                .MaximumLength(50).WithMessage("El número de referencia no puede exceder los 50 caracteres.");

            RuleFor(v => v.Comments)
                .MaximumLength(500).WithMessage("Los comentarios no pueden exceder los 500 caracteres.");
        }
    }
}
