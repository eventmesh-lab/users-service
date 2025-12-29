using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.application.DTOs.DTOResponse;
using users_service.application.Mappers;
using users_service.application.Queries.Queries;
using users_service.domain.Interfaces;

namespace users_service.application.Queries.Handlers
{
    public class GetIdByEmailHandler : IRequestHandler<GetIdByEmailQuery,Guid?>
    {
        public readonly IUserServices _userServices;
        /// Inicializa una nueva instancia del handler.
        public GetIdByEmailHandler(IUserServices userServices)
        {
            _userServices = userServices;
        }
        /// Maneja el comando GetUserEmailQuery.
        public async Task<Guid?> Handle(GetIdByEmailQuery request, CancellationToken cancellationToken)
        {
            var userRegistered = await _userServices.GetUserIdByEmailAsync(request.Email);

            if (userRegistered == null)
            {
                throw new ApplicationException($"El usuario con email {request.Email} no existe en la base de datos.");
            }
            return userRegistered;
        }
    }
}
