using System;
using System.Collections.Generic;
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
        /// <summary>Trae pedidos en estado 'processing' (equivalente a Confirmed en el WMS).</summary>
        public async Task<List<WcOrder>> GetOrdersAsync(
            string storeUrl, string key, string secret,
            int page = 1, int perPage = 100,
            CancellationToken ct = default)
        {
            var url    = $"/wp-json/wc/v3/orders?status=processing&page={page}&per_page={perPage}";
            var req    = BuildRequest(storeUrl, key, secret, url, HttpMethod.Get);
            var resp   = await _http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();

            var json   = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<WcOrder>>(json, _opts) ?? new();
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
            PropertyNameCaseInsensitive = true
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
        [JsonPropertyName("price")]       public string  Price      { get; set; } = "0";

        public decimal PriceDecimal => decimal.TryParse(Price, out var v) ? v : 0m;
    }
}
