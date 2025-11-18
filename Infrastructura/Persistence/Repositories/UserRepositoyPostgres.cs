using Aplication.DTOs;
using Domain.Entities0;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Mappers;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepositoyPostgres : IUserRepositoryPostgres
    {
        public readonly AppDbContext _context;

        public UserRepositoyPostgres(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddUser(User user, CancellationToken cancellationToken)
        {
            var model = UserMappers.ToModel(user);
            _context.Users.Add(model);
            await _context.SaveChangesAsync(cancellationToken);
        }

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

        public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var userModels = await _context.Users.ToListAsync(cancellationToken);

            var users = userModels
                .Select(UserMappers.ToDomain)
                .ToList();

            return users;
        }

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
    }
}
