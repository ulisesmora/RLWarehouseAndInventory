using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Services
{
    /// <summary>
    /// Gestiona el flujo OAuth 2.0 de Shopify:
    ///  1. Genera la URL de autorización (start)
    ///  2. Intercambia el código por un Access Token (callback)
    ///  3. Registra webhooks de pedidos
    ///  4. Valida la firma HMAC de los webhooks entrantes
    /// </summary>
    public class ShopifyOAuthService
    {
        private readonly HttpClient    _http;
        private readonly IConfiguration _cfg;

        public ShopifyOAuthService(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _cfg  = cfg;
        }

        // ── 1. Generar URL de autorización ────────────────────────────────────
        public string GetAuthorizationUrl(string shop, string state)
        {
            var clientId    = _cfg["Shopify:ClientId"]   ?? throw new InvalidOperationException("Shopify:ClientId no configurado.");
            var scopes      = _cfg["Shopify:Scopes"]     ?? "read_orders,write_orders,read_products";
            var callbackUrl = _cfg["Shopify:CallbackUrl"] ?? throw new InvalidOperationException("Shopify:CallbackUrl no configurado.");

            var domain = NormalizeDomain(shop);

            return $"https://{domain}/admin/oauth/authorize" +
                   $"?client_id={Uri.EscapeDataString(clientId)}" +
                   $"&scope={Uri.EscapeDataString(scopes)}" +
                   $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
                   $"&state={Uri.EscapeDataString(state)}";
        }

        // ── 2. Intercambiar código por Access Token ───────────────────────────
        public async Task<string> ExchangeCodeForTokenAsync(
            string shop, string code, CancellationToken ct = default)
        {
            var clientId     = _cfg["Shopify:ClientId"]     ?? throw new InvalidOperationException("Shopify:ClientId no configurado.");
            var clientSecret = _cfg["Shopify:ClientSecret"] ?? throw new InvalidOperationException("Shopify:ClientSecret no configurado.");
            var domain       = NormalizeDomain(shop);

            var response = await _http.PostAsJsonAsync(
                $"https://{domain}/admin/oauth/access_token",
                new { client_id = clientId, client_secret = clientSecret, code },
                ct);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<ShopifyTokenResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.AccessToken
                ?? throw new InvalidOperationException("Shopify no devolvió un access_token.");
        }

        // ── 3. Registrar webhooks de pedidos en la tienda ─────────────────────
        public async Task RegisterWebhooksAsync(
            string shop, string accessToken, string webhookBaseUrl, CancellationToken ct = default)
        {
            var domain = NormalizeDomain(shop);
            var topics = new[] { "orders/create", "orders/updated", "orders/cancelled" };

            foreach (var topic in topics)
            {
                try
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        $"https://{domain}/admin/api/2024-01/webhooks.json");

                    request.Headers.Add("X-Shopify-Access-Token", accessToken);
                    request.Content = JsonContent.Create(new
                    {
                        webhook = new
                        {
                            topic,
                            address = $"{webhookBaseUrl.TrimEnd('/')}/api/webhooks/shopify",
                            format  = "json"
                        }
                    });

                    var resp = await _http.SendAsync(request, ct);
                    // 422 = webhook ya registrado → no es un error
                    if (!resp.IsSuccessStatusCode && (int)resp.StatusCode != 422)
                        Console.WriteLine($"[SHOPIFY WEBHOOK] Error registrando {topic}: {resp.StatusCode}");
                    else
                        Console.WriteLine($"[SHOPIFY WEBHOOK] Registrado: {topic}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SHOPIFY WEBHOOK] Excepción en {topic}: {ex.Message}");
                }
            }
        }

        // ── 4. Validar firma HMAC del webhook ─────────────────────────────────
        public bool ValidateWebhookSignature(string rawBody, string hmacHeader)
        {
            var secret = _cfg["Shopify:ClientSecret"];
            if (string.IsNullOrEmpty(secret)) return false;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash       = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
            var computed   = Convert.ToBase64String(hash);

            return string.Equals(computed, hmacHeader, StringComparison.Ordinal);
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private static string NormalizeDomain(string shop)
        {
            var d = shop.Replace("https://", "").Replace("http://", "").TrimEnd('/');
            if (!d.Contains('.')) d += ".myshopify.com";
            return d;
        }
    }

    public class ShopifyTokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("scope")]         public string Scope       { get; set; } = string.Empty;
    }
}
