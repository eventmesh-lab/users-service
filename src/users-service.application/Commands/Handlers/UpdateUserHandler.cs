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
    /// Handler para el comando UpdateUserCommand.
    public class UpdateUserHandler: IRequestHandler<UpdateUserCommand, bool>
    {
        public readonly IMediator _mediator;
        public readonly IUserServices _userServices;
        public readonly IKeycloakRepository _usuarioKeycloakRepository;
        /// Inicializa una nueva instancia del handler.
        public UpdateUserHandler(IUserServices userServices, IKeycloakRepository usuarioKeycloakRepository)
        {
            _userServices = userServices;
            _usuarioKeycloakRepository = usuarioKeycloakRepository;
        }
        /// Maneja el comando UpdateUserCommand.
        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            // Validar si el usuario existe
            var oldUser = await _userServices.GetUserByEmailServices(request.Email, cancellationToken);
            
            if (oldUser == null)
            {
                throw new ApplicationException($"El usuario con email {request.Email} no existe en la base de datos.");
            }

            try
            {
                // Mapear comando a entidades del dominio
                var newUser = UserMapperApp.UpdateUserToDomain(request.UpdateUserDTO, oldUser);
                // Persistir actualizacion de usuario en postgresql
                var result = await _userServices.UpdateUserServices(request.Email, newUser);

                try
                {
                    if (!string.IsNullOrWhiteSpace(request.UpdateUserDTO.FirstName) ||!string.IsNullOrWhiteSpace(request.UpdateUserDTO.LastName))
                    {
                        // Persistir actualizacion de usuario en keycloak si el nombre o apellido cambiaron
                        var userInKeyclaok = await _usuarioKeycloakRepository.UpdateUserInKeycloakAsyncRepo(request.Email, newUser.FirstName, newUser.LastName);
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
