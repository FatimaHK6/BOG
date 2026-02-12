using BOG.DbModel;
using BOG.DbModel.Entities;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Entities.Lookups;
using BOG.DbModel.Entities.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BOG.Tests.API;

/// <summary>
/// Helper class to seed test data for API integration tests.
/// Ensures Courts, AttachmentTypes, and other lookup data exist before tests run.
/// </summary>
public static class TestDataHelper
{
    private static bool _dataSeeded = false;
    private const int TEST_USER_ID = 9999;  // Use a fixed ID for test user
    private static readonly object _seedLock = new object();

    /// <summary>
    /// Seeds required test data into the database.
    /// Safe to call multiple times (only seeds once).
    ///
    /// Note: EnsureCreated() applies all HasData() seeding from OnModelCreating(),
    /// so we don't need to manually seed RequestStatus, AttachmentType, DefendantType, etc.
    /// </summary>
    public static void EnsureTestDataSeeded<T>(WebApplicationFactory<T> factory) where T : class
    {
        lock (_seedLock)
        {
            if (_dataSeeded)
                return;

            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Ensure database is created with all seed data from HasData()
                dbContext.Database.EnsureCreated();

                // Create test User for CreatedByUserId references (saves internally)
                SeedTestUser(dbContext);

                // Create test Courts if they don't exist (requires RegionId and CityId)
                SeedCourts(dbContext);
                dbContext.SaveChanges();

                _dataSeeded = true;
            }
        }
    }

    /// <summary>
    /// Clears all test data from the database.
    /// Call this between test runs if needed.
    /// </summary>
    public static void ClearTestData<T>(WebApplicationFactory<T> factory) where T : class
    {
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Delete all data in reverse dependency order
            dbContext.RequestAttachments.RemoveRange(dbContext.RequestAttachments);
            dbContext.CaseRequestDefendants.RemoveRange(dbContext.CaseRequestDefendants);
            dbContext.CaseRequestPlaintiffs.RemoveRange(dbContext.CaseRequestPlaintiffs);
            dbContext.CaseRegistrationRequests.RemoveRange(dbContext.CaseRegistrationRequests);
            dbContext.Defendants.RemoveRange(dbContext.Defendants);
            dbContext.Plaintiffs.RemoveRange(dbContext.Plaintiffs);

            dbContext.SaveChanges();
        }

        _dataSeeded = false;
    }

    private static void SeedCourts(ApplicationDbContext dbContext)
    {
        // Check if courts already exist
        if (dbContext.Courts.Count() > 0)
            return;

        // Note: RegionId and CityId are required (foreign keys to seeded Region/City data)
        var courts = new List<Court>
        {
            new()
            {
                NameAr = "المحكمة العامة - الرياض",
                Name = "General Court - Riyadh",
                RegionId = 1,  // Riyadh region (seeded in ApplicationDbContext)
                CityId = 1,    // Riyadh city (seeded in ApplicationDbContext)
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new()
            {
                NameAr = "محكمة الاستئناف - الرياض",
                Name = "Appeal Court - Riyadh",
                RegionId = 1,  // Riyadh region
                CityId = 1,    // Riyadh city
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new()
            {
                NameAr = "المحكمة العامة - جدة",
                Name = "General Court - Jeddah",
                RegionId = 2,  // Makkah region
                CityId = 2,    // Jeddah city
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
        };

        dbContext.Courts.AddRange(courts);
    }

    private static void SeedTestUser(ApplicationDbContext dbContext)
    {
        // Check if test user with fixed ID already exists
        var existingUser = dbContext.Users.FirstOrDefault(u => u.Id == TEST_USER_ID);
        if (existingUser != null)
        {
            return;  // User already exists
        }

        // Use raw SQL to insert user with explicit ID (IDENTITY_INSERT required)
        var now = DateTime.UtcNow;
        dbContext.Database.ExecuteSqlRaw(
            @"SET IDENTITY_INSERT [dbo].[Users] ON;
              INSERT INTO [dbo].[Users] ([Id], [FirstName], [LastName], [Email], [PhoneNumber], [IsActive], [CreatedDate], [ModifiedDate], [IsDeleted])
              VALUES ({0}, 'Test', 'User', 'test@example.com', NULL, 1, {1}, {2}, 0);
              SET IDENTITY_INSERT [dbo].[Users] OFF;",
            TEST_USER_ID, now, now);
    }

    /// <summary>
    /// Creates a test CaseRegistrationRequest with required related data.
    /// </summary>
    public static CaseRegistrationRequest CreateTestRequest(ApplicationDbContext dbContext, int courtId = 1)
    {
        // Get the first available court (should be seeded)
        var court = dbContext.Courts.FirstOrDefault();
        if (court == null)
        {
            throw new InvalidOperationException("No courts found in database. Ensure test data is seeded first.");
        }

        // Verify RequestStatus 1 (Draft) exists - should be seeded
        var draftStatus = dbContext.RequestStatuses.FirstOrDefault(rs => rs.Id == 1);
        if (draftStatus == null)
        {
            throw new InvalidOperationException("Draft status (ID=1) not found in database. Ensure database is initialized.");
        }

        // Use raw SQL to insert request directly (bypasses foreign key validation issues in tests)
        var now = DateTime.UtcNow;
        dbContext.Database.ExecuteSqlRaw(
            @"INSERT INTO [dbo].[CaseRegistrationRequests]
              ([CourtId], [Subject], [Evidence], [Notes], [RequestStatusId], [SubmissionDate], [CompletionDeadline], [RejectionReason], [CaseNumber], [RegistrationNumber], [RegistrationDate], [CreatedByUserId], [LastModifiedByUserId], [CreatedDate], [ModifiedDate], [IsDeleted])
              VALUES ({0}, {1}, {2}, NULL, {3}, NULL, NULL, NULL, NULL, NULL, NULL, {4}, NULL, {5}, {6}, 0)",
            court.Id, "قضية اختبار - اختبار النظام", "وثائق اختبار النظام", 1, TEST_USER_ID, now, now);

        // Query back to get the created request
        var request = dbContext.CaseRegistrationRequests
            .OrderByDescending(r => r.Id)
            .FirstOrDefault();

        return request ?? throw new InvalidOperationException("Failed to create test request.");
    }

    /// <summary>
    /// Creates a test Defendant with required related data.
    /// </summary>
    public static Defendant CreateTestDefendant(ApplicationDbContext dbContext, int requestId)
    {
        // Get DefendantType 1 (Individual) - should be seeded
        var defendantType = dbContext.DefendantTypes.FirstOrDefault(dt => dt.Id == 1);
        if (defendantType == null)
        {
            throw new InvalidOperationException("DefendantType with ID=1 not found. Ensure database is initialized.");
        }

        var defendant = new Defendant
        {
            DefendantTypeId = defendantType.Id,
            FullName = "محمد علي - المدعى عليه",
            IdentityNumber = "1234567890",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        dbContext.Defendants.Add(defendant);
        dbContext.SaveChanges();

        // Create junction record
        var junction = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = requestId,
            DefendantId = defendant.Id,
            CreatedDate = DateTime.UtcNow
        };

        dbContext.CaseRequestDefendants.Add(junction);
        dbContext.SaveChanges();

        return defendant;
    }

    /// <summary>
    /// Creates a test Plaintiff with required related data.
    /// </summary>
    public static Plaintiff CreateTestPlaintiff(ApplicationDbContext dbContext, int requestId, bool isApplicant = true)
    {
        var plaintiff = new Plaintiff
        {
            FirstName = "محمد",
            FatherName = "علي",
            GrandfatherName = "أحمد",
            FamilyName = "الجعفري",
            IdentityNumber = "1111111111",
            MobileNumber = "0501234567",
            Email = "plaintiff@test.com",
            IsApplicant = isApplicant,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        dbContext.Plaintiffs.Add(plaintiff);
        dbContext.SaveChanges();

        // Create junction record
        var junction = new CaseRequestPlaintiff
        {
            CaseRegistrationRequestId = requestId,
            PlaintiffId = plaintiff.Id,
            CreatedDate = DateTime.UtcNow
        };

        dbContext.CaseRequestPlaintiffs.Add(junction);
        dbContext.SaveChanges();

        return plaintiff;
    }

    /// <summary>
    /// Creates a test RequestAttachment.
    /// </summary>
    public static RequestAttachment CreateTestAttachment(ApplicationDbContext dbContext, int requestId, int attachmentTypeId = 1)
    {
        // Get AttachmentType - should be seeded
        var attachmentType = dbContext.AttachmentTypes.FirstOrDefault(at => at.Id == attachmentTypeId);
        if (attachmentType == null)
        {
            throw new InvalidOperationException($"AttachmentType with ID={attachmentTypeId} not found. Ensure database is initialized.");
        }

        var attachment = new RequestAttachment
        {
            CaseRegistrationRequestId = requestId,
            AttachmentTypeId = attachmentType.Id,
            FileName = $"test_document_{Guid.NewGuid().ToString().Substring(0, 8)}.pdf",
            StoredFileName = $"stored_{Guid.NewGuid().ToString()}.pdf",
            FileSizeBytes = 102400,
            ContentType = "application/pdf",
            UploadDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        dbContext.RequestAttachments.Add(attachment);
        dbContext.SaveChanges();

        return attachment;
    }
}
