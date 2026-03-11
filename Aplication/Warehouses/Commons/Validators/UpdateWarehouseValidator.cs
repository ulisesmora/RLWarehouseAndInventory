using FluentValidation;
using Inventory.Application.Warehouses.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Commons.Validators
{
    public class UpdateWarehouseValidator : AbstractValidator<UpdateWarehouseCommand>
    {
        public UpdateWarehouseValidator()
        {
            RuleFor(v => v.Id).NotEmpty();
            RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
            RuleFor(v => v.Location).MaximumLength(200);
        }
    }
}
