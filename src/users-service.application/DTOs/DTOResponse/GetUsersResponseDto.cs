using users_service.domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.DTOs.DTOResponse
{
    public class GetUsersResponseDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Birthdate { get; set; }
        public string RoleUser { get; set; }

        public GetUsersResponseDto(string firstName, string lastName,
         string email, string phoneNumber, string address, string birthdate, string role)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            Birthdate = birthdate;
            RoleUser = role;
        }
        public GetUsersResponseDto()
        {
        }
    }
}
