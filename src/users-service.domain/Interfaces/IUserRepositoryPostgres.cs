using users_service.domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace users_service.domain.Interfaces
{// Interfaz del repositorio de postgresql para la persistencia de usuario.
    // Define el contrato para la gestion de usuario en postgresql.
    public interface IUserRepositoryPostgres
    {
        /// Agrega un nuevo usuario al repositorio.
        Task AddUser(User user, CancellationToken cancellationToken);
        /// Obtiene un usuario por su email.
        Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken);
        /// Obtiene todos los usuario existentes en el repositorio.
        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken);
        /// Elimina un usuario existente en el repositorio.
        Task<bool> DeleteUserByEmail(string email, CancellationToken cancellationToken);
        /// Actualiza un usuario existente en el repositorio.
        Task<HttpStatusCode> UpdateUser(string email, User userUpdated);
    }
}
