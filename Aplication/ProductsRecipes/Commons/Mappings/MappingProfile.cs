using AutoMapper;
using Inventory.Application.ProductsRecipes.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.ProductsRecipes.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductRecipe, ProductRecipeDto>()
    .ForMember(d => d.FinishedGoodName, opt => opt.MapFrom(s => s.FinishedGood.Name));

            CreateMap<RecipeIngredient, RecipeIngredientDto>()
                .ForMember(d => d.MaterialName, opt => opt.MapFrom(s => s.Material.Name));

            CreateMap<RecipeCost, RecipeCostDto>()
                .ForMember(d => d.CostType, opt => opt.MapFrom(s => s.CostType.ToString()));
        }
    }
}
