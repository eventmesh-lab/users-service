using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.DTOs.DTOResponse
{
    public class GetUserResponseDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime Birthdate { get; set; }

        public GetUserResponseDto(string name, string lastName, string email, string phoneNumber, string address, DateTime birthdate)
        {
            FullName = $"{name} {lastName}";
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            Birthdate = birthdate;
        }
    }
}
