using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Services
{
    /// <summary>
    /// Consume la API REST de Mercado Libre para obtener pedidos y productos.
    /// Docs: https://developers.mercadolibre.com.mx/es_ar/gestiona-ventas
    /// </summary>
    public class MercadoLibreApiService
    {
        private readonly HttpClient _http;
        private const string ApiBase = "https://api.mercadolibre.com";

        public MercadoLibreApiService(HttpClient http)
            => _http = http;

        // ── Órdenes ──────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene pedidos del vendedor. userId viene del token (ml_user_id guardado en ApiKey).
        /// </summary>
        public async Task<List<MlOrder>> GetOrdersAsync(
            string userId,
            string accessToken,
            int limit = 100,
            CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            // ML pagina de 50 en 50 máximo
            var perPage = Math.Min(limit, 50);
            var url     = $"{ApiBase}/orders/search?seller={userId}&order.status=paid&limit={perPage}&sort=date_desc";

            var response = await _http.GetFromJsonAsync<MlOrderSearchResponse>(url, ct);
            return response?.Results ?? new List<MlOrder>();
        }

        /// <summary>
        /// Obtiene el detalle completo de una orden (incluye shipping address).
        /// </summary>
        public async Task<MlOrderDetail?> GetOrderDetailAsync(
            long orderId,
            string accessToken,
            CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            return await _http.GetFromJsonAsync<MlOrderDetail>(
                $"{ApiBase}/orders/{orderId}", ct);
        }

        // ── Productos ─────────────────────────────────────────────────────────

        public async Task<List<MlProduct>> GetProductsAsync(
            string userId,
            string accessToken,
            CancellationToken ct = default)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            // Paso 1: obtener IDs de listings activos
            var searchUrl = $"{ApiBase}/users/{userId}/items/search?status=active&limit=100";
            var search    = await _http.GetFromJsonAsync<MlItemSearchResponse>(searchUrl, ct);

            if (search?.Results == null || search.Results.Count == 0)
                return new List<MlProduct>();

            // Paso 2: obtener detalles en lote (ML permite hasta 20 IDs por llamada)
            var products = new List<MlProduct>();
            foreach (var chunk in search.Results.Chunk(20))
            {
                var ids  = string.Join(",", chunk);
                var detailUrl = $"{ApiBase}/items?ids={ids}&attributes=id,title,seller_sku,price,thumbnail";
                var batch = await _http.GetFromJsonAsync<List<MlItemBatchResult>>(detailUrl, ct);
                if (batch != null)
                    products.AddRange(batch
                        .Where(r => r.Code == 200 && r.Body != null)
                        .Select(r => r.Body!));
            }

            return products;
        }
    }

    // ── DTOs de respuesta ML ──────────────────────────────────────────────────

    public class MlOrderSearchResponse
    {
        [JsonPropertyName("results")] public List<MlOrder> Results { get; set; } = new();
    }

    public class MlOrder
    {
        [JsonPropertyName("id")]           public long        Id          { get; set; }
        [JsonPropertyName("date_created")] public DateTime    DateCreated { get; set; }
        [JsonPropertyName("buyer")]        public MlBuyer?    Buyer       { get; set; }
        [JsonPropertyName("order_items")]  public List<MlOrderItem> OrderItems { get; set; } = new();
        [JsonPropertyName("total_amount")] public decimal     TotalAmount { get; set; }
        [JsonPropertyName("status")]       public string      Status      { get; set; } = string.Empty;
    }

    public class MlBuyer
    {
        [JsonPropertyName("id")]        public long   Id        { get; set; }
        [JsonPropertyName("nickname")]  public string Nickname  { get; set; } = string.Empty;
        [JsonPropertyName("first_name")]public string FirstName { get; set; } = string.Empty;
        [JsonPropertyName("last_name")] public string LastName  { get; set; } = string.Empty;
        [JsonPropertyName("email")]     public string Email     { get; set; } = string.Empty;
    }

    public class MlOrderItem
    {
        [JsonPropertyName("item")]            public MlItemRef? Item     { get; set; }
        [JsonPropertyName("quantity")]        public decimal    Quantity { get; set; }
        [JsonPropertyName("unit_price")]      public decimal    UnitPrice{ get; set; }
        [JsonPropertyName("seller_sku")]      public string?    Sku      { get; set; }
    }

    public class MlItemRef
    {
        [JsonPropertyName("id")]    public string Id    { get; set; } = string.Empty;
        [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
        [JsonPropertyName("seller_sku")] public string? Sku { get; set; }
    }

    // Detalle de orden con shipping
    public class MlOrderDetail
    {
        [JsonPropertyName("id")]             public long          Id          { get; set; }
        [JsonPropertyName("date_created")]   public DateTime      DateCreated { get; set; }
        [JsonPropertyName("buyer")]          public MlBuyer?      Buyer       { get; set; }
        [JsonPropertyName("order_items")]    public List<MlOrderItem> OrderItems { get; set; } = new();
        [JsonPropertyName("shipping")]       public MlShippingRef? Shipping   { get; set; }
    }

    public class MlShippingRef
    {
        [JsonPropertyName("id")]             public long   Id             { get; set; }
        [JsonPropertyName("receiver_address")] public MlReceiverAddress? ReceiverAddress { get; set; }
    }

    public class MlReceiverAddress
    {
        [JsonPropertyName("street_name")]   public string StreetName   { get; set; } = string.Empty;
        [JsonPropertyName("street_number")] public string StreetNumber { get; set; } = string.Empty;
        [JsonPropertyName("city")]          public MlCity? City        { get; set; }
        [JsonPropertyName("country")]       public MlCountry? Country  { get; set; }
    }

    public class MlCity    { [JsonPropertyName("name")] public string Name { get; set; } = string.Empty; }
    public class MlCountry { [JsonPropertyName("id")]   public string Id   { get; set; } = string.Empty; }

    // Productos
    public class MlItemSearchResponse
    {
        [JsonPropertyName("results")] public List<string> Results { get; set; } = new();
    }

    public class MlItemBatchResult
    {
        [JsonPropertyName("code")] public int        Code { get; set; }
        [JsonPropertyName("body")] public MlProduct? Body { get; set; }
    }

    public class MlProduct
    {
        [JsonPropertyName("id")]          public string  Id         { get; set; } = string.Empty;
        [JsonPropertyName("title")]       public string  Title      { get; set; } = string.Empty;
        [JsonPropertyName("seller_sku")]  public string? Sku        { get; set; }
        [JsonPropertyName("price")]       public decimal Price      { get; set; }
        [JsonPropertyName("thumbnail")]   public string? Thumbnail  { get; set; }
    }
}
