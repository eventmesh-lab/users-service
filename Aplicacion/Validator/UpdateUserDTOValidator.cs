using Aplication.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplication.Validator
{
    public class UpdateUserDTOValidator :AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserDTOValidator()
        {
            RuleFor(u => u.PhoneNumber)
                .Matches(@"^\d{11}$")
                .When(u => !string.IsNullOrWhiteSpace(u.PhoneNumber))
                    .WithMessage("El teléfono debe contener exactamente 11 dígitos.");

            RuleFor(u =>  u.Birthdate)
                .Must(date => date == null || date < DateTime.Today)
                    .WithMessage("La fecha debe estar en el pasado.")
                .Must(date => date == null || CalculateAge(date.Value) >= 18)
                    .WithMessage("Debes ser mayor de 18 años.");
        }
        private int CalculateAge(DateTime birthdate)
        {
            var today = DateTime.Today;

            int age = today.Year - birthdate.Year;

            // Si aún no ha cumplido años este año, se resta uno
            if (today.Month < birthdate.Month ||
               (today.Month == birthdate.Month && today.Day < birthdate.Day))
            {
                age--;
            }

            return age;

        }
    }
}
