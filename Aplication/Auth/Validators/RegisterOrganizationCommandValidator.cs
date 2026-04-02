using FluentValidation;
using Inventory.Application.Auth.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Auth.Validators
{
    public class RegisterOrganizationCommandValidator : AbstractValidator<RegisterOrganizationCommand>
    {
        public RegisterOrganizationCommandValidator()
        {
            RuleFor(x => x.OrganizationName).NotEmpty().MinimumLength(3);
            RuleFor(x => x.FullName).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6)
                .WithMessage("El password debe tener al menos 6 caracteres.");
        }

    }
}
