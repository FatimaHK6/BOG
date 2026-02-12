using BOG.DAL.Interfaces;
using BOG.DAL.Repositories;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BOG.Tests.DAL;

/// <summary>
/// Unit tests for DefendantRepository following TDD approach.
/// Tests before implementation to drive design.
/// </summary>
public class DefendantRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IDefendantRepository _repository;

    public DefendantRepositoryTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new DefendantRepository(_context);
    }

    #region GetByRequestIdAsync Tests

    [Fact]
    public async Task GetByRequestIdAsync_WithValidRequestId_ReturnsDefendants()
    {
        // Arrange
        var caseRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRegistrationRequests.Add(caseRequest);
        _context.SaveChanges();

        var defendant1 = new Defendant
        {
            DefendantTypeId = 1,
            FullName = "محمد أحمد",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        var defendant2 = new Defendant
        {
            DefendantTypeId = 2,
            FullName = "علي سالم",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        // Explicitly add defendants first
        _context.Defendants.Add(defendant1);
        _context.Defendants.Add(defendant2);
        _context.SaveChanges();

        // Then create junctions
        var junction1 = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = caseRequest.Id,
            DefendantId = defendant1.Id,
            CreatedDate = DateTime.UtcNow
        };
        var junction2 = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = caseRequest.Id,
            DefendantId = defendant2.Id,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRequestDefendants.Add(junction1);
        _context.CaseRequestDefendants.Add(junction2);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetByRequestIdAsync(caseRequest.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, d => d.FullName == "محمد أحمد");
        Assert.Contains(result, d => d.FullName == "علي سالم");
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
    public async Task GetByRequestIdAsync_ExcludesDeletedDefendants()
    {
        // Arrange
        var caseRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.CaseRegistrationRequests.Add(caseRequest);
        _context.SaveChanges();

        var defendant1 = new Defendant
        {
            DefendantTypeId = 1,
            FullName = "محمد أحمد",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        var defendant2 = new Defendant
        {
            DefendantTypeId = 1,
            FullName = "علي سالم",
            IsActive = true,
            IsDeleted = true, // Mark as deleted
            CreatedDate = DateTime.UtcNow
        };

        // Explicitly add defendants first
        _context.Defendants.Add(defendant1);
        _context.Defendants.Add(defendant2);
        _context.SaveChanges();

        // Then create junctions
        var junction1 = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = caseRequest.Id,
            DefendantId = defendant1.Id,
            CreatedDate = DateTime.UtcNow
        };
        var junction2 = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = caseRequest.Id,
            DefendantId = defendant2.Id,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRequestDefendants.Add(junction1);
        _context.CaseRequestDefendants.Add(junction2);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetByRequestIdAsync(caseRequest.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal("محمد أحمد", result.First().FullName);
    }

    #endregion

    #region ExistsByIdentityAsync Tests (ERR013 - Duplicate Check)

    [Fact]
    public async Task ExistsByIdentityAsync_WithDuplicateIdentity_ReturnsTrue()
    {
        // Arrange
        var caseRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.CaseRegistrationRequests.Add(caseRequest);
        _context.SaveChanges();

        var defendant = new Defendant
        {
            DefendantTypeId = 1,
            FullName = "محمد أحمد",
            IdentityNumber = "1234567890",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var junction = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = caseRequest.Id,
            Defendant = defendant,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRequestDefendants.Add(junction);
        _context.SaveChanges();

        // Act
        var result = await _repository.ExistsByIdentityAsync(caseRequest.Id, "1234567890", 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByIdentityAsync_WithNonExistentIdentity_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsByIdentityAsync(1, "9999999999", 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByIdentityAsync_DifferentDefendantType_ReturnsFalse()
    {
        // Arrange
        var caseRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.CaseRegistrationRequests.Add(caseRequest);
        _context.SaveChanges();

        var defendant = new Defendant
        {
            DefendantTypeId = 1,
            FullName = "محمد أحمد",
            IdentityNumber = "1234567890",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var junction = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = caseRequest.Id,
            Defendant = defendant,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRequestDefendants.Add(junction);
        _context.SaveChanges();

        // Act - Same identity but different type
        var result = await _repository.ExistsByIdentityAsync(caseRequest.Id, "1234567890", 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByIdentityAsync_DeletedDefendant_ReturnsFalse()
    {
        // Arrange
        var caseRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.CaseRegistrationRequests.Add(caseRequest);
        _context.SaveChanges();

        var defendant = new Defendant
        {
            DefendantTypeId = 1,
            FullName = "محمد أحمد",
            IdentityNumber = "1234567890",
            IsActive = true,
            IsDeleted = true, // Mark as deleted
            CreatedDate = DateTime.UtcNow
        };

        var junction = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = caseRequest.Id,
            Defendant = defendant,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRequestDefendants.Add(junction);
        _context.SaveChanges();

        // Act
        var result = await _repository.ExistsByIdentityAsync(caseRequest.Id, "1234567890", 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByIdentityAsync_NullIdentityNumber_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsByIdentityAsync(1, null, 1);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetByIdentityAsync Tests

    [Fact]
    public async Task GetByIdentityAsync_WithValidIdentity_ReturnsDefendant()
    {
        // Arrange
        var caseRequest = new CaseRegistrationRequest
        {
            RequestStatusId = 1,
            CourtId = 1,
            CreatedByUserId = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.CaseRegistrationRequests.Add(caseRequest);
        _context.SaveChanges();

        var defendant = new Defendant
        {
            DefendantTypeId = 1,
            FullName = "محمد أحمد",
            IdentityNumber = "1234567890",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var junction = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = caseRequest.Id,
            Defendant = defendant,
            CreatedDate = DateTime.UtcNow
        };

        _context.CaseRequestDefendants.Add(junction);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetByIdentityAsync(caseRequest.Id, "1234567890", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("محمد أحمد", result.FullName);
    }

    [Fact]
    public async Task GetByIdentityAsync_WithInvalidIdentity_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdentityAsync(1, "9999999999", 1);

        // Assert
        Assert.Null(result);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
