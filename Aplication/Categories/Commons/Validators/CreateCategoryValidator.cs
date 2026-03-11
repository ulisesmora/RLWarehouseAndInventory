using FluentValidation;
using Inventory.Application.Categories.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Commons.Validators
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500);
        }
    }
}
