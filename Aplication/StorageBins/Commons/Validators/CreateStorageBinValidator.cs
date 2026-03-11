using FluentValidation;
using Inventory.Application.StorageBins.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Commons.Validators
{
    public class CreateStorageBinValidator : AbstractValidator<CreateStorageBinCommand>
    {
        public CreateStorageBinValidator()
        {
            RuleFor(v => v.ZoneId).NotEmpty();

            RuleFor(v => v.Code)
                .NotEmpty().WithMessage("El código de ubicación es obligatorio.")
                .MaximumLength(50);

            // Validamos que no pongan capacidades negativas
            RuleFor(v => v.MaxWeight).GreaterThanOrEqualTo(0);
            RuleFor(v => v.MaxVolume).GreaterThanOrEqualTo(0);

            RuleFor(v => v.Width).GreaterThanOrEqualTo(0);
            RuleFor(v => v.Depth).GreaterThanOrEqualTo(0);
            RuleFor(v => v.Height).GreaterThanOrEqualTo(0);


        }
    }
}
