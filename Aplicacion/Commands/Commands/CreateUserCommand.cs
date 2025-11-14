using Aplication.DTOs;
using Aplication.DTOs.DTOResponse;
using Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplication.Commands.Commands
{
    public class CreateUserCommand : IRequest<CreateUserResponseDto>
    {
        public UserCreateDTO UserCreateDTO { get; set; }
        public CreateUserCommand(UserCreateDTO userCreateDTO)
        {
            UserCreateDTO = userCreateDTO;
        }
    }
}
