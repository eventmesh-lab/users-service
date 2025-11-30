using users_service.application.DTOs;
using users_service.application.DTOs.DTOResponse;
using users_service.domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Commands.Commands
{
    /// Comando para crear un nuevo usuario en estado borrador.
    public class CreateUserCommand : IRequest<CreateUserResponseDto>
    {
        public UserCreateDTO UserCreateDTO { get; set; }
        public CreateUserCommand(UserCreateDTO userCreateDTO)
        {
            UserCreateDTO = userCreateDTO;
        }
    }
}
