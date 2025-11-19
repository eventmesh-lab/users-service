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
    public static class UserMappers
    {
        public static User ToDomain (UserPostgres model)
        {
            return new User(model.Id, model.FirstName,model.LastName,
                Email.Create(model.Email), model.PhoneNumber, model.Address, DateTime.Parse(model.Birthdate), Role.CrearDesdeTexto(model.RoleUser));
        }

        public static UserPostgres ToModel(User user)
        {
            return new UserPostgres { Id=user.Id, FirstName=user.FirstName,LastName=user.LastName, 
                 Email=user.Email.Value, PhoneNumber=user.PhoneNumber, Address=user.Address , Birthdate= user.Birthdate.ToString(), RoleUser= user.RoleUser.Valor};
        }
    }
}
