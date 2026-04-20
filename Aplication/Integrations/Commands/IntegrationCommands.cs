using Inventory.Application.Integrations.Queries;
using Inventory.Domain;
using MediatR;
using System;

namespace Inventory.Application.Integrations.Commands
{
    /// <summary>Guarda credenciales y verifica la conexión con el canal externo.</summary>
    public record ConnectChannelCommand(
        SalesChannel Channel,
        string StoreUrl,
        string ApiKey,
        string ApiSecret
    ) : IRequest<ChannelStatusDto>;

    /// <summary>Elimina credenciales y marca el canal como desconectado.</summary>
    public record DisconnectChannelCommand(SalesChannel Channel) : IRequest<Unit>;

    /// <summary>
    /// Importa pedidos desde el canal externo.
    /// Aplica auto-match de SKU → Material y ejecuta FEFO allocation en los pedidos mapeados.
    /// </summary>
    public record ImportOrdersCommand(
        SalesChannel Channel,
        int          MaxOrders = 100    // límite por lote
    ) : IRequest<ImportResultDto>;

    /// <summary>Asigna manualmente un ChannelProductMapping a un Material del WMS.</summary>
    public record MapProductCommand(
        Guid   MappingId,
        Guid   MaterialId
    ) : IRequest<Unit>;
}
