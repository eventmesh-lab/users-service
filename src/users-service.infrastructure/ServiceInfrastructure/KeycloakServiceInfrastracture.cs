using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using users_service.application.DTOs;
using users_service.domain.Interfaces;
namespace MicroservicioUsuarios.Infrastructure.ServicesInfrastracture
{

    public class KeycloakServiceInfrastracture : IKeycloakServiceInfrastructure
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private string? _accessToken;
        private DateTime _tokenExpiryTime;

        public KeycloakServiceInfrastracture(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GetAdminTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiryTime > DateTime.UtcNow.AddMinutes(-1))
                return _accessToken!;

            var parameters = new Dictionary<string, string>
            {
                { "client_id", _config["Keycloak:AdmClientId"]!},
                { "grant_type", "password" },
                { "username", "admin" },
                { "password", "admin" }
           };

            var tokenUrl = $"{_config["Keycloak:BaseUrl"]}/realms/{_config["Keycloak:AdmRealm"]}/protocol/openid-connect/token";
            var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(parameters));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenJson = JsonDocument.Parse(content).RootElement;

            _accessToken = tokenJson.GetProperty("access_token").GetString();
            var expiresIn = tokenJson.GetProperty("expires_in").GetInt32();
            _tokenExpiryTime = DateTime.UtcNow.AddSeconds(expiresIn);

            return _accessToken!;
        }

        public async Task CreateUserAsync(string email, string name, string lastname, string password)
        {
            var token = await GetAdminTokenAsync();
            Console.WriteLine($"T es {token}");

            var createUserUrl = $"{_config["Keycloak:BaseUrl"]}/admin/realms/{_config["Keycloak:UserRealm"]}/users";

            var userPayload = new
            {
                username = email,
                email = email,
                firstName = name,
                lastName = lastname,
                enabled = true,
                credentials = new[]
                {
                new {
                    type = "password",
                    value = password,
                    temporary = false
                }
            }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, createUserUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(userPayload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create user in Keycloak: {error}");
            }
        }

        public async Task<string> GetUserTokenAsync(string email, string password)
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiryTime > DateTime.UtcNow.AddMinutes(-1))
                return _accessToken!;

            var parameters = new Dictionary<string, string>
            {
                { "client_id", _config["Keycloak:AdmClientId"]!},
                { "grant_type", "password" },
                { "username", email },
                { "password", password }
            };

            var tokenUrl = $"{_config["Keycloak:BaseUrl"]}/realms/{_config["Keycloak:AdmRealm"]}/protocol/openid-connect/token";
            var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(parameters));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenJson = JsonDocument.Parse(content).RootElement;

            _accessToken = tokenJson.GetProperty("access_token").GetString();
            var expiresIn = tokenJson.GetProperty("expires_in").GetInt32();
            _tokenExpiryTime = DateTime.UtcNow.AddSeconds(expiresIn);

            return _accessToken!;
        }

        public async Task<string> GetUserEmail(string username, string token)
        {
            var userSearchUrl = $"{_config["Keycloak:BaseUrl"]}/admin/realms/{_config["Keycloak:UserRealm"]}/users?username={username}";
            var request = new HttpRequestMessage(HttpMethod.Get, userSearchUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userResponse = await _httpClient.SendAsync(request);
            userResponse.EnsureSuccessStatusCode();
            var userContent = await userResponse.Content.ReadAsStringAsync();
            using var userDoc = JsonDocument.Parse(userContent);
            var userId = userDoc.RootElement.EnumerateArray().FirstOrDefault().GetProperty("id").GetString();

            if (string.IsNullOrEmpty(userId))
                throw new Exception($"Usuario '{username}' no encontrado en Keycloak.");
            return userId;
        }

        public async Task<JsonElement> GetRole(string roleName, string token)
        {
            var rolesUrl = $"{_config["Keycloak:BaseUrl"]}/admin/realms/{_config["Keycloak:UserRealm"]}/roles";
            var rolesRequest = new HttpRequestMessage(HttpMethod.Get, rolesUrl);
            rolesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var rolesResponse = await _httpClient.SendAsync(rolesRequest);

            rolesResponse.EnsureSuccessStatusCode();
            var rolesContent = await rolesResponse.Content.ReadAsStringAsync();
            using var rolesDoc = JsonDocument.Parse(rolesContent);
            var roleElement = rolesDoc.RootElement.EnumerateArray().FirstOrDefault(r => r.GetProperty("name").GetString() == roleName);

            if (roleElement.ValueKind == JsonValueKind.Undefined)
                throw new Exception($"Rol '{roleName}' no encontrado en el realm '{_config["Keycloak:UserRealm"]}'.");
            return roleElement.Clone();
        }



        public async Task AssignRealmRoleToUserAsync(string username, string roleName)
        {
            var token = await GetAdminTokenAsync();
            
            var userId = await GetUserEmail(username, token);

            var roleElement = await GetRole(roleName, token);

            var assignUrl = $"{_config["Keycloak:BaseUrl"]}/admin/realms/{_config["Keycloak:UserRealm"]}/users/{userId}/role-mappings/realm";
            var assignRequest = new HttpRequestMessage(HttpMethod.Post, assignUrl);
            assignRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var rolePayload = new[] { roleElement };
            var roleJson = System.Text.Json.JsonSerializer.Serialize(rolePayload);
            assignRequest.Content = new StringContent(roleJson, Encoding.UTF8, "application/json");

            var assignResponse = await _httpClient.SendAsync(assignRequest);
            assignResponse.EnsureSuccessStatusCode();

            Console.WriteLine($" Rol '{roleName}' asignado exitosamente al usuario '{username}'.");
        }

        public async Task ChangePasswordAsync( string email, string newPassword)
        {
            var userId = await GetUserIdByUsernameAsync(email);
            var accessToken = await GetAdminTokenAsync();
            var url = $"http://keycloak:8080/admin/realms/{_config["Keycloak:UserRealm"]}/users/{userId}/reset-password";

            var payload = new
            {
                type = "password",
                value = newPassword,
                temporary = false
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Contraseña cambiada correctamente.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al cambiar la contraseña: {response.StatusCode}\n{error}");
            }
        }

        public async Task<string> GetUserIdByUsernameAsync(string username)
        {
            var accessToken = await GetAdminTokenAsync();
            var realm = _config["Keycloak:UserRealm"];
            var url = $"http://keycloak:8080/admin/realms/{realm}/users?username={username}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error buscando usuario: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserKeycloakDTO>>(json);
            Console.WriteLine($"Respuesta JSON: {json}");
            Console.WriteLine($"Usuarios encontrados: {users?.Count}");
            return users?.FirstOrDefault()?.Id;
        }

        public async Task<bool> UpdateUserInKeycloakAsync(string oldEmail, string newName, string newLastname)
        {
            using var httpClient = new HttpClient();
            var tokenAdmin = await GetAdminTokenAsync();
            var userId = await GetUserIdByUsernameAsync(oldEmail);
            var baseUrl = "http://keycloak:8080";
            string url = $"{baseUrl}/admin/realms/{_config["Keycloak:UserRealm"]}/users/{userId}";
            var body = new
            {
                firstName = newName,
                lastName = newLastname
            };
            var jsonBody = JsonSerializer.Serialize(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);
            try {
                var response = await httpClient.PutAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Body: {responseBody}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
               throw new ApplicationException($"No se pudo actualizar el usuario en keyclaok", ex);
            }
            
            
        }




    }


}