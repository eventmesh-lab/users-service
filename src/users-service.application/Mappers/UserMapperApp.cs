using Aplication.Commands.Commands;
using Aplication.DTOs;
using Aplication.DTOs.DTOResponse;
using Domain.Entities0;
using Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Aplication.Mappers
{
    public class UserMapperApp
    {
        public static User ToDomain(CreateUserCommand request)
        {
            
            var email = Email.Create(request.UserCreateDTO.Email);
            var role = Role.CrearDesdeTexto(request.UserCreateDTO.RoleUser);

            return new User(request.UserCreateDTO.FirstName, request.UserCreateDTO.LastName, 
                email, request.UserCreateDTO.PhoneNumber, request.UserCreateDTO.Address, request.UserCreateDTO.Birthdate, role );
        }

        public static CreateUserResponseDto ToDto(User user)
        {

            return new CreateUserResponseDto(user.FirstName, user.LastName, user.Email.Value);
        }

        public static GetUserResponseDto ToGetUserDto(User user)
        {
            return new GetUserResponseDto(user.FirstName, user.LastName, user.Email.Value, user.PhoneNumber, user.Address, user.Birthdate);
        }

        public static User UpdateUserToDomain(UpdateUserDTO updateUserDTO, User user)
        {

            return new User
            {
                Id = user.Id,
                FirstName = updateUserDTO.FirstName ?? user.FirstName,
                LastName = updateUserDTO.LastName ?? user.LastName,
                Email = user.Email,
                PhoneNumber = updateUserDTO.PhoneNumber ?? user.PhoneNumber,
                Address = updateUserDTO.Address ?? user.Address,
                Birthdate = updateUserDTO.Birthdate ?? user.Birthdate,
                RoleUser = user.RoleUser
            };
        }


    }
}
