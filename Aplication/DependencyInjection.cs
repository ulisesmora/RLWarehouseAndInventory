using FluentValidation;
using Inventory.Application.Materials.Commons.Mappings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Inventory.Application
{
    public static class DependencyInjection
    {
        // Este es el método que tu Program.cs no encuentra
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // 1. Registrar AutoMapper
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(assembly);
            });

            // 2. Registrar Validadores
            services.AddValidatorsFromAssembly(assembly);

            // 3. Registrar AutoMapper (LA CORRECCIÓN)
            // En lugar de pasar 'assembly', usamos 'typeof(MappingProfile)'
            // Esto escanea automáticamente el assembly donde está tu perfil de mapeo.
            services.AddAutoMapper(cfg =>
            {
                // Esto busca todas las clases que hereden de 'Profile' en este proyecto
                cfg.AddMaps(assembly);
            });

            return services;
        }
    }
}
