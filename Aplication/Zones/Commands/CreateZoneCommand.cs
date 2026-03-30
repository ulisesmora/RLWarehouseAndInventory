using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Commands
{
    public record CreateZoneCommand(
        Guid WarehouseId, // Vital: ¿A qué edificio pertenece?
        string Name,
        string? Description,
        double Width,
        double Depth,
        double Height,
        bool AllowMixedLots,
        decimal? MinTemperatureCelsius,
        decimal? MaxTemperatureCelsius,
        List<string>? AllowedHazmatTags
    ) : IRequest<Guid>;
}
