using FluentValidation;
using Inventory.Application.Suppliers.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Suppliers.Commons.Validators
{
    public class CreateSupplierValidator : AbstractValidator<CreateSupplierCommand>
    {
        public CreateSupplierValidator()
        {
            RuleFor(v => v.Name).NotEmpty().MaximumLength(200);

            // Si mandan email, validamos que tenga formato correcto
            RuleFor(v => v.Email).EmailAddress().When(v => !string.IsNullOrEmpty(v.Email));
        }
    }
}
