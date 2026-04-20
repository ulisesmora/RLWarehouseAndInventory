using AutoMapper;
using Inventory.Application.ProductRecipes.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductRecipes.Commons.Mappings
{
    public class ProductRecipeMappingProfile : Profile
    {
        public ProductRecipeMappingProfile()
        {
            CreateMap<ProductRecipe, ProductRecipeDto>()
                .ForMember(dest => dest.FinishedGoodName, opt => opt.MapFrom(src => src.FinishedGood.Name));

            CreateMap<RecipeIngredient, RecipeIngredientDto>()
                .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material.Name))
                .ForMember(dest => dest.UnitOfMeasure, opt => opt.MapFrom(src => src.Material.UnitOfMeasure.Name));

            CreateMap<RecipeCost, RecipeCostDto>();
        }
    }
}
