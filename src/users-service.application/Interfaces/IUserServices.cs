using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.domain.Entities;

namespace users_service.application.Interfaces
{
    public interface IUserServices
    {
        Task AddUserPostgres(User user, CancellationToken cancellationToken);
        Task<User?> GetUserByEmailServices(string email, CancellationToken cancellationToken);
        Task<List<User>> GetAllUsersServices(CancellationToken cancellationToken);
        Task<bool> DeleteUserByEmailServices(string email, CancellationToken cancellationToken);
        Task<System.Net.HttpStatusCode> UpdateUserServices(string email, User newUser);
    }
}
