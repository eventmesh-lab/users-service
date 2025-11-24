using MicroserviciosUsuarios.Infrastructure.Repositories.Keycloak;
using MicroservicioUsuarios.Infrastructure.ServicesInfrastracture;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.domain.Interfaces;

namespace users_service.infrastructure.Tests.Persistence.Repositories.Keycloak
{
    public class KeycloakRepositoryTests
    {
        private readonly Mock<IKeycloakServiceInfrastructure> _authServiceMock;
        private readonly KeycloakRepository _repository;

        public KeycloakRepositoryTests()
        {
            _authServiceMock = new Mock<IKeycloakServiceInfrastructure>();
            _repository = new KeycloakRepository(_authServiceMock.Object);
        }

        [Fact]
        public async Task RegisterUserAsyncRepo_ShouldReturnTrue_WhenUserCreatedSuccessfully()
        {
            // Arrange
            string email = "test@example.com";
            string name = "John";
            string lastname = "Doe";
            string password = "Secret123";

            _authServiceMock
                .Setup(s => s.CreateUserAsync(email, name, lastname, password))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _repository.RegisterUserAsyncRepo(email, name, lastname, password);

            // Assert
            Assert.True(result);
            _authServiceMock.Verify(s => s.CreateUserAsync(email, name, lastname, password), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsyncRepo_ShouldThrowException_WhenServiceFails()
        {
            // Arrange
            string email = "fail@example.com";
            string name = "Jane";
            string lastname = "Smith";
            string password = "Error123";

            _authServiceMock
                .Setup(s => s.CreateUserAsync(email, name, lastname, password))
                .ThrowsAsync(new InvalidOperationException("Keycloak error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.RegisterUserAsyncRepo(email, name, lastname, password)
            );

            _authServiceMock.Verify(s => s.CreateUserAsync(email, name, lastname, password), Times.Once);
        }

        [Fact]
        public async Task AssignRealmRoleToUserAsyncRepo_ShouldReturnTrue_WhenRoleAssignedSuccessfully()
        {
            // Arrange
            string email = "user@example.com";
            string role = "admin";

            _authServiceMock
                .Setup(s => s.AssignRealmRoleToUserAsync(email, role))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _repository.AssignRealmRoleToUserAsyncRepo(email, role);

            // Assert
            Assert.True(result);
            _authServiceMock.Verify(s => s.AssignRealmRoleToUserAsync(email, role), Times.Once);
        }

        [Fact]
        public async Task AssignRealmRoleToUserAsyncRepo_ShouldThrowException_WhenServiceFails()
        {
            // Arrange
            string email = "user@example.com";
            string role = "editor";

            _authServiceMock
                .Setup(s => s.AssignRealmRoleToUserAsync(email, role))
                .ThrowsAsync(new InvalidOperationException("Keycloak error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.AssignRealmRoleToUserAsyncRepo(email, role)
            );

            _authServiceMock.Verify(s => s.AssignRealmRoleToUserAsync(email, role), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsyncRepo_ShouldCallService_WhenExecutedSuccessfully()
        {
            // Arrange
            string email = "user@example.com";
            string newPassword = "NewSecret123";

            _authServiceMock
                .Setup(s => s.ChangePasswordAsync(email, newPassword))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.ChangePasswordAsyncRepo(email, newPassword);

            // Assert
            _authServiceMock.Verify(s => s.ChangePasswordAsync(email, newPassword), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsyncRepo_ShouldThrowException_WhenServiceFails()
        {
            // Arrange
            string email = "user@example.com";
            string newPassword = "Fail123";

            _authServiceMock
                .Setup(s => s.ChangePasswordAsync(email, newPassword))
                .ThrowsAsync(new InvalidOperationException("Keycloak error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.ChangePasswordAsyncRepo(email, newPassword)
            );

            _authServiceMock.Verify(s => s.ChangePasswordAsync(email, newPassword), Times.Once);
        }

        [Fact]
        public async Task GetUserIdByUsernameAsyncRepo_ShouldReturnUserId_WhenServiceReturnsValidId()
        {
            // Arrange
            string email = "user@example.com";
            string expectedUserId = "12345";

            _authServiceMock
                .Setup(s => s.GetUserIdByUsernameAsync(email))
                .ReturnsAsync(expectedUserId);

            // Act
            var result = await _repository.GetUserIdByUsernameAsyncRepo(email);

            // Assert
            Assert.Equal(expectedUserId, result);
            _authServiceMock.Verify(s => s.GetUserIdByUsernameAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetUserIdByUsernameAsyncRepo_ShouldReturnNull_WhenServiceReturnsNull()
        {
            // Arrange
            string email = "null@example.com";

            _authServiceMock
                .Setup(s => s.GetUserIdByUsernameAsync(email))
                .ReturnsAsync((string)null);

            // Act
            var result = await _repository.GetUserIdByUsernameAsyncRepo(email);

            // Assert
            Assert.Null(result);
            _authServiceMock.Verify(s => s.GetUserIdByUsernameAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetUserIdByUsernameAsyncRepo_ShouldThrowException_WhenServiceFails()
        {
            // Arrange
            string email = "fail@example.com";

            _authServiceMock
                .Setup(s => s.GetUserIdByUsernameAsync(email))
                .ThrowsAsync(new InvalidOperationException("Keycloak error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.GetUserIdByUsernameAsyncRepo(email)
            );

            _authServiceMock.Verify(s => s.GetUserIdByUsernameAsync(email), Times.Once);
        }

        [Fact]
        public async Task UpdateUserInKeycloakAsyncRepo_ShouldReturnTrue_WhenServiceReturnsTrue()
        {
            // Arrange
            string oldEmail = "old@example.com";
            string newName = "John";
            string newLastname = "Doe";

            _authServiceMock
                .Setup(s => s.UpdateUserInKeycloakAsync(oldEmail, newName, newLastname))
                .ReturnsAsync(true);

            // Act
            var result = await _repository.UpdateUserInKeycloakAsyncRepo(oldEmail, newName, newLastname);

            // Assert
            Assert.True(result);
            _authServiceMock.Verify(s => s.UpdateUserInKeycloakAsync(oldEmail, newName, newLastname), Times.Once);
        }

        [Fact]
        public async Task UpdateUserInKeycloakAsyncRepo_ShouldReturnFalse_WhenServiceReturnsFalse()
        {
            // Arrange
            string oldEmail = "old@example.com";
            string newName = "Jane";
            string newLastname = "Smith";

            _authServiceMock
                .Setup(s => s.UpdateUserInKeycloakAsync(oldEmail, newName, newLastname))
                .ReturnsAsync(false);

            // Act
            var result = await _repository.UpdateUserInKeycloakAsyncRepo(oldEmail, newName, newLastname);

            // Assert
            Assert.False(result);
            _authServiceMock.Verify(s => s.UpdateUserInKeycloakAsync(oldEmail, newName, newLastname), Times.Once);
        }

        [Fact]
        public async Task UpdateUserInKeycloakAsyncRepo_ShouldThrowException_WhenServiceFails()
        {
            // Arrange
            string oldEmail = "fail@example.com";
            string newName = "Error";
            string newLastname = "Case";

            _authServiceMock
                .Setup(s => s.UpdateUserInKeycloakAsync(oldEmail, newName, newLastname))
                .ThrowsAsync(new InvalidOperationException("Keycloak error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.UpdateUserInKeycloakAsyncRepo(oldEmail, newName, newLastname)
            );

            _authServiceMock.Verify(s => s.UpdateUserInKeycloakAsync(oldEmail, newName, newLastname), Times.Once);
        }



    }
}
