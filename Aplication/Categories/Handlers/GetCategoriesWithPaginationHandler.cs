using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Categories.Queries;
using Inventory.Application.Materials.Commons.Models;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Handlers
{
    public class GetCategoriesWithPaginationHandler : IRequestHandler<GetCategoriesWithPaginationQuery, PaginatedList<CategoryDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetCategoriesWithPaginationHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<CategoryDto>> Handle(GetCategoriesWithPaginationQuery request, CancellationToken token)
        {
            var query = _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(x => x.Name.Contains(request.SearchTerm));
            }

            return await PaginatedList<CategoryDto>.CreateAsync(
                query.ProjectTo<CategoryDto>(_mapper.ConfigurationProvider),
                request.PageNumber,
                request.PageSize);
        }
    }
}
