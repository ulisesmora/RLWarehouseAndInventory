using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Services
{
    /// <summary>
    /// Cliente HTTP ligero para la Shopify Admin REST API 2024-01.
    /// Autenticación: X-Shopify-Access-Token header.
    /// El storeUrl debe ser el subdominio: "mystore.myshopify.com"
    /// </summary>
    public class ShopifyApiService
    {
        private readonly HttpClient _http;

        public ShopifyApiService(HttpClient http)
        {
            _http = http;
        }

        // ── Ping / Test connection ────────────────────────────────────────────
        public async Task<bool> TestConnectionAsync(
            string storeDomain, string accessToken,
            CancellationToken ct = default)
        {
            try
            {
                var req  = BuildRequest(storeDomain, accessToken,
                    "/admin/api/2024-01/shop.json", HttpMethod.Get);
                var resp = await _http.SendAsync(req, ct);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── Fetch orders ──────────────────────────────────────────────────────
        /// <summary>Trae pedidos abiertos (unfulfilled / partially_fulfilled).</summary>
        public async Task<List<ShopifyOrder>> GetOrdersAsync(
            string storeDomain, string accessToken,
            int limit = 250,
            CancellationToken ct = default)
        {
            var url  = $"/admin/api/2024-01/orders.json?status=open&fulfillment_status=unfulfilled&limit={limit}";
            var req  = BuildRequest(storeDomain, accessToken, url, HttpMethod.Get);
            var resp = await _http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();

            var json    = await resp.Content.ReadAsStringAsync(ct);
            var wrapper = JsonSerializer.Deserialize<ShopifyOrdersWrapper>(json, _opts);
            return wrapper?.Orders ?? new();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static HttpRequestMessage BuildRequest(
            string storeDomain, string accessToken,
            string path, HttpMethod method)
        {
            var domain = storeDomain.TrimEnd('/');
            if (!domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                domain = "https://" + domain;

            var msg = new HttpRequestMessage(method, domain + path);
            msg.Headers.Add("X-Shopify-Access-Token", accessToken);
            return msg;
        }

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // ── Shopify DTOs (subset de la API) ───────────────────────────────────────
    public class ShopifyOrdersWrapper
    {
        [JsonPropertyName("orders")] public List<ShopifyOrder> Orders { get; set; } = new();
    }

    public class ShopifyOrder
    {
        [JsonPropertyName("id")]          public long       Id           { get; set; }
        [JsonPropertyName("name")]        public string     Name         { get; set; } = string.Empty;
        [JsonPropertyName("email")]       public string     Email        { get; set; } = string.Empty;
        [JsonPropertyName("created_at")]  public DateTime   CreatedAt    { get; set; }
        [JsonPropertyName("customer")]    public ShopifyCustomer? Customer { get; set; }
        [JsonPropertyName("shipping_address")] public ShopifyAddress? ShippingAddress { get; set; }
        [JsonPropertyName("line_items")]  public List<ShopifyLineItem> LineItems { get; set; } = new();
    }

    public class ShopifyCustomer
    {
        [JsonPropertyName("first_name")] public string FirstName { get; set; } = string.Empty;
        [JsonPropertyName("last_name")]  public string LastName  { get; set; } = string.Empty;
        [JsonPropertyName("email")]      public string Email     { get; set; } = string.Empty;
    }

    public class ShopifyAddress
    {
        [JsonPropertyName("address1")] public string Address1 { get; set; } = string.Empty;
        [JsonPropertyName("city")]     public string City     { get; set; } = string.Empty;
        [JsonPropertyName("country")]  public string Country  { get; set; } = string.Empty;
    }

    public class ShopifyLineItem
    {
        [JsonPropertyName("id")]         public long    Id        { get; set; }
        [JsonPropertyName("product_id")] public long    ProductId { get; set; }
        [JsonPropertyName("title")]      public string  Title     { get; set; } = string.Empty;
        [JsonPropertyName("sku")]        public string  Sku       { get; set; } = string.Empty;
        [JsonPropertyName("quantity")]   public decimal Quantity  { get; set; }
        [JsonPropertyName("price")]      public string  Price     { get; set; } = "0";

        public decimal PriceDecimal => decimal.TryParse(Price, out var v) ? v : 0m;
    }
}
