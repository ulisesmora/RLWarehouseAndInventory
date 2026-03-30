using AutoMapper;
using AutoMapper.QueryableExtensions;
using Inventory.Application.StorageBins.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Inventory.Application.StorageBins.Handlers
{
    public class GetBinsByZoneQueryHandler : IRequestHandler<GetBinsByZoneQuery, List<StorageBinDto>>
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;

        public GetBinsByZoneQueryHandler(InventoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<StorageBinDto>> Handle(GetBinsByZoneQuery request, CancellationToken cancellationToken)
        {
            // Hacemos la proyección manual para hacer las matemáticas en SQL Server
            var query = _context.StorageBins
                .AsNoTracking()
                .Where(b => b.ZoneId == request.ZoneId)
                .Select(b => new StorageBinDto
                {
                    Id = b.Id,
                    Code = b.Code,
                    Description = b.Description,
                    PositionX = b.PositionX,
                    PositionY = b.PositionY,
                    PositionZ = b.PositionZ,
                    MaxWeight = b.MaxWeight,
                    MaxVolume = b.MaxVolume,
                    Width = b.Width,
                    Depth = b.Depth,
                    Height = b.Height,
                    physicalOffsetX = b.physicalOffsetX,
                    physicalOffsetZ = b.physicalOffsetZ,
                    rotation = b.rotation,
                    ZoneId = b.ZoneId,
                    ZoneName = b.Zone.Name,

                    // 🔥 MAGIA SQL: Sumamos peso y volumen directamente en la BD
                    CurrentWeight = b.StockItems.Sum(s => s.WeightKg ?? 0),
                    CurrentVolumeM3 = b.StockItems.Sum(s =>
                        ((s.LengthCm ?? 0) * (s.WidthCm ?? 0) * (s.HeightCm ?? 0)) / 1000000m),

                    // Solo traemos los items si el Frontend lo pidió (ahorra RAM)
                    ExistingItems = (bool)request.IncludeItems ? b.StockItems.Select(s => new BinItemDto
                    {
                        Id = s.Id,
                        ReferenceNumber = s.ReferenceNumber,
                        ContainerType = s.ContainerType.ToString(),
                        WeightKg = s.WeightKg,
                        WidthCm = s.WidthCm,
                        HeightCm = s.HeightCm,
                        DepthCm = s.LengthCm,
                        VolumeM3 = ((s.LengthCm ?? 0) * (s.WidthCm ?? 0) * (s.HeightCm ?? 0)) / 1000000m
                    }).ToList() : new List<BinItemDto>()
                });

            return await query.OrderBy(b => b.Code).ToListAsync(cancellationToken);
        }
    }
}
