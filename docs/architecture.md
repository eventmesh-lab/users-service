# Arquitectura del Servicio (Architecture)

Este documento describe la arquitectura interna del microservicio de usuarios, siguiendo los principios de Clean Architecture y Domain-Driven Design (DDD).

## 🏗️ Arquitectura General

El servicio está estructurado en 4 capas principales, siguiendo el patrón de Clean Architecture:

```
┌─────────────────────────────────────────┐
│         API Layer (Controllers)         │  ← Capa de Presentación
├─────────────────────────────────────────┤
│    Application Layer (CQRS Handlers)    │  ← Lógica de Aplicación
├─────────────────────────────────────────┤
│      Domain Layer (Entities, VOs)       │  ← Núcleo del Negocio
├─────────────────────────────────────────┤
│  Infrastructure (Repositories, EF Core) │  ← Implementaciones Técnicas
└─────────────────────────────────────────┘
```

### Responsabilidades por Capa

#### 1. **Domain Layer** (`users-service.domain`)
El núcleo del negocio, libre de dependencias externas.

- **Entities**: `User` - Agregado raíz que representa un usuario
- **Value Objects**: 
  - `Email`: Validación de formato de correo electrónico
  - `Role`: Representación del rol del usuario (Usuario, Organizador, Administrador, Soporte)
- **Enumerables**: `Rol` - Enum con roles permitidos
- **Interfaces**: 
  - `IUserServices`: Contrato para operaciones de persistencia de usuarios
  - `IKeycloakRepository`: Contrato para operaciones con Keycloak
- **Exceptions**: `UsuarioDTOException` - Excepciones específicas del dominio

#### 2. **Application Layer** (`users-service.application`)
Orquesta la lógica de negocio usando el patrón CQRS con MediatR.

**Commands (escritura)**:
- `CreateUserCommand`: Registro de nuevos usuarios
- `UpdateUserCommand`: Actualización de datos del usuario
- `ChangePasswordCommand`: Cambio de contraseña

**Queries (lectura)**:
- `GetUserEmailQuery`: Obtener usuario por email
- `GetUsersQuery`: Listar todos los usuarios
- `GetIdByEmailQuery`: Obtener ID de usuario por email

**DTOs**:
- `UserCreateDTO`: Datos de entrada para registro
- `UpdateUserDTO`: Datos de entrada para actualización
- `ChangePasswordDTO`: Datos para cambio de contraseña
- `UserKeycloakDTO`: Representación de usuario en Keycloak

**Validators** (FluentValidation):
- `UserDTOValidator`: Validación de datos de creación
- `UpdateUserDTOValidator`: Validación de datos de actualización
- `ChangePasswordValidator`: Validación de contraseña

#### 3. **Infrastructure Layer** (`users-service.infrastructure`)
Implementaciones técnicas de persistencia y servicios externos.

- **Persistence**:
  - `AppDbContext`: Contexto de Entity Framework Core
  - `UserRepositoyPostgres`: Implementación del repositorio de usuarios para PostgreSQL
  - `Configurations`: Configuraciones de mapeo de entidades
  - `Migrations`: Migraciones de base de datos

- **ServiceInfrastructure**:
  - `KeycloakServiceInfrastracture`: Cliente HTTP para integración con Keycloak

#### 4. **API Layer** (`users-service.api`)
Capa de presentación con ASP.NET Core.

- **Controllers**: `UserControllers` - Expone endpoints REST
- **Program.cs**: Configuración de servicios y pipeline HTTP

## 🔄 Flujo de Datos

### Ejemplo: Registro de Usuario

```
1. HTTP POST /api/users/registerUser
   ↓
2. UserControllers.CreateUser()
   ├─ Validación: UserDTOValidator
   └─ Si es válido →
   ↓
3. Envía CreateUserCommand via MediatR
   ↓
4. CreateUserCommandHandler.Handle()
   ├─ Crea entidad User (Domain)
   ├─ Guarda en PostgreSQL (IUserServices)
   └─ Registra en Keycloak (IKeycloakRepository)
   ↓
5. Retorna respuesta HTTP 200 OK
```

### Flujo Detallado por Capas

```
┌──────────────────────────────────────────────────────────┐
│ Cliente HTTP                                              │
└────────────────────┬─────────────────────────────────────┘
                     │ Request (JSON)
                     ↓
┌──────────────────────────────────────────────────────────┐
│ API Layer (Controller)                                    │
│ ├─ Recibe Request                                         │
│ ├─ Valida DTO con FluentValidation                       │
│ └─ Envía Command/Query a MediatR                         │
└────────────────────┬─────────────────────────────────────┘
                     │ Command/Query
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Application Layer (Handler)                               │
│ ├─ Ejecuta lógica de negocio                            │
│ ├─ Coordina operaciones entre Domain e Infrastructure   │
│ └─ Mapea entre Entities y DTOs                          │
└────────────────────┬─────────────────────────────────────┘
                     │ Operaciones
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Domain Layer                                              │
│ ├─ Valida reglas de negocio                             │
│ ├─ Crea/modifica Entities                               │
│ └─ Aplica Value Objects                                  │
└────────────────────┬─────────────────────────────────────┘
                     │ Interfaces
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Infrastructure Layer                                      │
│ ├─ Persiste en PostgreSQL (EF Core)                     │
│ └─ Comunica con Keycloak (HTTP Client)                  │
└────────────────────┬─────────────────────────────────────┘
                     │ Respuesta
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Servicios Externos                                        │
│ ├─ PostgreSQL (Base de datos)                           │
│ └─ Keycloak (Autenticación)                             │
└──────────────────────────────────────────────────────────┘
```

## 🌐 Dependencias Externas

### 1. PostgreSQL
**Propósito**: Base de datos relacional para persistencia de usuarios.

**Configuración**: 
- Host: `postgres` (Docker) o `localhost` (local)
- Puerto: `5432`
- Base de datos: `users-service`
- Usuario: `postgres`

**Esquema de datos**:
```sql
Users (
  Id UUID PRIMARY KEY,
  FirstName VARCHAR,
  LastName VARCHAR,
  Email VARCHAR UNIQUE,
  PhoneNumber VARCHAR,
  Address VARCHAR,
  Birthdate TIMESTAMP,
  RoleUser VARCHAR
)
```

### 2. Keycloak
**Propósito**: Gestión de identidades, autenticación y autorización (IAM).

**Integración**:
- **Admin API**: Usado para gestión de usuarios y roles
- **Realms**: 
  - `master`: Administración
  - `myrealm`: Usuarios de la aplicación
- **Clients**: 
  - `admin-cli`: Cliente para operaciones administrativas
  - `aspnetcore`: Cliente para la aplicación

**Operaciones expuestas**:
- Registro de usuarios en Keycloak
- Asignación de roles de realm
- Cambio de contraseñas
- Obtención de tokens de acceso (OAuth2/OIDC)
- Actualización de perfiles de usuario

**URLs de API utilizadas**:
```
POST   /realms/{realm}/protocol/openid-connect/token
POST   /admin/realms/{realm}/users
GET    /admin/realms/{realm}/users?username={username}
PUT    /admin/realms/{realm}/users/{userId}
PUT    /admin/realms/{realm}/users/{userId}/reset-password
POST   /admin/realms/{realm}/users/{userId}/role-mappings/realm
GET    /admin/realms/{realm}/roles
```

### 3. Frontend (CORS)
**Propósito**: Aplicación web que consume esta API.

**CORS configurado para**:
- `http://localhost:3000` (desarrollo)
- `http://localhost:7181` (local)

## 📊 Modelo de Datos

### Entidad Principal: User

```csharp
public class User
{
    Guid Id                 // Identificador único
    string FirstName        // Nombre
    string LastName         // Apellido
    Email Email            // Value Object: email validado
    string PhoneNumber     // Teléfono de contacto
    string Address         // Dirección física
    DateTime Birthdate     // Fecha de nacimiento
    Role RoleUser          // Value Object: rol del usuario
}
```

### Value Objects

**Email**:
- Valida formato mediante regex
- Asegura que no sea nulo o vacío
- Inmutable (record type)

**Role**:
- Valida que sea uno de los roles permitidos
- Roles: `Usuario`, `Organizador`, `Administrador`, `Soporte`
- Inmutable

### Patrones de Diseño Utilizados

1. **Clean Architecture**: Separación por capas con dependencias hacia el interior
2. **CQRS** (Command Query Responsibility Segregation): Separación de lecturas y escrituras
3. **Repository Pattern**: Abstracción de acceso a datos
4. **Value Object Pattern**: Validación y encapsulación de valores del dominio
5. **Mediator Pattern**: Desacoplamiento entre Controllers y Handlers (MediatR)
6. **Dependency Injection**: Inyección de dependencias nativa de ASP.NET Core

## 🔐 Seguridad

### Autenticación
- Delegada completamente a Keycloak
- Tokens JWT (no implementado en el código actual)
- Contraseñas hasheadas por Keycloak (bcrypt)

### Validación
- DTOs validados con FluentValidation
- Value Objects validan formato de datos (Email, Role)
- Validación en múltiples capas (API y Domain)

## 🗄️ Migraciones de Base de Datos

El servicio aplica automáticamente las migraciones pendientes al iniciar (ver `Program.cs:67-82`):

```csharp
context.Database.Migrate();
```

Para crear nuevas migraciones:
```bash
dotnet ef migrations add NombreMigracion \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api
```

## ⚠️ Deuda Técnica Detectada

### 1. **Hardcoded URLs en KeycloakServiceInfrastracture**
**Ubicación**: `KeycloakServiceInfrastracture.cs:181, 213, 238`

> Nota: El nombre de la clase tiene un typo (`Infrastracture` en lugar de `Infrastructure`), ver item #6.

```csharp
var url = $"http://keycloak:8080/admin/realms/{realm}/users/{userId}/reset-password";
```

**Problema**: URLs hardcodeadas con `http://keycloak:8080` en lugar de usar `_config["Keycloak:BaseUrl"]`.

**Impacto**: 
- Dificulta deployment en diferentes entornos
- Inconsistencia con otras partes del código que sí usan configuración

**Recomendación**: Usar siempre `_config["Keycloak:BaseUrl"]` para todas las URLs.

---

### 2. **Creación de HttpClient en UpdateUserInKeycloakAsyncRepo**
**Ubicación**: `KeycloakServiceInfrastracture.cs:234`

```csharp
using var httpClient = new HttpClient();
```

**Problema**: Crea una nueva instancia de `HttpClient` en lugar de usar el inyectado.

**Impacto**:
- Puede causar agotamiento de sockets (socket exhaustion)
- No aprovecha connection pooling
- Anti-patrón conocido en .NET

**Recomendación**: Usar `_httpClient` inyectado vía constructor.

---

### 3. **Console.WriteLine en código de producción**
**Ubicación**: Múltiples archivos (UserControllers.cs:137, KeycloakServiceInfrastracture.cs:56, 174, 199, etc.)

```csharp
Console.WriteLine("Llegó al controlador");
```

**Problema**: Uso de `Console.WriteLine` para logging en producción.

**Impacto**:
- No se integra con sistemas de logging estructurado
- Dificulta debugging en producción
- No respeta niveles de log

**Recomendación**: Usar `ILogger<T>` de ASP.NET Core.

---

### 4. **Secreto de cliente en appsettings.json**
**Ubicación**: `appsettings.json:18`

```json
"ClientSecret": "your-client-secret"
```

**Problema**: Placeholder de secreto en archivo de configuración versionado.

**Impacto**:
- Riesgo de filtración de secretos si se comete el valor real
- Mala práctica de seguridad

**Recomendación**: 
- Usar User Secrets para desarrollo
- Usar Variables de entorno para producción
- Documentar claramente en README

---

### 5. **Namespace inconsistente**
**Ubicación**: `KeycloakServiceInfrastracture.cs:8`

```csharp
namespace MicroservicioUsuarios.Infrastructure.ServicesInfrastracture
```

**Problema**: Namespace en español (`MicroservicioUsuarios`) mientras el resto usa inglés (`users_service`).

**Impacto**:
- Inconsistencia en la nomenclatura
- Confusión para desarrolladores

**Recomendación**: Unificar namespaces a `users_service.infrastructure.ServiceInfrastructure`.

---

### 6. **Typo en nombre de clase y namespace**
**Ubicación**: Multiple archivos

> Nota: Estos son los nombres REALES en el código fuente (con el typo incluido).

```csharp
KeycloakServiceInfrastracture  // Debería ser "Infrastructure"
ServicesInfrastracture         // Debería ser "Infrastructure"
```

**Problema**: Typo en "Infrastracture" (correcto: "Infrastructure").

**Impacto**: 
- Apariencia poco profesional
- Dificulta búsquedas en el código

**Recomendación**: Refactorizar nombres a `KeycloakServiceInfrastructure`.

---

### 7. **Ausencia de autorización en endpoints**
**Ubicación**: Todos los endpoints en `UserControllers.cs`

**Problema**: No hay atributos `[Authorize]` ni validación de roles.

**Impacto**:
- Cualquiera puede acceder a todas las operaciones
- No se aprovecha la integración con Keycloak
- Vulnerabilidad de seguridad crítica

**Recomendación**: 
- Implementar `[Authorize]` con políticas
- Configurar authentication middleware
- Restringir operaciones sensibles por rol

---

### 8. **Gestión de excepciones genérica**
**Ubicación**: Handlers de Commands/Queries

**Problema**: Captura de excepciones genéricas sin logging adecuado.

**Impacto**:
- Dificulta debugging
- Pérdida de información valiosa de errores

**Recomendación**: 
- Implementar middleware global de excepciones
- Usar logging estructurado
- Crear excepciones de dominio específicas

---

### 9. **Reutilización de token de administrador para operaciones de usuario**
**Ubicación**: `KeycloakServiceInfrastracture.GetUserTokenAsync:92`

**Problema**: El método `GetUserTokenAsync` reutiliza el mismo `_accessToken` que el admin.

**Impacto**:
- Bug potencial: tokens de usuario y admin se sobrescriben mutuamente
- El token del usuario nunca se devuelve correctamente

**Recomendación**: Crear campos separados o no cachear tokens de usuario.

---

### 10. **Falta de paginación en GetUsersQuery**
**Ubicación**: `GetUsersQuery` y handler correspondiente

**Problema**: Endpoint que lista todos los usuarios sin paginación.

**Impacto**:
- Problemas de performance con muchos usuarios
- Consumo excesivo de memoria
- Timeout en requests

**Recomendación**: Implementar paginación con parámetros `page` y `pageSize`.

---

### Resumen de Prioridades

| Prioridad | Issue | Esfuerzo | Riesgo |
|-----------|-------|----------|--------|
| 🔴 ALTA | Ausencia de autorización | Medio | Crítico |
| 🔴 ALTA | HttpClient mal usado | Bajo | Alto |
| 🟡 MEDIA | Hardcoded URLs | Bajo | Medio |
| 🟡 MEDIA | Secretos en appsettings | Bajo | Medio |
| 🟡 MEDIA | Falta de paginación | Medio | Medio |
| 🟡 MEDIA | Bug en caching de tokens | Bajo | Medio |
| 🟢 BAJA | Console.WriteLine | Bajo | Bajo |
| 🟢 BAJA | Typos en nombres | Bajo | Bajo |
| 🟢 BAJA | Namespace inconsistente | Bajo | Bajo |
| 🟢 BAJA | Gestión de excepciones | Alto | Bajo |
