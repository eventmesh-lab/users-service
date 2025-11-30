using users_service.application.Commands.Commands;
using users_service.application.Interfaces;
using users_service.application.Queries.Queries;
using users_service.domain.Interfaces;
using users_service.infrastructure.Persistence.Context;
using users_service.infrastructure.Persistence.Repositories;
using users_service.application.Services;
using MicroserviciosUsuarios.Infrastructure.Repositories.Keycloak;
using MicroservicioUsuarios.Infrastructure.ServicesInfrastracture;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// Add services to the container.

//crear variable para la cadena de conexion
var connectionString = builder.Configuration.GetConnectionString("ConnectionPostgre"); //ConnectionPostgre es el parametro de conexion que creamos en el appsetting
//registrar servicio para la conexion


builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString,
            b => b.MigrationsAssembly("users-service.infrastructure")));

//Inyeccion de dependencias
builder.Services.AddScoped<IUserRepositoryPostgres,UserRepositoyPostgres>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IKeycloakRepository, KeycloakRepository>();
builder.Services.AddScoped<IKeycloakServiceInfrastructure, KeycloakServiceInfrastracture>();
builder.Services.AddHttpClient<KeycloakServiceInfrastracture>();

// MediatR Configuration
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(CreateUserCommand).Assembly ,
    typeof(GetUserEmailQuery).Assembly,
    typeof(ChangePasswordCommand).Assembly,
    typeof(UpdateUserCommand).Assembly
   /* typeof(UpdateRolePermissionCommand).Assembly)*/));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowLocalhost3000");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Obtiene el DbContext
        var context = services.GetRequiredService<AppDbContext>(); 

        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones a la base de datos.");
    }
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
