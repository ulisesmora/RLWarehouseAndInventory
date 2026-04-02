using FluentValidation;
using Inventory.Application.Users.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Commons.Validators
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(v => v.FullName).NotEmpty().MaximumLength(150);
            RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(150);
            RuleFor(v => v.Password).NotEmpty().MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
            RuleFor(v => v.Role).IsInEnum();
        }
    }
}
