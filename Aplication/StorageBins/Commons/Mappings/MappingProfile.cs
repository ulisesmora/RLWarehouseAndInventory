using AutoMapper;
using Inventory.Application.StorageBins.Commands;
using Inventory.Application.StorageBins.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateStorageBinCommand, StorageBin>();

            CreateMap<StorageBin, StorageBinDto>()
                .ForMember(d => d.ZoneName, opt => opt.MapFrom(s => s.Zone.Name));

            // Map para Update (que haremos después)
            CreateMap<UpdateStorageBinCommand, StorageBin>();
        }
    }
}
