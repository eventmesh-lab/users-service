using Aplication.Interfaces;
using Domain.Entities0;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Insfrastructure.Services
{
    public class UserServices : IUserServices
    {
        public readonly IUserRepositoryPostgres _repositoryPostgres;

        public UserServices( IUserRepositoryPostgres repositoryPostgres)
        {
            _repositoryPostgres= repositoryPostgres;
        }

        public async Task AddUserPostgres(User user, CancellationToken cancellationToken)
        {
            await _repositoryPostgres.AddUser(user, cancellationToken);
        }

        public async Task<User?> GetUserByEmailServices(string email, CancellationToken cancellationToken)
        {
            return await _repositoryPostgres.GetUserByEmail(email, cancellationToken);
        }

        public async Task<List<User>> GetAllUsersServices(CancellationToken cancellationToken)
        {
            return  await _repositoryPostgres.GetAllUsersAsync(cancellationToken);
        }
        public async Task<bool> DeleteUserByEmailServices(string email, CancellationToken cancellationToken)
        {
            return await _repositoryPostgres.DeleteUserByEmail(email, cancellationToken);
        }

        public async Task<HttpStatusCode> UpdateUserServices(string email, User newUser)
        {
            return await _repositoryPostgres.UpdateUser(email, newUser);
        }

    }
}
