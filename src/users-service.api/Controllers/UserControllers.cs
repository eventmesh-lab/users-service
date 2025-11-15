using Aplication.Commands.Commands;
using Aplication.DTOs;
using Aplication.Interfaces;
using Aplication.Queries.Queries;
using Aplication.Validator;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserControllers : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserControllers(IUserServices userService, IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("registerUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Llegó al controlador");
            Console.WriteLine($"Request Data: {request.FirstName}, {request.LastName}, {request.Email}, {request.PhoneNumber}, {request.Address}, " +
                $"{request.Birthdate}, {request.RoleUser}");

            var validator = new UsuarioDTOValidator();
            var resultado = validator.Validate(request);

            if (!resultado.IsValid)
            {
                var errores = string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage));
                throw new UsuarioDTOException(new Exception(errores));
            }
            var command = new CreateUserCommand(request);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(new { Usuario = response, Mensaje = "Usuario registrado exitosamente." });
        }


        [HttpGet("getUser/{email}")]
        public async Task<IActionResult> GetUser(string email, CancellationToken cancellationToken)
        {
            Console.WriteLine("Llegó al controlador");
            if (email == null || string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El email no puede estar vacío.");
            }

            var command = new GetUserEmailCommand(email);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(new { Usuario = response, Mensaje = "Usuario encontrado exitosamente." });
        }

        [HttpPost("changePassword/{email}")]

        public async Task<IActionResult> ChangePassword(string email, [FromBody] ChangePasswordDTO changePasswordDTO, CancellationToken cancellationToken)
        {
            Console.WriteLine("Llegó al controlador para cambiar la contraseña");

            if (changePasswordDTO == null || string.IsNullOrWhiteSpace(changePasswordDTO.NewPassword))
            {
                throw new Exception("La nueva contraseña no puede estar vacía.");
            }
            var validator = new ChangePasswordValidator();
            var resultado = validator.Validate(changePasswordDTO);

            if (!resultado.IsValid)
            {
                var errores = string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage));
                throw new UsuarioDTOException(new Exception(errores));
            }

            var response = await _mediator.Send(new ChangePasswordCommand(email, changePasswordDTO));
            return response ? Ok("Contraseña cambiada exitosamente.") : BadRequest(" Token inválido o expirado.");
        }

        [HttpPut("updateUser/{email}")]
        public async Task<IActionResult> UpdateUser(string email, [FromBody] UpdateUserDTO updateUserDTO, CancellationToken cancellationToken)
        {
            Console.WriteLine("Llegó al controlador para actualizar el usuario");
            /* var validator = new UsuarioDTOValidator();
             var resultado = validator.Validate(updateUserDTO);
             if (!resultado.IsValid)
             {
                 var errores = string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage));
                 throw new UsuarioDTOException(new Exception(errores));
             }*/
            var command = new UpdateUserCommand(email, updateUserDTO);
            var response = await _mediator.Send(command, cancellationToken);
            if (response)
            {
                return Ok("Usuario actualizado exitosamente.");
            }
            else
            {
                return BadRequest("No se pudo actualizar el usuario.");
            }
        }
    }
}




