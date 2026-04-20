using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Services
{
    /// <summary>
    /// Gestiona el flujo OAuth 2.0 de Mercado Libre (Authorization Code).
    /// Docs: https://developers.mercadolibre.com.mx/es_ar/autenticacion-y-autorizacion
    /// </summary>
    public class MercadoLibreOAuthService
    {
        private readonly HttpClient      _http;
        private readonly IConfiguration _cfg;

        // ML usa endpoints distintos por país; México = MLM
        private const string AuthBase  = "https://auth.mercadolibre.com.mx";
        private const string ApiBase   = "https://api.mercadolibre.com";

        public MercadoLibreOAuthService(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _cfg  = cfg;
        }

        // ── 1. URL de autorización ────────────────────────────────────────────
        public string GetAuthorizationUrl(string state)
        {
            var clientId    = _cfg["MercadoLibre:ClientId"]   ?? throw new InvalidOperationException("MercadoLibre:ClientId no configurado.");
            var callbackUrl = _cfg["MercadoLibre:CallbackUrl"] ?? throw new InvalidOperationException("MercadoLibre:CallbackUrl no configurado.");

            return $"{AuthBase}/authorization" +
                   $"?response_type=code" +
                   $"&client_id={Uri.EscapeDataString(clientId)}" +
                   $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
                   $"&state={Uri.EscapeDataString(state)}";
        }

        // ── 2. Intercambiar código por tokens ─────────────────────────────────
        public async Task<MlTokenResponse> ExchangeCodeForTokenAsync(string code)
        {
            var clientId     = _cfg["MercadoLibre:ClientId"]     ?? throw new InvalidOperationException("MercadoLibre:ClientId no configurado.");
            var clientSecret = _cfg["MercadoLibre:ClientSecret"] ?? throw new InvalidOperationException("MercadoLibre:ClientSecret no configurado.");
            var callbackUrl  = _cfg["MercadoLibre:CallbackUrl"]  ?? throw new InvalidOperationException("MercadoLibre:CallbackUrl no configurado.");

            var body = new FormUrlEncodedContent(new[]
            {
                new System.Collections.Generic.KeyValuePair<string,string>("grant_type",    "authorization_code"),
                new System.Collections.Generic.KeyValuePair<string,string>("client_id",     clientId),
                new System.Collections.Generic.KeyValuePair<string,string>("client_secret", clientSecret),
                new System.Collections.Generic.KeyValuePair<string,string>("code",          code),
                new System.Collections.Generic.KeyValuePair<string,string>("redirect_uri",  callbackUrl),
            });

            var resp = await _http.PostAsync($"{ApiBase}/oauth/token", body);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<MlTokenResponse>()
                   ?? throw new InvalidOperationException("Respuesta vacía de ML token endpoint.");
        }

        // ── 3. Refrescar access_token (expira en 6 h) ─────────────────────────
        public async Task<MlTokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var clientId     = _cfg["MercadoLibre:ClientId"]     ?? throw new InvalidOperationException("MercadoLibre:ClientId no configurado.");
            var clientSecret = _cfg["MercadoLibre:ClientSecret"] ?? throw new InvalidOperationException("MercadoLibre:ClientSecret no configurado.");

            var body = new FormUrlEncodedContent(new[]
            {
                new System.Collections.Generic.KeyValuePair<string,string>("grant_type",    "refresh_token"),
                new System.Collections.Generic.KeyValuePair<string,string>("client_id",     clientId),
                new System.Collections.Generic.KeyValuePair<string,string>("client_secret", clientSecret),
                new System.Collections.Generic.KeyValuePair<string,string>("refresh_token", refreshToken),
            });

            var resp = await _http.PostAsync($"{ApiBase}/oauth/token", body);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<MlTokenResponse>()
                   ?? throw new InvalidOperationException("Respuesta vacía de ML refresh endpoint.");
        }
    }

    public class MlTokenResponse
    {
        [JsonPropertyName("access_token")]  public string AccessToken  { get; set; } = string.Empty;
        [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = string.Empty;
        [JsonPropertyName("user_id")]       public long   UserId       { get; set; }
        [JsonPropertyName("expires_in")]    public int    ExpiresIn    { get; set; }  // segundos (21600 = 6 h)
        [JsonPropertyName("token_type")]    public string TokenType    { get; set; } = string.Empty;
    }
}
