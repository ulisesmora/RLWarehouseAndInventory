using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.UnitOfMesaure.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Handlers
{
    public class GetUnitOfMeasureByIdHandler : IRequestHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasureDto>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetUnitOfMeasureByIdHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UnitOfMeasureDto> Handle(GetUnitOfMeasureByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _context.UnitOfMeasures
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .ProjectTo<UnitOfMeasureDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null) throw new Exception("Unidad de Medida no encontrada.");

            return dto;
        }
    }
}
