using AutoMapper;
using Inventory.Application.StockMovements.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StockMovements.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<StockItem, StockItemDto>()
    .ForMember(d => d.WarehouseName, opt => opt.MapFrom(s => s.Warehouse.Name))
    .ForMember(d => d.StorageBinCode, opt => opt.MapFrom(s => s.StorageBin.Code))
    .ForMember(d => d.LotNumber, opt => opt.MapFrom(s => s.Lot.LotNumber))
    .ForMember(d => d.StatusName, opt => opt.MapFrom(s => s.Status.Name));

            CreateMap<StockMovement, StockMovementDto>()
                .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString())) // Convierte el Enum a texto
                .ForMember(d => d.WarehouseName, opt => opt.MapFrom(s => s.Warehouse.Name))
                .ForMember(d => d.StorageBinCode, opt => opt.MapFrom(s => s.StorageBin.Code))
                .ForMember(d => d.LotNumber, opt => opt.MapFrom(s => s.Lot.LotNumber));
        }
    }
}
