using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using users_service.application.DTOs;
using users_service.domain.Interfaces;
using users_service.domain.Entities;
using users_service.domain.ValueObjects;
using users_service.infrastructure.Mappers;
using users_service.infrastructure.Persistence.Context;
using users_service.infrastructure.Persistence.Models;

namespace users_service.infrastructure.Persistence.Repositories
{
    /// Implementacion del repositorio de postgresql para la persistencia de usuario.
    public class UserRepositoyPostgres : IUserServices
    {
        public readonly AppDbContext _context;

        /// Inicializa una nueva instancia del repositorio con el contexto de la base de datos proporcionado.
        public UserRepositoyPostgres(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// Agrega un nuevo usuario al repositorio.
        public async Task AddUserPostgres(User user, CancellationToken cancellationToken)
        {
            var model = UserMappers.ToModel(user);
            _context.Users.Add(model);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// Obtiene un usuario por su email.
        public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            var userModel = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (userModel == null)
            {
                return null;
            }
            return UserMappers.ToDomain(userModel);
        }

        /// Obtiene todos los usuario existentes en el repositorio.
        public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var userModels = await _context.Users.ToListAsync(cancellationToken);

            var users = userModels
                .Select(UserMappers.ToDomain)
                .ToList();

            return users;
        }

        /// Elimina un usuario existente en el repositorio.
        public async Task<bool> DeleteUserByEmail(string email, CancellationToken cancellationToken)
        {

            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario != null)
            {
                try
                {
                    _context.Users.Remove(usuario);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception )
                {
                    return false;

                }
            }
            return false;

         }

        /// Actualiza un usuario existente en el repositorio.
        public async Task<HttpStatusCode> UpdateUser(string email, User newUser)
         {
            var userUpdated = UserMappers.ToModel(newUser);
            var user = await _context.Set<UserPostgres>()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) 
                return HttpStatusCode.NotFound;

            user.FirstName = newUser.FirstName;
            user.LastName = newUser.LastName ;
            user.PhoneNumber = newUser.PhoneNumber;
            user.Address = newUser.Address ;
            user.Birthdate = newUser.Birthdate.ToString();

            _context.Set<UserPostgres>().Update(user);
             await _context.SaveChangesAsync();
            return HttpStatusCode.OK;
         }

        public async Task<Guid?> GetUserIdByEmailAsync(string email)
        {
            var user = await _context.Users
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.Email == email);

            // Retorna el ID si existe, o null si no se encuentra
            return user?.Id;
        }
    }
}
