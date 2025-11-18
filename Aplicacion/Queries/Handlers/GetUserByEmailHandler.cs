using Aplication.DTOs.DTOResponse;
using Aplication.Interfaces;
using Aplication.Mappers;
using Aplication.Queries.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplication.Queries.Handlers
{
    public class GetUserByEmailHandler : IRequestHandler<GetUserEmailQuery, GetUserResponseDto>
    {
        public readonly IMediator _mediator;
        public readonly IUserServices _userServices;
        public GetUserByEmailHandler(IUserServices userServices, IKeycloakRepository usuarioKeycloakRepository)
        {
            _userServices = userServices;
        }
        public async Task<GetUserResponseDto> Handle(GetUserEmailQuery request, CancellationToken cancellationToken)
        {
            var userRegistered = await _userServices.GetUserByEmailServices(request.Email, cancellationToken);

            if ( userRegistered == null)
            {
                throw new ApplicationException($"El usuario con email {request.Email} no existe en la base de datos.");
            }
            return UserMapperApp.ToGetUserDto(userRegistered);
        }
    }
}
