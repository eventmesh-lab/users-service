using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.DTOs
{
    /// DTO de entrada para cambiar la contraseña de un usuario.
    public class ChangePasswordDTO
    {
        public string NewPassword { get; set; }
        public ChangePasswordDTO( string newPassword)
        {
            NewPassword = newPassword;
        }
    }
}
