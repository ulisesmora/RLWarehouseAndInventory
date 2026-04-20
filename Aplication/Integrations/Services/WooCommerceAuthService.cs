using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Inventory.Application.Integrations.Services
{
    /// <summary>
    /// Gestiona el flujo WC Auth de WooCommerce:
    ///  1. Genera la URL de autorización (WC Auth v1)
    ///  2. Valida la firma HMAC de los webhooks entrantes
    ///
    /// No requiere registro previo de la app — WooCommerce genera las credenciales
    /// automáticamente cuando el administrador de la tienda aprueba el acceso.
    /// </summary>
    public class WooCommerceAuthService
    {
        private readonly IConfiguration _cfg;

        public WooCommerceAuthService(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        // ── 1. Generar URL de autorización WC Auth ────────────────────────────
        /// <summary>
        /// Redirige al usuario a la página de autorización de su WordPress.
        /// WooCommerce llamará a callbackUrl con el Consumer Key/Secret cuando el admin apruebe.
        /// </summary>
        public string GetAuthorizationUrl(
            string storeUrl,
            string callbackUrl,
            string returnUrl,
            string userId)
        {
            var baseUrl = storeUrl.TrimEnd('/');
            return $"{baseUrl}/wc-auth/v1/authorize" +
                   $"?app_name={Uri.EscapeDataString("NEXWARE WMS")}" +
                   $"&scope=read_write" +
                   $"&user_id={Uri.EscapeDataString(userId)}" +
                   $"&return_url={Uri.EscapeDataString(returnUrl)}" +
                   $"&callback_url={Uri.EscapeDataString(callbackUrl)}";
        }

        // ── 2. Validar firma HMAC del webhook ─────────────────────────────────
        public bool ValidateWebhookSignature(string rawBody, string signatureHeader, string secret)
        {
            if (string.IsNullOrEmpty(secret)) return false;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash       = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
            var computed   = Convert.ToBase64String(hash);

            return string.Equals(computed, signatureHeader, StringComparison.Ordinal);
        }
    }
}
