using FluentValidation;
using Inventory.Application.Materials.Commands;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Commons.Validators
{
    public class UpdateMaterialValidator : AbstractValidator<UpdateMaterialCommand>
    {
        public UpdateMaterialValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("El ID del material es obligatorio.");

			RuleFor(v => v.Name)
				 .NotEmpty().WithMessage("El nombre es obligatorio.")
				 .MaximumLength(200);

			RuleFor(v => v.SKU)
				.NotEmpty().WithMessage("El SKU es obligatorio.")
				.MaximumLength(50);

			RuleFor(v => v.Type)
				.IsInEnum().WithMessage("Tipo de material inválido.");

			RuleFor(v => v.UnitOfMeasureId)
				.NotEmpty().WithMessage("La unidad de medida es obligatoria.");

			RuleFor(v => v.CategoryId)
				.NotEmpty().WithMessage("La categoría es obligatoria.");

			// Validar que no sean negativos
			RuleFor(v => v.ReorderPoint).GreaterThanOrEqualTo(0);
			RuleFor(v => v.TargetStock).GreaterThanOrEqualTo(0);
			RuleFor(v => v.StandardCost).GreaterThanOrEqualTo(0);

			// --- Reglas Condicionales (Lo que faltaba) ---

			// 1. Si enviaron BarCode, validar su longitud
			When(x => !string.IsNullOrEmpty(x.BarCode), () =>
			{
				RuleFor(x => x.BarCode).MaximumLength(100);
			});

            // 3. Apilamiento: No puedes tener 0 capas. Si existe, mínimo ocupa 1 capa (el piso).
            RuleFor(v => v.MaxStackingLayers)
                .GreaterThanOrEqualTo(1).WithMessage("El máximo de capas de apilamiento debe ser al menos 1.");

            // 4. Paradoja Térmica: La máxima no puede ser menor a la mínima
            When(x => x.MinTemperatureCelsius.HasValue && x.MaxTemperatureCelsius.HasValue, () =>
            {
                RuleFor(x => x.MaxTemperatureCelsius)
                    .GreaterThanOrEqualTo(x => x.MinTemperatureCelsius)
                    .WithMessage("La temperatura máxima no puede ser menor a la temperatura mínima.");
            });

            // 5. Integridad de los Tags (Hazmat)
            When(x => x.HazmatTags != null && x.HazmatTags.Any(), () =>
            {
                RuleForEach(x => x.HazmatTags)
                    .NotEmpty().WithMessage("Las etiquetas de seguridad no pueden contener valores vacíos.")
                    .MaximumLength(50).WithMessage("Cada etiqueta de seguridad no debe exceder los 50 caracteres.");
            });

            // 2. Si es PRODUCTO TERMINADO, exigimos Precio de Venta
            When(x => x.Type == MaterialType.FinishedGood, () =>
			{
				RuleFor(x => x.SalesPrice)
					.NotNull().WithMessage("El precio de venta es obligatorio para productos terminados.")
					.GreaterThan(0).WithMessage("El precio de venta debe ser mayor a 0.");
			});
		}
    }
}
