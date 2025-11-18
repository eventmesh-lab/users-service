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
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<GetUsersResponseDto>>
    {
        public readonly IMediator _mediator;
        public readonly IUserServices _userServices;
        public GetUsersHandler(IUserServices userServices)
        {
            _userServices = userServices;
        }
        public async Task<List<GetUsersResponseDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var usersRegistered = await _userServices.GetAllUsersServices( cancellationToken);

            if (usersRegistered == null)
            {
                throw new ApplicationException($"No existen usuarios en la base de datos.");
            }
            return UserMapperApp.ToResponseGetList(usersRegistered);
        }
    }
}
