using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.domain.Entities;

namespace users_service.domain.Interfaces
{
    /// Interfaz que define los servicios relacionados con los usuarios.
    public interface IUserServices
    {
        // Agrega un nuevo usuario a la base de datos PostgreSQL.
        Task AddUserPostgres(User user, CancellationToken cancellationToken);
        // Obtiene un usuario por su correo electrónico.
        Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken);
        // Obtiene todos los usuarios.
        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken);
        // Elimina un usuario por su correo electrónico.
        Task<bool> DeleteUserByEmail(string email, CancellationToken cancellationToken);
        // Actualiza los datos de un usuario.
        Task<System.Net.HttpStatusCode> UpdateUser(string email, User newUser);
        Task<Guid?> GetUserIdByEmailAsync(string email);
    }
}
