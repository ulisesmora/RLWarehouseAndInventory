using AutoMapper;
using Inventory.Application.Lots.Commands;
using Inventory.Application.Lots.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Lots.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Lot, LotDto>()
                .ForMember(d => d.MaterialName, opt => opt.MapFrom(s => s.Material.Name))

                // 1. Todo lo que hay en el edificio (Físico)
                .ForMember(d => d.QuantityOnHand,
                           opt => opt.MapFrom(s => s.StockItems.Sum(i => i.QuantityOnHand)))


                .ForMember(dest => dest.QuantityAvailable,
           opt => opt.MapFrom(src => src.StockItems.Sum(s => s.QuantityOnHand - s.QuantityReserved)))

                .ForMember(dest => dest.TotalQuantity,
           opt => opt.MapFrom(src => src.StockItems.Sum(s => s.QuantityOnHand)))


    .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name))
    .ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => src.Supplier.Id))
    .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Material.UnitOfMeasure.Name))

.ForMember(dest => dest.QuantityReserved,
           opt => opt.MapFrom(src => src.StockItems.Sum(s => s.QuantityReserved)));



            CreateMap<CreateLotCommand, Lot>();
            CreateMap<UpdateLotCommand, Lot>();
        }
    }
}
