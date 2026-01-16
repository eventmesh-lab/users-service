using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using users_service.application.DTOs;
using users_service.infrastructure.ServiceInfrastructure;
using Xunit;

namespace users_service.tests.Infrastructure.Services
{
    public class ActivityServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly ActivityService _service;

        public ActivityServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/")
            };
            _service = new ActivityService(_httpClient);
        }

        [Fact]
        public async Task RegisterActivityAsync_ShouldReturnTrue_WhenResponseIsSuccess()
        {
            var email = "test@user.com";
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = await _service.RegisterActivityAsync(email, "Login", "Security");

            Assert.True(result);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains(email)),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task RegisterActivityAsync_ShouldReturnFalse_WhenResponseIsError()
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var result = await _service.RegisterActivityAsync("test@user.com", "Action", "Category");

            Assert.False(result);
        }

        [Fact]
        public async Task RegisterActivityAsync_ShouldReturnFalse_WhenExceptionOccurs()
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            var result = await _service.RegisterActivityAsync("test@user.com", "Action", "Category");

            Assert.False(result);
        }

        [Fact]
        public async Task RegisterActivityAsync_ShouldSendCorrectJsonBody()
        {
            var email = "user@test.com";
            var action = "UpdatePassword";
            var category = "Security";

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains(email)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            await _service.RegisterActivityAsync(email, action, category);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains(email)),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}