using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.domain.Entities;

namespace users_service.application.Interfaces
{
    /// Interfaz que define los servicios relacionados con los usuarios.
    public interface IUserServices
    {
        // Agrega un nuevo usuario a la base de datos PostgreSQL.
        Task AddUserPostgres(User user, CancellationToken cancellationToken);
        // Obtiene un usuario por su correo electrónico.
        Task<User?> GetUserByEmailServices(string email, CancellationToken cancellationToken);
        // Obtiene todos los usuarios.
        Task<List<User>> GetAllUsersServices(CancellationToken cancellationToken);
        // Elimina un usuario por su correo electrónico.
        Task<bool> DeleteUserByEmailServices(string email, CancellationToken cancellationToken);
        // Actualiza los datos de un usuario.
        Task<System.Net.HttpStatusCode> UpdateUserServices(string email, User newUser);
    }
}
