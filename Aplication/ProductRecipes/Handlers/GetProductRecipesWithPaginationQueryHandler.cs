using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.ProductRecipes.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Handlers
{
    public class GetProductRecipesWithPaginationQueryHandler : IRequestHandler<GetProductRecipesWithPaginationQuery, PaginatedList<ProductRecipeDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetProductRecipesWithPaginationQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<ProductRecipeDto>> Handle(GetProductRecipesWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ProductRecipes
                .Include(r => r.FinishedGood)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                // Buscamos por nombre de receta
                query = query.Where(x => x.Name.Contains(term));
            }

            // 3. Ordenamiento
            query = query.OrderBy(x => x.Name); // Ordenamos alfabéticamente por nombre de receta

            // 4. Proyección y Ejecución Final
            return await PaginatedList<ProductRecipeDto>.CreateAsync(
                query.ProjectTo<ProductRecipeDto>(_mapper.ConfigurationProvider),
                request.PageNumber,
                request.PageSize
            );
        }
    }

    public class GetProductRecipeByIdQueryHandler : IRequestHandler<GetProductRecipeByIdQuery, ProductRecipeDto>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetProductRecipeByIdQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductRecipeDto> Handle(GetProductRecipeByIdQuery request, CancellationToken cancellationToken)
        {
            var recipe = await _context.ProductRecipes
                .Include(r => r.FinishedGood)
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.Material)
                        .ThenInclude(m => m.UnitOfMeasure)
                .Include(r => r.AdditionalCosts)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (recipe == null)
            {
                throw new Exception($"No se encontró la receta con ID {request.Id}");
            }

            return _mapper.Map<ProductRecipeDto>(recipe);
        }
    }
}
