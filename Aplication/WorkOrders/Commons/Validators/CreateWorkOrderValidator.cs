using FluentValidation;
using Inventory.Application.WorkOrders.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Commons.Validators
{
    public class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderCommand>
    {
        public CreateWorkOrderValidator()
        {
            RuleFor(v => v.ProductRecipeId).NotEmpty().WithMessage("Debe seleccionar una receta.");
            RuleFor(v => v.PlannedQuantity).GreaterThan(0).WithMessage("La cantidad a producir debe ser mayor a cero.");
            RuleFor(v => v.PlannedStartDate).NotEmpty().WithMessage("Debe definir una fecha de inicio planeada.");
        }
    }
}
