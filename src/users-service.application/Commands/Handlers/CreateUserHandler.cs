using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using users_service.application.Commands.Commands;
using users_service.application.DTOs.DTOResponse;
using users_service.application.Mappers;
using users_service.domain.Entities;
using users_service.domain.Interfaces;

namespace users_service.application.Commands.Handlers
{
    /// Handler para el comando CreateUserCommand.
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResponseDto>
    {
        public readonly IMediator _mediator;
        public readonly IUserServices _userServices;
        public readonly IKeycloakRepository _usuarioKeycloakRepository;
        private readonly IActivityService _activityService;
        /// Inicializa una nueva instancia del handler.
        public CreateUserHandler(IUserServices userServices,IActivityService activityService, IKeycloakRepository usuarioKeycloakRepository)
        {
            _userServices = userServices;
            _usuarioKeycloakRepository = usuarioKeycloakRepository;
            _activityService = activityService;
        }
        /// Maneja el comando CrearEventoCommand.
        public async Task<CreateUserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Mapear comando a entidades del dominio
            var user = UserMapperApp.ToDomain(request);

            // Valida que no exista un usuario con el mismo email
            if (_userServices.GetUserByEmail(request.UserCreateDTO.Email, cancellationToken).Result != null)
            {
                throw new ApplicationException($"El usuario con email {user.Email} ya existe en la base de datos.");
            }
            try
            {
                // Persistir el usuario en postgres
                await _userServices.AddUserPostgres(user, cancellationToken);
            }
            catch (Exception ex)
            {

                throw new ApplicationException($"No se pudo registar el usuario en la base de datos", ex);
            }
            Console.WriteLine("Usuario agregado a la base de datos Postgres.");
            try
            {
                // Persistir el usuario en keycloak
                await _usuarioKeycloakRepository.RegisterUserAsyncRepo(request.UserCreateDTO.Email, request.UserCreateDTO.FirstName,
                                                                    request.UserCreateDTO.LastName, request.UserCreateDTO.Password);
            }
            catch (Exception ex)
            {
                // Si falla el registro en keycloak, eliminar el usuario de postgres
                await _userServices.DeleteUserByEmail(request.UserCreateDTO.Email, cancellationToken);
                throw new ApplicationException($"No se pudo registar el usuario en keycloak", ex);
            }
            Console.WriteLine("Usuario registrado en Keycloak.");
            try
            {
                // Persistir rol al usuario en keycloak
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


            await _activityService.RegisterActivityAsync(
                email: request.UserCreateDTO.Email,
                action: $"Cuenta creada ",
                category: "Seguridad"
            );
            return UserMapperApp.ToDto(user);
        }
    }
}
