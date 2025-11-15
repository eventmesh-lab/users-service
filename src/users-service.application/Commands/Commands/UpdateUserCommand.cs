using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aplication.DTOs;
using MediatR;

namespace Aplication.Commands.Commands
{
    public class UpdateUserCommand : IRequest<bool>
    {
        public string Email { get; set; }
        public UpdateUserDTO UpdateUserDTO { get; set; }

        public UpdateUserCommand(string email, UpdateUserDTO updateUserDTO)
        {
            Email = email;
            UpdateUserDTO = updateUserDTO;
        }
    }
}
