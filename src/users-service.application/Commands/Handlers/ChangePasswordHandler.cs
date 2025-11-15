using Aplication.Commands.Commands;
using Aplication.DTOs.DTOResponse;
using Aplication.Interfaces;
using Domain.Entities0;
using Insfrastructure.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplication.Commands.Handlers
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        public readonly IUserServices _userServices;
        public readonly IKeycloakRepository _usuarioKeycloakRepository;
        public ChangePasswordHandler(IUserServices userServices, IKeycloakRepository usuarioKeycloakRepository)
        {
            _userServices = userServices;
            _usuarioKeycloakRepository = usuarioKeycloakRepository;
        }
        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {

            var user = await _userServices.GetUserByEmailServices(request.Email, cancellationToken);
            if (user == null)
            {
                throw new ApplicationException($"El usuario {request.Email} no existe en la base de datos.");
            }
            try { 
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
