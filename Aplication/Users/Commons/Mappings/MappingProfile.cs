using AutoMapper;
using Inventory.Application.Users.Commands;
using Inventory.Application.Users.Queries;
using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Commons.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // De Entity a DTO (El enum se convierte a string automáticamente si AutoMapper está configurado, o definimos el ToString)
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // De Command a Entity
            CreateMap<CreateUserCommand, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // El hash se hace en el handler
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore()); // Se saca del CurrentUserService

            CreateMap<UpdateUserCommand, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Protegemos el ID
        }
    }
}
