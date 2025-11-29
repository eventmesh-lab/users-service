using users_service.domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.DTOs
{
    /// DTO de entrada para registrar un usuario.
    public class UserCreateDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime Birthdate { get; set; }
        public string RoleUser { get; set; }
        public string Password { get; set; }

    }
}
