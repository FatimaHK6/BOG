using BOG.DAL.Interfaces;
using BOG.DAL.Repositories;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BOG.Tests.DAL;

/// <summary>
/// Unit tests for RequestAttachmentRepository following TDD approach.
/// </summary>
public class RequestAttachmentRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IRequestAttachmentRepository _repository;

    public RequestAttachmentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new RequestAttachmentRepository(_context);
    }

    #region GetByRequestIdAsync Tests

    [Fact]
    public async Task GetByRequestIdAsync_WithValidRequestId_ReturnsAttachments()
    {
        // Arrange
        var request = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(request);
        _context.SaveChanges();

        var attachment1 = new RequestAttachment
        {
            CaseRegistrationRequestId = request.Id,
            AttachmentTypeId = 1,
            FileName = "document1.pdf",
            StoredFileName = "stored1.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow
        };

        var attachment2 = new RequestAttachment
        {
            CaseRegistrationRequestId = request.Id,
            AttachmentTypeId = 2,
            FileName = "document2.pdf",
            StoredFileName = "stored2.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 2048,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow
        };

        _context.RequestAttachments.Add(attachment1);
        _context.RequestAttachments.Add(attachment2);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetByRequestIdAsync(request.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, a => a.FileName == "document1.pdf");
        Assert.Contains(result, a => a.FileName == "document2.pdf");
    }

    [Fact]
    public async Task GetByRequestIdAsync_WithInvalidRequestId_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByRequestIdAsync(999);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByRequestIdAsync_ExcludesDeletedAttachments()
    {
        // Arrange
        var request = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(request);
        _context.SaveChanges();

        var activeAttachment = new RequestAttachment
        {
            CaseRegistrationRequestId = request.Id,
            AttachmentTypeId = 1,
            FileName = "active.pdf",
            StoredFileName = "active.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow
        };

        var deletedAttachment = new RequestAttachment
        {
            CaseRegistrationRequestId = request.Id,
            AttachmentTypeId = 1,
            FileName = "deleted.pdf",
            StoredFileName = "deleted.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024,
            IsActive = true,
            IsDeleted = true, // Soft deleted
            CreatedDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow
        };

        _context.RequestAttachments.Add(activeAttachment);
        _context.RequestAttachments.Add(deletedAttachment);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetByRequestIdAsync(request.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal("active.pdf", result.First().FileName);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
