using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BOG.API.Controllers;
using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BOG.API.Tests.Controllers
{
    /// <summary>
    /// Integration tests for DeficienciesController
    /// Tests the full workflow of retrieving and updating deficiencies
    /// </summary>
    public class DeficienciesControllerTests
    {
        private readonly Mock<IDeficiencyBL> _mockDeficiencyBL;
        private readonly Mock<ILogger<DeficienciesController>> _mockLogger;
        private readonly DeficienciesController _controller;

        public DeficienciesControllerTests()
        {
            _mockDeficiencyBL = new Mock<IDeficiencyBL>();
            _mockLogger = new Mock<ILogger<DeficienciesController>>();
            _controller = new DeficienciesController(_mockDeficiencyBL.Object, _mockLogger.Object);
        }

        #region GetDeficiencies Tests

        [Fact]
        public async Task GetDeficiencies_WithValidRequestId_ReturnsOkWithDeficiencies()
        {
            // Arrange
            var requestId = 1;
            var deficiencies = new List<DeficiencyVM>
            {
                new DeficiencyVM
                {
                    Id = 1,
                    CaseRegistrationRequestId = requestId,
                    DeficiencyDescriptionId = 1,
                    DeficiencyTypeId = 1,
                    DeficiencyTypeName = "Documents",
                    DeficiencyTypeNameAr = "المستندات",
                    DescriptionAr = "نقص في المستندات",
                    DescriptionEn = "Missing documents",
                    DisplayOrder = 0,
                    CreatedDate = System.DateTime.UtcNow,
                    ModifiedDate = System.DateTime.UtcNow
                }
            };

            _mockDeficiencyBL
                .Setup(x => x.GetDeficienciesAsync(requestId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deficiencies);

            // Act
            var result = await _controller.GetDeficiencies(requestId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _mockDeficiencyBL.Verify(
                x => x.GetDeficienciesAsync(requestId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetDeficiencies_WithNonExistentRequest_ReturnsOkWithEmptyList()
        {
            // Arrange
            var requestId = 9999;
            _mockDeficiencyBL
                .Setup(x => x.GetDeficienciesAsync(requestId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DeficiencyVM>());

            // Act
            var result = await _controller.GetDeficiencies(requestId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<IEnumerable<DeficiencyVM>>(okResult.Value);
            Assert.Empty(returnedList);
        }

        [Fact]
        public async Task GetDeficiencies_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var requestId = 1;
            var exceptionMessage = "Database connection failed";
            _mockDeficiencyBL
                .Setup(x => x.GetDeficienciesAsync(requestId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetDeficiencies(requestId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region UpdateDeficiencies Tests

        [Fact]
        public async Task UpdateDeficiencies_WithValidPayload_ReturnsOkWithUpdatedDeficiencies()
        {
            // Arrange
            var requestId = 1;
            var dto = new DeficienciesBatchUpdateDTO
            {
                Deficiencies = new List<RequestDeficiencyDTO>
                {
                    new RequestDeficiencyDTO { DeficiencyDescriptionId = 1 },
                    new RequestDeficiencyDTO { DeficiencyDescriptionId = 3 }
                }
            };

            var updatedDeficiencies = new List<DeficiencyVM>
            {
                new DeficiencyVM
                {
                    Id = 1,
                    CaseRegistrationRequestId = requestId,
                    DeficiencyDescriptionId = 1,
                    DeficiencyTypeId = 1,
                    DeficiencyTypeName = "Documents",
                    DeficiencyTypeNameAr = "المستندات",
                    DescriptionAr = "نقص في المستندات",
                    DisplayOrder = 0,
                    CreatedDate = System.DateTime.UtcNow,
                    ModifiedDate = System.DateTime.UtcNow
                },
                new DeficiencyVM
                {
                    Id = 2,
                    CaseRegistrationRequestId = requestId,
                    DeficiencyDescriptionId = 3,
                    DeficiencyTypeId = 1,
                    DeficiencyTypeName = "Documents",
                    DeficiencyTypeNameAr = "المستندات",
                    DescriptionAr = "نقص آخر",
                    DisplayOrder = 1,
                    CreatedDate = System.DateTime.UtcNow,
                    ModifiedDate = System.DateTime.UtcNow
                }
            };

            _mockDeficiencyBL
                .Setup(x => x.UpdateDeficienciesAsync(requestId, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedDeficiencies);

            // Act
            var result = await _controller.UpdateDeficiencies(requestId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _mockDeficiencyBL.Verify(
                x => x.UpdateDeficienciesAsync(requestId, dto, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateDeficiencies_WithEmptyDeficiencies_ReturnsOkWithEmptyList()
        {
            // Arrange
            var requestId = 1;
            var dto = new DeficienciesBatchUpdateDTO
            {
                Deficiencies = new List<RequestDeficiencyDTO>()
            };

            _mockDeficiencyBL
                .Setup(x => x.UpdateDeficienciesAsync(requestId, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DeficiencyVM>());

            // Act
            var result = await _controller.UpdateDeficiencies(requestId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<IEnumerable<DeficiencyVM>>(okResult.Value);
            Assert.Empty(returnedList);
        }

        [Fact]
        public async Task UpdateDeficiencies_WithNonExistentRequest_ReturnsNotFound()
        {
            // Arrange
            var requestId = 9999;
            var dto = new DeficienciesBatchUpdateDTO
            {
                Deficiencies = new List<RequestDeficiencyDTO>
                {
                    new RequestDeficiencyDTO { DeficiencyDescriptionId = 1 }
                }
            };

            _mockDeficiencyBL
                .Setup(x => x.UpdateDeficienciesAsync(requestId, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException($"Case registration request {requestId} not found"));

            // Act
            var result = await _controller.UpdateDeficiencies(requestId, dto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateDeficiencies_WithInvalidStatus_ReturnsBadRequest()
        {
            // Arrange
            var requestId = 1;
            var dto = new DeficienciesBatchUpdateDTO
            {
                Deficiencies = new List<RequestDeficiencyDTO>
                {
                    new RequestDeficiencyDTO { DeficiencyDescriptionId = 1 }
                }
            };

            _mockDeficiencyBL
                .Setup(x => x.UpdateDeficienciesAsync(requestId, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Request cannot be edited in current status"));

            // Act
            var result = await _controller.UpdateDeficiencies(requestId, dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateDeficiencies_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var requestId = 1;
            var dto = new DeficienciesBatchUpdateDTO
            {
                Deficiencies = new List<RequestDeficiencyDTO>
                {
                    new RequestDeficiencyDTO { DeficiencyDescriptionId = 1 }
                }
            };

            _mockDeficiencyBL
                .Setup(x => x.UpdateDeficienciesAsync(requestId, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.UpdateDeficiencies(requestId, dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region Display Order Tests

        [Fact]
        public async Task UpdateDeficiencies_PreservesDisplayOrder()
        {
            // Arrange
            var requestId = 1;
            var dto = new DeficienciesBatchUpdateDTO
            {
                Deficiencies = new List<RequestDeficiencyDTO>
                {
                    new RequestDeficiencyDTO { DeficiencyDescriptionId = 1 },
                    new RequestDeficiencyDTO { DeficiencyDescriptionId = 2 },
                    new RequestDeficiencyDTO { DeficiencyDescriptionId = 3 }
                }
            };

            var updatedDeficiencies = new List<DeficiencyVM>
            {
                new DeficiencyVM { DisplayOrder = 0 },
                new DeficiencyVM { DisplayOrder = 1 },
                new DeficiencyVM { DisplayOrder = 2 }
            };

            _mockDeficiencyBL
                .Setup(x => x.UpdateDeficienciesAsync(requestId, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedDeficiencies);

            // Act
            var result = await _controller.UpdateDeficiencies(requestId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<IEnumerable<DeficiencyVM>>(okResult.Value);
            var items = new List<DeficiencyVM>(returnedList);

            Assert.Equal(0, items[0].DisplayOrder);
            Assert.Equal(1, items[1].DisplayOrder);
            Assert.Equal(2, items[2].DisplayOrder);
        }

        #endregion

        #region Data Integrity Tests

        [Fact]
        public async Task UpdateDeficiencies_IncludesAllRequiredFields()
        {
            // Arrange
            var requestId = 1;
            var dto = new DeficienciesBatchUpdateDTO
            {
                Deficiencies = new List<RequestDeficiencyDTO>
                {
                    new RequestDeficiencyDTO { DeficiencyDescriptionId = 1 }
                }
            };

            var expectedDeficiency = new DeficiencyVM
            {
                Id = 1,
                CaseRegistrationRequestId = requestId,
                DeficiencyDescriptionId = 1,
                DeficiencyTypeId = 1,
                DeficiencyTypeName = "Documents",
                DeficiencyTypeNameAr = "المستندات",
                DescriptionAr = "نقص",
                DescriptionEn = "Deficiency",
                DisplayOrder = 0,
                CreatedDate = System.DateTime.UtcNow,
                ModifiedDate = System.DateTime.UtcNow
            };

            _mockDeficiencyBL
                .Setup(x => x.UpdateDeficienciesAsync(requestId, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DeficiencyVM> { expectedDeficiency });

            // Act
            var result = await _controller.UpdateDeficiencies(requestId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<IEnumerable<DeficiencyVM>>(okResult.Value);
            var item = Assert.Single(returnedList);

            Assert.Equal(1, item.Id);
            Assert.Equal(requestId, item.CaseRegistrationRequestId);
            Assert.Equal(1, item.DeficiencyDescriptionId);
            Assert.NotNull(item.DeficiencyTypeName);
            Assert.NotNull(item.DescriptionAr);
        }

        #endregion
    }
}
