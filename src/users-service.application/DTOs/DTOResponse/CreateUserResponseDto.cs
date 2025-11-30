using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.DTOs.DTOResponse
{
    /// DTO de respuesta para representar un nuevo usuario creado.
    public class CreateUserResponseDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        public CreateUserResponseDto(string name,  string lastName, string email)
        {
            FullName = $"{name} {lastName}";
            Email = email;
        }
    }
}
