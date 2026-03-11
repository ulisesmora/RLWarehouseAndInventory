using Inventory.Application;
using Inventory.Application.Categories.Handlers;
using Inventory.Persistence;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Esta línea hace la magia: convierte todos los enums a texto en JSON y Swagger
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}); ;
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // El puerto de tu Angular
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<InventoryDbContext>();

        // Opción A: Solo validar conexión (lanza excepción si falla)
        // context.Database.CanConnect(); 

        // Opción B (Recomendada en Dev): Aplica migraciones pendientes y crea la DB si no existe
        context.Database.Migrate();

        Console.WriteLine("--> Conexión a BD exitosa y migraciones aplicadas.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error durante la migración o conexión a la base de datos.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();
app.UseCors("AllowAngular");
app.MapControllers();

app.Run();
