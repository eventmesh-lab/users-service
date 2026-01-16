using users_service.application.DTOs.DTOResponse;
using users_service.domain.Interfaces;
using users_service.application.Mappers;
using users_service.application.Queries.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Queries.Handlers
{
    /// Handler para el comando GetUsersQuery.
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<GetUsersResponseDto>>
    {
        public readonly IUserServices _userServices;
        /// Inicializa una nueva instancia del handler.
        public GetUsersHandler(IUserServices userServices)
        {
            _userServices = userServices;
        }
        /// Maneja el comando GetUsersQuery.
        public async Task<List<GetUsersResponseDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var usersRegistered = await _userServices.GetAllUsersAsync( cancellationToken);

            if (usersRegistered == null)
            {
                throw new ApplicationException($"No existen usuarios en la base de datos.");
            }
            return UserMapperApp.ToResponseGetList(usersRegistered);
        }
    }
}
