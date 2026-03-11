using AutoMapper;
using Inventory.Application.UnitOfMesaure.Commands;
using Inventory.Application.UnitOfMesaure.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeo simple: Coincide nombres de propiedades automáticamente
            CreateMap<CreateUnitOfMeasureCommand, UnitOfMeasure>();
            CreateMap<UpdateUnitOfMeasureCommand, UnitOfMeasure>();
            CreateMap<UnitOfMeasure, UnitOfMeasureDto>();
            CreateMap<DeleteUnitOfMeasureCommand, Guid>();



        }
    }
}
