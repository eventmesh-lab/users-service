using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using users_service.api.Controllers;
using users_service.application.Commands.Commands;
using users_service.application.DTOs;
using users_service.application.DTOs.DTOResponse;
using users_service.application.Interfaces;
using users_service.application.Queries.Queries;
using users_service.domain.Exceptions;
using Xunit;
using Moq;

namespace users_service.tests
{
    public class UserControllersTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IUserServices> _userServiceMock;
        private readonly UserControllers _controller;

        public UserControllersTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _userServiceMock = new Mock<IUserServices>();
            _controller = new UserControllers(_userServiceMock.Object, _mediatorMock.Object);
        }

        [Fact]
        public async Task CreateUser_DebeRetornarOk_SiDTOValido()
        {
            // Arrange
            var dto = new UserCreateDTO
            {
                FirstName = "David",
                LastName = "Perez",
                Email = "user@gmail.com",
                PhoneNumber = "12345678910",
                Address = "123 Test St",
                Birthdate = new DateTime(2000, 1, 1),
                RoleUser = "Usuario",
                Password = "password123"
            };
            CreateUserResponseDto responseDto = new CreateUserResponseDto(dto.FirstName, dto.LastName, dto.Email);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.CreateUser(dto, CancellationToken.None);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

        }


        [Fact]
        public async Task CreateUser_InvalidData_ReturnsBadRequest()
        {
            var invalidDto = new UserCreateDTO
            {
                FirstName = "Juan",
                Email = "invalid-email",
                Birthdate = DateTime.Today
            };

            var result = await _controller.CreateUser(invalidDto, CancellationToken.None);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.StatusCode.Should().Be(400);
            badRequest.Value.ToString().Should().Contain("El formato del correo es inválido");
        }

        [Fact]
        public async Task CreateUser_InternalException_Returns500()
        {
            var validDto = new UserCreateDTO
            {
                FirstName = "Juan",
                LastName = "Perez",
                Email = "juan@test.com",
                Password = "password123",
                PhoneNumber = "12345678901",
                Address = "Calle 123",
                Birthdate = DateTime.Today.AddYears(-20),
                RoleUser = "User"
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new Exception("Error DB"));

            var result = await _controller.CreateUser(validDto, CancellationToken.None);

            var serverError = result.Should().BeOfType<ObjectResult>().Subject;
            serverError.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetUser_ValidEmail_ReturnsOkWithUser()
        {
            // Arrange
            var email = "test@test.com";
            var fakeUser = new GetUserResponseDto("David", " Perez", email,
                "04126110032", "Caracas", new DateTime(1990, 1, 1));

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetUserEmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _controller.GetUser(email, CancellationToken.None);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetUser_InvalidEmail_ThrowsException(string email)
        {
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetUser(email, CancellationToken.None));
            ex.Message.Should().Be("El email no puede estar vacío.");
        }

        [Fact]
        public async Task ChangePassword_ValidData_ReturnsOk()
        {
            var dto = new ChangePasswordDTO("newpass123");
            string email = "test@test.com";

            _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);

            var result = await _controller.ChangePassword(email, dto, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be("Contraseña cambiada exitosamente.");
        }

        [Fact]
        public async Task ChangePassword_MediatorReturnsFalse_ReturnsBadRequest()
        {
            var dto = new ChangePasswordDTO("newpass123");
            string email = "test@test.com";

            _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);

            var result = await _controller.ChangePassword(email, dto, CancellationToken.None);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be(" Token inválido o expirado.");
        }

        [Fact]
        public async Task ChangePassword_EmptyPassword_Returns500()
        {
            var dto = new ChangePasswordDTO("");

            var result = await _controller.ChangePassword("email", dto, CancellationToken.None);

            var serverError = result.Should().BeOfType<ObjectResult>().Subject;
            serverError.StatusCode.Should().Be(500);
            serverError.Value.ToString().Should().Contain("La nueva contraseña no puede estar vacía.");
        }

        [Fact]
        public async Task ChangePassword_ShortPassword_ReturnsBadRequest()
        {
            var dto = new ChangePasswordDTO("123");

            var result = await _controller.ChangePassword("email", dto, CancellationToken.None);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.ToString().Should().Contain("La contraseña debe tener al menos 6 caracteres");
        }

        [Fact]
        public async Task UpdateUser_ValidData_ReturnsOk()
        {
            var dto = new UpdateUserDTO
            {
                PhoneNumber = "12345678901",
                Birthdate = DateTime.Today.AddYears(-20)
            };
            string email = "test@test.com";

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);

            var result = await _controller.UpdateUser(email, dto, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be("Usuario actualizado exitosamente.");
        }

        [Fact]
        public async Task UpdateUser_MediatorReturnsFalse_ReturnsBadRequest()
        {
            var dto = new UpdateUserDTO { PhoneNumber = "12345678901" };

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);

            var result = await _controller.UpdateUser("email", dto, CancellationToken.None);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("No se pudo actualizar el usuario.");
        }

        [Fact]
        public async Task UpdateUser_InvalidPhone_ReturnsBadRequest()
        {
            var dto = new UpdateUserDTO { PhoneNumber = "123" };

            var result = await _controller.UpdateUser("email", dto, CancellationToken.None);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.ToString().Should().Contain("El teléfono debe contener exactamente 11 dígitos");
        }

        [Fact]
        public async Task UpdateUser_GenericException_Returns500()
        {
            var dto = new UpdateUserDTO();
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new Exception("Error"));

            var result = await _controller.UpdateUser("email", dto, CancellationToken.None);

            var serverError = result.Should().BeOfType<ObjectResult>().Subject;
            serverError.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetUsers_ReturnsOk()
        {
            // Usamos una lista vacía pero del tipo correcto si el Query retorna IEnumerable<GetUsersResponseDto>
            // Si retorna object, new List<object> está bien. Asumiré que retorna una lista de DTOs.
            var list = new List<GetUsersResponseDto>();

            // IMPORTANTE: Asegúrate de que GetUsersQuery devuelve el mismo tipo que pones aquí en el Mock.
            // Si GetUsersQuery retorna List<GetUsersResponseDto>, usa eso. Si es object, usa object.
            // Según tu código GetUsersQuery parece devolver una lista genérica.

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(list);

            var result = await _controller.GetUsers(CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public void UsuarioDTOException_Constructor_Works()
        {
            var inner = new Exception("Inner");
            var ex = new UsuarioDTOException(inner);

            ex.Message.Should().StartWith("Los datos ingresados no son válidos.");
            ex.InnerException.Should().Be(inner);
        }
    }
}