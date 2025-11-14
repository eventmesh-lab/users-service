using Aplication.Interfaces;
using MicroserviciosUsuarios.Infrastructure.Repositories.Keycloak;
using MicroservicioUsuarios.Infrastructure.ServicesInfrastracture;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroserviciosUsuarios.Infrastructure.Repositories.Keycloak
{

    public class KeycloakRepository : IKeycloakRepository
    {
        private readonly KeycloakServiceInfrastracture _authService;

        public KeycloakRepository(KeycloakServiceInfrastracture authService)
        {
            _authService = authService;
        }

        public async Task<bool> RegisterUserAsyncRepo(string email, string name, string lastname,string password)
        {

            await _authService.CreateUserAsync(email, name,lastname, password);
            return true;
            /*
            try
            {
                await _authService.CreateUserAsync(email, name, password);
                return true; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error al registrar usuario en Keycloak: {ex.Message}");
                return false; 
            }*/
        }

        public async Task<bool> AssignRealmRoleToUserAsyncRepo(string email, string rol) {
            await _authService.AssignRealmRoleToUserAsync(email, rol);
            return true;
        }

        public async Task ChangePasswordAsyncRepo(string email, string newPassword)
        {
            await _authService.ChangePasswordAsync(email, newPassword);
        }

        public async Task<string> GetUserIdByUsernameAsyncRepo(string email)
        {
            return await _authService.GetUserIdByUsernameAsync(email);
        }

        public Task<bool> UpdateUserInKeycloakAsyncRepo(string oldEmail, string newName, string newLastname)
        {
            return _authService.UpdateUserInKeycloakAsync(oldEmail, newName, newLastname);
        }
    }

}