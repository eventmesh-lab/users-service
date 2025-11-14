using Aplication.Commands.Commands;
using Aplication.Interfaces;
using Aplication.Queries.Queries;
using Domain.Interfaces;
using Insfrastructure.Persistence.Context;
using Insfrastructure.Persistence.Repositories;
using Insfrastructure.Services;
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
            b => b.MigrationsAssembly("Infrastructure")));

//Inyeccion de dependencias
builder.Services.AddScoped<IUserRepositoryPostgres,UserRepositoyPostgres>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IKeycloakRepository, KeycloakRepository>();
builder.Services.AddHttpClient<KeycloakServiceInfrastracture>();

// MediatR Configuration
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(CreateUserCommand).Assembly ,
    typeof(GetUserEmailCommand).Assembly,
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
