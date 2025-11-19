using users_service.domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace users_service.domain.Interfaces
{
    public interface IUserRepositoryPostgres
    {
        Task AddUser(User user, CancellationToken cancellationToken);
        Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken);
        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<bool> DeleteUserByEmail(string email, CancellationToken cancellationToken);
        Task<HttpStatusCode> UpdateUser(string email, User userUpdated);
    }
}
