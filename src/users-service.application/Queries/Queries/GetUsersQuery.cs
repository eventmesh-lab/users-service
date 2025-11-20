using users_service.application.DTOs.DTOResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Queries.Queries
{
    public class GetUsersQuery : IRequest<List<GetUsersResponseDto>>
    {
    }
}
