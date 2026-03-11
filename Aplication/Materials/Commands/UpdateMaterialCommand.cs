using Inventory.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Commands
{
    public record UpdateMaterialCommand(
		Guid Id,
		string Name,
		string SKU,
		MaterialType Type,
		Guid UnitOfMeasureId,
		Guid CategoryId,

		// Opcionales (Nullable)
		string? Description,
		string? BarCode,
		decimal? Weight,
		decimal? Volume,

		// Numéricos (Pueden ser 0)
		decimal ReorderPoint,
		decimal TargetStock,
		decimal StandardCost,
		decimal? SalesPrice
	) : IRequest<Unit>;
}
