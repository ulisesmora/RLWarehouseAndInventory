using FluentValidation;
using Inventory.Application.Zones.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Commons.Validators
{
    public class CreateZoneValidator : AbstractValidator<CreateZoneCommand>
    {
        public CreateZoneValidator()
        {
            RuleFor(v => v.WarehouseId)
                .NotEmpty().WithMessage("Debes especificar el almacén.");

            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("El nombre de la zona es obligatorio.")
                .MaximumLength(100);

            // Validar dimensiones para evitar zonas negativas o de tamaño 0
            RuleFor(v => v.Width).GreaterThan(0).WithMessage("El ancho debe ser mayor a 0.");
            RuleFor(v => v.Depth).GreaterThan(0).WithMessage("La profundidad debe ser mayor a 0.");
            RuleFor(v => v.Height).GreaterThan(0).WithMessage("La altura debe ser mayor a 0.");
        }
    }
}
