using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.domain.ValueObjects;

namespace users_service.infrastructure.Persistence.Models
{
    /// Modelo de datos que representa un usuario en la base de datos PostgreSQL.
    public class UserPostgres
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Birthdate { get; set; }
        public string RoleUser { get; set; }

    }
}
