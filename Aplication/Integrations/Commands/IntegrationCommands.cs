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

    /// <summary>Sincroniza el catálogo de productos del canal externo creando/actualizando ChannelProductMappings.</summary>
    public record SyncProductsCommand(SalesChannel Channel) : IRequest<SyncProductsResultDto>;

    /// <summary>
    /// Procesa un pedido individual recibido por webhook.
    /// El StoreIdentifier se usa para localizar el ChannelConfig (y por ende el tenant).
    /// </summary>
    public record ProcessWebhookOrderCommand(
        SalesChannel Channel,
        string       StoreIdentifier,   // dominio de la tienda p.ej. mystore.myshopify.com
        string       RawPayload,        // body original para auditoría
        string       ExtOrderId,
        string       ExtOrderNum,
        string       CustomerName,
        string       CustomerEmail,
        string       ShippingAddress,
        DateTime     CreatedAt,
        string       LineItemsJson      // serializado para no exponer tipos externos
    ) : IRequest<Unit>;
}

public class SyncProductsResultDto
{
    public int Created  { get; set; }
    public int Updated  { get; set; }
    public int Matched  { get; set; }
    public string Message { get; set; } = string.Empty;
}
