using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using users_service.application.Services;
using users_service.domain.Entities;
using users_service.domain.Interfaces;
using users_service.domain.ValueObjects;

namespace users_service.application.Tests.Services
{
    public class UserServicesTests
    {
        private readonly Mock<IUserRepositoryPostgres> _mockRepo;
        private readonly UserServices _userServices; // System Under Test

        public UserServicesTests()
        {
            _mockRepo = new Mock<IUserRepositoryPostgres>();
            _userServices = new UserServices(_mockRepo.Object);
        }

        [Fact]
        public async Task AddUserPostgres_ShouldCallRepositoryAddUser()
        {
            // Arrange
            var user = new User {
                Id = new Guid(),
                FirstName = "David",
                LastName = "Perez",
                Email = Email.Create("user@test.com"),
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = new DateTime(2000, 1, 1),
                RoleUser = Role.CrearDesdeTexto("Usuario")
            };
            var token = CancellationToken.None;

            _mockRepo.Setup(r => r.AddUser(user, token))
                .Returns(Task.CompletedTask);

            // Act
            await _userServices.AddUserPostgres(user, token);

            // Assert
            _mockRepo.Verify(r => r.AddUser(user, token), Times.Once);
        }

        [Fact]
        public async Task AddUserPostgres_ShouldPropagateException_WhenRepositoryFails()
        {
            // Arrange
            var user = new User
            {
                Id = new Guid(),
                FirstName = "David",
                LastName = "Perez",
                Email = Email.Create("user@test.com"),
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = new DateTime(1990, 1, 1),
                RoleUser = Role.CrearDesdeTexto("Usuario")
            };
            var token = CancellationToken.None;

            _mockRepo.Setup(r => r.AddUser(user, token))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userServices.AddUserPostgres(user, token));
        }

        [Fact]
        public async Task GetUserPostgres_ShouldCallRepositoryGetUser()
        {
            // Arrange
            var email = "user@gmail.com";
            var token = CancellationToken.None;

            _mockRepo.Setup(r => r.GetUserByEmail(email, token))
                .ReturnsAsync(new User());

            // Act
            await _userServices.GetUserByEmailServices(email, token);

            // Assert
            _mockRepo.Verify(r => r.GetUserByEmail(email, token), Times.Once);
        }

        [Fact]
        public async Task GetUserPostgres_ShouldPropagateException_WhenRepositoryFails()
        {
            // Arrange
            var email = "user@gmail.com";
            var token = CancellationToken.None;

            _mockRepo.Setup(r => r.GetUserByEmail(email, token))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userServices.GetUserByEmailServices(email, token));
        }

        [Fact]
        public async Task GetAllUser_ShouldCallRepositoryGetAllUser()
        {
            // Arrange

             var token = CancellationToken.None;

            _mockRepo.Setup(r => r.GetAllUsersAsync( token))
                .ReturnsAsync(new List<User>());

            // Act
            await _userServices.GetAllUsersServices(token);

            // Assert
            _mockRepo.Verify(r => r.GetAllUsersAsync(token), Times.Once);
        }

        [Fact]
        public async Task GetAllUser_ShouldPropagateException_WhenRepositoryFails()
        {
            // Arrange
            var token = CancellationToken.None;

            _mockRepo.Setup(r => r.GetAllUsersAsync(token))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userServices.GetAllUsersServices(token));
        }

        [Fact]
        public async Task DeleteUserPostgres_ShouldCallRepositoryGetUser()
        {
            // Arrange
            var email = "user@gmail.com";
            var token = CancellationToken.None;

            _mockRepo.Setup(r => r.DeleteUserByEmail(email, token))
                .ReturnsAsync(true);

            // Act
            await _userServices.DeleteUserByEmailServices(email, token);

            // Assert
            _mockRepo.Verify(r => r.DeleteUserByEmail(email, token), Times.Once);
        }

        [Fact]
        public async Task DeleteUserPostgres_ShouldPropagateException_WhenRepositoryFails()
        {
            // Arrange
            var email = "user@gmail.com";
            var token = CancellationToken.None;

            _mockRepo.Setup(r => r.DeleteUserByEmail(email, token))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userServices.DeleteUserByEmailServices(email, token));
        }

        [Fact]
        public async Task UpdateUserServices_ShouldReturnStatusCode_WhenRepositorySucceeds()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                Id = new Guid(),
                FirstName = "David",
                LastName = "Perez",
                Email = Email.Create("user@test.com"),
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = new DateTime(2000, 1, 1),
                RoleUser = Role.CrearDesdeTexto("Usuario")
            };

            _mockRepo.Setup(r => r.UpdateUser(email, user))
                .ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await _userServices.UpdateUserServices(email, user);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
            _mockRepo.Verify(r => r.UpdateUser(email, user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserServices_ShouldPropagateException_WhenRepositoryFails()
        {
            // Arrange
            var email = "fail@example.com";
            var user = new User
            {
                Id = new Guid(),
                FirstName = "David",
                LastName = "Perez",
                Email = Email.Create("user@test.com"),
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = new DateTime(1990, 1, 1),
                RoleUser = Role.CrearDesdeTexto("Usuario")
            };

            _mockRepo.Setup(r => r.UpdateUser(email, user))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userServices.UpdateUserServices(email, user));
        }


    }
}
