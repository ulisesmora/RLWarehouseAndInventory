using AutoMapper;
using Inventory.Application.Warehouses.Commands;
using Inventory.Application.Warehouses.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {
            CreateMap<Warehouse, WarehouseDto>()
    .ForMember(d => d.ZonesCount, opt => opt.MapFrom(s => s.Zones.Count));

            CreateMap<UpdateWarehouseCommand, Warehouse>();
            CreateMap<CreateWarehouseCommand, Warehouse>();
        }
       
    }
}
