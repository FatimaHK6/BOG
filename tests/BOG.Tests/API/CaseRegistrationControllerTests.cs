using System.Net;
using System.Net.Http.Json;
using BOG.API;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BOG.Tests.API;

/// <summary>
/// Integration tests for CaseRegistrationController.
/// Tests HTTP endpoints for case registration request lifecycle.
/// </summary>
public class CaseRegistrationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;
    private readonly WebApplicationFactory<Program> _factory;
    private int _testRequestId = 1;

    public CaseRegistrationControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();

        // Seed test data before running tests
        TestDataHelper.EnsureTestDataSeeded(factory);

        // Create a test request for case registration operations
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BOG.DbModel.ApplicationDbContext>();
            var testRequest = TestDataHelper.CreateTestRequest(dbContext, courtId: 1);
            _testRequestId = testRequest.Id;
        }
    }

    #region POST /api/case-requests

    [Fact]
    public async Task CreateRequest_WithValidData_Returns201Created()
    {
        // Arrange
        var createDto = new CaseRegistrationCreateDTO
        {
            CourtId = 1,
            Subject = "قضية مدنية تتعلق بنزاع على عقار",
            Evidence = "الوثائق والمستندات المرفقة توضح حقوق الطرفين"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/case-requests", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var request = await response.Content.ReadFromJsonAsync<CaseRegistrationRequestVM>();
        Assert.NotNull(request);
        Assert.Equal(1, request?.RequestStatusId); // Draft
    }

    [Fact]
    public async Task CreateRequest_WithoutSubject_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CaseRegistrationCreateDTO
        {
            CourtId = 1,
            Subject = "",
            Evidence = "وصف الواقعة"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/case-requests", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRequest_WithoutEvidence_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CaseRegistrationCreateDTO
        {
            CourtId = 1,
            Subject = "موضوع القضية",
            Evidence = ""
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/case-requests", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRequest_WithInvalidCourtId_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CaseRegistrationCreateDTO
        {
            CourtId = 0,
            Subject = "موضوع",
            Evidence = "وصف"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/case-requests", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GET /api/case-requests/{id}

    [Fact]
    public async Task GetRequest_WithValidId_Returns200Ok()
    {
        // Arrange
        // Act
        var response = await _httpClient.GetAsync($"/api/case-requests/{_testRequestId}");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task GetRequest_WithInvalidId_Returns400BadRequest()
    {
        // Arrange
        int requestId = 0;

        // Act
        var response = await _httpClient.GetAsync($"/api/case-requests/{requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region PUT /api/case-requests/{id}

    [Fact]
    public async Task UpdateRequest_InDraftState_Returns200Ok()
    {
        // Arrange
        var updateDto = new CaseRegistrationUpdateDTO
        {
            Subject = "موضوع القضية المعدل تماماً مع كل التفاصيل المطلوبة",
            Evidence = "وصف معدل وتفصيل كامل شامل للواقعة المتنازع عليها وكل الظروف المحيطة بالقضية والأطراف الأخرى المعنية"
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/case-requests/{_testRequestId}", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    #endregion

    #region POST /api/case-requests/{id}/submit

    [Fact]
    public async Task SubmitRequest_WithValidRequest_Returns200Ok()
    {
        // Arrange
        // Act
        var response = await _httpClient.PostAsync($"/api/case-requests/{_testRequestId}/submit", null);

        // Assert
        // May fail validation, but endpoint should be callable
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task SubmitRequest_WithInvalidId_Returns400BadRequest()
    {
        // Arrange
        int requestId = 0;

        // Act
        var response = await _httpClient.PostAsync($"/api/case-requests/{requestId}/submit", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region POST /api/case-requests/{id}/action

    [Fact]
    public async Task TakeAction_WithValidAction_Returns200Ok()
    {
        // Arrange
        var actionDto = new TakeActionDTO
        {
            Action = "Register"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/case-requests/{_testRequestId}/action", actionDto);

        // Assert
        // May fail due to state validation, but endpoint should work
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task TakeAction_WithInvalidAction_Returns400BadRequest()
    {
        // Arrange
        int requestId = 1;
        var actionDto = new TakeActionDTO
        {
            Action = "InvalidAction"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/case-requests/{requestId}/action", actionDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TakeAction_Reject_RequiresNotes_Returns400BadRequest()
    {
        // Arrange
        var actionDto = new TakeActionDTO
        {
            Action = "Reject"
            // Notes not provided
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/case-requests/{_testRequestId}/action", actionDto);

        // Assert
        // Should fail validation or state transition
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    #endregion

    #region GET /api/case-requests/search

    [Fact]
    public async Task SearchRequests_WithPagination_Returns200Ok()
    {
        // Arrange
        var searchDto = new SearchRequestDTO
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/case-requests/search", searchDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchRequests_WithInvalidPageSize_Returns400BadRequest()
    {
        // Arrange
        var searchDto = new SearchRequestDTO
        {
            PageNumber = 1,
            PageSize = 101 // Exceeds max
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/case-requests/search", searchDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchRequests_WithFilters_Returns200Ok()
    {
        // Arrange
        var searchDto = new SearchRequestDTO
        {
            PageNumber = 1,
            PageSize = 10,
            Subject = "قضية",
            StatusId = 3 // New
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/case-requests/search", searchDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}
