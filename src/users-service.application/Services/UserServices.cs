using users_service.application.Interfaces;
using users_service.domain.Entities;
using users_service.domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Services
{
    
    public class UserServices : IUserServices
    {
        public readonly IUserRepositoryPostgres _repositoryPostgres;

        public UserServices( IUserRepositoryPostgres repositoryPostgres)
        {
            _repositoryPostgres= repositoryPostgres;
        }
        /// Agrega un nuevo usuario a la base de datos PostgreSQL.
        public async Task AddUserPostgres(User user, CancellationToken cancellationToken)
        {
            await _repositoryPostgres.AddUser(user, cancellationToken);
        }
        /// Obtiene un usuario por su correo electrónico.
        public async Task<User?> GetUserByEmailServices(string email, CancellationToken cancellationToken)
        {
            return await _repositoryPostgres.GetUserByEmail(email, cancellationToken);
        }
        /// Obtiene todos los usuarios.
        public async Task<List<User>> GetAllUsersServices(CancellationToken cancellationToken)
        {
            return  await _repositoryPostgres.GetAllUsersAsync(cancellationToken);
        }
        /// Elimina un usuario por su email.
        public async Task<bool> DeleteUserByEmailServices(string email, CancellationToken cancellationToken)
        {
            return await _repositoryPostgres.DeleteUserByEmail(email, cancellationToken);
        }
        /// Actualiza los datos de un usuario.
        public async Task<HttpStatusCode> UpdateUserServices(string email, User newUser)
        {
            return await _repositoryPostgres.UpdateUser(email, newUser);
        }

    }
}
