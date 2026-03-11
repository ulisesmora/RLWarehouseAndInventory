using AutoMapper;
using Inventory.Application.Materials.Commands;
using Inventory.Application.Materials.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Materials.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeo simple: Coincide nombres de propiedades automáticamente
            CreateMap<CreateMaterialCommand, Material>()
                // Si necesitas valores por defecto que no vienen en el comando:
                .ForMember(dest => dest.IsStockable, opt => opt.MapFrom(src => true));

            // En MappingProfile.cs
            CreateMap<UpdateMaterialCommand, Material>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            // Opcional: Solo mapea si no es nulo, o mapeo directo normal


            CreateMap<Material, MaterialDto>()
    .ForMember(dest => dest.UnitOfMeasureName, opt => opt.MapFrom(src => src.UnitOfMeasure.Name))
    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
    .ForMember(dest => dest.CategoryDescription, opt => opt.MapFrom(src => src.Category.Description));

        }


    }
}
