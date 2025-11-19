using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.application.DTOs;
using MediatR;

namespace users_service.application.Commands.Commands
{
    public class ChangePasswordCommand: IRequest<bool>
    {
        public string Email { get; set; }
        public ChangePasswordDTO ChangePasswordDto { get; set; }
        public ChangePasswordCommand(string email, ChangePasswordDTO changePasswordDTO)
        {
            Email = email;
            ChangePasswordDto = changePasswordDTO;
        }
    }
}
