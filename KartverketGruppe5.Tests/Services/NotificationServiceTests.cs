using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using Xunit;
using FluentAssertions;
using KartverketGruppe5.Services;

namespace KartverketGruppe5.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly NotificationService _sut;
        private readonly ITempDataDictionary _tempData;

        public NotificationServiceTests()
        {
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _tempDataFactory = Substitute.For<ITempDataDictionaryFactory>();
            _tempData = Substitute.For<ITempDataDictionary>();
            
            var httpContext = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Returns(httpContext);
            _tempDataFactory.GetTempData(httpContext).Returns(_tempData);
            
            _sut = new NotificationService(_httpContextAccessor, _tempDataFactory);
        }

        [Fact]
        public void AddSuccessMessage_ValidMessage_StoresInTempData()
        {
            // Arrange
            var message = "Test success message";

            // Act
            _sut.AddSuccessMessage(message);

            // Assert
            _tempData.Received(1)["Notification"] = Arg.Is<string>(s => 
                s.Contains(message) && s.Contains("bg-green-100"));
        }

        [Fact]
        public void AddErrorMessage_ValidMessage_StoresInTempData()
        {
            // Arrange
            var message = "Test error message";

            // Act
            _sut.AddErrorMessage(message);

            // Assert
            _tempData.Received(1)["Notification"] = Arg.Is<string>(s => 
                s.Contains(message) && s.Contains("bg-red-100"));
        }

        [Fact]
        public void Constructor_NullHttpContext_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new NotificationService(null!, _tempDataFactory);
                // Act & Assert
            var exception = Assert.Throws<NullReferenceException>(() => 
                service.AddSuccessMessage("test"));
            
            exception.Message.Should().Be("Object reference not set to an instance of an object.");
        }
    }
} 