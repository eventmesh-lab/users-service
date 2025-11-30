using users_service.application.Commands.Commands;
using users_service.application.DTOs.DTOResponse;
using users_service.application.Interfaces;
using users_service.domain.Entities;
using users_service.application.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Commands.Handlers
{
    /// Handler para el comando ChangePasswordCommand.
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        public readonly IMediator _mediator;
        public readonly IUserServices _userServices;
        public readonly IKeycloakRepository _usuarioKeycloakRepository;

        /// Inicializa una nueva instancia del handler.
        public ChangePasswordHandler(IUserServices userServices, IKeycloakRepository usuarioKeycloakRepository)
        {
            _userServices = userServices;
            _usuarioKeycloakRepository = usuarioKeycloakRepository;
        }
        /// Maneja el comando CrearEventoCommand.
        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // Validar si el usuario existe
            var user = await _userServices.GetUserByEmailServices(request.Email, cancellationToken);
            if (user == null)
            {
                throw new ApplicationException($"El usuario {request.Email} no existe en la base de datos.");
            }
            try {
                // Persistir la nueva contraseña en Keycloak
                var userId = await _usuarioKeycloakRepository.GetUserIdByUsernameAsyncRepo(request.Email);
                Console.WriteLine($"UserId obtenido de Keycloak: {userId}");
                await _usuarioKeycloakRepository.ChangePasswordAsyncRepo(request.Email, request.ChangePasswordDto.NewPassword);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"No se pudo cambiar la contraseña en la base de datos", ex);
            }
            
        }
    }
}
