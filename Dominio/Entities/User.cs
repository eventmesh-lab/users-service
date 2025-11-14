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
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Email Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime Birthdate { get; set; }
        public Role RoleUser { get; set; }

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
