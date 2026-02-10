# Developer-B: Defendants & Request Management Module - Implementation Plan

## Overview

Implementation of **UC 6.5.1.1.11 - 6.5.1.1.24** (14 use cases) for the BOG Legal Case Management System following **phased-by-layer approach with Test-Driven Development (TDD)** and **mock integration services**.

**Scope**: Defendant management, case registration request workflow, request actions (register/reject/completion), search functionality, attachments, and state management.

## Current State Assessment

| Layer | Status | Details |
|-------|--------|---------|
| **BOG.DbModel** | ✅ 100% | All 11 entities, 10 lookups, migrations applied |
| **BOG.DAL** | ❌ 0% | No repositories for case registration |
| **BOG.BL** | ⚠️ 5% | Only Permissions constants |
| **BOG.DTO** | ❌ 0% | No DTOs for case registration |
| **BOG.VM** | ❌ 0% | No VMs for case registration |
| **BOG.API** | ❌ 0% | No controllers for case registration |
| **BOG.Integration** | ❌ 0% | Empty project |

**Database entities already created:**
- CaseRegistrationRequest, Plaintiff, Defendant, Representative, Claim, RelatedCase, RequestClassification, RequestAttachment, PlaintiffAttachment, CaseRequestPlaintiff, CaseRequestDefendant

## Implementation Strategy

**Approach**: Phased by Layer (Complete DAL → Integration → BL → DTO/VM → API)
**Testing**: Test-Driven Development (write tests first, then implementation)
**Integrations**: Mock implementations only (switch to production via config later)
**Timeline**: 7 weeks (35 working days)

---

## Phase 1: Data Access Layer (DAL) - Week 1 (5 days)

### Objective
Create all repository interfaces and implementations for case registration entities, following TDD approach.

### 1.1 Repository Interfaces (Day 1)

**Location**: `src/Backend/BOG.DAL/Interfaces/`

Create 5 repository interfaces inheriting from `IRepository<T>`:

1. **IDefendantRepository.cs**
   ```csharp
   Task<IEnumerable<Defendant>> GetByRequestIdAsync(int requestId);
   Task<Defendant?> GetByIdentityAsync(int requestId, string identityNumber, int defendantTypeId);
   Task<bool> ExistsByIdentityAsync(int requestId, string identityNumber, int defendantTypeId);
   ```

2. **ICaseRegistrationRequestRepository.cs**
   ```csharp
   Task<CaseRegistrationRequest?> GetWithDetailsAsync(int requestId);
   Task<IEnumerable<CaseRegistrationRequest>> GetByStatusAsync(int statusId);
   Task<IEnumerable<CaseRegistrationRequest>> GetPendingCompletionExpiredAsync();
   ```

3. **IRequestAttachmentRepository.cs**
   ```csharp
   Task<IEnumerable<RequestAttachment>> GetByRequestIdAsync(int requestId);
   Task<IEnumerable<RequestAttachment>> GetMandatoryByRequestIdAsync(int requestId);
   ```

4. **IRequestDeficiencyRepository.cs**
   ```csharp
   Task<IEnumerable<RequestDeficiency>> GetByRequestIdAsync(int requestId);
   ```

5. **IAdditionalInfoRepository.cs**
   ```csharp
   Task<AdditionalInfo?> GetByRequestIdAsync(int requestId);
   ```

**Pattern to Follow**: Study `src/Backend/BOG.DAL/Interfaces/IUserRepository.cs`

### 1.2 TDD: Write Repository Tests First (Day 2)

**Location**: `tests/BOG.Tests/DAL/`

Create test files:
- `DefendantRepositoryTests.cs`
- `CaseRegistrationRequestRepositoryTests.cs`
- `RequestAttachmentRepositoryTests.cs`

**Key tests to write:**
- GetByRequestIdAsync returns correct defendants
- ExistsByIdentityAsync detects duplicates (ERR013)
- GetWithDetailsAsync includes navigation properties
- GetPendingCompletionExpiredAsync filters by deadline
- Soft delete filter works correctly (IsDeleted = true excluded)

**Test Setup**:
```csharp
private ApplicationDbContext CreateInMemoryContext()
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options);
}
```

### 1.3 Repository Implementations (Days 3-4)

**Location**: `src/Backend/BOG.DAL/Repositories/`

Implement repositories following pattern in `src/Backend/BOG.DAL/Repositories/UserRepository.cs`:

**Key patterns to follow:**
- Inherit from `Repository<TEntity>`
- Use `AsNoTracking()` for read-only queries
- Always filter `!IsDeleted` in custom queries
- Use `.Include()` for navigation properties
- Validate parameters (null checks)
- Include `CancellationToken` in all methods

**DefendantRepository.cs example:**
```csharp
public class DefendantRepository : Repository<Defendant>, IDefendantRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public DefendantRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<Defendant>> GetByRequestIdAsync(int requestId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(d => d.DefendantType)
            .Include(d => d.IdentityType)
            .Where(d => d.RequestId == requestId && !d.IsDeleted)
            .OrderBy(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsByIdentityAsync(int requestId, string identityNumber, int defendantTypeId)
    {
        if (string.IsNullOrWhiteSpace(identityNumber))
            return false;

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(d => d.RequestId == requestId
                && d.IdentityNumber == identityNumber
                && d.DefendantTypeId == defendantTypeId
                && !d.IsDeleted);
    }
}
```

### 1.4 Verify Tests Pass (Day 4)

Run all DAL tests:
```bash
dotnet test --filter "Category=DAL"
```

All tests should pass green before moving to Phase 2.

### 1.5 Register Repositories in DI (Day 5)

**Location**: `src/Backend/BOG.API/Extensions/ServiceCollectionExtensions.cs`

Add to `AddRepositories()` method:
```csharp
// Case Registration repositories
services.AddScoped<IDefendantRepository, DefendantRepository>();
services.AddScoped<ICaseRegistrationRequestRepository, CaseRegistrationRequestRepository>();
services.AddScoped<IRequestAttachmentRepository, RequestAttachmentRepository>();
services.AddScoped<IRequestDeficiencyRepository, RequestDeficiencyRepository>();
services.AddScoped<IAdditionalInfoRepository, AdditionalInfoRepository>();
```

**Verification**: Build solution successfully
```bash
dotnet build src/Backend/BOG.sln
```

---

## Phase 2: Integration Services Layer - Week 2 (5 days)

### Objective
Create mock integration services for SMS, Email, and Case Management with configuration support.

### 2.1 Project Structure Setup (Day 1)

**Location**: `src/Backend/BOG.Integration/`

Create folder structure:
```
BOG.Integration/
├── Interfaces/
├── Services/Sms/
├── Services/Email/
├── Services/CaseManagement/
├── DTOs/Sms/
├── DTOs/Email/
├── DTOs/CaseManagement/
├── Enums/
├── Configuration/
├── Exceptions/
└── Extensions/
```

### 2.2 Core Interfaces & DTOs (Day 2)

Create interfaces:
1. **ISmsService.cs**
   ```csharp
   Task<SmsResult> SendSmsAsync(string mobileNumber, string message);
   Task<SmsResult> SendTemplatedSmsAsync(string mobileNumber, SmsTemplate template, SmsTemplateParameters parameters);
   ```

2. **IEmailService.cs**
   ```csharp
   Task<EmailResult> SendEmailAsync(string toEmail, string subject, string body);
   Task<EmailResult> SendTemplatedEmailAsync(string toEmail, EmailTemplate template, EmailTemplateParameters parameters);
   ```

3. **ICaseManagementService.cs**
   ```csharp
   Task<CaseRegistrationResult> RegisterCaseAsync(CaseRegistrationData registrationData);
   Task<CaseStatusResult> GetCaseStatusAsync(string caseNumber);
   ```

Create DTOs for results and parameters (SmsResult, EmailResult, CaseRegistrationData, etc.)

### 2.3 Enums & Configuration (Day 2)

**SmsTemplate.cs enum:**
```csharp
public enum SmsTemplate
{
    REQUEST_SUBMITTED = 1,
    REQUEST_REGISTERED = 2,
    REQUEST_REJECTED = 3,
    COMPLETION_REQUIRED = 4,
    DEFICIENCY_REMINDER = 5,
    AUTO_REJECTED = 6
}
```

**Configuration classes** (SmsSettings, EmailSettings, CaseManagementSettings) with `UseMock` flag.

### 2.4 TDD: Write Integration Tests (Day 3)

**Location**: `tests/BOG.Tests/Integration/`

Test files:
- `MockSmsServiceTests.cs`
- `MockEmailServiceTests.cs`
- `MockCaseManagementServiceTests.cs`

**Key tests:**
- SendSmsAsync validates Saudi mobile format (05XXXXXXXX)
- SendTemplatedSmsAsync builds correct message from template
- RegisterCaseAsync validates required fields (plaintiffs, defendants, subject)
- Mock services log messages correctly
- Configuration UseMock flag switches implementations

### 2.5 Mock Service Implementations (Day 4)

**MockSmsService.cs:**
```csharp
public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public async Task<SmsResult> SendSmsAsync(string mobileNumber, string message)
    {
        // Validate Saudi mobile format
        if (!IsValidSaudiMobileNumber(mobileNumber))
            return new SmsResult { IsSuccess = false, ErrorMessage = "Invalid format" };

        // Simulate delay
        await Task.Delay(100);

        var messageId = $"MOCK-SMS-{Guid.NewGuid().ToString().Substring(0, 8)}";

        _logger.LogInformation("[MOCK SMS] To: {Mobile}, MessageId: {MessageId}, Message: {Message}",
            mobileNumber, messageId, message);

        return new SmsResult { IsSuccess = true, MessageId = messageId, MobileNumber = mobileNumber };
    }

    private static bool IsValidSaudiMobileNumber(string mobile)
        => mobile?.Length == 10 && mobile.StartsWith("05");
}
```

**MockCaseManagementService.cs:**
```csharp
public class MockCaseManagementService : ICaseManagementService
{
    private static int _caseCounter = 1000;

    public async Task<CaseRegistrationResult> RegisterCaseAsync(CaseRegistrationData data)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(data.Subject))
            return new CaseRegistrationResult { IsSuccess = false, ErrorMessage = "Subject required" };

        if (!data.Plaintiffs.Any())
            return new CaseRegistrationResult { IsSuccess = false, ErrorMessage = "Plaintiff required" };

        await Task.Delay(500); // Simulate network call

        var caseNumber = $"TEST-CASE-{Interlocked.Increment(ref _caseCounter)}";
        var registrationNumber = $"TEST-REG-{_caseCounter}";

        _logger.LogInformation("[MOCK CASE] Registered: {CaseNumber}", caseNumber);

        return new CaseRegistrationResult
        {
            IsSuccess = true,
            CaseNumber = caseNumber,
            RegistrationNumber = registrationNumber,
            RegistrationDate = DateTime.UtcNow
        };
    }
}
```

### 2.6 DI Registration & Configuration (Day 5)

**IntegrationServiceCollectionExtensions.cs:**
```csharp
public static IServiceCollection AddIntegrationServices(this IServiceCollection services, IConfiguration configuration)
{
    // Register configuration
    services.Configure<SmsSettings>(configuration.GetSection("Integration:Sms"));
    services.Configure<EmailSettings>(configuration.GetSection("Integration:Email"));
    services.Configure<CaseManagementSettings>(configuration.GetSection("Integration:CaseManagement"));

    // Register services based on UseMock flag
    var useMockSms = configuration.GetValue<bool>("Integration:Sms:UseMock", true);
    if (useMockSms)
        services.AddScoped<ISmsService, MockSmsService>();

    var useMockEmail = configuration.GetValue<bool>("Integration:Email:UseMock", true);
    if (useMockEmail)
        services.AddScoped<IEmailService, MockEmailService>();

    var useMockCaseManagement = configuration.GetValue<bool>("Integration:CaseManagement:UseMock", true);
    if (useMockCaseManagement)
        services.AddScoped<ICaseManagementService, MockCaseManagementService>();

    return services;
}
```

**appsettings.json:**
```json
{
  "Integration": {
    "Sms": {
      "UseMock": true,
      "SenderName": "MOJ-BOG"
    },
    "Email": {
      "UseMock": true,
      "FromEmail": "noreply@moj.gov.sa",
      "FromName": "وزارة العدل - نظام إدارة الدعاوى"
    },
    "CaseManagement": {
      "UseMock": true,
      "MockCaseNumberPrefix": "TEST-CASE-",
      "MockRegistrationNumberPrefix": "TEST-REG-"
    }
  }
}
```

Update `ServiceCollectionExtensions.cs` to call `.AddIntegrationServices(configuration)`.

**Verification**: Run integration tests
```bash
dotnet test --filter "Category=Integration"
```

---

## Phase 3: Business Logic Layer (BL) - Weeks 3-4 (10 days)

### Objective
Implement all BL services with comprehensive validation, state management, and business rules.

### 3.1 BL Interfaces (Day 1)

**Location**: `src/Backend/BOG.BL/Interfaces/`

Create 5 BL interface files:

1. **IDefendantBL.cs**
   ```csharp
   Task<DefendantVM> CreateDefendantAsync(int requestId, DefendantCreateDTO dto);
   Task<DefendantVM?> GetDefendantByIdAsync(int defendantId);
   Task<IEnumerable<DefendantListVM>> GetDefendantsByRequestIdAsync(int requestId);
   Task<DefendantVM> UpdateDefendantAsync(int defendantId, DefendantUpdateDTO dto);
   Task DeleteDefendantAsync(int defendantId);
   ```

2. **ICaseRegistrationBL.cs**
3. **IRequestActionBL.cs**
4. **IRequestAttachmentBL.cs**
5. **IAdditionalInfoBL.cs**

### 3.2 TDD: Write BL Tests First (Days 2-3)

**Location**: `tests/BOG.Tests/BL/`

Critical test files:
- **DefendantBLTests.cs**
- **CaseRegistrationBLTests.cs**
- **RequestActionBLTests.cs**

**Key tests for DefendantBL:**
```csharp
[Fact]
public async Task CreateDefendant_WithDuplicateIdentity_ThrowsERR013()

[Fact]
public async Task CreateDefendant_OnlyFullNameRequired_Success()

[Theory]
[InlineData(6)] // Registered
[InlineData(10)] // Rejected
public async Task CreateDefendant_InvalidState_ThrowsException(int statusId)
```

**Key tests for CaseRegistrationBL:**
```csharp
[Fact]
public async Task SubmitRequest_NoPlaintiffs_ThrowsERR001()

[Fact]
public async Task SubmitRequest_NoDefendants_ThrowsERR002()

[Fact]
public async Task SubmitRequest_EmptySubject_ThrowsERR006()

[Fact]
public async Task SubmitRequest_AllValidationsPassed_ChangesStatusToNew()
```

**Key tests for RequestActionBL:**
```csharp
[Theory]
[InlineData(3, "Register", true)]  // New -> Register = valid
[InlineData(1, "Register", false)] // Draft -> Register = invalid
public async Task TakeAction_StateTransition_ValidatesCorrectly(int currentStatus, string action, bool shouldSucceed)

[Fact]
public async Task RegisterCase_Success_SendsNotifications()

[Fact]
public async Task ProcessExpiredCompletionRequests_ExpiredDeadline_AutoRejects()
```

### 3.3 BL Service Implementations (Days 4-8)

**Pattern to Follow**: Study `src/Backend/BOG.BL/Services/UserBL.cs`

Implement services following these patterns:
- Constructor DI (repositories + UnitOfWork + integration services)
- Null validation at start of every method
- Business rule validation before DB operations
- Soft delete (IsDeleted = true)
- Private MapToViewModel helper methods
- Return VMs, not entities

### 3.4 Register BL Services in DI (Day 9)

**Location**: `src/Backend/BOG.API/Extensions/ServiceCollectionExtensions.cs`

Add to `AddBusinessLogicServices()`:
```csharp
// Case Registration services
services.AddScoped<IDefendantBL, DefendantBL>();
services.AddScoped<ICaseRegistrationBL, CaseRegistrationBL>();
services.AddScoped<IRequestActionBL, RequestActionBL>();
services.AddScoped<IRequestAttachmentBL, RequestAttachmentBL>();
services.AddScoped<IAdditionalInfoBL, AdditionalInfoBL>();
```

### 3.5 Background Job Setup (Day 10)

**Location**: `src/Backend/BOG.API/BackgroundServices/CompletionDeadlineCheckerService.cs`

```csharp
public class CompletionDeadlineCheckerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Process expired completion requests
            // Run daily at midnight
        }
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddHostedService<CompletionDeadlineCheckerService>();
```

**Verification**: Run all BL tests
```bash
dotnet test --filter "Category=BL"
```

---

## Phase 4: DTOs & View Models - Week 5 (5 days)

### Objective
Create all DTOs and VMs for API layer.

### 4.1 DTOs (Days 1-2)

**Location**: `src/Backend/BOG.DTO/`

Create DTOs with Data Annotations:
- `DefendantCreateDTO.cs`
- `DefendantUpdateDTO.cs`
- `CaseRegistrationCreateDTO.cs`
- `TakeActionDTO.cs`
- `SearchRequestDTO.cs`
- `RequestDeficiencyDTO.cs`
- `RequestAttachmentDTO.cs`

### 4.2 View Models (Days 3-4)

**Location**: `src/Backend/BOG.VM/`

Create VMs:
- `PagedResult<T>.cs` (shared)
- `DefendantVM.cs`
- `DefendantListVM.cs`
- `CaseRegistrationRequestVM.cs`
- `CaseRegistrationRequestDetailsVM.cs`
- `RequestAttachmentVM.cs`

### 4.3 Validation Testing (Day 5)

Write tests for DTO validation attributes.

**Verification**: Build successfully
```bash
dotnet build src/Backend/BOG.sln
```

---

## Phase 5: API Controllers - Week 6 (5 days)

### Objective
Create RESTful API controllers with comprehensive error handling.

### 5.1 TDD: Write Controller Tests (Days 1-2)

**Location**: `tests/BOG.Tests/API/`

Use `WebApplicationFactory<Program>` for integration testing.

### 5.2 Controller Implementations (Days 3-5)

**Pattern to Follow**: Study `src/Backend/BOG.API/Controllers/UsersController.cs`

**Location**: `src/Backend/BOG.API/Controllers/`

Create controllers:
- **DefendantsController.cs**
- **CaseRegistrationController.cs**

Implement following patterns:
- Full try-catch error handling
- ILogger injection and usage
- ProducesResponseType attributes
- Proper HTTP status codes
- CreatedAtAction for POST
- NoContent for DELETE

**Verification**: Run API integration tests
```bash
dotnet test --filter "Category=API"
```

---

## Phase 6: End-to-End Testing & Refinement - Week 7 (5 days)

### 6.1 E2E Test Scenarios (Days 1-2)

**Manual Testing via Swagger** (http://localhost:5001/swagger):

**Scenario 1: Complete Happy Path**
1. POST /api/case-requests → Create Draft
2. POST /api/case-requests/{id}/defendants → Add Defendant
3. POST /api/case-requests/{id}/submit → Submit
4. POST /api/case-requests/{id}/action → Register Case

**Scenario 2: Validation Testing**
1. Submit without plaintiffs → ERR001
2. Submit without defendants → ERR002
3. Add duplicate defendant → ERR013
4. Upload non-PDF → BR04 error

**Scenario 3: State Transitions**
Test all valid and invalid state transitions.

**Scenario 4: Auto-Rejection (BR05)**
Test 30-day auto-rejection for PendingCompletion.

**Scenario 5: Search Functionality**
Test search with various filters and pagination.

### 6.2 Automated E2E Tests (Days 3-4)

**Location**: `tests/BOG.Tests/E2E/CaseRegistrationE2ETests.cs`

### 6.3 Performance Testing (Day 4)

Test with large datasets (1000+ records).

### 6.4 Final Refinements (Day 5)

- Fix bugs found during E2E testing
- Optimize slow queries
- Add missing validations
- Update documentation

**Verification Checklist:**
- [ ] All unit tests pass (DAL, BL, Integration)
- [ ] All API integration tests pass
- [ ] All E2E scenarios work correctly
- [ ] All validation rules enforced
- [ ] All state transitions validated
- [ ] Background job runs successfully
- [ ] Search functionality works with pagination
- [ ] Notifications logged correctly

---

## Business Rules Validation Summary

| Code | Rule | Implementation Location |
|------|------|------------------------|
| ERR001 | At least one plaintiff required | CaseRegistrationBL.ValidateForSubmissionAsync |
| ERR002 | At least one defendant required | CaseRegistrationBL.ValidateForSubmissionAsync |
| ERR003 | Mandatory attachments incomplete | CaseRegistrationBL.ValidateForSubmissionAsync |
| ERR004 | Applicant not specified | CaseRegistrationBL.ValidateForSubmissionAsync |
| ERR005 | Classifications not specified | CaseRegistrationBL.ValidateForSubmissionAsync |
| ERR006 | Subject is empty | CaseRegistrationBL.ValidateForSubmissionAsync |
| ERR007 | Evidence is empty | CaseRegistrationBL.ValidateForSubmissionAsync |
| ERR013 | Defendant already exists | DefendantBL.CreateDefendantAsync |
| BR04 | PDF only, max 4MB | RequestAttachmentBL.AddAttachmentAsync |
| BR05 | Auto-reject after 30 days | RequestActionBL.ProcessExpiredCompletionRequestsAsync |

---

## State Transition Matrix

| From Status | Action | To Status | Validation |
|-------------|--------|-----------|------------|
| None (0) | Create | Draft (1) | - |
| Draft (1) | Submit | New (3) | ERR001-007 |
| New (3) | Register | Registered (6) | Data complete |
| New (3) | SendToJudge | OnJudgeDesk (5) | - |
| New (3) | Reject | Rejected (10) | Notes required |
| New (3) | RequestCompletion | PendingCompletion (8) | Deficiencies required |
| OnJudgeDesk (5) | Register | Registered (6) | - |
| OnJudgeDesk (5) | Reject | Rejected (10) | Notes required |
| PendingCompletion (8) | Complete | UnderReview (9) | Documents submitted |
| PendingCompletion (8) | Expire (30 days) | Rejected (10) | Automatic (BR05) |

---

## Commands Reference

```bash
# Build solution
dotnet build src/Backend/BOG.sln

# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=DAL"
dotnet test --filter "Category=BL"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=API"

# Run API locally
dotnet run --project src/Backend/BOG.API

# Watch mode for development
dotnet watch run --project src/Backend/BOG.API
```

---

## Success Criteria

The implementation is complete when:

1. ✅ All 130+ files created following established patterns
2. ✅ All tests pass (unit, integration, E2E)
3. ✅ Code coverage ≥ 80% on BL layer
4. ✅ All 14 use cases functional via API
5. ✅ All 10 validation rules enforced
6. ✅ State machine working correctly
7. ✅ Background job auto-rejecting expired requests
8. ✅ Search functionality with pagination
9. ✅ Mock notifications logged correctly
10. ✅ Swagger UI fully functional
11. ✅ No build warnings or errors
12. ✅ Documentation updated

**Estimated Total Effort**: 7 weeks (175 hours) for one developer following TDD approach.

---

*This plan implements UC 6.5.1.1.11 - 6.5.1.1.24 following the phased-by-layer approach with Test-Driven Development and mock integration services.*
