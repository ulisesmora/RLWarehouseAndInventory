using FluentValidation;
using Inventory.Application.Materials.Commands;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Commons.Validators
{
    public class CreateMaterialValidator : AbstractValidator<CreateMaterialCommand>
    {
        public CreateMaterialValidator()
        {
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
