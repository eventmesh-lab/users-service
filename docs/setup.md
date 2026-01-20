# Guía de Configuración (Setup Guide)

Guía completa de configuración y despliegue del microservicio de usuarios.

## 📋 Prerequisitos

### Para desarrollo local

- **.NET 8.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker** y **Docker Compose** - [Descargar](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 16** (opcional si usas Docker)
- **IDE recomendado**: Visual Studio 2022, Rider, o VS Code con extensión C#

### Para producción

- **.NET 8.0 Runtime (ASP.NET Core)**
- **PostgreSQL 16** o superior
- **Keycloak 26.0** o superior

---

## 🔧 Variables de Entorno

### Tabla Completa de Configuración

| Variable | Tipo | Requerido | Default | Descripción |
|----------|------|-----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | string | No | Production | Entorno de ejecución: `Development`, `Staging`, `Production` |
| `ASPNETCORE_URLS` | string | No | http://localhost:5000 | URLs donde escucha el servicio (ej: `http://+:7181`) |
| `ConnectionStrings__ConnectionPostgre` | string | Sí | - | Cadena de conexión a PostgreSQL |
| `Keycloak__BaseUrl` | string | Sí | - | URL base de Keycloak (ej: `http://keycloak:8080`) |
| `Keycloak__AdmRealm` | string | Sí | master | Realm de administración de Keycloak |
| `Keycloak__UserRealm` | string | Sí | myrealm | Realm de usuarios de la aplicación |
| `Keycloak__AdmClientId` | string | Sí | admin-cli | Client ID para operaciones administrativas |
| `Keycloak__UserClientId` | string | Sí | aspnetcore | Client ID de la aplicación |
| `Keycloak__ClientSecret` | string | Sí | - | Secret del cliente (no versionar) |
| `Cors__AllowedOrigins__0` | string | No | - | Primera URL permitida por CORS |
| `Cors__AllowedOrigins__1` | string | No | - | Segunda URL permitida por CORS |

### Detalles de Variables Críticas

#### ConnectionStrings__ConnectionPostgre

Formato de cadena de conexión para PostgreSQL:

```bash
# Formato estándar
Host=<hostname>;Port=<port>;Database=<dbname>;Username=<user>;Password=<password>

# Ejemplo local
Host=localhost;Port=5432;Database=users-service;Username=postgres;Password=postgres

# Ejemplo Docker
Host=postgres;Port=5432;Database=users-service;Username=postgres;Password=postgres

# Ejemplo producción con SSL
Host=prod-db.example.com;Port=5432;Database=users-service;Username=app_user;Password=SecurePass123;SSL Mode=Require
```

#### Keycloak__BaseUrl

URL completa (incluye protocolo y puerto) sin trailing slash:

```bash
# Desarrollo con Docker
http://keycloak:8080

# Desarrollo local
http://localhost:8180

# Producción
https://auth.example.com
```

#### Keycloak__ClientSecret

Secret del cliente OAuth2 de Keycloak. **NUNCA versionar este valor**.

Cómo obtenerlo:
1. Accede a Keycloak Admin Console
2. Navega a: Realms → [tu-realm] → Clients → [tu-client]
3. Pestaña "Credentials"
4. Copia el "Client Secret"

---

## 🐳 Docker

### Construcción de la Imagen

El proyecto incluye un `Dockerfile` multi-stage optimizado.

#### Build manual

```bash
# Desde la raíz del proyecto
docker build -t users-service:latest .

# Build con tag específico
docker build -t users-service:1.0.0 .

# Build para arquitectura específica
docker build --platform linux/amd64 -t users-service:latest .
```

#### Análisis del Dockerfile

```dockerfile
# Stage 1: Build (imagen SDK completa - ~900MB)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solo .csproj y restaura (aprovecha cache de Docker)
COPY src/users-service.domain/users-service.domain.csproj ./src/users-service.domain/
COPY src/users-service.application/users-service.application.csproj ./src/users-service.application/
COPY src/users-service.infrastructure/users-service.infrastructure.csproj ./src/users-service.infrastructure/
COPY src/users-service.api/users-service.api.csproj ./src/users-service.api/

# Restaura todas las dependencias
WORKDIR /src/src/users-service.api
RUN dotnet restore

# Copia el resto del código fuente
WORKDIR /src
COPY src/ ./src/

# Compila y publica en modo Release
WORKDIR /src/src/users-service.api
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime (imagen runtime ligera - ~220MB)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 7181
ENTRYPOINT ["dotnet", "users-service.api.dll"]
```

**Ventajas del multi-stage build**:
- Imagen final ligera (~220MB vs ~900MB)
- No incluye herramientas de build en producción
- Mejor seguridad (menos superficie de ataque)

### Docker Compose

El proyecto incluye `docker-compose.yml` con tres servicios: PostgreSQL, Keycloak y la API.

#### Levantar todos los servicios

```bash
# Levantar en modo detached (background)
docker-compose up -d

# Levantar y ver logs en tiempo real
docker-compose up

# Levantar solo un servicio específico
docker-compose up -d postgres
```

#### Ver logs

```bash
# Todos los servicios
docker-compose logs -f

# Solo API
docker-compose logs -f api

# Solo Keycloak
docker-compose logs -f keycloak

# Últimas 100 líneas
docker-compose logs --tail=100 api
```

#### Comandos útiles

```bash
# Ver estado de servicios
docker-compose ps

# Detener todos los servicios
docker-compose stop

# Detener y eliminar contenedores
docker-compose down

# Detener y eliminar contenedores + volúmenes (datos)
docker-compose down -v

# Reconstruir imágenes antes de levantar
docker-compose up --build -d

# Escalar servicio API (múltiples instancias)
docker-compose up -d --scale api=3
```

#### Health Checks

Los servicios incluyen health checks para asegurar disponibilidad:

**PostgreSQL**:
```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U postgres"]
  interval: 10s
  timeout: 5s
  retries: 5
```

**Keycloak**:
```yaml
healthcheck:
  test: ["CMD-SHELL", "/opt/keycloak/bin/kc.sh health --check-ready || exit 1"]
  interval: 10s
  timeout: 5s
  retries: 30
  start_period: 60s
```

#### Volúmenes persistentes

El docker-compose crea tres volúmenes para persistencia de datos:

```yaml
volumes:
  postgres_data:    # Datos de PostgreSQL
  keycloak_data:    # Configuración y datos de Keycloak
  api_data:         # Datos de la aplicación (si es necesario)
```

Gestionar volúmenes:
```bash
# Listar volúmenes
docker volume ls

# Inspeccionar volumen
docker volume inspect users-service_postgres_data

# Eliminar volúmenes no usados
docker volume prune
```

---

## ⚙️ Scripts y Comandos .NET

### Comandos Comunes

#### Restaurar dependencias

```bash
# Restaurar todas las dependencias del solution
dotnet restore

# Restaurar proyecto específico
dotnet restore src/users-service.api/users-service.api.csproj
```

#### Compilar

```bash
# Compilar en modo Debug
dotnet build

# Compilar en modo Release
dotnet build -c Release

# Compilar proyecto específico
dotnet build src/users-service.api/users-service.api.csproj

# Compilar sin restaurar (más rápido si ya restauraste)
dotnet build --no-restore
```

#### Ejecutar

```bash
# Ejecutar proyecto API
dotnet run --project src/users-service.api

# Ejecutar con perfil específico
dotnet run --project src/users-service.api --launch-profile Development

# Ejecutar con variables de entorno
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/users-service.api
```

#### Publicar

```bash
# Publicar para despliegue
dotnet publish -c Release -o ./publish

# Publicar self-contained (incluye runtime)
dotnet publish -c Release --self-contained true -r linux-x64 -o ./publish

# Publicar con single-file
dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true -o ./publish
```

### Migraciones de Entity Framework Core

#### Crear migración

```bash
# Crear nueva migración
dotnet ef migrations add NombreDeLaMigracion \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api

# Ejemplo real
dotnet ef migrations add AddUserPhoneNumber \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api
```

#### Aplicar migraciones

```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api

# Aplicar hasta una migración específica
dotnet ef database update NombreDeLaMigracion \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api

# Revertir todas las migraciones (cuidado!)
dotnet ef database update 0 \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api
```

#### Listar migraciones

```bash
# Ver historial de migraciones
dotnet ef migrations list \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api
```

#### Eliminar última migración

```bash
# Eliminar migración (solo si no se aplicó a la BD)
dotnet ef migrations remove \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api
```

#### Generar script SQL

```bash
# Generar script SQL de todas las migraciones
dotnet ef migrations script \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api \
  --output migrations.sql

# Script incremental (desde migración A hasta B)
dotnet ef migrations script MigracionA MigracionB \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api \
  --output migration-A-to-B.sql
```

### Tests

#### Ejecutar tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests de un proyecto específico
dotnet test tests/users-service.api.Tests/

# Ejecutar tests con verbosidad detallada
dotnet test --verbosity detailed

# Ejecutar tests en paralelo
dotnet test --parallel
```

#### Tests con cobertura

```bash
# Cobertura con coverlet
dotnet test /p:CollectCoverage=true

# Cobertura en formato HTML
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Ver reporte de cobertura (requiere ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

#### Ejecutar tests por categoría

```bash
# Solo tests unitarios (si están categorizados)
dotnet test --filter Category=Unit

# Solo tests de integración
dotnet test --filter Category=Integration
```

### Herramientas de desarrollo

#### Instalar Entity Framework Core Tools

```bash
# Instalar EF Core CLI tools globalmente
dotnet tool install --global dotnet-ef

# Actualizar a última versión
dotnet tool update --global dotnet-ef

# Verificar instalación
dotnet ef --version
```

#### User Secrets (para desarrollo)

```bash
# Inicializar user secrets en el proyecto API
dotnet user-secrets init --project src/users-service.api

# Agregar un secreto
dotnet user-secrets set "Keycloak:ClientSecret" "tu-secreto-aqui" \
  --project src/users-service.api

# Listar todos los secretos
dotnet user-secrets list --project src/users-service.api

# Eliminar un secreto
dotnet user-secrets remove "Keycloak:ClientSecret" \
  --project src/users-service.api

# Limpiar todos los secretos
dotnet user-secrets clear --project src/users-service.api
```

#### Watch mode (desarrollo)

```bash
# Ejecutar con hot reload (reinicia automáticamente)
dotnet watch --project src/users-service.api

# Watch con navegador abierto
dotnet watch --project src/users-service.api --launch-profile Development
```

---

## 🚀 Configuración por Entorno

### Development

**Archivo**: `appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "ConnectionPostgre": "Host=localhost;Port=5432;Database=users-service;Username=postgres;Password=postgres"
  },
  "Keycloak": {
    "BaseUrl": "http://localhost:8180",
    "AdmRealm": "master",
    "UserRealm": "myrealm",
    "AdmClientId": "admin-cli",
    "UserClientId": "aspnetcore",
    "ClientSecret": "usar-user-secrets"
  }
}
```

**Configurar User Secrets**:
```bash
dotnet user-secrets set "Keycloak:ClientSecret" "PzaioIxlVKVINnJ7VJwCILdBoUlUWB05" \
  --project src/users-service.api
```

### Staging

Usar variables de entorno o archivos de configuración específicos.

```bash
export ASPNETCORE_ENVIRONMENT=Staging
export ConnectionStrings__ConnectionPostgre="Host=staging-db.example.com;Port=5432;Database=users-service;Username=appuser;Password=stagingpass"
export Keycloak__BaseUrl="https://auth-staging.example.com"
export Keycloak__ClientSecret="staging-secret"
```

### Production

**⚠️ IMPORTANTE**: Nunca versionar secretos de producción.

#### Opción 1: Variables de entorno

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://+:80"
export ConnectionStrings__ConnectionPostgre="Host=prod-db.internal;Port=5432;Database=users-service;Username=app_user;Password=${DB_PASSWORD};SSL Mode=Require"
export Keycloak__BaseUrl="https://auth.example.com"
export Keycloak__ClientSecret="${KEYCLOAK_SECRET}"
```

#### Opción 2: Azure Key Vault / AWS Secrets Manager

```csharp
// En Program.cs (ejemplo para Azure)
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

#### Opción 3: Kubernetes Secrets

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: users-service-secrets
type: Opaque
stringData:
  connection-string: "Host=postgres;Port=5432;..."
  keycloak-secret: "production-secret"
```

---

## 🔍 Verificación de Instalación

### 1. Verificar servicios con Docker Compose

```bash
# Levantar servicios
docker-compose up -d

# Verificar estado (todos deben estar "healthy")
docker-compose ps

# Deberías ver:
# NAME              STATUS              PORTS
# users-api         Up (healthy)        0.0.0.0:7181->7181/tcp
# users-postgres    Up (healthy)        0.0.0.0:5432->5432/tcp
# keycloak          Up (healthy)        0.0.0.0:8180->8080/tcp
```

### 2. Verificar API

```bash
# Verificar que la API responde
curl http://localhost:7181/api/users/getUsers

# Acceder a Swagger UI
open http://localhost:7181/swagger
```

### 3. Verificar PostgreSQL

```bash
# Conectar con psql
docker exec -it users-postgres psql -U postgres -d users-service

# Listar tablas
\dt

# Salir
\q
```

### 4. Verificar Keycloak

```bash
# Acceder a Admin Console
open http://localhost:8180

# Credenciales por defecto:
# Usuario: admin
# Contraseña: admin
```

### 5. Test end-to-end

```bash
# Script de prueba completo
#!/bin/bash

echo "1. Registrando usuario..."
curl -X POST http://localhost:7181/api/users/registerUser \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Test",
    "lastName": "User",
    "email": "test@example.com",
    "phoneNumber": "+573001234567",
    "address": "Test Address",
    "birthdate": "1990-01-01T00:00:00",
    "roleUser": "Usuario",
    "password": "TestPass123!"
  }'

echo -e "\n2. Obteniendo usuario..."
curl http://localhost:7181/api/users/getUser/test@example.com

echo -e "\n3. Listando usuarios..."
curl http://localhost:7181/api/users/getUsers

echo -e "\n✅ Test completado!"
```

---

## 🛠️ Troubleshooting

### Problema: Migraciones no se aplican

**Síntoma**: Error al iniciar: `Cannot connect to database` o `Table 'Users' doesn't exist`

**Solución**:
```bash
# Aplicar migraciones manualmente
dotnet ef database update \
  --project src/users-service.infrastructure \
  --startup-project src/users-service.api
```

### Problema: No se puede conectar a Keycloak

**Síntoma**: `Failed to create user in Keycloak: Connection refused`

**Causas comunes**:
1. Keycloak no está levantado: `docker-compose up -d keycloak`
2. URL incorrecta en configuración
3. Keycloak aún no está ready (espera ~60s después de iniciar)

**Verificar**:
```bash
# Ver logs de Keycloak
docker-compose logs keycloak

# Verificar health
curl http://localhost:8180/health/ready
```

### Problema: Puerto 7181 ya está en uso

**Solución**:
```bash
# Opción 1: Cambiar puerto en docker-compose.yml
ports:
  - "8080:7181"  # Usa puerto 8080 externamente

# Opción 2: Matar proceso que usa el puerto (Linux/Mac)
lsof -ti:7181 | xargs kill -9

# Opción 3: Matar proceso (Windows)
netstat -ano | findstr :7181
taskkill /PID <PID> /F
```

### Problema: Error de permisos en volúmenes Docker

**Síntoma**: `Permission denied` en logs de PostgreSQL

**Solución**:
```bash
# Linux: Dar permisos a volúmenes
sudo chown -R $USER:$USER $(docker volume inspect users-service_postgres_data --format '{{ .Mountpoint }}')

# O recrear volúmenes
docker-compose down -v
docker-compose up -d
```

---

## 📦 Despliegue en Producción

### Checklist pre-despliegue

- [ ] Configurar secretos de forma segura (NO en appsettings.json)
- [ ] Habilitar HTTPS con certificados válidos
- [ ] Configurar logging estructurado (Serilog, Application Insights)
- [ ] Habilitar autenticación/autorización en endpoints
- [ ] Configurar rate limiting
- [ ] Revisar CORS para producción
- [ ] Configurar health checks y readiness probes
- [ ] Configurar backups automáticos de PostgreSQL
- [ ] Revisar límites de recursos (CPU, memoria)
- [ ] Configurar monitoreo y alertas
- [ ] Documentar runbook de operaciones

### Comandos de despliegue ejemplo

```bash
# Build optimizado para producción
dotnet publish -c Release -o ./publish

# Crear imagen Docker
docker build -t users-service:1.0.0 -t users-service:latest .

# Push a registry (ejemplo con Docker Hub)
docker tag users-service:latest myregistry/users-service:1.0.0
docker push myregistry/users-service:1.0.0

# Deploy con Kubernetes (ejemplo)
kubectl apply -f k8s/deployment.yaml
kubectl rollout status deployment/users-service
```
