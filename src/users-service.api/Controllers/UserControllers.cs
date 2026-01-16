using users_service.application.Commands.Commands;
using users_service.application.DTOs;
using users_service.domain.Interfaces;
using users_service.application.Queries.Handlers;
using users_service.application.Queries.Queries;
using users_service.application.Validator;
using users_service.domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace users_service.api.Controllers
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
            try
            {

                var validator = new UserDTOValidator();
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
            catch (UsuarioDTOException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message =  ex.Message });
            }
        }


        [HttpGet("getUser/{email}")]
        public async Task<IActionResult> GetUser(string email, CancellationToken cancellationToken)
        {
            if (email == null || string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El email no puede estar vacío.");
            }

            var command = new GetUserEmailQuery(email);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(new { Usuario = response, Mensaje = "Usuario encontrado exitosamente." });
        }

        [HttpPost("changePassword/{email}")]

        public async Task<IActionResult> ChangePassword(string email, [FromBody] ChangePasswordDTO changePasswordDTO, CancellationToken cancellationToken)
        {
            try
            {

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
            catch (UsuarioDTOException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("updateUser/{email}")]
        public async Task<IActionResult> UpdateUser(string email, [FromBody] UpdateUserDTO updateUserDTO, CancellationToken cancellationToken)
        {
            try
            {
                var validator = new UpdateUserDTOValidator();
                var resultado = validator.Validate(updateUserDTO);
                if (!resultado.IsValid)
                {
                    var errores = string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage));
                    throw new UsuarioDTOException(new Exception(errores));
                }

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
            catch (UsuarioDTOException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsers( CancellationToken cancellationToken)
        {
            Console.WriteLine("Llegó al controlador");

            var command = new GetUsersQuery();
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }

        [HttpGet("getIdUser/{email}")]
        public async Task<IActionResult> GetIdUser(string email, CancellationToken cancellationToken)
        {
            if (email == null || string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El email no puede estar vacío.");
            }

            var command = new GetIdByEmailQuery(email);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }


    }
}


