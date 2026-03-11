using FluentValidation;
using Inventory.Application.StatusCatalogs.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StatusCatalogs.Commons.Validator
{
    public class CreateStatusCatalogValidator : AbstractValidator<CreateStatusCatalogCommand>
    {
        public CreateStatusCatalogValidator()
        {
            RuleFor(v => v.Name).NotEmpty().MaximumLength(50);
            RuleFor(v => v.Description).MaximumLength(200);
        }
    }
}
