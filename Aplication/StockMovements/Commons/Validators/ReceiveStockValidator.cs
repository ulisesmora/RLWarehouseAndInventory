using FluentValidation;
using Inventory.Application.StockMovements.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commons.Validators
{
    public class ReceiveStockBatchItemValidator : AbstractValidator<ReceiveStockBatchItem>
    {
        public ReceiveStockBatchItemValidator()
        {
            RuleFor(v => v.MaterialId)
                .NotEmpty().WithMessage("El Material es obligatorio en cada contenedor.");

            RuleFor(v => v.WarehouseId)
                .NotEmpty().WithMessage("El Almacén destino es obligatorio en cada contenedor.");

            RuleFor(v => v.StatusId)
                .NotEmpty().WithMessage("Debe asignar un Estado (Status) a cada contenedor.");

            RuleFor(v => v.Quantity)
                .GreaterThan(0).WithMessage("La cantidad a recibir debe ser mayor a cero en todos los contenedores.");

            RuleFor(v => v.ReferenceNumber)
                .MaximumLength(50).WithMessage("El número de referencia/LPN no puede exceder los 50 caracteres.");

            RuleFor(v => v.Comments)
                .MaximumLength(500).WithMessage("Los comentarios no pueden exceder los 500 caracteres.");
        }
    }

    // 2. VALIDADOR PADRE: Valida el Comando general que llega al endpoint
    public class ReceiveStockValidator : AbstractValidator<ReceiveStockCommand>
    {
        public ReceiveStockValidator()
        {
            // Validamos que el usuario esté presente

            // Validamos que la lista no venga nula ni vacía
            RuleFor(v => v.Items)
                .NotEmpty().WithMessage("Debe enviar al menos un contenedor para recibir inventario.");

            // 🔥 LA MAGIA: Aplicamos el validador hijo a cada elemento de la lista
            RuleForEach(v => v.Items)
                .SetValidator(new ReceiveStockBatchItemValidator());
        }
    }
}
