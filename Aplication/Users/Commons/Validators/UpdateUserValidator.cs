using FluentValidation;
using Inventory.Application.Users.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Commons.Validators
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(v => v.Id).NotEmpty();
            RuleFor(v => v.FullName).NotEmpty().MaximumLength(150);
            RuleFor(v => v.Role).IsInEnum();
        }
    }
}
