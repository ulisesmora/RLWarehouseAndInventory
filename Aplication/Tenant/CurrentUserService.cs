using Inventory.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Inventory.Application.Tenant
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetTenantId()
        {
            // Buscamos exactamente el claim "tenant_id" que inyectamos en el AuthController
            if (_httpContextAccessor?.HttpContext?.User == null)
                return null;


            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;

            if (Guid.TryParse(tenantClaim, out var tenantId))
            {
                return tenantId;
            }

            return null; // Si no hay token o no tiene tenant, devuelve null
        }

        public Guid? GetUserId()
        {
            // El ID del usuario normalmente se guarda en "sub" (Subject) o NameIdentifier
            var userClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            if (Guid.TryParse(userClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
