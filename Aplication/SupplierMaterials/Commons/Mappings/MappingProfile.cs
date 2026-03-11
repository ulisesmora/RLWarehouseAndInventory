using AutoMapper;
using Inventory.Application.SupplierMaterials.Commands;
using Inventory.Application.SupplierMaterials.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.SupplierMaterials.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SupplierMaterial, SupplierMaterialDto>()
    .ForMember(d => d.SupplierName, opt => opt.MapFrom(s => s.Supplier.Name))
    .ForMember(d => d.MaterialName, opt => opt.MapFrom(s => s.Material.Name));

            CreateMap<CreateSupplierMaterialCommand, SupplierMaterial>();
            CreateMap<UpdateSupplierMaterialCommand, SupplierMaterial>();
        }
    }
}
