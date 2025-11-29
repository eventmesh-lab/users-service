using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.domain.Entities;
using users_service.domain.ValueObjects;
using users_service.infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace users_service.infrastructure.Mappers
{
    // Clase  que contiene métodos de mapeo entre User y UserPostgres.
    public static class UserMappers
    {
        /// Mapea un UserPostgres a un User.
        public static User ToDomain (UserPostgres model)
        {
            return new User(model.Id, model.FirstName,model.LastName,
                Email.Create(model.Email), model.PhoneNumber, model.Address, DateTime.Parse(model.Birthdate), Role.CrearDesdeTexto(model.RoleUser));
        }
        /// Mapea un User a un UserPostgres.
        public static UserPostgres ToModel(User user)
        {
            return new UserPostgres { Id=user.Id, FirstName=user.FirstName,LastName=user.LastName, 
                 Email=user.Email.Value, PhoneNumber=user.PhoneNumber, Address=user.Address , Birthdate= user.Birthdate.ToString(), RoleUser= user.RoleUser.Valor};
        }
    }
}
