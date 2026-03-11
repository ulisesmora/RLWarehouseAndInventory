using FluentValidation;
using Inventory.Application.UnitOfMesaure.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Commons.Validators
{
    public class CreateUnitOfMeasureValidator : AbstractValidator<CreateUnitOfMeasureCommand>
    {
        public CreateUnitOfMeasureValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(10);
            RuleFor(x => x.ConversionFactor).GreaterThan(0);
        }
    }
}
