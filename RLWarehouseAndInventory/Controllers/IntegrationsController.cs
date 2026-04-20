using Inventory.Application.Integrations.Commands;
using Inventory.Application.Integrations.Queries;
using Inventory.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Inventory.API.Controllers
{
    /// <summary>
    /// Gestiona la conexión con canales externos (WooCommerce, Shopify) y la
    /// importación / mapeo de sus pedidos al WMS.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IntegrationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IntegrationsController(IMediator mediator)
            => _mediator = mediator;

        // ── GET /api/integrations/status ──────────────────────────────────────
        /// <summary>Devuelve el estado de todos los canales de venta soportados.</summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
            => Ok(await _mediator.Send(new GetChannelStatusQuery()));

        // ── GET /api/integrations/unmapped ───────────────────────────────────
        /// <summary>
        /// Devuelve los productos importados que no tienen material asignado en el WMS.
        /// Acepta ?channel=WooCommerce|Shopify para filtrar por canal.
        /// </summary>
        [HttpGet("unmapped")]
        public async Task<IActionResult> GetUnmapped([FromQuery] string? channel = null)
            => Ok(await _mediator.Send(new GetUnmappedProductsQuery(channel)));

        // ── POST /api/integrations/{channel}/connect ─────────────────────────
        /// <summary>
        /// Guarda las credenciales del canal y verifica la conexión.
        /// Para WooCommerce: StoreUrl + ApiKey (Consumer Key) + ApiSecret (Consumer Secret).
        /// Para Shopify: StoreUrl (mystore.myshopify.com) + ApiSecret (Access Token).
        /// </summary>
        [HttpPost("{channel}/connect")]
        public async Task<IActionResult> Connect(
            string channel,
            [FromBody] ConnectChannelRequest body)
        {
            if (!Enum.TryParse<SalesChannel>(channel, ignoreCase: true, out var salesChannel))
                return BadRequest($"Canal '{channel}' no reconocido.");

            var result = await _mediator.Send(new ConnectChannelCommand(
                salesChannel,
                body.StoreUrl  ?? string.Empty,
                body.ApiKey    ?? string.Empty,
                body.ApiSecret ?? string.Empty));

            return Ok(result);
        }

        // ── DELETE /api/integrations/{channel} ───────────────────────────────
        /// <summary>Desconecta el canal y borra sus credenciales almacenadas.</summary>
        [HttpDelete("{channel}")]
        public async Task<IActionResult> Disconnect(string channel)
        {
            if (!Enum.TryParse<SalesChannel>(channel, ignoreCase: true, out var salesChannel))
                return BadRequest($"Canal '{channel}' no reconocido.");

            await _mediator.Send(new DisconnectChannelCommand(salesChannel));
            return NoContent();
        }

        // ── POST /api/integrations/{channel}/import ──────────────────────────
        /// <summary>
        /// Importa pedidos desde el canal externo.
        /// Aplica auto-match SKU → Material y FEFO allocation en los pedidos completamente mapeados.
        /// </summary>
        [HttpPost("{channel}/import")]
        public async Task<IActionResult> Import(
            string channel,
            [FromBody] ImportOrdersRequest? body = null)
        {
            if (!Enum.TryParse<SalesChannel>(channel, ignoreCase: true, out var salesChannel))
                return BadRequest($"Canal '{channel}' no reconocido.");

            var result = await _mediator.Send(new ImportOrdersCommand(
                salesChannel,
                body?.MaxOrders ?? 100));

            return Ok(result);
        }

        // ── POST /api/integrations/products/{id}/map ─────────────────────────
        /// <summary>
        /// Asigna manualmente un Material del WMS a un producto externo no mapeado.
        /// </summary>
        [HttpPost("products/{id}/map")]
        public async Task<IActionResult> MapProduct(
            Guid id,
            [FromBody] MapProductRequest body)
        {
            await _mediator.Send(new MapProductCommand(id, body.MaterialId));
            return NoContent();
        }
    }

    // ── Request body DTOs ─────────────────────────────────────────────────────

    public class ConnectChannelRequest
    {
        public string? StoreUrl  { get; set; }
        public string? ApiKey    { get; set; }
        public string? ApiSecret { get; set; }
    }

    public class ImportOrdersRequest
    {
        public int MaxOrders { get; set; } = 100;
    }

    public class MapProductRequest
    {
        public Guid MaterialId { get; set; }
    }
}
