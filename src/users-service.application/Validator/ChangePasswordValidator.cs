using users_service.application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application.Validator
{
    //Validador para el DTO de cambio de contraseña
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDTO>
    {
        public ChangePasswordValidator()
        {

            RuleFor(u => u.NewPassword)
                    .NotEmpty().WithMessage("La contraseña es obligatoria.")
                    .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
        }
    }
}
