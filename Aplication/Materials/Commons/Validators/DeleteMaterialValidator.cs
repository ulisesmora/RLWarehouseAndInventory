using FluentValidation;
using Inventory.Application.Materials.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Commons.Validators
{
    public class DeleteMaterialValidator : AbstractValidator<DeleteMaterialCommand>
    {
        public DeleteMaterialValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("El ID del material es obligatorio.");
        }
    }
}
