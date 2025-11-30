using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Interfaces
{
    /// Interfaz del repositorio de keycloak.
    public interface IKeycloakRepository
    {
        /// Registra un usaurio en keycloak.
        Task<bool> RegisterUserAsyncRepo(string email, string name, string lastname, string password);
        /// Asigna un rol a un usuario en keycloak.
        Task<bool> AssignRealmRoleToUserAsyncRepo(string email, string rol);
        //Cambia la contraseña de un usuario en keycloak.
        Task ChangePasswordAsyncRepo(string email, string newPassword);
        //Consigue el Id de un usuario en keycloak a partir de su username (email).
        Task<string> GetUserIdByUsernameAsyncRepo(string email);
        //Actualiza el nombre y apellido de un usuario en keycloak.
        Task<bool> UpdateUserInKeycloakAsyncRepo(string oldEmail, string newName, string newLastname);

    }
}
