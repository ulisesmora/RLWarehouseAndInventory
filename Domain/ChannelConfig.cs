using Domain;
using System;

namespace Inventory.Domain
{
    /// <summary>
    /// Credenciales de conexión a un canal de venta externo (WooCommerce, Shopify…).
    /// Una organización puede tener una sola config activa por canal.
    /// Las claves se almacenan cifradas — nunca se devuelven en texto plano.
    /// </summary>
    public class ChannelConfig : BaseTenantEntity
    {
        public SalesChannel Channel { get; set; }

        /// <summary>¿Las credenciales han sido verificadas y el canal está activo?</summary>
        public bool IsConnected { get; set; } = false;

        /// <summary>URL base de la tienda. Ej: https://mitienda.com  |  mystore.myshopify.com</summary>
        public string StoreUrl { get; set; } = string.Empty;

        /// <summary>WooCommerce: Consumer Key  |  Shopify: no usado (el token va en ApiSecret)</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>WooCommerce: Consumer Secret  |  Shopify: Access Token</summary>
        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>Última vez que se importaron pedidos con éxito</summary>
        public DateTime? LastSyncAt { get; set; }

        /// <summary>Total de pedidos importados desde este canal</summary>
        public int TotalImported { get; set; } = 0;

        /// <summary>Mensaje del último error de sincronización (null si OK)</summary>
        public string? LastError { get; set; }
    }
}
