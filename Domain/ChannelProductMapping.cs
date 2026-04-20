using Domain;
using System;

namespace Inventory.Domain
{
    /// <summary>
    /// Mapeo entre un SKU externo (WooCommerce/Shopify) y un Material del WMS.
    /// Se crea automáticamente cuando se importan pedidos.
    /// - Si se encontró coincidencia por SKU o nombre: IsAutoMapped = true.
    /// - Si no hubo match: MaterialId = null, queda pendiente de asignación manual.
    /// </summary>
    public class ChannelProductMapping : BaseTenantEntity
    {
        public SalesChannel Channel { get; set; }

        /// <summary>SKU tal como viene del canal externo</summary>
        public string ExternalSku { get; set; } = string.Empty;

        /// <summary>Nombre del producto tal como viene del canal externo</summary>
        public string ExternalProductName { get; set; } = string.Empty;

        /// <summary>ID del producto en el canal externo (útil como referencia)</summary>
        public string ExternalProductId { get; set; } = string.Empty;

        /// <summary>
        /// Material del WMS asignado a este SKU.
        /// Null = sin mapeo (pedido importado en estado Draft hasta que se asigne).
        /// </summary>
        public Guid? MaterialId { get; set; }
        public virtual Material? Material { get; set; }

        /// <summary>true = lo resolvió el algoritmo automático  |  false = asignado por el usuario</summary>
        public bool IsAutoMapped { get; set; } = false;

        /// <summary>Confianza del auto-match: "ExactSku", "ExactName", "FuzzyName", "Manual"</summary>
        public string MatchMethod { get; set; } = "None";
    }
}
