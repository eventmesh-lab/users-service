using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using users_service.domain.Entities;
using users_service.domain.ValueObjects;
using users_service.infrastructure.Persistence.Context;
using users_service.infrastructure.Persistence.Models;
using users_service.infrastructure.Persistence.Repositories;


namespace users_service.infrastructure.Tests.Persistence.Repositories
{
    public class UserRepositoryPostgresTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // DB única por test
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddUser_ShouldAddUserToDatabase()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = new UserRepositoyPostgres(context);

            var user = new User
            {
                FirstName = "David",
                LastName = "Perez",
                Email = Email.Create("user@test.com"),
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = new DateTime(2000, 1, 1),
                RoleUser = Role.CrearDesdeTexto("Usuario")
            };
            var token = CancellationToken.None;

            // Act
            await repo.AddUser(user, token);

            // Assert
            var savedUser = await context.Users.FirstOrDefaultAsync();
            Assert.NotNull(savedUser);
            Assert.Equal(user.FirstName, savedUser.FirstName);
        }

        [Fact]
        public async Task AddUser_ShouldPropagateException_WhenSaveChangesFails()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            var repo = new UserRepositoyPostgres(context);

            var user = new User
            {
                FirstName = "David",
                LastName = "Perez",
                Email = Email.Create("user@test.com"),
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = new DateTime(1990, 1, 1),
                RoleUser = Role.CrearDesdeTexto("Usuario")
            };
            var token = new CancellationToken(true); // token cancelado

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(
                () => repo.AddUser(user, token));
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnMappedUsers_WhenUsersExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.Users.Add(new UserPostgres()            {
                Id = Guid.NewGuid(),
                FirstName = "David",
                LastName = "Perez",
                Email = "user@test.com",
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = "2000, 1, 1",
                RoleUser = "Usuario"
            });
            await context.SaveChangesAsync();

            var repo = new UserRepositoyPostgres(context);
            var token = CancellationToken.None;

            // Act
            var result = await repo.GetAllUsersAsync(token);

            // Assert
            Assert.Single(result);
            var user = result[0];
            Assert.Equal("David", user.FirstName);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = new UserRepositoyPostgres(context);
            var token = CancellationToken.None;

            // Act
            var result = await repo.GetAllUsersAsync(token);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldThrowTaskCanceledException_WhenTokenIsCancelled()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = new UserRepositoyPostgres(context);
            var token = new CancellationToken(true); // cancelado

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => repo.GetAllUsersAsync(token));
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnMappedUser_WhenUserExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.Users.Add(new UserPostgres()
            {
                Id = Guid.NewGuid(),
                FirstName = "David",
                LastName = "Perez",
                Email = "user@test.com",
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = "2000, 1, 1",
                RoleUser = "Usuario"
            });
            await context.SaveChangesAsync();

            var repo = new UserRepositoyPostgres(context);
            var token = CancellationToken.None;

            // Act
            var result = await repo.GetUserByEmail("user@test.com", token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("David", result.FirstName);
            Assert.Equal("Perez", result.LastName);
            Assert.Equal("user@test.com", result.Email.Value); 
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = new UserRepositoyPostgres(context);
            var token = CancellationToken.None;

            // Act
            var result = await repo.GetUserByEmail("users@test.com", token);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldThrowOperationCanceledException_WhenTokenIsCancelled()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = new UserRepositoyPostgres(context);
            var token = new CancellationToken(true); // cancelado

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => repo.GetUserByEmail("user@test.com", token));
        }

        [Fact]
        public async Task DeleteUserByEmail_ShouldReturnTrue_WhenUserIsDeleted()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.Users.Add(new UserPostgres()
            {
                Id = Guid.NewGuid(),
                FirstName = "David",
                LastName = "Perez",
                Email = "user@test.com",
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = "2000, 1, 1",
                RoleUser = "Usuario"
            });
            await context.SaveChangesAsync();

            var repo = new UserRepositoyPostgres(context);
            var token = CancellationToken.None;

            // Act
            var result = await repo.DeleteUserByEmail("user@test.com", token);

            // Assert
            Assert.True(result);
            Assert.Empty(context.Users); 
        }

        [Fact]
        public async Task DeleteUserByEmail_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = new UserRepositoyPostgres(context);
            var token = CancellationToken.None;

            // Act
            var result = await repo.DeleteUserByEmail("user@test.com", token);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUserByEmail_ShouldReturnFalse_WhenSaveChangesThrowsException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Users.Add(new UserPostgres()
            {
                Id = Guid.NewGuid(),
                FirstName = "David",
                LastName = "Perez",
                Email = "user@test.com",
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = "2000, 1, 1",
                RoleUser = "Usuario"
            });
            await context.SaveChangesAsync();

            // Creamos un repo pero cancelamos el token para forzar excepción
            var repo = new UserRepositoyPostgres(context);
            var token = new CancellationToken(true); 

            // Act
            var result = await repo.DeleteUserByEmail("error@test.com", token);

            // Assert
            Assert.False(result);
        }
        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenUserIsUpdated()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var existingUser = new UserPostgres
            {
                Id = Guid.NewGuid(),
                FirstName = "David",
                LastName = "Perez",
                Email = "user@test.com",
                PhoneNumber = "12345678910",
                Address = "Caracas",
                Birthdate = "2000-01-01",
                RoleUser = "Usuario"
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var repo = new UserRepositoyPostgres(context);
            var newUser = new User
            {
                Id = existingUser.Id,
                FirstName = "Carlos",
                LastName = "Lopez",
                Email = Email.Create("user@test.com"),
                PhoneNumber = "9876543210",
                Address = "Valencia",
                Birthdate = new DateTime(1995, 5, 5),
                RoleUser = Role.CrearDesdeTexto("Usuario")
            };

            // Act
            var result = await repo.UpdateUser("user@test.com", newUser);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);

            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "user@test.com");
            Assert.Equal("Carlos", updatedUser.FirstName);
            Assert.Equal("Lopez", updatedUser.LastName);
            Assert.Equal("9876543210", updatedUser.PhoneNumber);
            Assert.Equal("Valencia", updatedUser.Address);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = new UserRepositoyPostgres(context);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Carlos",
                LastName = "Lopez",
                Email = Email.Create("notfound@test.com"),
                PhoneNumber = "9876543210",
                Address = "Valencia",
                Birthdate = new DateTime(1995, 5, 5),
                RoleUser = Role.CrearDesdeTexto("Usuario")
            };

            // Act
            var result = await repo.UpdateUser("notfound@test.com", newUser);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result);
        }
    }
}
