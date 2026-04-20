using AutoMapper;
using Inventory.Application.WorkOrders.Queries;
using Inventory.Domain;
using System;

namespace Inventory.Application.WorkOrders.Commons.Mappings
{
    public class WorkOrderMappingProfile : Profile
    {
        public WorkOrderMappingProfile()
        {
            CreateMap<WorkOrder, WorkOrderDto>()
                .ForMember(d => d.RecipeName,      o => o.MapFrom(s => s.ProductRecipe.Name))
                .ForMember(d => d.FinishedGoodName, o => o.MapFrom(s => s.FinishedGood.Name))
                .ForMember(d => d.Status,           o => o.MapFrom(s => s.Status.ToString()));

            CreateMap<ProductionPickTask, ProductionPickTaskDto>()
                .ForMember(d => d.MaterialName,  o => o.MapFrom(s => s.Material.Name))
                .ForMember(d => d.LpnCode,       o => o.MapFrom(s =>
                    s.SourceStockItem != null ? s.SourceStockItem.ReferenceNumber : string.Empty))
                .ForMember(d => d.LotNumber,     o => o.MapFrom(s =>
                    s.SourceStockItem != null && s.SourceStockItem.Lot != null
                        ? s.SourceStockItem.Lot.LotNumber : string.Empty))
                .ForMember(d => d.BinCode,       o => o.MapFrom(s =>
                    s.SourceStockItem != null && s.SourceStockItem.StorageBin != null
                        ? s.SourceStockItem.StorageBin.Code : string.Empty))
                .ForMember(d => d.ZoneName,      o => o.MapFrom(s =>
                    s.SourceStockItem != null && s.SourceStockItem.StorageBin != null
                        ? s.SourceStockItem.StorageBin.Zone.Name : string.Empty))
                .ForMember(d => d.LocationLabel, o => o.MapFrom(s =>
                    s.SourceStockItem != null && s.SourceStockItem.StorageBin != null
                        ? $"{s.SourceStockItem.StorageBin.Zone.Name} - {s.SourceStockItem.StorageBin.Code}"
                        : "Sin ubicación"))
                .ForMember(d => d.SourceBinId,   o => o.MapFrom(s =>
                    s.SourceStockItem != null ? s.SourceStockItem.StorageBinId : (Guid?)null))
                .ForMember(d => d.SourceZoneId,  o => o.MapFrom(s =>
                    s.SourceStockItem != null && s.SourceStockItem.StorageBin != null
                        ? (Guid?)s.SourceStockItem.StorageBin.ZoneId : null))
                .ForMember(d => d.Status,        o => o.MapFrom(s => s.Status.ToString()));
        }
    }
}
