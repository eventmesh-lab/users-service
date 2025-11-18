using Aplication.Commands.Commands;
using Aplication.DTOs.DTOResponse;
using Aplication.Interfaces;
using Aplication.Mappers;
using Domain.Entities0;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplication.Commands.Handlers
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResponseDto>
    {
        public readonly IMediator _mediator;
        public readonly IUserServices _userServices;
        public readonly IKeycloakRepository _usuarioKeycloakRepository;
        public CreateUserHandler(IUserServices userServices, IKeycloakRepository usuarioKeycloakRepository)
        {
            _userServices = userServices;
            _usuarioKeycloakRepository = usuarioKeycloakRepository;
        }
        public async Task<CreateUserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {

            var user = UserMapperApp.ToDomain(request);
            
            if(_userServices.GetUserByEmailServices(request.UserCreateDTO.Email, cancellationToken).Result != null)
            {
                throw new ApplicationException($"El usuario con email {user.Email} ya existe en la base de datos.");
            }
            try
            {
                await _userServices.AddUserPostgres(user, cancellationToken);
            }
            catch (Exception ex)
            {

                throw new ApplicationException($"No se pudo registar el usuario en la base de datos", ex);
            }
            Console.WriteLine("Usuario agregado a la base de datos Postgres.");
            try
            {
                await _usuarioKeycloakRepository.RegisterUserAsyncRepo(request.UserCreateDTO.Email, request.UserCreateDTO.FirstName,
                                                                    request.UserCreateDTO.LastName, request.UserCreateDTO.Password);
            }
            catch (Exception ex)
            {
                await _userServices.DeleteUserByEmailServices(request.UserCreateDTO.Email, cancellationToken);
                throw new ApplicationException($"No se pudo registar el usuario en keycloak", ex);
            }
            Console.WriteLine("Usuario registrado en Keycloak.");
            try
            {
                await _usuarioKeycloakRepository.AssignRealmRoleToUserAsyncRepo(
                    request.UserCreateDTO.Email,
                    request.UserCreateDTO.RoleUser
                );
            }
            catch (Exception ex)
            {
                
                throw new ApplicationException($"No se pudo asignar el rol al usuario {request.UserCreateDTO.Email}", ex);
            }
            Console.WriteLine("Rol de usuario registrado en Keycloak.");
            return UserMapperApp.ToDto(user);
        }
    }
}
