using Inventory.Application.Integrations.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Integrations.Handlers
{
    public class DisconnectChannelCommandHandler
        : IRequestHandler<DisconnectChannelCommand, Unit>
    {
        private readonly InventoryDbContext _context;

        public DisconnectChannelCommandHandler(InventoryDbContext context)
            => _context = context;

        public async Task<Unit> Handle(
            DisconnectChannelCommand request,
            CancellationToken cancellationToken)
        {
            var config = await _context.ChannelConfigs
                .FirstOrDefaultAsync(c => c.Channel == request.Channel, cancellationToken);

            if (config == null)
                throw new KeyNotFoundException($"No hay configuración para el canal '{request.Channel}'.");

            // Borramos credenciales y marcamos desconectado
            config.IsConnected = false;
            config.ApiKey      = string.Empty;
            config.ApiSecret   = string.Empty;
            config.LastError   = null;

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"[DISCONNECT] Canal {request.Channel} desconectado.");
            return Unit.Value;
        }
    }
}
