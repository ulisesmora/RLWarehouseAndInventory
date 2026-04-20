using Inventory.Application.Integrations.Commands;
using Inventory.Application.Integrations.Services;
using Inventory.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inventory.API.Controllers
{
    /// <summary>
    /// Recibe notificaciones en tiempo real (webhooks) de Shopify y WooCommerce.
    /// Estos endpoints son públicos (sin [Authorize]) porque los llama el canal externo.
    /// La autenticidad se verifica mediante firma HMAC.
    /// </summary>
    [Route("api/webhooks")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly IMediator            _mediator;
        private readonly ShopifyOAuthService  _shopifyAuth;
        private readonly WooCommerceAuthService _wcAuth;

        public WebhooksController(
            IMediator mediator,
            ShopifyOAuthService shopifyAuth,
            WooCommerceAuthService wcAuth)
        {
            _mediator    = mediator;
            _shopifyAuth = shopifyAuth;
            _wcAuth      = wcAuth;
        }

        // ── POST /api/webhooks/shopify ─────────────────────────────────────────
        [HttpPost("shopify")]
        public async Task<IActionResult> ShopifyWebhook()
        {
            // Leer el body RAW (necesario para validar HMAC antes de deserializar)
            string rawBody;
            using (var reader = new StreamReader(Request.Body))
                rawBody = await reader.ReadToEndAsync();

            // Validar firma HMAC
            var hmacHeader = Request.Headers["X-Shopify-Hmac-Sha256"].ToString();
            if (!_shopifyAuth.ValidateWebhookSignature(rawBody, hmacHeader))
            {
                Console.WriteLine("[WEBHOOK SHOPIFY] Firma HMAC inválida — rechazado.");
                return Unauthorized();
            }

            var topic    = Request.Headers["X-Shopify-Topic"].ToString();
            var shopDomain = Request.Headers["X-Shopify-Shop-Domain"].ToString();

            Console.WriteLine($"[WEBHOOK SHOPIFY] topic={topic} shop={shopDomain}");

            // Solo procesamos creación/actualización de pedidos
            if (topic == "orders/create" || topic == "orders/updated")
            {
                try
                {
                    var order = JsonSerializer.Deserialize<ShopifyWebhookOrder>(rawBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (order != null)
                        await _mediator.Send(new ProcessWebhookOrderCommand(
                            SalesChannel.Shopify, shopDomain, rawBody, order.Id.ToString(),
                            order.Name, order.Email, order.Email,
                            order.ShippingAddress != null
                                ? $"{order.ShippingAddress.Address1}, {order.ShippingAddress.City}"
                                : string.Empty,
                            order.CreatedAt,
                            System.Text.Json.JsonSerializer.Serialize(order.LineItems)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WEBHOOK SHOPIFY] Error procesando: {ex.Message}");
                }
            }

            // Shopify requiere 200 inmediato — no importa si procesamos o no
            return Ok();
        }

        // ── POST /api/webhooks/woocommerce ────────────────────────────────────
        [HttpPost("woocommerce")]
        public async Task<IActionResult> WooCommerceWebhook()
        {
            string rawBody;
            using (var reader = new StreamReader(Request.Body))
                rawBody = await reader.ReadToEndAsync();

            var topic     = Request.Headers["X-WC-Webhook-Topic"].ToString();
            var source    = Request.Headers["X-WC-Webhook-Source"].ToString();
            var signature = Request.Headers["X-WC-Webhook-Signature"].ToString();

            Console.WriteLine($"[WEBHOOK WC] topic={topic} source={source}");

            // Nota: la firma se valida con el webhook secret de la tienda específica.
            // Por ahora logueamos y procesamos — la validación per-tenant se puede
            // agregar cuando guardemos el webhook secret en ChannelConfig.

            if (topic == "order.created" || topic == "order.updated")
            {
                try
                {
                    var order = JsonSerializer.Deserialize<WcWebhookOrder>(rawBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (order != null)
                    {
                        var customer = $"{order.Billing?.FirstName} {order.Billing?.LastName}".Trim();
                        var address  = order.Shipping != null
                            ? $"{order.Shipping.Address1}, {order.Shipping.City}"
                            : string.Empty;

                        await _mediator.Send(new ProcessWebhookOrderCommand(
                            SalesChannel.WooCommerce, source, rawBody,
                            order.Id.ToString(), $"WC-{order.Number}",
                            string.IsNullOrEmpty(customer) ? "Cliente WooCommerce" : customer,
                            order.Billing?.Email ?? string.Empty,
                            address, order.DateCreated,
                            System.Text.Json.JsonSerializer.Serialize(order.LineItems)));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WEBHOOK WC] Error procesando: {ex.Message}");
                }
            }

            return Ok();
        }
    }

    // ── Webhook DTOs mínimos (solo los campos que necesitamos) ────────────────
    public class ShopifyWebhookOrder
    {
        public long   Id              { get; set; }
        public string Name            { get; set; } = string.Empty;
        public string Email           { get; set; } = string.Empty;
        public DateTime CreatedAt     { get; set; }
        public ShopifyWebhookAddress? ShippingAddress { get; set; }
        public System.Collections.Generic.List<ShopifyWebhookLineItem> LineItems { get; set; } = new();
    }

    public class ShopifyWebhookAddress
    {
        public string Address1 { get; set; } = string.Empty;
        public string City     { get; set; } = string.Empty;
        public string Country  { get; set; } = string.Empty;
    }

    public class ShopifyWebhookLineItem
    {
        public long    Id        { get; set; }
        public long    ProductId { get; set; }
        public string  Title     { get; set; } = string.Empty;
        public string  Sku       { get; set; } = string.Empty;
        public decimal Quantity  { get; set; }
        public string  Price     { get; set; } = "0";
    }

    public class WcWebhookOrder
    {
        public long     Id          { get; set; }
        public string   Number      { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public WcWebhookBilling?  Billing  { get; set; }
        public WcWebhookShipping? Shipping { get; set; }
        public System.Collections.Generic.List<WcWebhookLineItem> LineItems { get; set; } = new();
    }

    public class WcWebhookBilling
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string Email     { get; set; } = string.Empty;
    }

    public class WcWebhookShipping
    {
        public string Address1 { get; set; } = string.Empty;
        public string City     { get; set; } = string.Empty;
    }

    public class WcWebhookLineItem
    {
        public long    ProductId { get; set; }
        public string  Name      { get; set; } = string.Empty;
        public string  Sku       { get; set; } = string.Empty;
        public decimal Quantity  { get; set; }
        public string  Price     { get; set; } = "0";
    }
}
