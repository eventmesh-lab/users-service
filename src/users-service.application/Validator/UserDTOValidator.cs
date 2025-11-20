using users_service.application.DTOs;
using FluentValidation;
namespace users_service.application.Validator
{
    public class UserDTOValidator : AbstractValidator<UserCreateDTO>
    {
        public UserDTOValidator()
        {
            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage(" El nombre es obligatorio.");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage(" El apellido es obligatorio.");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage(" El correo es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo es inválido.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(u => u.PhoneNumber)
                .NotEmpty().WithMessage("El telefono es obligatorio.")
                .Matches(@"^\d{11}$").WithMessage(" El teléfono debe contener 11 dígitos.");

            RuleFor(u => u.Address)
                .NotEmpty().WithMessage(" La dirección es obligatoria.");

            RuleFor(u => u.Birthdate)
                .NotEmpty().WithMessage("La fecha de nacimiento es obligatoria.")
                .Must(date => date < DateTime.Today).WithMessage("La fecha de nacimiento debe estar en el pasado.")
                .Must(date => CalculateAge(date) >= 18).WithMessage("Debes ser mayor de 18 años.");

            RuleFor(u => u.RoleUser)
                 .NotEmpty().WithMessage("El Rol es obligatorio.");
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