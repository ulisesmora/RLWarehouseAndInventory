using FluentValidation;
using Inventory.Application.SalesOrders.Commands;

namespace Inventory.Application.SalesOrders.Commons.Validators
{
    public class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderCommand>
    {
        public CreateSalesOrderValidator()
        {
            RuleFor(v => v.CustomerName)
                .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
                .MaximumLength(200).WithMessage("El nombre del cliente no puede superar los 200 caracteres.");

            RuleFor(v => v.CustomerEmail)
                .EmailAddress().WithMessage("El email del cliente no tiene un formato válido.")
                .When(v => !string.IsNullOrWhiteSpace(v.CustomerEmail));

            RuleFor(v => v.SourceChannel)
                .IsInEnum().WithMessage("El canal de venta indicado no es válido.");

            RuleFor(v => v.Lines)
                .NotEmpty().WithMessage("El pedido debe tener al menos una línea de producto.");

            RuleForEach(v => v.Lines).ChildRules(line =>
            {
                line.RuleFor(l => l.MaterialId)
                    .NotEmpty().WithMessage("Cada línea debe tener un material válido.");

                line.RuleFor(l => l.OrderedQuantity)
                    .GreaterThan(0).WithMessage("La cantidad pedida debe ser mayor a cero.");

                line.RuleFor(l => l.UnitPrice)
                    .GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo.");
            });
        }
    }
}
