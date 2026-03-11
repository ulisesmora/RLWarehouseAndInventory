using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Commands
{
    public record CreateStorageBinCommand(
        Guid ZoneId, // Obligatorio: ¿En qué habitación está?
        string Code, // El código de etiqueta
        string? Description,

        // Coordenadas 3D
        double PositionX,
        double PositionY,
        double PositionZ,

           double Width,  // Ancho
     double Depth ,  // Profundidad
     double Height , // Altura


         double physicalOffsetX,
     double physicalOffsetZ,
     double rotation,

    // Límites físicos (Opcionales, default 0 si no se controlan)
    decimal MaxWeight,
        decimal MaxVolume
    ) : IRequest<Guid>;
}
