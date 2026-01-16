using MicroservicioUsuarios.Infrastructure.ServicesInfrastracture;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Contrib.HttpClient;
using Moq.Protected;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using users_service.application.DTOs;
using users_service.domain.Interfaces;

namespace users_service.infrastructure.Tests.ServiceInfrastructure
{
    public class KeycloakServiceInfrastructureTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly KeycloakServiceInfrastracture _sut;
        private IConfiguration BuildConfig()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Keycloak:AdmClientId", "test-client" },
                { "Keycloak:AdmClientSecret", "secret" },
                { "Keycloak:BaseUrl", "http://localhost" },
                { "Keycloak:AdmRealm", "master" },
                { "Keycloak:UserRealm", "users-realm" },
                { "Keycloak:AdmClientScope", "openid" }, 
                { "Keycloak:AdmClientGrantType", "password" }

            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        public KeycloakServiceInfrastructureTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);
            _config = BuildConfig();
            _sut = new KeycloakServiceInfrastracture(_httpClient, _config);
        }

        [Fact]
        public async Task GetAdminTokenAsync_ReturnsCachedToken_WhenNotExpired()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Keycloak:AdmClientId", "test-client" },
                { "Keycloak:BaseUrl", "http://localhost" },
                { "Keycloak:AdmRealm", "master" },
                { "Keycloak:UserRealm", "users-realm" }

            };

            var _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new KeycloakServiceInfrastracture(_httpClient, _config);
            var tokenField = typeof(KeycloakServiceInfrastracture)
                .GetField("_accessToken", BindingFlags.NonPublic | BindingFlags.Instance);
            var expiryField = typeof(KeycloakServiceInfrastracture)
                .GetField("_tokenExpiryTime", BindingFlags.NonPublic | BindingFlags.Instance);

            tokenField!.SetValue(service, "cached-token");
            expiryField!.SetValue(service, DateTime.UtcNow.AddMinutes(5));

            var result = await service.GetAdminTokenAsync();

            Assert.Equal("cached-token", result);
        }

        [Fact]
        public async Task GetAdminTokenAsync_CallsKeycloak_WhenTokenExpired()
        {
            // Configuración en memoria
            var inMemorySettings = new Dictionary<string, string?>
        {
            { "Keycloak:AdmClientId", "test-client" },
            { "Keycloak:BaseUrl", "http://localhost" },
            { "Keycloak:AdmRealm", "master" }
        };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Simular respuesta HTTP exitosa
            var responseJson = "{\"access_token\":\"new-token\",\"expires_in\":60}";
            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == "http://localhost/realms/master/protocol/openid-connect/token"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);

            // Instanciar el servicio
            var service = new KeycloakServiceInfrastracture(httpClient, config);

            // Ejecutar método
            var result = await service.GetAdminTokenAsync();

            // Validar resultado
            Assert.Equal("new-token", result);

            // Validar que se llamó exactamente una vez
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == "http://localhost/realms/master/protocol/openid-connect/token"),
                ItExpr.IsAny<CancellationToken>()
            );
        }


        [Fact]
        public async Task GetAdminTokenAsync_ThrowsException_WhenHttpFails()
        {
            // Configuración en memoria para evitar uriString null
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Keycloak:AdmClientId", "test-client" },
                { "Keycloak:BaseUrl", "http://localhost" },
                { "Keycloak:AdmRealm", "master" }
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Simular respuesta HTTP fallida
            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == "http://localhost/realms/master/protocol/openid-connect/token"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new KeycloakServiceInfrastracture(httpClient, config);

            // Validar que lanza excepción por HTTP 400
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetAdminTokenAsync());

            // Verificar que se llamó una vez
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == "http://localhost/realms/master/protocol/openid-connect/token"),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetAdminTokenAsync_SetsExpiryCorrectly()
        {
            // Configuración en memoria
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Keycloak:AdmClientId", "test-client" },
                { "Keycloak:BaseUrl", "http://localhost" },
                { "Keycloak:AdmRealm", "master" },
                { "Keycloak:UserRealm", "users-realm" }

            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Simular respuesta HTTP con expires_in
            var responseJson = "{\"access_token\":\"exp-token\",\"expires_in\":120}";
            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == "http://localhost/realms/master/protocol/openid-connect/token"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new KeycloakServiceInfrastracture(httpClient, config);

            // Ejecutar método
            var result = await service.GetAdminTokenAsync();

            // Validar token
            Assert.Equal("exp-token", result);

            // Validar expiración
            var expiryField = typeof(KeycloakServiceInfrastracture)
                .GetField("_tokenExpiryTime", BindingFlags.NonPublic | BindingFlags.Instance);
            var expiry = (DateTime)expiryField!.GetValue(service)!;

            Assert.True(expiry > DateTime.UtcNow.AddSeconds(100)); // margen de seguridad
        }

        [Fact]
        public async Task GetUserTokenAsync_ReturnsCachedToken_WhenNotExpired()
        {
            // Arrange
            typeof(KeycloakServiceInfrastracture)
                .GetField("_accessToken", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(_sut, "cached-token");

            typeof(KeycloakServiceInfrastracture)
                .GetField("_tokenExpiryTime", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(_sut, DateTime.UtcNow.AddMinutes(5));

            // Act
            var result = await _sut.GetUserTokenAsync("user@test.com", "pass");

            // Assert
            Assert.Equal("cached-token", result);
            _handlerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetUserTokenAsync_FetchesNewToken_WhenExpired()
        {
            // Arrange
            var responseJson = "{\"access_token\":\"new-token\",\"expires_in\":60}";
            SetupHttpResponse(HttpStatusCode.OK, responseJson);

            // Act
            var result = await _sut.GetUserTokenAsync("user@test.com", "pass");

            // Assert
            Assert.Equal("new-token", result);
        }

        [Fact]
        public async Task GetUserTokenAsync_ThrowsException_WhenHttpFails()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.BadRequest, "error");

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _sut.GetUserTokenAsync("user@test.com", "pass"));
        }

        [Fact]
        public async Task GetUserTokenAsync_ThrowsException_WhenJsonInvalid()
        {
            // Arrange
            var invalidJson = "{\"wrong_property\":\"value\"}";
            SetupHttpResponse(HttpStatusCode.OK, invalidJson);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.GetUserTokenAsync("user@test.com", "pass"));
        }

        [Fact]
        public async Task GetUserEmail_ReturnsUserId_WhenUserExists()
        {
            // Arrange
            var json = "[{\"id\":\"user-123\"}]";
            SetupHttpResponse(HttpStatusCode.OK, json);

            // Act
            var result = await _sut.GetUserEmail("testuser", "token");

            // Assert
            Assert.Equal("user-123", result);
        }

        [Fact]
        public async Task GetUserEmail_ThrowsException_WhenHttpFails()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.BadRequest, "error");

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _sut.GetUserEmail("testuser", "token"));
        }

        [Fact]
        public async Task GetUserEmail_ThrowsException_WhenUserIdEmpty()
        {
            // Arrange
            var json = "[{\"id\":\"\"}]";
            SetupHttpResponse(HttpStatusCode.OK, json);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _sut.GetUserEmail("testuser", "token"));

            Assert.Contains("Usuario 'testuser' no encontrado", ex.Message);
        }

        [Fact]
        public async Task GetUserEmail_ThrowsException_WhenNoUsersFound()
        {
            // Arrange
            var json = "[]"; // lista vacía
            SetupHttpResponse(HttpStatusCode.OK, json);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetUserEmail("testuser", "token"));
        }

        [Fact]
        public async Task GetRole_ReturnsRole_WhenFound()
        {
            var json = "[{\"name\":\"admin\",\"description\":\"Admin role\"}]";
            SetupHttpResponse(HttpStatusCode.OK, json);

            var result = await _sut.GetRole("admin", "token");

            Assert.Equal("admin", result.GetProperty("name").GetString());
        }

        [Fact]
        public async Task GetRole_ThrowsException_WhenRoleNotFound()
        {
            var json = "[{\"name\":\"user\"}]"; // no coincide con "admin"
            SetupHttpResponse(HttpStatusCode.OK, json);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _sut.GetRole("admin", "token"));

            Assert.Contains("Rol 'admin' no encontrado", ex.Message);
        }

        [Fact]
        public async Task GetRole_ThrowsHttpRequestException_WhenHttpFails()
        {
            SetupHttpResponse(HttpStatusCode.Unauthorized, "Unauthorized");

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _sut.GetRole("admin", "token"));
        }

        [Fact]
        public async Task GetRole_ThrowsException_WhenJsonInvalid()
        {
            var json = "[{\"invalid\":\"data\"}]"; // no contiene "name"
            SetupHttpResponse(HttpStatusCode.OK, json);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.GetRole("admin", "token"));
        }


        [Fact]
        public async Task GetUserIdByUsernameAsync_UserExists_ReturnsUserId()
        {
            // Arrange
            var username = "juan.perez";
            var expectedUserId = "12345-abcde";

            // 1. Configurar respuesta del Token (POST)
            SetupTokenResponse();

            // 2. Configurar respuesta de búsqueda de Usuario (GET)
            var usersList = new List<UserKeycloakDTO>
        {
            new UserKeycloakDTO { Id = expectedUserId, Username = username }
        };

            var userJson = JsonSerializer.Serialize(usersList);

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"users?username={username}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(userJson)
                });

            // Act
            var result = await _sut.GetUserIdByUsernameAsyncRepo(username);

            // Assert
            Assert.Equal(expectedUserId, result);
        }

        [Fact]
        public async Task GetUserIdByUsernameAsync_ApiError_ReturnsNullAndLogsError()
        {
            // Arrange
            SetupTokenResponse();

            // Simulamos un error 404 o 500 al buscar el usuario
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound // El código maneja !IsSuccessStatusCode
                });

            // Act
            var result = await _sut.GetUserIdByUsernameAsyncRepo("cualquiera");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAdminTokenAsync_TokenRequestFails_ThrowsException()
        {
            // Arrange
            // Simulamos que la petición del Token falla (ej. credenciales malas)
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            // Act & Assert
            // Como GetUserId llama a GetAdminToken, la excepción subirá
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _sut.GetUserIdByUsernameAsyncRepo("usuario"));
        }

        [Fact]
        public async Task GetAdminTokenAsync_UsesCachedToken_DoesNotCallApiTwice()
        {
            // Arrange
            SetupTokenResponse(); // Configuramos una respuesta de token válida

            // Configuramos respuesta genérica para el GET de usuario para que no falle
            _handlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("[]")
               });

            // Act
            // Llamada 1: Debería pedir el token vía HTTP
            await _sut.GetUserIdByUsernameAsyncRepo("user1");

            // Llamada 2: Debería reusar el token en memoria (cache)
            await _sut.GetUserIdByUsernameAsyncRepo("user2");

            // Assert
            // Verificamos que el POST (solicitud de token) se hizo EXACTAMENTE 1 VEZ
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(), // <-- Esto confirma que la lógica de caché funciona
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            );
        }


        [Fact]
        public async Task CreateUserAsync_Success_SendsCorrectRequest()
        {
            // Arrange
            var email = "nuevo@test.com";
            SetupTokenResponse(); // 1. Configuramos que el Token responda OK

            // 2. Configuramos que el endpoint de Crear Usuario responda OK (201 Created)
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().EndsWith("/users") // Identifica llamada de creación
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent("")
                });

            // Act
            // No esperamos retorno, solo que no falle
            await _sut.RegisterUserAsyncRepo(email, "Juan", "Perez", "123456");

            // Assert
            // Verificamos que se llamó al endpoint de usuarios con el JSON correcto
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith("/users") &&
                    CheckPayload(req, email) // Verificamos contenido del JSON
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task CreateUserAsync_KeycloakReturnsError_ThrowsException()
        {
            // Arrange
            SetupTokenResponse(); // El token funciona bien

            // Simulamos que Keycloak dice "Bad Request" o "Conflict" (ej. usuario ya existe)
            var errorMsg = "User already exists";
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/users")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Conflict, // 409
                    Content = new StringContent(errorMsg)
                });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _sut.RegisterUserAsyncRepo("test@test.com", "A", "B", "pass"));

            // Verificamos que la excepción contiene el mensaje de error de Keycloak
            Assert.Contains("Failed to create user", ex.Message);
            Assert.Contains(errorMsg, ex.Message);
        }

        [Fact]
        public async Task CreateUserAsync_TokenFailure_ThrowsHttpRequestException()
        {
            // Arrange
            // Simulamos que falla la obtención del token (401 Unauthorized)
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("token")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            // Act & Assert
            // Esto fallará antes de intentar crear el usuario, dentro de GetAdminTokenAsync
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _sut.RegisterUserAsyncRepo("test@test.com", "A", "B", "pass"));
        }

        [Fact]
        public async Task AssignRealmRoleToUserAsync_Should_AssignRole_When_AllCallsSucceed()
        {
            // Arrange
            var username = "juan.perez";
            var roleName = "admin-role";

            // 1. Mock para el Token (GetAdminTokenAsync)
            MockHttpMessage("http://localhost/realms/master/protocol/openid-connect/token",
                JsonSerializer.Serialize(new { access_token = "fake-jwt-token", expires_in = 300 }));

            // 2. Mock para buscar Usuario (GetUserEmail)
            // Nota: Devuelve un array con un objeto que tiene "id"
            MockHttpMessage($"http://localhost/admin/realms/users-realm/users?username={username}",
                JsonSerializer.Serialize(new[] { new { id = "user-guid-123" } }));

            // 3. Mock para buscar Rol (GetRole)
            // Nota: Devuelve un array donde uno coincide con el nombre
            MockHttpMessage("http://localhost/admin/realms/users-realm/roles",
                JsonSerializer.Serialize(new[] { new { name = roleName, id = "role-guid-456" }, new { name = "other", id = "789" } }));

            // 4. Mock para la Asignación final (POST final)
            MockHttpMessage("http://localhost/admin/realms/users-realm/users/user-guid-123/role-mappings/realm",
                "", HttpMethod.Post, HttpStatusCode.NoContent);

            // Act
            await _sut.AssignRealmRoleToUserAsyncRepo(username, roleName);

            // Assert
            // Verificamos que se hayan realizado las 4 llamadas
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(4), // Token, User, Role, Assign
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        // --- PRUEBA 2: CACHÉ DEL TOKEN ---
        // Cubre: La lógica del "if" en GetAdminTokenAsync para no pedir token si ya existe
        [Fact]
        public async Task GetAdminTokenAsync_Should_UseCachedToken_OnSecondCall()
        {
            // Arrange
            MockHttpMessage("http://localhost/realms/master/protocol/openid-connect/token",
                 JsonSerializer.Serialize(new { access_token = "token-A", expires_in = 300 }));

            // Act
            // Primera llamada: Debe hacer HTTP request
            var token1 = await _sut.GetAdminTokenAsync();

            // Segunda llamada: Debe usar caché (sin HTTP request extra)
            var token2 = await _sut.GetAdminTokenAsync();

            // Assert
            Assert.Equal("token-A", token1);
            Assert.Equal("token-A", token2);

            // Verificamos que SendAsync se llamó SOLO UNA VEZ a pesar de haber invocado el método dos veces
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("openid-connect/token")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        // --- PRUEBA 3: USUARIO NO ENCONTRADO ---
        // Cubre: La excepción en GetUserEmail
        [Fact]
        public async Task AssignRealmRoleToUserAsync_Should_ThrowException_When_UserNotFound()
        {
            // Arrange
            var username = "fantasma";

            // Token OK
            MockHttpMessage("http://localhost/realms/master/protocol/openid-connect/token",
                JsonSerializer.Serialize(new { access_token = "token", expires_in = 300 }));

            // CAMBIO IMPORTANTE:
            // Enviamos un objeto con ID vacío en lugar de una lista vacía [].
            // Esto permite que el código pase el .GetProperty("id") y caiga en el if(string.IsNullOrEmpty)
            // lanzando así tu Exception personalizada en lugar de InvalidOperationException.
            MockHttpMessage($"http://localhost/admin/realms/users-realm/users?username={username}",
                JsonSerializer.Serialize(new[] { new { id = "" } }));

            // Act & Assert
            // Ahora sí esperamos "Exception" exactamente
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.AssignRealmRoleToUserAsyncRepo(username, "rol"));
            Assert.Contains($"Usuario '{username}' no encontrado", ex.Message);
        }
        [Fact]
        public async Task AssignRealmRoleToUserAsync_Should_ThrowException_When_RoleNotFound()
        {
            // Arrange
            var roleName = "rol-inexistente";

            // Token OK
            MockHttpMessage("http://localhost/realms/master/protocol/openid-connect/token",
                JsonSerializer.Serialize(new { access_token = "token", expires_in = 300 }));

            // Usuario OK
            MockHttpMessage($"http://localhost/admin/realms/users-realm/users?username=algo",
                JsonSerializer.Serialize(new[] { new { id = "123" } }));

            // Roles OK, pero devuelve una lista que NO contiene el rol buscado
            MockHttpMessage("http://localhost/admin/realms/users-realm/roles",
                JsonSerializer.Serialize(new[] { new { name = "otro-rol", id = "999" } }));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.AssignRealmRoleToUserAsyncRepo("algo", roleName));
            Assert.Contains($"Rol '{roleName}' no encontrado", ex.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldPrintSuccess_WhenAllCallsSucceed()
        {
            // Arrange
            // 1. Token
            SetupMockResponse("protocol/openid-connect/token", HttpMethod.Post, new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GetTokenResponseJson())
            });

            // 2. Get User ID
            var usersJson = JsonSerializer.Serialize(new List<UserKeycloakDTO>
        {
            new UserKeycloakDTO { Id = "user-123", Username = "test@test.com" }
        });
            SetupMockResponse("users?username=", HttpMethod.Get, new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(usersJson)
            });

            // 3. Reset Password (PUT)
            SetupMockResponse("reset-password", HttpMethod.Put, new HttpResponseMessage(HttpStatusCode.NoContent)); // 204 No Content es común en PUT exitosos

            // Act
            // Capturamos la consola para verificar el output (opcional, pero bueno para cobertura visual)
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                await _sut.ChangePasswordAsyncRepo("test@test.com", "newPass123");
                var output = sw.ToString();

                // Assert
                Assert.Contains("Contraseña cambiada correctamente", output);
            }
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldPrintError_WhenResetFails()
        {
            // Arrange
            // 1. Token
            SetupMockResponse("protocol/openid-connect/token", HttpMethod.Post, new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GetTokenResponseJson())
            });

            // 2. Get User ID
            var usersJson = JsonSerializer.Serialize(new List<UserKeycloakDTO>
        {
            new UserKeycloakDTO { Id = "user-123", Username = "test@test.com" }
        });
            SetupMockResponse("users?username=", HttpMethod.Get, new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(usersJson)
            });

            // 3. Reset Password falla (ej. 400 Bad Request)
            SetupMockResponse("reset-password", HttpMethod.Put, new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Password complexity requirement not met")
            });

            // Act
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                await _sut.ChangePasswordAsyncRepo("test@test.com", "weak");
                var output = sw.ToString();

                // Assert
                Assert.Contains("Error al cambiar la contraseña", output);
                Assert.Contains("BadRequest", output);
            }
        }

        [Fact]
        public async Task UpdateUserInKeycloakAsync_DebeLanzarExcepcion_CuandoFallaRedEnUpdate()
        {
            // Arrange
            SetupMockResponseUpdate(HttpMethod.Post, "token", CreateSuccessJson(new { access_token = "tkn", expires_in = 300 }));
            SetupMockResponseUpdate(HttpMethod.Get, "users", CreateSuccessJson(new List<UserKeycloakDTO> { new UserKeycloakDTO { Id = "user-123" } }));

            // Simulamos una excepción al hacer el PUT
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Error de red"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApplicationException>(() =>
                _sut.UpdateUserInKeycloakAsyncRepo("email", "nom", "ape"));

            Assert.Contains("No se pudo actualizar el usuario", ex.Message);
        }



        // Este método permite configurar respuestas específicas basadas en la URL
        private void MockHttpMessage(string urlContains, string responseContent, HttpMethod method = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains(urlContains) &&
                        (method == null || req.Method == method)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });
        }

        private void SetupTokenResponse()
        {
            // Configuramos la respuesta exitosa del Token para que el flujo continúe
            var tokenResponse = new
            {
                access_token = "fake-jwt-token",
                expires_in = 300
            };
            var json = System.Text.Json.JsonSerializer.Serialize(tokenResponse);

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("token") 
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });
        }

        // Función auxiliar para inspeccionar el JSON enviado en el request
        private bool CheckPayload(HttpRequestMessage req, string expectedEmail)
        {
            if (req.Content == null) return false;

            // Leemos el contenido síncronamente para la prueba
            var json = req.Content.ReadAsStringAsync().Result;

            // Validamos que el JSON contenga el email que enviamos
            return json.Contains(expectedEmail) && json.Contains("\"enabled\":true");
        }

        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }

        private void SetupMockResponse(string urlPart, HttpMethod method, HttpResponseMessage response)
        {
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains(urlPart) &&
                        req.Method == method),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }
        private HttpResponseMessage CreateSuccessJson(object content)
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
            };
        }

        private string GetTokenResponseJson()
        {
            return JsonSerializer.Serialize(new
            {
                access_token = "fake-jwt-token",
                expires_in = 300 // 5 minutos
            });
        }

        private void SetupMockResponseUpdate(HttpMethod method, string urlPart, HttpResponseMessage response)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == method &&
                        req.RequestUri.ToString().Contains(urlPart)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }
    }


}
