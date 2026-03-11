using AutoMapper;
using Inventory.Application.ProductsRecipes.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductsRecipes.Handlers
{
    public class GetRecipeByFinishedGoodQueryHandler : IRequestHandler<GetRecipeByFinishedGoodQuery, ProductRecipeDto>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetRecipeByFinishedGoodQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductRecipeDto> Handle(GetRecipeByFinishedGoodQuery request, CancellationToken cancellationToken)
        {
            var recipe = await _context.ProductRecipes
                .Include(r => r.FinishedGood)
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.Material) // Vital para traer el nombre de la tela/botón
                .Include(r => r.AdditionalCosts)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.FinishedGoodId == request.FinishedGoodId, cancellationToken);

            if (recipe == null) return null;

            return _mapper.Map<ProductRecipeDto>(recipe);
        }
    }
}
