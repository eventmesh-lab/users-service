using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities0
{
    public class User
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required Email Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
        public DateTime Birthdate { get; set; }
        public required Role RoleUser { get; set; }

        public User() { }

        public User(Guid id,string firstName,string lastName,
         Email email,string phoneNumber, string address , DateTime birthdate, Role role)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            Birthdate = birthdate;
            RoleUser = role;
        }

        public User(string firstName, string lastName, 
         Email email,  String phoneNumber, string address , DateTime birthdate, Role role)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            Birthdate = birthdate;
            RoleUser = role;
        }
    }
}
