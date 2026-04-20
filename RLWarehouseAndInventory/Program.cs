using Inventory.Application;
using Inventory.Application.Auth.Interfaces;
using Inventory.Application.Auth.Services;
using Inventory.Application.Categories.Handlers;
using Inventory.Application.Integrations.Services;
using Inventory.Application.Tenant;
using Inventory.Domain;
using Inventory.Persistence;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            RoleClaimType = "role",
            // Usa la clave secreta larga de tu appsettings.json para verificar la firma
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Esta l�nea hace la magia: convierte todos los enums a texto en JSON y Swagger
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}); ;
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── HTTP Clients para integraciones externas ──────────────────────────────
builder.Services.AddHttpClient<WooCommerceApiService>();
builder.Services.AddHttpClient<ShopifyApiService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://bblazar-warehouse-crm-delta.vercel.app"
            ) // El puerto de tu Angular
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<InventoryDbContext>();

        // Opci�n A: Solo validar conexi�n (lanza excepci�n si falla)
        // context.Database.CanConnect(); 

        // Opci�n B (Recomendada en Dev): Aplica migraciones pendientes y crea la DB si no existe
        context.Database.Migrate();

        Console.WriteLine("--> Conexi�n a BD exitosa y migraciones aplicadas.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurri� un error durante la migraci�n o conexi�n a la base de datos.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowAngular");
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

app.Run();
