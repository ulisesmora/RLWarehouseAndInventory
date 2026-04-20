using Inventory.Application.Integrations.Commands;
using Inventory.Application.Integrations.Queries;
using Inventory.Application.Integrations.Services;
using Inventory.Application.Tenant;
using Inventory.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IntegrationsController : ControllerBase
    {
        private readonly IMediator              _mediator;
        private readonly ShopifyOAuthService    _shopifyOAuth;
        private readonly WooCommerceAuthService  _wcAuth;
        private readonly MercadoLibreOAuthService _mlOAuth;
        private readonly OAuthStateService      _oauthState;
        private readonly ICurrentUserService    _currentUser;
        private readonly IConfiguration         _cfg;

        public IntegrationsController(
            IMediator mediator,
            ShopifyOAuthService      shopifyOAuth,
            WooCommerceAuthService   wcAuth,
            MercadoLibreOAuthService mlOAuth,
            OAuthStateService        oauthState,
            ICurrentUserService      currentUser,
            IConfiguration           cfg)
        {
            _mediator     = mediator;
            _shopifyOAuth  = shopifyOAuth;
            _wcAuth        = wcAuth;
            _mlOAuth       = mlOAuth;
            _oauthState    = oauthState;
            _currentUser   = currentUser;
            _cfg           = cfg;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ESTADO / PRODUCTOS SIN MAPEAR
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
            => Ok(await _mediator.Send(new GetChannelStatusQuery()));

        [HttpGet("unmapped")]
        public async Task<IActionResult> GetUnmapped([FromQuery] string? channel = null)
            => Ok(await _mediator.Send(new GetUnmappedProductsQuery(channel)));

        // ══════════════════════════════════════════════════════════════════════
        // OAUTH 2.0 — SHOPIFY
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Inicia el flujo OAuth de Shopify.
        /// Redirige al usuario a la pantalla de autorización de su tienda.
        /// </summary>
        [HttpGet("shopify/oauth/start")]
        public IActionResult ShopifyOAuthStart([FromQuery] string shop)
        {
            if (string.IsNullOrWhiteSpace(shop))
                return BadRequest("El parámetro 'shop' es requerido (ej: mitienda.myshopify.com).");

            var orgId = _currentUser.GetTenantId() ?? Guid.Empty;
            var state = _oauthState.GenerateState(orgId, shop);
            var url   = _shopifyOAuth.GetAuthorizationUrl(shop, state);

            return Ok(new { authorizationUrl = url });

        }

        /// <summary>
        /// Callback OAuth de Shopify. Shopify llama a este endpoint tras la autorización.
        /// Intercambia el código por el Access Token y registra los webhooks automáticamente.
        /// </summary>
        [HttpGet("shopify/oauth/callback")]
        [AllowAnonymous]   // Shopify llama a esto sin JWT
        public async Task<IActionResult> ShopifyOAuthCallback(
            [FromQuery] string code,
            [FromQuery] string state,
            [FromQuery] string shop)
        {
            var frontendBase = _cfg["Frontend:BaseUrl"] ?? "http://localhost:4200";

            // Validar state (anti-CSRF)
            var stateEntry = _oauthState.Consume(state);
            if (stateEntry == null)
                return Redirect($"{frontendBase}/integrations?error=invalid_state");

            try
            {
                // Intercambiar código por Access Token
                var accessToken = await _shopifyOAuth.ExchangeCodeForTokenAsync(shop, code);

                // Guardar en ChannelConfig usando el handler existente (ConnectChannelCommand)
                // Pasamos el orgId del state para evitar depender del contexto de usuario
                await _mediator.Send(new ConnectChannelCommand(
                    SalesChannel.Shopify,
                    $"https://{shop.Replace("https://", "").TrimEnd('/')}",
                    ApiKey:    string.Empty,   // Shopify no usa API Key separada
                    ApiSecret: accessToken));

                // Registrar webhooks automáticamente
                var backendBase = _cfg["Shopify:CallbackUrl"]?
                    .Replace("/api/integrations/shopify/oauth/callback", "")
                    ?? Request.Scheme + "://" + Request.Host;

                await _shopifyOAuth.RegisterWebhooksAsync(shop, accessToken, backendBase);

                Console.WriteLine($"[OAUTH SHOPIFY] ✓ Conectado: {shop}");
                return Redirect($"{frontendBase}/integrations?connected=shopify");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OAUTH SHOPIFY] Error: {ex.Message}");
                return Redirect($"{frontendBase}/integrations?error=shopify_failed");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // OAUTH — WOOCOMMERCE (WC Auth v1)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Inicia el flujo WC Auth de WooCommerce.
        /// Redirige al usuario a la pantalla de autorización de su WordPress.
        /// </summary>
        [HttpGet("woocommerce/oauth/start")]
        public IActionResult WooCommerceOAuthStart([FromQuery] string storeUrl)
        {
            if (string.IsNullOrWhiteSpace(storeUrl))
                return BadRequest("El parámetro 'storeUrl' es requerido.");

            var orgId = _currentUser.GetTenantId() ?? Guid.Empty;
            // Usamos el orgId como user_id para identificar el tenant en el callback
            var userId = orgId.ToString("N");

            // Guardar el storeUrl en state para recuperarlo en el callback
            _oauthState.GenerateState(orgId, storeUrl);

            var backendBase  = _cfg["Shopify:CallbackUrl"]?
                .Replace("/api/integrations/shopify/oauth/callback", "")
                ?? Request.Scheme + "://" + Request.Host;

            var callbackUrl  = $"{backendBase}/api/integrations/woocommerce/oauth/callback";
            var frontendBase = _cfg["Frontend:BaseUrl"] ?? "http://localhost:4200";
            var returnUrl    = $"{frontendBase}/integrations?connected=woocommerce";

            var url = _wcAuth.GetAuthorizationUrl(storeUrl, callbackUrl, returnUrl, userId);
            return Ok(new { authorizationUrl = url });
        }

        /// <summary>
        /// WooCommerce llama a este endpoint (POST) con el Consumer Key/Secret generado.
        /// </summary>
        [HttpPost("woocommerce/oauth/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> WooCommerceOAuthCallback(
            [FromForm] string key_id,
            [FromForm] string user_id,
            [FromForm] string consumer_key,
            [FromForm] string consumer_secret,
            [FromForm] string key_permissions)
        {
            Console.WriteLine($"[OAUTH WC] callback user_id={user_id} permissions={key_permissions}");

            if (!Guid.TryParse(user_id, out var orgId))
            {
                Console.WriteLine("[OAUTH WC] user_id inválido.");
                return BadRequest("user_id inválido.");
            }

            try
            {
                // Necesitamos el storeUrl — lo buscamos en el state activo para este orgId
                // (Alternativamente: buscar en ChannelConfigs si ya existe una entrada sin credenciales)
                // Por ahora guardamos sin storeUrl y el usuario puede completarlo
                await _mediator.Send(new ConnectChannelCommand(
                    SalesChannel.WooCommerce,
                    StoreUrl:  string.Empty,   // Se actualizará cuando tengamos el storeUrl
                    ApiKey:    consumer_key,
                    ApiSecret: consumer_secret));

                Console.WriteLine($"[OAUTH WC] ✓ Credenciales guardadas para org={orgId}");
                return Ok("Autorización completada. Puedes cerrar esta ventana.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OAUTH WC] Error: {ex.Message}");
                return StatusCode(500, "Error guardando credenciales.");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // OAUTH — MERCADO LIBRE
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Inicia el flujo OAuth 2.0 de Mercado Libre.
        /// Redirige al usuario a la pantalla de autorización de ML.
        /// </summary>
        [HttpGet("mercadolibre/oauth/start")]
        public IActionResult MercadoLibreOAuthStart()
        {
            var orgId = _currentUser.GetTenantId() ?? Guid.Empty;
            var state = _oauthState.GenerateState(orgId);
            var url   = _mlOAuth.GetAuthorizationUrl(state);
            return Ok(new { authorizationUrl = url });
        }

        /// <summary>
        /// Callback OAuth de Mercado Libre.
        /// ML llama a este endpoint con code + state tras la autorización del usuario.
        /// </summary>
        [HttpGet("mercadolibre/oauth/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> MercadoLibreOAuthCallback(
            [FromQuery] string code,
            [FromQuery] string state,
            CancellationToken cancellationToken)
        {
            var frontendBase = _cfg["Frontend:BaseUrl"] ?? "http://localhost:4200";

            var stateEntry = _oauthState.Consume(state);
            if (stateEntry == null)
                return Redirect($"{frontendBase}/integrations?error=invalid_state");

            try
            {
                var tokens = await _mlOAuth.ExchangeCodeForTokenAsync(code);

                // Guardamos:
                //   StoreUrl  = URL de la API de ML (referencia)
                //   ApiKey    = ML user_id  (necesario para consultar órdenes y productos)
                //   ApiSecret = access_token (Bearer para llamadas a la API)
                //
                // NOTA: el refresh_token también se necesitaría para renovar el acceso.
                // Por simplicidad lo guardamos en ApiKey con formato "userId|refreshToken"
                // y en ApiSecret el access_token actual.
                await _mediator.Send(new ConnectChannelCommand(
                    SalesChannel.MercadoLibre,
                    StoreUrl:  $"https://api.mercadolibre.com/users/{tokens.UserId}",
                    ApiKey:    $"{tokens.UserId}|{tokens.RefreshToken}",
                    ApiSecret: tokens.AccessToken),
                    cancellationToken);

                Console.WriteLine($"[OAUTH ML] ✓ Conectado: userId={tokens.UserId}");
                return Redirect($"{frontendBase}/integrations?connected=mercadolibre");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OAUTH ML] Error: {ex.Message}");
                return Redirect($"{frontendBase}/integrations?error=ml_failed");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONEXIÓN MANUAL (fallback para tiendas que no soporten WC Auth)
        // ══════════════════════════════════════════════════════════════════════

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

        [HttpDelete("{channel}")]
        public async Task<IActionResult> Disconnect(string channel)
        {
            if (!Enum.TryParse<SalesChannel>(channel, ignoreCase: true, out var salesChannel))
                return BadRequest($"Canal '{channel}' no reconocido.");

            await _mediator.Send(new DisconnectChannelCommand(salesChannel));
            return NoContent();
        }

        // ══════════════════════════════════════════════════════════════════════
        // IMPORTACIÓN Y SINCRONIZACIÓN
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("{channel}/import")]
        public async Task<IActionResult> Import(
            string channel,
            [FromBody] ImportOrdersRequest? body = null)
        {
            if (!Enum.TryParse<SalesChannel>(channel, ignoreCase: true, out var salesChannel))
                return BadRequest($"Canal '{channel}' no reconocido.");

            var result = await _mediator.Send(new ImportOrdersCommand(
                salesChannel, body?.MaxOrders ?? 100));

            return Ok(result);
        }

        [HttpPost("{channel}/sync-products")]
        public async Task<IActionResult> SyncProducts(string channel)
        {
            if (!Enum.TryParse<SalesChannel>(channel, ignoreCase: true, out var salesChannel))
                return BadRequest($"Canal '{channel}' no reconocido.");

            var result = await _mediator.Send(new SyncProductsCommand(salesChannel));
            return Ok(result);
        }

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
