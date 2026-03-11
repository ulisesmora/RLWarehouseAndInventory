using FluentValidation;
using Inventory.Application.StorageBins.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Commons.Validators
{
    public class UpdateStorageBinValidator : AbstractValidator<UpdateStorageBinCommand>
    {
        public UpdateStorageBinValidator()
        {
            RuleFor(v => v.Id).NotEmpty();
            RuleFor(v => v.Code).NotEmpty().MaximumLength(50);
            RuleFor(v => v.MaxWeight).GreaterThanOrEqualTo(0);
            RuleFor(v => v.MaxVolume).GreaterThanOrEqualTo(0);
        }
    }
}
