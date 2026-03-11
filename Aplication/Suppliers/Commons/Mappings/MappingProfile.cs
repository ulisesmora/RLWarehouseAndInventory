using AutoMapper;
using Inventory.Application.Suppliers.Queries;
using Inventory.Application.Suppliers.Commands;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Suppliers.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Supplier, SupplierDto>(); // De BD a Frontend

            CreateMap<CreateSupplierCommand, Supplier>(); // De Comando a BD
            CreateMap<UpdateSupplierCommand, Supplier>(); // Para actualizar

        }
    }
}
