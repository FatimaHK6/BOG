using BOG.DAL.Interfaces;
using BOG.DAL.Repositories;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BOG.Tests.DAL;

/// <summary>
/// Unit tests for CaseRegistrationRequestRepository following TDD approach.
/// </summary>
public class CaseRegistrationRequestRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ICaseRegistrationRequestRepository _repository;

    public CaseRegistrationRequestRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CaseRegistrationRequestRepository(_context);
    }

    #region GetWithDetailsAsync Tests

    [Fact]
    public async Task GetWithDetailsAsync_WithValidRequestId_ReturnsRequestWithNavigationProperties()
    {
        // Arrange
        var request = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            Subject = "Test Case",
            Evidence = "Test Evidence",
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(request);
        _context.SaveChanges();

        // Reload to ensure ID is set
        var requestId = request.Id;

        // Act
        var result = await _repository.GetWithDetailsAsync(requestId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Case", result.Subject);
        Assert.Equal("Test Evidence", result.Evidence);
        Assert.NotNull(result.CaseRequestPlaintiffs);
        Assert.NotNull(result.CaseRequestDefendants);
    }

    [Fact]
    public async Task GetWithDetailsAsync_WithInvalidRequestId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetWithDetailsAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWithDetailsAsync_ExcludesDeletedRequests()
    {
        // Arrange
        var request = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            Subject = "Test Case",
            IsDeleted = true,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(request);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetWithDetailsAsync(request.Id);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetPendingCompletionExpiredAsync Tests (BR05 - Auto-Rejection)

    [Fact]
    public async Task GetPendingCompletionExpiredAsync_WithExpiredDeadline_ReturnsRequests()
    {
        // Arrange
        var expiredRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 6, // PendingCompletion
            CourtId = 1,
            Subject = "Expired Case",
            ModifiedDate = DateTime.UtcNow.AddDays(-31), // Expired (past 30-day deadline)
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        var validRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 6, // PendingCompletion
            CourtId = 1,
            Subject = "Valid Case",
            ModifiedDate = DateTime.UtcNow.AddDays(-29), // Not yet expired (within 30 days)
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(expiredRequest);
        _context.CaseRegistrationRequests.Add(validRequest);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetPendingCompletionExpiredAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Expired Case", result.First().Subject);
    }

    [Fact]
    public async Task GetPendingCompletionExpiredAsync_NoExpiredRequests_ReturnsEmpty()
    {
        // Arrange
        var validRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 6, // PendingCompletion
            CourtId = 1,
            Subject = "Valid Case",
            ModifiedDate = DateTime.UtcNow.AddDays(-15), // Within 30-day deadline
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(validRequest);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetPendingCompletionExpiredAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPendingCompletionExpiredAsync_FiltersByPendingCompletionStatus()
    {
        // Arrange
        var registeredRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 4, // Registered (not PendingCompletion)
            CourtId = 1,
            Subject = "Registered Case",
            ModifiedDate = DateTime.UtcNow.AddDays(-31), // Expired, but wrong status
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(registeredRequest);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetPendingCompletionExpiredAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // Should not include non-PendingCompletion requests
    }

    [Fact]
    public async Task GetPendingCompletionExpiredAsync_ExcludesDeletedRequests()
    {
        // Arrange
        var deletedRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 6, // PendingCompletion
            CourtId = 1,
            Subject = "Deleted Case",
            ModifiedDate = DateTime.UtcNow.AddDays(-31), // Expired, but deleted
            IsDeleted = true,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(deletedRequest);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetPendingCompletionExpiredAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // Should exclude deleted requests
    }

    #endregion

    #region GetByStatusAsync Tests

    [Fact]
    public async Task GetByStatusAsync_WithValidStatus_ReturnsRequests()
    {
        // Arrange
        var request1 = new CaseRegistrationRequest
        {
            RequestStatusId = 1, // Draft
            CourtId = 1,
            Subject = "Draft 1",
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow.AddSeconds(-1)
        };
        var request2 = new CaseRegistrationRequest
        {
            RequestStatusId = 1, // Draft
            CourtId = 1,
            Subject = "Draft 2",
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(request1);
        _context.CaseRegistrationRequests.Add(request2);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetByStatusAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByStatusAsync_ExcludesDeletedRequests()
    {
        // Arrange
        var activeRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            Subject = "Active",
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };
        var deletedRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            Subject = "Deleted",
            IsDeleted = true,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow.AddSeconds(-1)
        };

        _context.CaseRegistrationRequests.Add(activeRequest);
        _context.CaseRegistrationRequests.Add(deletedRequest);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetByStatusAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Active", result.First().Subject);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
