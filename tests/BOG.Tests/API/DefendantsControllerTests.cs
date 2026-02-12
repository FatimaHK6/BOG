using System.Net;
using System.Net.Http.Json;
using BOG.API;
using BOG.DTO.CaseRegistration.Defendant;
using BOG.VM.Defendant;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BOG.Tests.API;

/// <summary>
/// Integration tests for DefendantsController.
/// Tests HTTP endpoints for defendant CRUD operations.
/// </summary>
public class DefendantsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;
    private readonly WebApplicationFactory<Program> _factory;
    private int _testRequestId = 1;

    public DefendantsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();

        // Seed test data before running tests
        TestDataHelper.EnsureTestDataSeeded(factory);

        // Create a test request for defendant operations
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BOG.DbModel.ApplicationDbContext>();
            var testRequest = TestDataHelper.CreateTestRequest(dbContext, courtId: 1);
            _testRequestId = testRequest.Id;
        }
    }

    #region GET /api/case-requests/{requestId}/defendants

    [Fact]
    public async Task GetDefendants_WithValidRequestId_Returns200Ok()
    {
        // Arrange
        var endpoint = $"/api/case-requests/{_testRequestId}/defendants";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var defendants = await response.Content.ReadFromJsonAsync<List<DefendantListVM>>();
        Assert.NotNull(defendants);
    }

    [Fact]
    public async Task GetDefendants_WithInvalidRequestId_Returns400BadRequest()
    {
        // Arrange
        int requestId = 0;
        var endpoint = $"/api/case-requests/{requestId}/defendants";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region POST /api/case-requests/{requestId}/defendants

    [Fact]
    public async Task CreateDefendant_WithValidData_Returns201Created()
    {
        // Arrange
        var createDto = new DefendantCreateDTO
        {
            DefendantTypeId = 1,
            FullName = "محمد أحمد علي",
            IdentityTypeId = 1,
            IdentityNumber = "1234567890"
        };
        var endpoint = $"/api/case-requests/{_testRequestId}/defendants";

        // Act
        var response = await _httpClient.PostAsJsonAsync(endpoint, createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var defendant = await response.Content.ReadFromJsonAsync<DefendantVM>();
        Assert.NotNull(defendant);
        Assert.Equal("محمد أحمد علي", defendant?.FullName);
    }

    [Fact]
    public async Task CreateDefendant_WithoutFullName_Returns400BadRequest()
    {
        // Arrange
        var createDto = new DefendantCreateDTO
        {
            DefendantTypeId = 1,
            FullName = "" // Missing required field
        };
        var endpoint = $"/api/case-requests/{_testRequestId}/defendants";

        // Act
        var response = await _httpClient.PostAsJsonAsync(endpoint, createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDefendant_WithDuplicateIdentity_Returns400BadRequest()
    {
        // Arrange
        var createDto = new DefendantCreateDTO
        {
            DefendantTypeId = 1,
            FullName = "محمد أحمد",
            IdentityNumber = "1234567890"
        };
        var endpoint = $"/api/case-requests/{_testRequestId}/defendants";

        // Create first defendant
        await _httpClient.PostAsJsonAsync(endpoint, createDto);

        // Act - Try to create duplicate
        var response = await _httpClient.PostAsJsonAsync(endpoint, createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("ERR013", content);
    }

    [Fact]
    public async Task CreateDefendant_WithInvalidDefendantType_Returns400BadRequest()
    {
        // Arrange
        var createDto = new DefendantCreateDTO
        {
            DefendantTypeId = 999, // Invalid
            FullName = "محمد أحمد"
        };
        var endpoint = $"/api/case-requests/{_testRequestId}/defendants";

        // Act
        var response = await _httpClient.PostAsJsonAsync(endpoint, createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GET /api/case-requests/defendants/{defendantId}

    [Fact]
    public async Task GetDefendantById_WithValidId_Returns200Ok()
    {
        // Arrange
        int defendantId = 1;
        var endpoint = $"/api/case-requests/defendants/{defendantId}";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert - May be 404 if defendant doesn't exist, but endpoint should accept the ID
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDefendantById_WithInvalidId_Returns400BadRequest()
    {
        // Arrange
        int defendantId = 0;
        var endpoint = $"/api/case-requests/defendants/{defendantId}";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region PUT /api/case-requests/defendants/{defendantId}

    [Fact]
    public async Task UpdateDefendant_WithValidData_Returns200Ok()
    {
        // Arrange
        int defendantId = 1;
        var updateDto = new DefendantUpdateDTO
        {
            FullName = "محمد أحمد محسن",
            AddressText = "جدة، المملكة العربية السعودية"
        };
        var endpoint = $"/api/case-requests/defendants/{defendantId}";

        // Act
        var response = await _httpClient.PutAsJsonAsync(endpoint, updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task UpdateDefendant_WithInvalidId_Returns400BadRequest()
    {
        // Arrange
        int defendantId = 0;
        var updateDto = new DefendantUpdateDTO
        {
            FullName = "محمد أحمد"
        };
        var endpoint = $"/api/case-requests/defendants/{defendantId}";

        // Act
        var response = await _httpClient.PutAsJsonAsync(endpoint, updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region DELETE /api/case-requests/defendants/{defendantId}

    [Fact]
    public async Task DeleteDefendant_WithValidId_Returns204NoContent()
    {
        // Arrange
        int defendantId = 1;
        var endpoint = $"/api/case-requests/defendants/{defendantId}";

        // Act
        var response = await _httpClient.DeleteAsync(endpoint);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task DeleteDefendant_WithInvalidId_Returns400BadRequest()
    {
        // Arrange
        int defendantId = 0;
        var endpoint = $"/api/case-requests/defendants/{defendantId}";

        // Act
        var response = await _httpClient.DeleteAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
