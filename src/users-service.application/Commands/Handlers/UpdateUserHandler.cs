using users_service.application.Commands.Commands;
using users_service.application.DTOs.DTOResponse;
using users_service.application.Interfaces;
using users_service.application.Mappers;
using users_service.domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Commands.Handlers
{
    public class UpdateUserHandler: IRequestHandler<UpdateUserCommand, bool>
    {
        public readonly IMediator _mediator;
        public readonly IUserServices _userServices;
        public readonly IKeycloakRepository _usuarioKeycloakRepository;
        public UpdateUserHandler(IUserServices userServices, IKeycloakRepository usuarioKeycloakRepository)
        {
            _userServices = userServices;
            _usuarioKeycloakRepository = usuarioKeycloakRepository;
        }
        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var oldUser = await _userServices.GetUserByEmailServices(request.Email, cancellationToken);
            if (oldUser == null)
            {
                throw new ApplicationException($"El usuario con email {request.Email} no existe en la base de datos.");
            }
            try
            {
                Console.WriteLine("Actualizando usuario en la base de datos Postgres.");
                var newUser = UserMapperApp.UpdateUserToDomain(request.UpdateUserDTO, oldUser);
                Console.WriteLine($"Nuevo usuario mapeado: {newUser.FirstName}, {newUser.LastName}, {newUser.Email}, {newUser.PhoneNumber}, {newUser.Address}, " +
                    $"{newUser.Birthdate}, {newUser.RoleUser}");
                var result = await _userServices.UpdateUserServices(request.Email, newUser);
                Console.WriteLine("Usuario actualizado en la base de datos Postgres.");

                try
                {
                    if (!string.IsNullOrWhiteSpace(request.UpdateUserDTO.FirstName) ||!string.IsNullOrWhiteSpace(request.UpdateUserDTO.LastName)
)
                    {
                        var userInKeyclaok = await _usuarioKeycloakRepository.UpdateUserInKeycloakAsyncRepo(request.Email, newUser.FirstName, newUser.LastName);
                        Console.WriteLine($"Keycloak: {userInKeyclaok}");
                    }
                    return result == System.Net.HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    await _userServices.UpdateUserServices(request.Email, oldUser);
                    throw new ApplicationException($"No se pudo actualizar el usuario en keyclaok", ex);
                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"No se pudo actualizar el usuario en la base de datos", ex);
            }
        }


    }
}
