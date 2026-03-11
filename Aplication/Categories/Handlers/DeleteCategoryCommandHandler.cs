using Inventory.Application.Categories.Commands;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Handlers
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly InventoryDbContext _context;

        public DeleteCategoryCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Categories.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new Exception($"La categoría con ID {request.Id} no fue encontrada.");
                // Nota: Si tienes una 'NotFoundException' personalizada, úsala aquí.
            }

            // Esto NO lo borra físicamente gracias a tu DbContext, solo hace el UPDATE IsDeleted = true
            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
