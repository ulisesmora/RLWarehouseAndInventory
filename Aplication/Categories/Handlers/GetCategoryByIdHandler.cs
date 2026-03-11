using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.Categories.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Handlers
{
    public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetCategoryByIdHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken token)
        {
            var dto = await _context.Categories
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(token);

            if (dto == null) throw new Exception("Categoría no encontrada.");

            return dto;
        }
    }
}
