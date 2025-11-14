using Aplication.DTOs.DTOResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplication.Queries.Queries
{
    public class GetUserEmailCommand: IRequest<GetUserResponseDto>
    {
        public string Email { get; set; }
        public GetUserEmailCommand(string email)
        {
            Email = email;
        }
    }
}
