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
                 // Navegación hacia Material
                 .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material.Name))
                 .ForMember(dest => dest.MaterialSKU, opt => opt.MapFrom(src => src.Material.SKU))

                 // Navegación hacia Warehouse y Status
                 .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
                 .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.Name))

                 // Navegación hacia Lot (Ojo: Lot puede ser nulo, AutoMapper lo maneja bien)
                 .ForMember(dest => dest.LotNumber, opt => opt.MapFrom(src => src.Lot.LotNumber))

                 // Navegación hacia StorageBin
                 // Asumiendo que tu entidad StorageBin tiene una propiedad 'Code' o 'Name'. Cambia "Code" por lo que uses.
                 .ForMember(dest => dest.StorageBinCode, opt => opt.MapFrom(src => src.StorageBin.Code))
                 .ForMember(dest => dest.ZoneId, opt => opt.MapFrom(src => src.StorageBin != null ? src.StorageBin.ZoneId : (Guid?)null))
    .ForMember(dest => dest.ZoneName, opt => opt.MapFrom(src => src.StorageBin != null ? src.StorageBin.Zone.Name : null))

                 // El Enum ContainerType se convierte a String automáticamente por AutoMapper, 
                 // pero si quieres estar 100% seguro:
                 .ForMember(dest => dest.ContainerType, opt => opt.MapFrom(src => src.ContainerType.ToString()))
                 .ForMember(dest => dest.UnitOfMeasureName, opt => opt.MapFrom(src => src.Material.UnitOfMeasure.Name))
    .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.Lot.ExpirationDate));

            CreateMap<StockMovement, StockMovementDto>()
                // Convertir Enum a String
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))

                // Mapear el Material
                .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material.Name))

                // Mapear el Almacén
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))

                // Mapear la Ubicación (si existe)
                .ForMember(dest => dest.StorageBinCode, opt => opt.MapFrom(src => src.StorageBin.Code))

                // Mapear el Lote (si existe)
                .ForMember(dest => dest.LotNumber, opt => opt.MapFrom(src => src.Lot.LotNumber))

                // 🔥 Mapear el LPN del contenedor directamente desde la relación StockItem
                .ForMember(dest => dest.StockItemLpn, opt => opt.MapFrom(src => src.StockItem.ReferenceNumber));
        }
    }
}
