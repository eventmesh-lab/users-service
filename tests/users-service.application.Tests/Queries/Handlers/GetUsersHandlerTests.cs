using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using users_service.application.Interfaces;
using users_service.application.Queries.Handlers;
using users_service.application.Queries.Queries;
using users_service.domain.Entities;
using users_service.domain.ValueObjects;

namespace users_service.application.Tests.Queries.Handlers
{
    public class GetUsersHandlerTests
    {
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly GetUsersHandler _handler;
        private GetUsersQuery query = new GetUsersQuery();

        public static User user = new User(Guid.NewGuid(), "David", "Perez", Email.Create("user@gmail.com"),
            "12345678910", "123 Test St", new DateTime(1990, 1, 1),
            Role.CrearDesdeTexto("Usuario"));
        public static User user2 = new User(Guid.NewGuid(), "Mauricio", "Marquez", Email.Create("user1@gmail.com"),
            "12345678910", "123 Test St", new DateTime(1990, 1, 1),
            Role.CrearDesdeTexto("Usuario"));

        public List<User> listUser = new List<User> { user, user2 };



        public GetUsersHandlerTests()
        {
            _userServicesMock = new Mock<IUserServices>();
            _handler = new GetUsersHandler(_userServicesMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenNotExists()
        {
            _userServicesMock.Setup(s => s.GetAllUsersServices( It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<User>)null);

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(query, CancellationToken.None));
            Assert.Equal($"No existen usuarios en la base de datos.", ex.Message);
        }
        [Fact]
        public async Task Handle_GetUsersResponseDto_Success()
        {
            _userServicesMock.Setup(s => s.GetAllUsersServices( It.IsAny<CancellationToken>()))
                .ReturnsAsync(listUser);

            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("David", result[0].FirstName);
            Assert.Equal("Mauricio", result[1].FirstName);

        }
    }
}
