using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Commands
{
    public record UpdateStorageBinCommand(
        Guid Id,
        string Code, // Ej: Cambiar "A-01" a "B-01"
        string? Description,

        // Coordenadas (por si mueven el estante)
        double PositionX,
        double PositionY,
        double PositionZ,
          double Width,  // Ancho
     double Depth,  // Profundidad
     double Height,

         double physicalOffsetX,
     double physicalOffsetZ,
     double rotation,

        // Nuevos límites
        decimal MaxWeight,
        decimal MaxVolume
    ) : IRequest;
}
