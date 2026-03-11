using AutoMapper;
using Inventory.Application.Categories.Commands;
using Inventory.Application.Categories.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Commons.Mappings
{
    public class MappingProfiler : Profile
    {
        public MappingProfiler() {
            CreateMap<CreateCategoryCommand, Category>();
            CreateMap<UpdateCategoryCommand, Category>();
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.ParentCategoryName, opt => opt.MapFrom(s => s.ParentCategory.Name));

            CreateMap<DeleteCategoryCommand, Guid>();
        }

    }
}
