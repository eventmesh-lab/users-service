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
using users_service.application.Queries.Handlers;
using users_service.application.Queries.Queries;
using users_service.domain.Entities;
using users_service.domain.ValueObjects;

namespace users_service.application.Tests.Queries.Handlers
{
    public class GetUserByEmailHandlerTests
    {
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly GetUserByEmailHandler _handler;
        private GetUserEmailQuery query = new GetUserEmailQuery("user@test.com");

        public User user = new User(Guid.NewGuid(), "David", "Perez", Email.Create("user@gmail.com"), 
            "12345678910", "123 Test St", new DateTime(1990, 1, 1),
            Role.CrearDesdeTexto("Usuario"));
        public GetUserByEmailHandlerTests()
        {
            _userServicesMock = new Mock<IUserServices>();
            _handler = new GetUserByEmailHandler(_userServicesMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserNotExists()
        {
            _userServicesMock.Setup(s => s.GetUserByEmailServices(query.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(query, CancellationToken.None));
            Assert.Equal($"El usuario con email user@test.com no existe en la base de datos.", ex.Message);
        }
        [Fact]
        public async Task Handle_GetUserResponseDto_Success()
        {
            _userServicesMock.Setup(s => s.GetUserByEmailServices(query.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var response = await _handler.Handle(query, CancellationToken.None);
            Assert.Equal(response.Email , user.Email.Value);
        }
    }
}
