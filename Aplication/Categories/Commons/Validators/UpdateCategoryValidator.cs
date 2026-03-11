using FluentValidation;
using Inventory.Application.Categories.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Commons.Validators
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }
}
