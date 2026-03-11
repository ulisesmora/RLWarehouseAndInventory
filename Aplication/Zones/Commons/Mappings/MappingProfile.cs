using AutoMapper;
using Inventory.Application.Zones.Commands;
using Inventory.Application.Zones.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Zones.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {

            CreateMap<Zone, ZoneDetailDto>()
    .ForMember(d => d.WarehouseName, opt => opt.MapFrom(s => s.Warehouse.Name))
    .ForMember(d => d.Bins, opt => opt.MapFrom(s => s.Bins)); // Mapea la colección

            // Para la sub-lista de Bins
            CreateMap<CreateZoneCommand, Zone>();
            CreateMap<StorageBin, ZoneBinDto>();

            // Para Update (Command -> Entity)
            CreateMap<UpdateZoneCommand, Zone>();

        }
    }
}
