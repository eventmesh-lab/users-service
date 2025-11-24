using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace users_service.domain.Interfaces
{
    public interface IKeycloakServiceInfrastructure
    {
        Task<string> GetAdminTokenAsync();
        Task CreateUserAsync(string email, string name, string lastname, string password);
        Task<string> GetUserEmail(string username, string token);
        Task<JsonElement> GetRole(string roleName, string token);
        Task AssignRealmRoleToUserAsync(string username, string roleName);
        Task ChangePasswordAsync(string email, string newPassword);
        Task<bool> UpdateUserInKeycloakAsync(string oldEmail, string newName, string newLastname);
        Task<string> GetUserIdByUsernameAsync(string username);
    }
}
