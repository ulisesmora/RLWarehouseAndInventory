using AutoMapper;
using Inventory.Application.Users.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Handlers
{
    public class UpdateUserCommandHandler(
     InventoryDbContext context,
     IMapper mapper) : IRequestHandler<UpdateUserCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = await context.User.FindAsync(new object[] { request.Id }, cancellationToken)
                ?? throw new KeyNotFoundException($"User con Id {request.Id} no encontrado.");

            mapper.Map(request, entity);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
