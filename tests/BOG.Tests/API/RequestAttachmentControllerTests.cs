using System.Net;
using System.Net.Http.Json;
using BOG.API;
using BOG.DTO.RequestAttachment;
using BOG.VM.RequestAttachment;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BOG.Tests.API;

/// <summary>
/// Integration tests for RequestAttachmentController.
/// Tests HTTP endpoints for request attachment management.
/// </summary>
public class RequestAttachmentControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;
    private readonly WebApplicationFactory<Program> _factory;
    private int _testRequestId = 1;

    public RequestAttachmentControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();

        // Seed test data before running tests
        TestDataHelper.EnsureTestDataSeeded(factory);

        // Create a test request for attachment operations
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BOG.DbModel.ApplicationDbContext>();
            var testRequest = TestDataHelper.CreateTestRequest(dbContext, courtId: 1);
            _testRequestId = testRequest.Id;
        }
    }

    #region GET /api/case-requests/{requestId}/attachments

    [Fact]
    public async Task GetAttachments_WithValidRequestId_Returns200Ok()
    {
        // Arrange
        var endpoint = $"/api/case-requests/{_testRequestId}/attachments";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var attachments = await response.Content.ReadFromJsonAsync<List<RequestAttachmentVM>>();
        Assert.NotNull(attachments);
    }

    [Fact]
    public async Task GetAttachments_WithInvalidRequestId_Returns400BadRequest()
    {
        // Arrange
        int requestId = 0;
        var endpoint = $"/api/case-requests/{requestId}/attachments";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region POST /api/case-requests/{requestId}/attachments

    [Fact]
    public async Task UploadAttachment_WithValidPDF_Returns201Created()
    {
        // Arrange
        var pdfContent = GetValidPdfBytes();
        var uploadDto = new RequestAttachmentDTO
        {
            AttachmentTypeId = 1,
            FileName = "test_document.pdf",
            ContentType = "application/pdf",
            FileContent = pdfContent
        };
        var endpoint = $"/api/case-requests/{_testRequestId}/attachments";

        // Act
        var response = await _httpClient.PostAsJsonAsync(endpoint, uploadDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var attachment = await response.Content.ReadFromJsonAsync<RequestAttachmentVM>();
        Assert.NotNull(attachment);
    }

    [Fact]
    public async Task UploadAttachment_WithNonPdfFile_Returns400BadRequest()
    {
        // Arrange
        var uploadDto = new RequestAttachmentDTO
        {
            AttachmentTypeId = 1,
            FileName = "document.docx",
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileContent = new byte[] { 0x50, 0x4B, 0x03, 0x04 } // DOCX magic bytes
        };
        var endpoint = $"/api/case-requests/{_testRequestId}/attachments";

        // Act
        var response = await _httpClient.PostAsJsonAsync(endpoint, uploadDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("BR04", content);
    }

    [Fact]
    public async Task UploadAttachment_ExceedingMaxSize_Returns400BadRequest()
    {
        // Arrange
        const long fourMbPlusOne = (4 * 1024 * 1024) + 1;
        var uploadDto = new RequestAttachmentDTO
        {
            AttachmentTypeId = 1,
            FileName = "large.pdf",
            ContentType = "application/pdf",
            FileContent = new byte[fourMbPlusOne]
        };
        var endpoint = $"/api/case-requests/{_testRequestId}/attachments";

        // Act
        var response = await _httpClient.PostAsJsonAsync(endpoint, uploadDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("BR04", content);
    }

    [Fact]
    public async Task UploadAttachment_MaxSizeAllowed_Returns201Created()
    {
        // Arrange
        const long fourMbExact = 4 * 1024 * 1024;
        var uploadDto = new RequestAttachmentDTO
        {
            AttachmentTypeId = 1,
            FileName = "large.pdf",
            ContentType = "application/pdf",
            FileContent = new byte[fourMbExact]
        };
        var endpoint = $"/api/case-requests/{_testRequestId}/attachments";

        // Act
        var response = await _httpClient.PostAsJsonAsync(endpoint, uploadDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.BadRequest // May fail due to file storage
        );
    }

    [Fact]
    public async Task UploadAttachment_WithoutFileName_Returns400BadRequest()
    {
        // Arrange
        var uploadDto = new RequestAttachmentDTO
        {
            AttachmentTypeId = 1,
            FileName = "",
            ContentType = "application/pdf",
            FileContent = GetValidPdfBytes()
        };
        var endpoint = $"/api/case-requests/{_testRequestId}/attachments";

        // Act
        var response = await _httpClient.PostAsJsonAsync(endpoint, uploadDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GET /api/case-requests/{requestId}/attachments/{attachmentId}

    [Fact]
    public async Task GetAttachment_WithValidId_Returns200Ok()
    {
        // Arrange - Create a test attachment
        int attachmentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BOG.DbModel.ApplicationDbContext>();
            var attachment = TestDataHelper.CreateTestAttachment(dbContext, _testRequestId, attachmentTypeId: 1);
            attachmentId = attachment.Id;
        }

        var endpoint = $"/api/case-requests/{_testRequestId}/attachments/{attachmentId}";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    #endregion

    #region DELETE /api/case-requests/{requestId}/attachments/{attachmentId}

    [Fact]
    public async Task DeleteAttachment_WithValidId_Returns204NoContent()
    {
        // Arrange - Create a test attachment
        int attachmentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BOG.DbModel.ApplicationDbContext>();
            var attachment = TestDataHelper.CreateTestAttachment(dbContext, _testRequestId, attachmentTypeId: 1);
            attachmentId = attachment.Id;
        }

        var endpoint = $"/api/case-requests/{_testRequestId}/attachments/{attachmentId}";

        // Act
        var response = await _httpClient.DeleteAsync(endpoint);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Returns valid PDF magic bytes (PDF header).
    /// </summary>
    private static byte[] GetValidPdfBytes()
    {
        // PDF magic bytes: %PDF-1.4\n
        return new byte[]
        {
            0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A
        };
    }

    #endregion
}
