using FluentValidation;
using Inventory.Application.Lots.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Commons.Validators
{
    public class CreateLotValidator : AbstractValidator<CreateLotCommand>
    {
        public CreateLotValidator()
        {
            RuleFor(v => v.MaterialId)
                 .NotEmpty().WithMessage("El material es obligatorio.");

            RuleFor(v => v.LotNumber)
                .NotEmpty().WithMessage("El código del lote interno es obligatorio.")
                .MaximumLength(50).WithMessage("El código no puede exceder los 50 caracteres.");

           
            RuleFor(v => v.VendorBatchNumber)
                .MaximumLength(100).WithMessage("El código del lote del proveedor es demasiado largo.");

            RuleFor(v => v.InitialReceivedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("La cantidad inicial no puede ser negativa.");

            When(x => x.ManufacturingDate.HasValue && x.ExpirationDate.HasValue, () =>
            {
                RuleFor(x => x.ExpirationDate)
                    .GreaterThan(x => x.ManufacturingDate)
                    .WithMessage("La fecha de expiración debe ser posterior a la de fabricación.");
            });
        }
    }
}
