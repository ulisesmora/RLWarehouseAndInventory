using FluentValidation;
using Inventory.Application.Warehouses.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Commons.Validators
{
    public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseCommand>
    {
        public CreateWarehouseValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("El nombre del almacén es obligatorio.")
                .MaximumLength(100);

            RuleFor(v => v.Location)
                .MaximumLength(200);

            
        }
    }
}
