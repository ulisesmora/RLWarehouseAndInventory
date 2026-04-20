using FluentValidation;
using Inventory.Application.ProductRecipes.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Commons.Validators
{
    public class CreateProductRecipeValidator : AbstractValidator<CreateProductRecipeCommand>
    {
        public CreateProductRecipeValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("El nombre de la receta es obligatorio.")
                .MaximumLength(200).WithMessage("El nombre no puede exceder los 200 caracteres.");

            RuleFor(v => v.FinishedGoodId)
                .NotEmpty().WithMessage("Debe especificar qué material se va a producir (FinishedGoodId).");

            RuleFor(v => v.Ingredients)
                .NotEmpty().WithMessage("La receta debe tener al menos un ingrediente.");

            RuleForEach(v => v.Ingredients).SetValidator(new CreateRecipeIngredientValidator());
            RuleForEach(v => v.AdditionalCosts).SetValidator(new CreateRecipeCostValidator());
        }
    }

    public class CreateRecipeIngredientValidator : AbstractValidator<CreateRecipeIngredientDto>
    {
        public CreateRecipeIngredientValidator()
        {
            RuleFor(i => i.MaterialId).NotEmpty().WithMessage("El ID del material es obligatorio.");
            RuleFor(i => i.QuantityRequired).GreaterThan(0).WithMessage("La cantidad del ingrediente debe ser mayor a 0.");
        }
    }

    public class CreateRecipeCostValidator : AbstractValidator<CreateRecipeCostDto>
    {
        public CreateRecipeCostValidator()
        {
            RuleFor(c => c.Description).NotEmpty().WithMessage("La descripción del costo es obligatoria.");
            RuleFor(c => c.EstimatedCost).GreaterThanOrEqualTo(0).WithMessage("El costo estimado no puede ser negativo.");
        }
    }
}
