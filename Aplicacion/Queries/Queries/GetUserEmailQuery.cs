using Aplication.DTOs.DTOResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplication.Queries.Queries
{
    public class GetUserEmailQuery: IRequest<GetUserResponseDto>
    {
        public string Email { get; set; }
        public GetUserEmailQuery(string email)
        {
            Email = email;
        }
    }
}
