using AutoMapper;
using Inventory.Application.SalesOrders.Queries;
using Inventory.Domain;
using System;

namespace Inventory.Application.SalesOrders.Commons.Mappings
{
    public class SalesOrderMappingProfile : Profile
    {
        public SalesOrderMappingProfile()
        {
            // ── SalesOrder → SalesOrderDto ─────────────────────────────────────
            CreateMap<SalesOrder, SalesOrderDto>()
                .ForMember(d => d.Status,        o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.SourceChannel, o => o.MapFrom(s => s.SourceChannel.ToString()));

            // ── SalesOrderLine → SalesOrderLineDto ────────────────────────────
            CreateMap<SalesOrderLine, SalesOrderLineDto>()
                .ForMember(d => d.MaterialName,
                    o => o.MapFrom(s => s.Material != null
                        ? s.Material.Name
                        : s.ExternalProductName ?? string.Empty))
                .ForMember(d => d.ExternalProductName, o => o.MapFrom(s => s.ExternalProductName))
                .ForMember(d => d.ExternalSku,         o => o.MapFrom(s => s.ExternalSku))
                .ForMember(d => d.Status,              o => o.MapFrom(s => s.Status.ToString()));

            // ── OutboundPickTask → OutboundPickTaskDto ─────────────────────────
            CreateMap<OutboundPickTask, OutboundPickTaskDto>()
                .ForMember(d => d.MaterialName,   o => o.MapFrom(s =>
                    s.Material != null ? s.Material.Name : string.Empty))
                .ForMember(d => d.LpnCode,        o => o.MapFrom(s =>
                    s.SourceStockItem != null ? s.SourceStockItem.ReferenceNumber : string.Empty))
                .ForMember(d => d.LotNumber,      o => o.MapFrom(s =>
                    s.SourceStockItem != null && s.SourceStockItem.Lot != null
                        ? s.SourceStockItem.Lot.LotNumber : string.Empty))
                .ForMember(d => d.BinCode,        o => o.MapFrom(s =>
                    s.SourceStockItem != null && s.SourceStockItem.StorageBin != null
                        ? s.SourceStockItem.StorageBin.Code : string.Empty))
                .ForMember(d => d.ZoneName,       o => o.MapFrom(s =>
                    s.SourceStockItem != null && s.SourceStockItem.StorageBin != null
                        ? s.SourceStockItem.StorageBin.Zone.Name : string.Empty))
                .ForMember(d => d.LocationLabel,  o => o.MapFrom(s =>
                    s.SourceStockItem != null && s.SourceStockItem.StorageBin != null
                        ? $"{s.SourceStockItem.StorageBin.Zone.Name} - {s.SourceStockItem.StorageBin.Code}"
                        : "Sin ubicación"))
                .ForMember(d => d.Status,         o => o.MapFrom(s => s.Status.ToString()));
        }
    }
}
