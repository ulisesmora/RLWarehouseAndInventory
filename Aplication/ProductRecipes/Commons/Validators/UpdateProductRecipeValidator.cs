using FluentValidation;
using Inventory.Application.ProductRecipes.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Commons.Validators
{
    public class UpdateProductRecipeValidator : AbstractValidator<UpdateProductRecipeCommand>
    {
        public UpdateProductRecipeValidator()
        {
            RuleFor(v => v.Id).NotEmpty().WithMessage("El ID de la receta es obligatorio.");
            RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
            RuleFor(v => v.FinishedGoodId).NotEmpty();
            RuleFor(v => v.Ingredients).NotEmpty().WithMessage("La receta debe tener al menos un ingrediente.");

            RuleForEach(v => v.Ingredients).ChildRules(ingredients =>
            {
                ingredients.RuleFor(i => i.MaterialId).NotEmpty();
                ingredients.RuleFor(i => i.QuantityRequired).GreaterThan(0);
            });
        }
    }
}
