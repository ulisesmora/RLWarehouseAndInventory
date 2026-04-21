using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Services
{
    /// <summary>
    /// Cliente HTTP ligero para la WooCommerce REST API v3.
    /// Autenticación: Basic Auth (Consumer Key : Consumer Secret).
    /// </summary>
    public class WooCommerceApiService
    {
        private readonly HttpClient _http;

        public WooCommerceApiService(HttpClient http)
        {
            _http = http;
        }

        // ── Ping / Test connection ────────────────────────────────────────────
        public async Task<bool> TestConnectionAsync(
            string storeUrl, string key, string secret,
            CancellationToken ct = default)
        {
            try
            {
                var req = BuildRequest(storeUrl, key, secret,
                    "/wp-json/wc/v3/system_status", HttpMethod.Get);
                var resp = await _http.SendAsync(req, ct);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── Fetch orders ──────────────────────────────────────────────────────
        /// <summary>
        /// Trae pedidos en estado 'processing'.
        /// <paramref name="after"/> limita a pedidos creados a partir de esa fecha (ISO 8601).
        /// </summary>
        public async Task<List<WcOrder>> GetOrdersAsync(
            string storeUrl, string key, string secret,
            int page = 1, int perPage = 100,
            DateTime? after = null,
            CancellationToken ct = default)
        {
            var afterParam = after.HasValue
                ? $"&after={after.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}"
                : string.Empty;

            var url  = $"/wp-json/wc/v3/orders?status=processing&page={page}&per_page={perPage}{afterParam}";
            var req  = BuildRequest(storeUrl, key, secret, url, HttpMethod.Get);
            var resp = await _http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<WcOrder>>(json, _opts) ?? new();
        }

        // ── Fetch products ────────────────────────────────────────────────────
        public async Task<List<WcProduct>> GetProductsAsync(
            string storeUrl, string key, string secret,
            int page = 1, int perPage = 100,
            CancellationToken ct = default)
        {
            var url  = $"/wp-json/wc/v3/products?status=publish&page={page}&per_page={perPage}";
            var req  = BuildRequest(storeUrl, key, secret, url, HttpMethod.Get);
            var resp = await _http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<WcProduct>>(json, _opts) ?? new();
        }

        // ── Webhook registration ──────────────────────────────────────────────
        /// <summary>
        /// Registra los webhooks de order.created y order.updated en la tienda WooCommerce.
        /// Si ya existen webhooks apuntando a la misma deliveryUrl, los omite para evitar duplicados.
        /// Devuelve la lista de webhooks creados (vacía si ya existían todos).
        /// </summary>
        public async Task<List<WcWebhook>> RegisterWebhooksAsync(
            string storeUrl, string key, string secret,
            string deliveryUrl,
            CancellationToken ct = default)
        {
            var topics  = new[] { "order.created", "order.updated" };
            var created = new List<WcWebhook>();

            // 1. Obtener webhooks existentes para evitar duplicados
            List<WcWebhook> existing;
            try
            {
                var listReq  = BuildRequest(storeUrl, key, secret, "/wp-json/wc/v3/webhooks?per_page=100", HttpMethod.Get);
                var listResp = await _http.SendAsync(listReq, ct);
                var listJson = await listResp.Content.ReadAsStringAsync(ct);
                existing = listResp.IsSuccessStatusCode
                    ? JsonSerializer.Deserialize<List<WcWebhook>>(listJson, _opts) ?? new()
                    : new();
            }
            catch { existing = new(); }

            // 2. Crear los que no existan aún
            foreach (var topic in topics)
            {
                var alreadyExists = existing.Any(w =>
                    w.Topic == topic &&
                    w.DeliveryUrl.TrimEnd('/').Equals(deliveryUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));

                if (alreadyExists) continue;

                var body    = JsonSerializer.Serialize(new { name = $"bblazzr – {topic}", topic, delivery_url = deliveryUrl, status = "active" });
                var postReq = BuildRequest(storeUrl, key, secret, "/wp-json/wc/v3/webhooks", HttpMethod.Post);
                postReq.Content = new StringContent(body, Encoding.UTF8, "application/json");

                var postResp = await _http.SendAsync(postReq, ct);
                if (postResp.IsSuccessStatusCode)
                {
                    var json    = await postResp.Content.ReadAsStringAsync(ct);
                    var webhook = JsonSerializer.Deserialize<WcWebhook>(json, _opts);
                    if (webhook != null) created.Add(webhook);
                }
            }

            return created;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static HttpRequestMessage BuildRequest(
            string storeUrl, string key, string secret,
            string path, HttpMethod method)
        {
            var baseUrl  = storeUrl.TrimEnd('/');
            var fullUrl  = baseUrl + path;
            var msg      = new HttpRequestMessage(method, fullUrl);
            var encoded  = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{key}:{secret}"));
            msg.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
            return msg;
        }

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true,
            // WooCommerce puede devolver precios como número (19.99) o como string ("19.99")
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
    }

    // ── WooCommerce DTOs (subset de la API) ───────────────────────────────────
    public class WcOrder
    {
        [JsonPropertyName("id")]          public long   Id          { get; set; }
        [JsonPropertyName("number")]      public string Number      { get; set; } = string.Empty;
        [JsonPropertyName("status")]      public string Status      { get; set; } = string.Empty;
        [JsonPropertyName("date_created")] public DateTime DateCreated { get; set; }
        [JsonPropertyName("billing")]     public WcBilling Billing  { get; set; } = new();
        [JsonPropertyName("shipping")]    public WcShipping Shipping { get; set; } = new();
        [JsonPropertyName("line_items")]  public List<WcLineItem> LineItems { get; set; } = new();
    }

    public class WcBilling
    {
        [JsonPropertyName("first_name")] public string FirstName { get; set; } = string.Empty;
        [JsonPropertyName("last_name")]  public string LastName  { get; set; } = string.Empty;
        [JsonPropertyName("email")]      public string Email     { get; set; } = string.Empty;
        [JsonPropertyName("address_1")]  public string Address1  { get; set; } = string.Empty;
        [JsonPropertyName("city")]       public string City      { get; set; } = string.Empty;
        [JsonPropertyName("country")]    public string Country   { get; set; } = string.Empty;
    }

    public class WcShipping
    {
        [JsonPropertyName("address_1")] public string Address1 { get; set; } = string.Empty;
        [JsonPropertyName("city")]      public string City     { get; set; } = string.Empty;
        [JsonPropertyName("country")]   public string Country  { get; set; } = string.Empty;
    }

    public class WcLineItem
    {
        [JsonPropertyName("id")]          public long    Id         { get; set; }
        [JsonPropertyName("product_id")]  public long    ProductId  { get; set; }
        [JsonPropertyName("name")]        public string  Name       { get; set; } = string.Empty;
        [JsonPropertyName("sku")]         public string  Sku        { get; set; } = string.Empty;
        [JsonPropertyName("quantity")]    public decimal Quantity   { get; set; }
        // WooCommerce envía price como número JSON (19.99), no como string
        [JsonPropertyName("price")]       public decimal Price      { get; set; }
    }

    // ── Webhook DTOs ─────────────────────────────────────────────────────────
    public class WcWebhook
    {
        [JsonPropertyName("id")]           public long   Id          { get; set; }
        [JsonPropertyName("name")]         public string Name        { get; set; } = string.Empty;
        [JsonPropertyName("topic")]        public string Topic       { get; set; } = string.Empty;
        [JsonPropertyName("delivery_url")] public string DeliveryUrl { get; set; } = string.Empty;
        [JsonPropertyName("status")]       public string Status      { get; set; } = string.Empty;
    }

    // ── Product DTOs ──────────────────────────────────────────────────────────
    public class WcProduct
    {
        [JsonPropertyName("id")]          public long    Id          { get; set; }
        [JsonPropertyName("name")]        public string  Name        { get; set; } = string.Empty;
        [JsonPropertyName("sku")]         public string  Sku         { get; set; } = string.Empty;
        [JsonPropertyName("type")]        public string  Type        { get; set; } = string.Empty; // simple|variable
        [JsonPropertyName("variations")]  public List<long> VariationIds { get; set; } = new();
    }
}
