using Inventory.Application.Users.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Handlers
{
    public class DeleteUserCommandHandler(InventoryDbContext context) : IRequestHandler<DeleteUserCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var entity = await context.User.FindAsync(new object[] { request.Id }, cancellationToken)
                ?? throw new KeyNotFoundException($"User con Id {request.Id} no encontrado.");

            // Soft Delete (recomendado) o Hard Delete
            entity.IsActive = false;

            await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
