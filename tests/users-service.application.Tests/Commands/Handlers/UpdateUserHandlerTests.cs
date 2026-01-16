using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using users_service.application.Commands.Commands;
using users_service.application.Commands.Handlers;
using users_service.application.DTOs;
using users_service.domain.Interfaces;
using users_service.domain.Entities;
using users_service.domain.ValueObjects;

namespace users_service.application.Tests.Commands.Handlers
{
    public class UpdateUserHandlerTests
    {
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly Mock<IKeycloakRepository> _keycloakRepoMock;
        private readonly Mock<IActivityService> _activityServiceMock;
        private readonly UpdateUserHandler _handler;
        private UpdateUserCommand command = new UpdateUserCommand("user@test.com", new UpdateUserDTO
        {
            FirstName = "David",
            LastName = "Perez",
            PhoneNumber = "12345678910",
            Address = "Caracas",
            Birthdate = new DateTime(1990, 1, 1)
        });

        public UpdateUserHandlerTests()
        {
            _userServicesMock = new Mock<IUserServices>();
            _keycloakRepoMock = new Mock<IKeycloakRepository>();
            _activityServiceMock = new Mock<IActivityService>();
            _handler = new UpdateUserHandler(_userServicesMock.Object, _activityServiceMock.Object, _keycloakRepoMock.Object);
        }
            
        [Fact]
        public async Task Handle_ShouldThrow_WhenUserNotExists()
        {
            _userServicesMock.Setup(s => s.GetUserByEmail(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal($"El usuario con email user@test.com no existe en la base de datos.", ex.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenAddUserPostgresFails()
        {
            var oldUser= _userServicesMock.Setup(s => s.GetUserByEmail(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User());
             _userServicesMock.Setup(s => s.UpdateUser(command.Email,It.IsAny<User>()))
                .ThrowsAsync(new Exception("DB error"));

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal($"No se pudo actualizar el usuario en la base de datos", ex.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenAddUserKeyclaokFails()
        {
            var oldUser = _userServicesMock.Setup(s => s.GetUserByEmail(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User());
            _userServicesMock.Setup(s => s.UpdateUser(command.Email, It.IsAny<User>()))
                .ReturnsAsync(HttpStatusCode.OK);
            _keycloakRepoMock.Setup(s => s.UpdateUserInKeycloakAsyncRepo(command.Email,command.UpdateUserDTO.FirstName,command.UpdateUserDTO.LastName))
                .ThrowsAsync(new Exception("DB error"));

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
        }
        [Fact]
        public async Task Handle_True_Success()
        {
            _userServicesMock.Setup(s => s.GetUserByEmail(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User());
            _userServicesMock.Setup(s => s.UpdateUser(command.Email, It.IsAny<User>()))
                .ReturnsAsync(HttpStatusCode.OK);
            _keycloakRepoMock.Setup(s => s.UpdateUserInKeycloakAsyncRepo(command.Email, command.UpdateUserDTO.FirstName, command.UpdateUserDTO.LastName))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_SuccessWithDoesNotEnterTheIf()
        {
            UpdateUserCommand request = new UpdateUserCommand("user@test.com", new UpdateUserDTO
            {
                FirstName = "",
                LastName = "",
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = new DateTime(1990, 1, 1)
            });
            _userServicesMock.Setup(s => s.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User());
            _userServicesMock.Setup(s => s.UpdateUser(request.Email, It.IsAny<User>()))
                .ReturnsAsync(HttpStatusCode.OK);
            _keycloakRepoMock.Setup(s => s.UpdateUserInKeycloakAsyncRepo(request.Email, request.UpdateUserDTO.FirstName, request.UpdateUserDTO.LastName))
                .ReturnsAsync(true);

            _keycloakRepoMock.Verify(p=>p.UpdateUserInKeycloakAsyncRepo(request.Email, request.UpdateUserDTO.FirstName, request.UpdateUserDTO.LastName), Times.Never());
        }
    }
}
