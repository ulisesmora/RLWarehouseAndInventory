using System;
using System.Collections.Concurrent;

namespace Inventory.Application.Integrations.Services
{
    /// <summary>
    /// Servicio singleton que almacena en memoria los tokens de estado OAuth temporales.
    /// Previene ataques CSRF durante el flujo de autorización.
    /// </summary>
    public class OAuthStateService
    {
        private readonly ConcurrentDictionary<string, OAuthStateEntry> _states = new();

        public string GenerateState(Guid organizationId, string? shopDomain = null)
        {
            // Limpiar estados expirados
            var now = DateTime.UtcNow;
            foreach (var key in _states.Keys)
                if (_states.TryGetValue(key, out var e) && e.ExpiresAt < now)
                    _states.TryRemove(key, out _);

            var state = Guid.NewGuid().ToString("N");
            _states[state] = new OAuthStateEntry
            {
                State          = state,
                OrganizationId = organizationId,
                ShopDomain     = shopDomain,
                ExpiresAt      = DateTime.UtcNow.AddMinutes(10)
            };
            return state;
        }

        public OAuthStateEntry? Consume(string state)
        {
            if (_states.TryRemove(state, out var entry))
            {
                if (entry.ExpiresAt >= DateTime.UtcNow)
                    return entry;
            }
            return null;
        }
    }

    public class OAuthStateEntry
    {
        public string  State          { get; set; } = string.Empty;
        public Guid    OrganizationId { get; set; }
        public string? ShopDomain     { get; set; }
        public DateTime ExpiresAt     { get; set; }
    }
}
