using Inventory.Application.Integrations.Services;
using Inventory.Domain;
using Inventory.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inventory.API.BackgroundServices
{
    /// <summary>
    /// Background service que refresca el access_token de Mercado Libre cada 5 horas.
    /// El access_token de ML expira en 6 horas (21600 s); refrescamos a las 5 para tener margen.
    ///
    /// Almacenamiento en ChannelConfig:
    ///   ApiKey    = "{userId}|{refreshToken}"
    ///   ApiSecret = access_token actual
    /// </summary>
    public class MercadoLibreTokenRefreshService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromHours(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MercadoLibreTokenRefreshService> _logger;

        public MercadoLibreTokenRefreshService(
            IServiceScopeFactory scopeFactory,
            ILogger<MercadoLibreTokenRefreshService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[ML Refresh] Servicio iniciado. Intervalo={Hours}h", Interval.TotalHours);

            // Primer refresco al arrancar (por si el token expiró durante un deploy)
            await RefreshAllAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Interval, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    await RefreshAllAsync(stoppingToken);
            }
        }

        private async Task RefreshAllAsync(CancellationToken ct)
        {
            _logger.LogInformation("[ML Refresh] Iniciando ciclo de refresco...");

            using var scope  = _scopeFactory.CreateScope();
            var context      = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var oauthService = scope.ServiceProvider.GetRequiredService<MercadoLibreOAuthService>();

            var mlConfigs = await context.ChannelConfigs
                .Where(c => c.Channel == SalesChannel.MercadoLibre && c.IsConnected)
                .ToListAsync(ct);

            if (mlConfigs.Count == 0)
            {
                _logger.LogInformation("[ML Refresh] Sin canales ML conectados — nada que refrescar.");
                return;
            }

            foreach (var config in mlConfigs)
            {
                try
                {
                    // ApiKey formato: "{userId}|{refreshToken}"
                    var parts = config.ApiKey?.Split('|');
                    if (parts == null || parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
                    {
                        _logger.LogWarning("[ML Refresh] ConfigId={Id}: ApiKey no tiene refreshToken. Omitiendo.", config.Id);
                        continue;
                    }

                    var userId       = parts[0];
                    var refreshToken = parts[1];

                    _logger.LogInformation("[ML Refresh] Refrescando token para userId={UserId}...", userId);

                    var newTokens = await oauthService.RefreshTokenAsync(refreshToken);

                    // ML rota el refresh_token en cada renovación — guardamos ambos nuevos
                    config.ApiKey    = $"{newTokens.UserId}|{newTokens.RefreshToken}";
                    config.ApiSecret = newTokens.AccessToken;
                    config.LastError = null;

                    _logger.LogInformation(
                        "[ML Refresh] ✓ Token renovado para userId={UserId}. Expira en {Minutes} min.",
                        newTokens.UserId, newTokens.ExpiresIn / 60);
                }
                catch (Exception ex)
                {
                    config.LastError = $"[{DateTime.UtcNow:HH:mm}] Refresh falló: {ex.Message}";
                    _logger.LogError(ex, "[ML Refresh] ✗ Error refrescando token para ConfigId={Id}", config.Id);
                    // No propagamos — un canal fallando no debe tirar los demás
                }
            }

            await context.SaveChangesAsync(ct);
            _logger.LogInformation("[ML Refresh] Ciclo completado para {Count} canal(es).", mlConfigs.Count);
        }
    }
}
