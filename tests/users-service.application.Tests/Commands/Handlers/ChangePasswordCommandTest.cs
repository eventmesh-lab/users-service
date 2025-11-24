using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.application.Commands.Commands;
using users_service.application.Commands.Handlers;
using users_service.application.DTOs;
using users_service.application.Interfaces;
using users_service.domain.Entities;

namespace users_service.application.Tests.Commands.Handlers
{
    public class ChangePasswordCommandTest
    {
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly Mock<IKeycloakRepository> _keycloakRepoMock;
        private readonly ChangePasswordHandler _handler;                                                                                                                                                                                                                                                                                                                                        
        private ChangePasswordCommand command = new ChangePasswordCommand("user@test.com", new ChangePasswordDTO("NewPassword"));

        public ChangePasswordCommandTest()
        {
            _userServicesMock = new Mock<IUserServices>();
            _keycloakRepoMock = new Mock<IKeycloakRepository>();
            _handler = new ChangePasswordHandler(_userServicesMock.Object, _keycloakRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserNotExists()
        {
            _userServicesMock.Setup(s => s.GetUserByEmailServices(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal($"El usuario user@test.com no existe en la base de datos.", ex.Message);
        }
        [Fact]
        public async Task Handle_ShouldThrow_WhenPasswordCannotBeUpdated()
        {
            _userServicesMock.Setup(s => s.GetUserByEmailServices(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User());
            _keycloakRepoMock.Setup(s => s.ChangePasswordAsyncRepo(command.Email, command.ChangePasswordDto.NewPassword))
                .ThrowsAsync(new Exception("DB error"));

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal($"No se pudo cambiar la contraseña en la base de datos", ex.Message);
        }
        [Fact]
        public async Task Handle_True_Success()
        {
            _userServicesMock.Setup(s => s.GetUserByEmailServices(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User());
            _keycloakRepoMock.Setup(s => s.ChangePasswordAsyncRepo(command.Email, command.ChangePasswordDto.NewPassword))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);
            Assert.True(result);
        }
    }
}
