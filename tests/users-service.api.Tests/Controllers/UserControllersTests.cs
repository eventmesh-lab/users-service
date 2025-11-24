using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using users_service.api.Controllers;
using users_service.application.Commands.Commands;
using users_service.application.DTOs;
using users_service.application.DTOs.DTOResponse;
using users_service.application.Interfaces;
using users_service.application.Queries.Queries;
using users_service.domain.Entities;
using users_service.domain.Exceptions;
using users_service.domain.ValueObjects;

namespace users_service.api.Tests.Controllers
{
    public class UserControllersTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly UserControllers _controller;

        public UserControllersTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new UserControllers(Mock.Of<IUserServices>(), _mockMediator.Object);
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

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.CreateUser(dto, CancellationToken.None);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

        }

        [Fact]
        public async Task CreateUser_DebeRetornarBadRequest_SiDTOInvalido()
        {
            // Arrange
            var dto = new UserCreateDTO
            {
                FirstName = "",
                LastName = "Perez",
                Email = "user@gmail.com",
                PhoneNumber = "04126110032",
                Address = "123 Test St",
                Birthdate = new DateTime(2000, 1, 1),
                RoleUser = "Usuario",
                Password = "password123"
            };

            var result = await _controller.CreateUser(dto, CancellationToken.None);

            // Assert
            var ex = Assert.IsType<ObjectResult>(result);
            var error = ex.Value.GetType().GetProperty("message")?.GetValue(ex.Value, null);
            Assert.Equal("Los datos ingresados no son válidos.  El nombre es obligatorio.", error);

        }

        [Fact]
        public async Task CreateUser_DebeRetornarBadRequest_YearInvalido()
        {
            // Arrange
            var dto = new UserCreateDTO
            {
                FirstName = "David",
                LastName = "Perez",
                Email = "user@gmail.com",
                PhoneNumber = "04126110032",
                Address = "123 Test St",
                Birthdate = new DateTime(2025, 1, 1),
                RoleUser = "Usuario",
                Password = "password123"
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            var result = await _controller.CreateUser(dto, CancellationToken.None);

            // Assert
            var ex = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, ex.StatusCode);
            var error = ex.Value.GetType().GetProperty("message")?.GetValue(ex.Value, null);
            Assert.Equal("Los datos ingresados no son válidos. Debes ser mayor de 18 años.", error);

        }

        [Fact]
        public async Task CreateUser_DebeRetornar500_SiOcurreErrorInesperado()
        {
            // Arrange
            var dto = new UserCreateDTO
            {
                FirstName = "David",
                LastName = "Perez",
                Email = "user@gmail.com",
                PhoneNumber = "12345678910",
                Address = "123 Test St",
                Birthdate = new DateTime(1990, 1, 1),
                RoleUser = "Usuario"
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            // Act
            var result = await _controller.CreateUser(dto, CancellationToken.None);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetUser_EmailIsNull_ThrowsException()
        {
            // Arrange
            string email = null;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetUser(email, CancellationToken.None));

            Assert.Equal("El email no puede estar vacío.", ex.Message);
        }

        [Fact]
        public async Task GetUser_EmailIsEmpty_ThrowsException()
        {
            // Arrange
            string email = "   ";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetUser(email, CancellationToken.None));

            Assert.Equal("El email no puede estar vacío.", ex.Message);
        }

        [Fact]
        public async Task GetUser_ValidEmail_ReturnsOkWithUser()
        {
            // Arrange
            var email = "test@test.com";
            var fakeUser = new GetUserResponseDto("David", " Perez", email,
                "04126110032", "Caracas", new DateTime(1990, 1, 1));

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUserEmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _controller.GetUser(email, CancellationToken.None);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_ThrowException_WhenInvalidEmail()
        {
            // Arrange
            string email = null;
            var changePasswordDTO = new ChangePasswordDTO(" ");

            // Act & Assert

            var result = await _controller.ChangePassword(email, changePasswordDTO, CancellationToken.None);
            var ex = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, ex.StatusCode);
            var error = ex.Value.GetType().GetProperty("message")?.GetValue(ex.Value, null);
            Assert.Equal("La nueva contraseña no puede estar vacía.", error);
        }

        [Fact]
        public async Task ChangePassword_ReturnBadRequest_WhenDTOInvalid()
        {
            // Arrange
            string email = null;
            var changePasswordDTO = new ChangePasswordDTO("123");

            var result = await _controller.ChangePassword(email, changePasswordDTO, CancellationToken.None);

            // Assert
            var ex = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, ex.StatusCode);
            var error = ex.Value.GetType().GetProperty("message")?.GetValue(ex.Value, null);
            Assert.Equal("Los datos ingresados no son válidos. La contraseña debe tener al menos 6 caracteres.", error);

        }

        [Fact]
        public async Task ChangePassword_ReturnOk_Success()
        {
            // Arrange
            string email = "user@gmail.com";
            var changePasswordDTO = new ChangePasswordDTO("newPasword123");


            _mockMediator
                .Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ChangePassword(email, changePasswordDTO, CancellationToken.None);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

        }

        [Fact]
        public async Task UpdateUser_ThrowException_WhenPhoneInvalid()
        {
            // Arrange
            string email = "user@gmail.com";
            var dto = new UpdateUserDTO()
            {
                FirstName = "",
                LastName = "",
                PhoneNumber = "04126112",
                Address = "Caracas",
                Birthdate = new DateTime(2000, 1, 1),
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UsuarioDTOException>(() =>
                _controller.UpdateUser(email, dto, CancellationToken.None));

            Assert.Equal("Los datos ingresados no son válidos. El teléfono debe contener exactamente 11 dígitos.",
                ex.Message);

        }

        [Fact]
        public async Task UpdateUser_ThrowException_WhenEmailInvalid()
        {
            // Arrange
            string email = "user@gmail.com";
            var dto = new UpdateUserDTO()
            {
                FirstName = "",
                LastName = "",
                PhoneNumber = "04126110032",
                Address = "Caracas",
                Birthdate = new DateTime(2020, 1, 1),
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UsuarioDTOException>(() =>
                _controller.UpdateUser(email, dto, CancellationToken.None));

            Assert.Equal("Los datos ingresados no son válidos. Debes ser mayor de 18 años.", ex.Message);

        }

        [Fact]
        public async Task UpdateUser_BadRequest_UpdateFail()
        {
            // Arrange
            string email = "user@gmail.com";
            var dto = new UpdateUserDTO()
            {
                FirstName = "User",
                LastName = "User",
                PhoneNumber = "04126110032",
                Address = "Caracas",
                Birthdate = new DateTime(2000, 1, 1),
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var result = await _controller.UpdateUser(email, dto, CancellationToken.None);

            var ex = Assert.IsType<BadRequestObjectResult>(result);
            var error = Assert.IsType<string>(ex.Value);
            Assert.Equal("No se pudo actualizar el usuario.", error);

        }

        [Fact]
        public async Task UpdateUser_Success()
        {
            // Arrange
            string email = "user@gmail.com";
            var dto = new UpdateUserDTO()
            {
                FirstName = "User",
                LastName = "User",
                PhoneNumber = "04126110032",
                Address = "Caracas",
                Birthdate = new DateTime(2000, 1, 1),
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            // Act & Assert
            var result = await _controller.UpdateUser(email, dto, CancellationToken.None);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            Assert.Equal("Usuario actualizado exitosamente.", objectResult.Value);

        }

        [Fact]
        public async Task GetUsers_Success_ListUser_Success()
        {
            GetUsersResponseDto user = new GetUsersResponseDto("David", "Perez", "user@gmail.com",
                        "12345678910", "123 Test St", "2000, 1, 1",
                                   "Usuario");
            GetUsersResponseDto user2 = new GetUsersResponseDto( "Mauricio", "Marquez", "user1@gmail.com",
                        "12345678910", "123 Test St", "2000, 1, 1",
                                    "Usuario");
             List<GetUsersResponseDto> listUser = new List<GetUsersResponseDto> { user, user2 };

             _mockMediator
                 .Setup(m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(listUser);

             var result = await _controller.GetUsers( CancellationToken.None);

            var objectResult = Assert.IsType<OkObjectResult>(result);
             Assert.Equal(200, objectResult.StatusCode);

        }
    }
    

}

