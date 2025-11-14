using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities0;

namespace Aplication.Interfaces
{
    public interface IUserServices
    {
        Task AddUserPostgres(User user, CancellationToken cancellationToken);
        Task<User?> GetUserByEmailServices(string email, CancellationToken cancellationToken);
        Task<bool> DeleteUserByEmailServices(string email, CancellationToken cancellationToken);
        Task<System.Net.HttpStatusCode> UpdateUserServices(string email, User newUser);
    }
}
