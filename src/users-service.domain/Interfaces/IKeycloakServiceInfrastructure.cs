using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace users_service.domain.Interfaces
{ // Interfaz del servicio de keycloak para la persistencia de usuario.
  // Define el contrato para la gestion de usuario en keycloak.
    public interface IKeycloakServiceInfrastructure
    {
        /// Obtiene el token del adminitrador keycloak.
        Task<string> GetAdminTokenAsync();
        /// Agrega un nuevo usuario a keycloak.
        Task CreateUserAsync(string email, string name, string lastname, string password);
        /// Obtiene un usuario por su email.
        Task<string> GetUserEmail(string username, string token);
        /// Obtiene los roles de un usuario.
        Task<JsonElement> GetRole(string roleName, string token);
        /// Agrega roles a un usuario en keycloak.
        Task AssignRealmRoleToUserAsync(string username, string roleName);
        /// Cambia la contraseña asignada a la cuenta de un usuario a keycloak.
        Task ChangePasswordAsync(string email, string newPassword);
        /// Actualiza un usuario a keycloak.
        Task<bool> UpdateUserInKeycloakAsync(string oldEmail, string newName, string newLastname);
        /// Obtiene el id de un usuario en keycloak por su username.
        Task<string> GetUserIdByUsernameAsync(string username);
    }
}
