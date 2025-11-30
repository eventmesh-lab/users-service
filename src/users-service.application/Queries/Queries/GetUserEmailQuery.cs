using users_service.application.DTOs.DTOResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Queries.Queries
{
    /// Comando para consultar un usuario mediante su email.
    public class GetUserEmailQuery: IRequest<GetUserResponseDto>
    {
        public string Email { get; set; }
        public GetUserEmailQuery(string email)
        {
            Email = email;
        }
    }
}
