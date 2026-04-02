using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Tenant
{
    public interface ITenantService
    {
        string GetTenantId();
    }
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTenantId()
        {
            // Leemos el ID de la empresa directamente del Token JWT del usuario
            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;

            // Si no hay token (ej. al hacer login o migraciones base), usamos el esquema "public" de Postgres
            return string.IsNullOrEmpty(tenantClaim) ? "public" : $"tenant_{tenantClaim.Replace("-", "")}";
        }
    }
}
