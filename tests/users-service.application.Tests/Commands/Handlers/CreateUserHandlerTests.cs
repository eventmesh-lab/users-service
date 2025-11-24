using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using users_service.application.Commands.Commands;
using users_service.application.Commands.Handlers;
using users_service.application.DTOs;
using users_service.application.Interfaces;
using users_service.domain.Entities;

namespace users_service.application.Tests.Commands.Handlers
{
    public class CreateUserHandlerTests
    {
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly Mock<IKeycloakRepository> _keycloakRepoMock;
        private readonly CreateUserHandler _handler;

        public CreateUserHandlerTests()
        {
            _userServicesMock = new Mock<IUserServices>();
            _keycloakRepoMock = new Mock<IKeycloakRepository>();
            _handler = new CreateUserHandler(_userServicesMock.Object, _keycloakRepoMock.Object);
        }

        private CreateUserCommand BuildCommand() =>
            new CreateUserCommand(new UserCreateDTO
            {
                Email = "user@test.com",
                FirstName = "David",
                LastName = "Perez",
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = new DateTime(2000, 1, 1),
                Password = "123456",
                RoleUser = "Usuario"
            });

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserAlreadyExists()
        {
            var command = BuildCommand();
            _userServicesMock.Setup(s => s.GetUserByEmailServices(command.UserCreateDTO.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User());

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal($"El usuario con email user@test.com ya existe en la base de datos.", ex.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenAddUserFails()
        {
            var command = BuildCommand();
            _userServicesMock.Setup(s => s.GetUserByEmailServices(command.UserCreateDTO.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _userServicesMock.Setup(s => s.AddUserPostgres(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
        }
        [Fact]
        public async Task Handle_ShouldThrow_WhenAddUserKeycloakFails()
        {
            var command = BuildCommand();
            _userServicesMock.Setup(s => s.GetUserByEmailServices(command.UserCreateDTO.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _userServicesMock.Setup(s => s.AddUserPostgres(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _keycloakRepoMock.Setup(s => s.RegisterUserAsyncRepo(command.UserCreateDTO.Email, command.UserCreateDTO.FirstName, 
                    command.UserCreateDTO.LastName, command.UserCreateDTO.Password))
                .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
        }
        [Fact]
        public async Task Handle_ShouldThrow_WhenAddRoleFails()
        {
            var command = BuildCommand();
            _userServicesMock.Setup(s => s.GetUserByEmailServices(command.UserCreateDTO.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _userServicesMock.Setup(s => s.AddUserPostgres(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _keycloakRepoMock.Setup(s => s.RegisterUserAsyncRepo(command.UserCreateDTO.Email, command.UserCreateDTO.FirstName,
                    command.UserCreateDTO.LastName, command.UserCreateDTO.Password))
                    .ReturnsAsync(true);
            _keycloakRepoMock.Setup(s => s.AssignRealmRoleToUserAsyncRepo(command.UserCreateDTO.Email, command.UserCreateDTO.RoleUser))
                .ThrowsAsync(new Exception("DB error"));
            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));
        }
        [Fact]
        public async Task Handle_CreateUserResponseDto_Success()
        {
            var command = BuildCommand();
            _userServicesMock.Setup(s => s.GetUserByEmailServices(command.UserCreateDTO.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _userServicesMock.Setup(s => s.AddUserPostgres(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _keycloakRepoMock.Setup(s => s.RegisterUserAsyncRepo(command.UserCreateDTO.Email, command.UserCreateDTO.FirstName,
                    command.UserCreateDTO.LastName, command.UserCreateDTO.Password))
                .ReturnsAsync(true);
            _keycloakRepoMock.Setup(s => s.AssignRealmRoleToUserAsyncRepo(command.UserCreateDTO.Email, command.UserCreateDTO.RoleUser))
                .ReturnsAsync(true);
            var result = await _handler.Handle(command, CancellationToken.None);
             Assert.Equal(result.FullName, command.UserCreateDTO.FirstName+" "+command.UserCreateDTO.LastName);
        }



    }
}
