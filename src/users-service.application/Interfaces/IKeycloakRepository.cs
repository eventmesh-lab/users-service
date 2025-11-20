using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Interfaces
{
    public interface IKeycloakRepository
    {
        Task<bool> RegisterUserAsyncRepo(string email, string name, string lastname, string password);
        Task<bool> AssignRealmRoleToUserAsyncRepo(string email, string rol);
        Task ChangePasswordAsyncRepo(string email, string newPassword);
        Task<string> GetUserIdByUsernameAsyncRepo(string email);
        Task<bool> UpdateUserInKeycloakAsyncRepo(string oldEmail, string newName, string newLastname);

    }
}
