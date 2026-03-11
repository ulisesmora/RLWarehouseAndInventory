using FluentValidation;
using Inventory.Application.SupplierMaterials.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Commons.Validators
{
    public class CreateSupplierMaterialValidator : AbstractValidator<CreateSupplierMaterialCommand>
    {
        public CreateSupplierMaterialValidator()
        {
            RuleFor(v => v.SupplierId).NotEmpty();
            RuleFor(v => v.MaterialId).NotEmpty();
            RuleFor(v => v.UnitCost).GreaterThanOrEqualTo(0);
            RuleFor(v => v.MinimumOrderQuantity).GreaterThan(0);
            RuleFor(v => v.LeadTimeDays).GreaterThanOrEqualTo(0);
            RuleFor(v => v.Currency).NotEmpty().Length(3); // Ej: USD, EUR, MXN
        }
    }
}
