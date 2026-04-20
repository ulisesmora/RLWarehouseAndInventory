using MediatR;
using System.Collections.Generic;

namespace Inventory.Application.Integrations.Queries
{
    /// <summary>Devuelve el estado de todos los canales configurados para esta organización.</summary>
    public record GetChannelStatusQuery : IRequest<List<ChannelStatusDto>>;

    /// <summary>Devuelve los productos sin mapeo de material (pendientes de asignación manual).</summary>
    public record GetUnmappedProductsQuery(string? Channel = null)
        : IRequest<List<UnmappedProductDto>>;
}
