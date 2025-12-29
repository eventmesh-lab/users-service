using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.application.DTOs.DTOResponse;

namespace users_service.application.Queries.Queries
{
    public class GetIdByEmailQuery : IRequest<Guid?>
    {
        public string Email { get; set; }
        public GetIdByEmailQuery(string email)
        {
            Email = email;
        }
    }
}