using AutoMapper;
using Inventory.Application.StatusCatalogs.Commands;
using Inventory.Application.StatusCatalogs.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StatusCatalogs.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<StatusCatalog, StatusCatalogDto>();

            CreateMap<CreateStatusCatalogCommand, StatusCatalog>();
            CreateMap<UpdateStatusCatalogCommand, StatusCatalog>();
        }
    }
}
