# Users Service

Microservicio de gestión de usuarios para EventMesh Lab, implementado con arquitectura limpia (Clean Architecture) siguiendo principios de Domain-Driven Design (DDD).

## 🎯 Descripción

Este servicio resuelve la gestión centralizada de usuarios en la plataforma EventMesh, proporcionando:

- **Registro y autenticación**: Integración con Keycloak para gestión de identidades
- **Gestión de perfiles**: CRUD completo de información de usuarios
- **Control de roles**: Asignación de roles (Usuario, Organizador, Administrador, Soporte)
- **Cambio de contraseñas**: Funcionalidad segura de recuperación y cambio de credenciales

## 📚 Tabla de Contenidos

- [Arquitectura](docs/architecture.md) - Flujo de datos, capas y dependencias externas
- [API Documentation](docs/api.md) - Endpoints, contratos y ejemplos
- [Setup Guide](docs/setup.md) - Guía completa de configuración y despliegue

## 🛠️ Stack Tecnológico

| Componente | Tecnología | Versión |
|------------|-----------|---------|
| Runtime | .NET | 8.0 |
| Framework | ASP.NET Core | 8.0 |
| Base de Datos | PostgreSQL | 16 |
| Auth/AuthZ | Keycloak | 26.0 |
| ORM | Entity Framework Core | 9.0 |
| Patrón CQRS | MediatR | 13.0 |
| Validación | FluentValidation | - |
| Containerización | Docker | - |

## 🚀 Quick Start

### Prerequisitos
- Docker y Docker Compose instalados
- .NET 8.0 SDK (solo para desarrollo)

### Levantar el servicio con Docker Compose

```bash
# Clona el repositorio
git clone https://github.com/eventmesh-lab/users-service.git
cd users-service

# Levanta todos los servicios (PostgreSQL, Keycloak, API)
docker-compose up -d

# Verifica que los servicios estén corriendo
docker-compose ps
```

El servicio estará disponible en:
- **API**: http://localhost:7181
- **Swagger UI**: http://localhost:7181/swagger
- **Keycloak**: http://localhost:8180 (admin/admin)

### Desarrollo local sin Docker

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar migraciones
dotnet ef database update --project src/users-service.infrastructure --startup-project src/users-service.api

# Ejecutar el servicio
dotnet run --project src/users-service.api
```

## 📝 Ejemplo de uso rápido

```bash
# Registrar un nuevo usuario
curl -X POST http://localhost:7181/api/users/registerUser \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan.perez@example.com",
    "phoneNumber": "+573001234567",
    "address": "Calle 123 #45-67",
    "birthdate": "1990-05-15T00:00:00",
    "roleUser": "Usuario",
    "password": "SecurePass123!"
  }'
```

## 🧪 Tests

```bash
# Ejecutar todos los tests
dotnet test

# Tests con cobertura
dotnet test /p:CollectCoverage=true
```

## 📦 Estructura del Proyecto

```
users-service/
├── src/
│   ├── users-service.api/          # Capa de presentación (Controllers, Program.cs)
│   ├── users-service.application/   # Capa de aplicación (Commands, Queries, DTOs)
│   ├── users-service.domain/        # Capa de dominio (Entities, Value Objects)
│   └── users-service.infrastructure/ # Capa de infraestructura (Repositories, EF Context)
├── tests/                           # Tests unitarios e integración
├── docs/                            # Documentación técnica
├── Dockerfile                       # Imagen Docker del servicio
└── docker-compose.yml              # Orquestación de servicios
```

## 🔗 Enlaces útiles

- [Documentación de .NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Clean Architecture Pattern](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 📄 Licencia

Este proyecto es parte de EventMesh Lab.
