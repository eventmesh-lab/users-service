# Documentación de API (API Documentation)

Documentación de los endpoints REST expuestos por el servicio de usuarios.

## Base URL

```
http://localhost:7181/api/users
```

## Headers Comunes

```http
Content-Type: application/json
Accept: application/json
```

> **Nota**: Actualmente no hay autenticación implementada en los endpoints. Ver [Deuda Técnica](architecture.md#7-ausencia-de-autorización-en-endpoints).

## 📋 Índice de Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/registerUser` | Registra un nuevo usuario |
| GET | `/getUser/{email}` | Obtiene usuario por email |
| POST | `/changePassword/{email}` | Cambia la contraseña de un usuario |
| PUT | `/updateUser/{email}` | Actualiza datos de un usuario |
| GET | `/getUsers` | Lista todos los usuarios |
| GET | `/getIdUser/{email}` | Obtiene el ID de un usuario por email |

---

## Endpoints Detallados

### 1. Registrar Usuario

Registra un nuevo usuario en el sistema y en Keycloak.

```http
POST /api/users/registerUser
```

#### Request Body

```json
{
  "firstName": "string",      // Requerido: Nombre del usuario
  "lastName": "string",       // Requerido: Apellido del usuario
  "email": "string",          // Requerido: Email válido (único)
  "phoneNumber": "string",    // Requerido: Número de teléfono
  "address": "string",        // Requerido: Dirección física
  "birthdate": "datetime",    // Requerido: Fecha de nacimiento (ISO 8601)
  "roleUser": "string",       // Requerido: Usuario|Organizador|Administrador|Soporte
  "password": "string"        // Requerido: Contraseña (mínimo 8 caracteres)
}
```

#### Validaciones

- **firstName**: No vacío, longitud mínima 2 caracteres
- **lastName**: No vacío, longitud mínima 2 caracteres
- **email**: Formato válido (regex), único en la base de datos
- **phoneNumber**: No vacío
- **address**: No vacío
- **birthdate**: Fecha válida, usuario debe ser mayor de 13 años
- **roleUser**: Debe ser uno de: `Usuario`, `Organizador`, `Administrador`, `Soporte`
- **password**: Mínimo 8 caracteres, al menos una mayúscula, un número y un carácter especial

#### Response Success (200 OK)

```json
{
  "usuario": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan.perez@example.com",
    "phoneNumber": "+573001234567",
    "address": "Calle 123 #45-67, Bogotá",
    "birthdate": "1990-05-15T00:00:00Z",
    "roleUser": "Usuario"
  },
  "mensaje": "Usuario registrado exitosamente."
}
```

#### Response Error (400 Bad Request)

```json
{
  "message": "El email ya está registrado; La contraseña debe tener al menos 8 caracteres"
}
```

#### Response Error (500 Internal Server Error)

```json
{
  "message": "Failed to create user in Keycloak: ..."
}
```

#### Ejemplo cURL

```bash
curl -X POST http://localhost:7181/api/users/registerUser \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "María",
    "lastName": "García",
    "email": "maria.garcia@example.com",
    "phoneNumber": "+573109876543",
    "address": "Carrera 7 #32-16, Medellín",
    "birthdate": "1995-08-22T00:00:00",
    "roleUser": "Organizador",
    "password": "SecurePass123!"
  }'
```

---

### 2. Obtener Usuario por Email

Recupera la información completa de un usuario mediante su email.

```http
GET /api/users/getUser/{email}
```

#### Path Parameters

- `email` (string, required): Email del usuario a buscar

#### Response Success (200 OK)

```json
{
  "usuario": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan.perez@example.com",
    "phoneNumber": "+573001234567",
    "address": "Calle 123 #45-67, Bogotá",
    "birthdate": "1990-05-15T00:00:00Z",
    "roleUser": "Usuario"
  },
  "mensaje": "Usuario encontrado exitosamente."
}
```

#### Response Error (500 Internal Server Error)

```json
{
  "message": "El email no puede estar vacío."
}
```

#### Ejemplo cURL

```bash
curl -X GET http://localhost:7181/api/users/getUser/juan.perez@example.com
```

---

### 3. Cambiar Contraseña

Cambia la contraseña de un usuario existente en Keycloak.

```http
POST /api/users/changePassword/{email}
```

#### Path Parameters

- `email` (string, required): Email del usuario

#### Request Body

```json
{
  "newPassword": "string"  // Requerido: Nueva contraseña (mínimo 8 caracteres)
}
```

#### Validaciones

- **newPassword**: Mínimo 8 caracteres, al menos una mayúscula, un número y un carácter especial

#### Response Success (200 OK)

```json
"Contraseña cambiada exitosamente."
```

#### Response Error (400 Bad Request)

```json
{
  "message": "La contraseña debe tener al menos 8 caracteres; Debe contener al menos una letra mayúscula"
}
```

#### Response Error (500 Internal Server Error)

```json
{
  "message": "Error al cambiar la contraseña: 404\nUser not found"
}
```

#### Ejemplo cURL

```bash
curl -X POST http://localhost:7181/api/users/changePassword/juan.perez@example.com \
  -H "Content-Type: application/json" \
  -d '{
    "newPassword": "NewSecurePass456!"
  }'
```

---

### 4. Actualizar Usuario

Actualiza parcialmente los datos de un usuario existente.

```http
PUT /api/users/updateUser/{email}
```

#### Path Parameters

- `email` (string, required): Email del usuario a actualizar

#### Request Body

Todos los campos son opcionales. Solo se actualizarán los campos proporcionados.

```json
{
  "firstName": "string",      // Opcional: Nuevo nombre
  "lastName": "string",       // Opcional: Nuevo apellido
  "phoneNumber": "string",    // Opcional: Nuevo teléfono
  "address": "string",        // Opcional: Nueva dirección
  "birthdate": "datetime"     // Opcional: Nueva fecha de nacimiento
}
```

#### Validaciones

- **firstName** (si se proporciona): Mínimo 2 caracteres
- **lastName** (si se proporciona): Mínimo 2 caracteres
- **phoneNumber** (si se proporciona): No vacío
- **address** (si se proporciona): No vacío
- **birthdate** (si se proporciona): Fecha válida

> **Nota**: El email y el rol no se pueden actualizar con este endpoint.

#### Response Success (200 OK)

```json
"Usuario actualizado exitosamente."
```

#### Response Error (400 Bad Request)

```json
{
  "message": "El nombre debe tener al menos 2 caracteres"
}
```

```json
"No se pudo actualizar el usuario."
```

#### Response Error (500 Internal Server Error)

```json
{
  "message": "No se pudo actualizar el usuario en keyclaok"
}
```

#### Ejemplo cURL

```bash
curl -X PUT http://localhost:7181/api/users/updateUser/juan.perez@example.com \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+573201234567",
    "address": "Calle 45 #67-89, Bogotá"
  }'
```

---

### 5. Listar Todos los Usuarios

Obtiene la lista completa de todos los usuarios registrados.

```http
GET /api/users/getUsers
```

#### Response Success (200 OK)

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan.perez@example.com",
    "phoneNumber": "+573001234567",
    "address": "Calle 123 #45-67, Bogotá",
    "birthdate": "1990-05-15T00:00:00Z",
    "roleUser": "Usuario"
  },
  {
    "id": "7cb92a14-8d3e-4ac5-9f2b-1e847b99cd82",
    "firstName": "María",
    "lastName": "García",
    "email": "maria.garcia@example.com",
    "phoneNumber": "+573109876543",
    "address": "Carrera 7 #32-16, Medellín",
    "birthdate": "1995-08-22T00:00:00Z",
    "roleUser": "Organizador"
  }
]
```

> ⚠️ **Advertencia**: Este endpoint no tiene paginación y puede retornar grandes volúmenes de datos. Ver [Deuda Técnica](architecture.md#10-falta-de-paginación-en-getusersquery).

#### Ejemplo cURL

```bash
curl -X GET http://localhost:7181/api/users/getUsers
```

---

### 6. Obtener ID por Email

Obtiene únicamente el ID (GUID) de un usuario mediante su email.

```http
GET /api/users/getIdUser/{email}
```

#### Path Parameters

- `email` (string, required): Email del usuario

#### Response Success (200 OK)

```json
"3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

#### Response Error (500 Internal Server Error)

```json
{
  "message": "El email no puede estar vacío."
}
```

#### Ejemplo cURL

```bash
curl -X GET http://localhost:7181/api/users/getIdUser/juan.perez@example.com
```

---

## 🧪 Ejemplo de Flujo Completo

### 1. Registrar un usuario

```bash
curl -X POST http://localhost:7181/api/users/registerUser \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Carlos",
    "lastName": "Rodríguez",
    "email": "carlos.rodriguez@example.com",
    "phoneNumber": "+573151234567",
    "address": "Avenida 15 #23-45, Cali",
    "birthdate": "1988-03-10T00:00:00",
    "roleUser": "Usuario",
    "password": "MySecurePass123!"
  }'
```

### 2. Obtener información del usuario

```bash
curl -X GET http://localhost:7181/api/users/getUser/carlos.rodriguez@example.com
```

### 3. Actualizar teléfono

```bash
curl -X PUT http://localhost:7181/api/users/updateUser/carlos.rodriguez@example.com \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+573159876543"
  }'
```

### 4. Cambiar contraseña

```bash
curl -X POST http://localhost:7181/api/users/changePassword/carlos.rodriguez@example.com \
  -H "Content-Type: application/json" \
  -d '{
    "newPassword": "MyNewSecurePass456!"
  }'
```

### 5. Obtener el ID del usuario

```bash
curl -X GET http://localhost:7181/api/users/getIdUser/carlos.rodriguez@example.com
```

---

## 📊 Códigos de Estado HTTP

| Código | Descripción | Cuándo se usa |
|--------|-------------|---------------|
| 200 OK | Operación exitosa | Todas las operaciones exitosas |
| 400 Bad Request | Error de validación o lógica de negocio | Datos inválidos, usuario ya existe, etc. |
| 500 Internal Server Error | Error del servidor | Errores de conexión a BD, Keycloak, excepciones no controladas |

---

## 🔍 Swagger UI

El servicio expone documentación interactiva con Swagger UI (solo en modo Development):

```
http://localhost:7181/swagger
```

Desde Swagger UI puedes:
- Ver todos los endpoints disponibles
- Probar requests directamente
- Ver los esquemas de DTOs
- Exportar especificación OpenAPI

---

## 🛡️ Consideraciones de Seguridad

### Estado Actual

❌ **Sin autenticación**: Cualquiera puede acceder a todos los endpoints  
❌ **Sin autorización**: No hay control de roles  
❌ **Sin rate limiting**: Vulnerable a ataques de fuerza bruta  
❌ **Sin validación de CORS estricta**: Solo localhost permitido

### Recomendaciones

1. **Implementar JWT Bearer Authentication** con Keycloak
2. **Agregar políticas de autorización** basadas en roles
3. **Implementar rate limiting** para prevenir abusos
4. **Validar y sanitizar** todas las entradas
5. **Usar HTTPS** en producción
6. **Implementar auditoría** de operaciones sensibles

Ejemplo de endpoint protegido (recomendado):

```csharp
[Authorize(Roles = "Administrador")]
[HttpGet("getUsers")]
public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
{
    // ...
}
```

---

## 📝 Notas Adicionales

### Formato de Fechas

Todas las fechas utilizan formato ISO 8601:
```
YYYY-MM-DDTHH:mm:ssZ
Ejemplo: 2023-12-15T14:30:00Z
```

### Roles Válidos

- `Usuario`: Usuario estándar con permisos básicos
- `Organizador`: Puede crear y gestionar eventos
- `Administrador`: Acceso completo al sistema
- `Soporte`: Acceso para soporte técnico

### Caracteres Especiales en Email

Los emails en URL deben estar correctamente encoded:
```bash
# Email: user+test@example.com
curl http://localhost:7181/api/users/getUser/user%2Btest%40example.com
```

### Timeout

Los endpoints tienen un timeout por defecto de 30 segundos configurado en ASP.NET Core.
