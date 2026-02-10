# Implementation Plan: إنهاء الطلب (Request Completion) Tab

**Use Case**: 6.5.1.1.23
**User Role**: موظف القيد (Registration Clerk/Officer)
**Tab Position**: After إجراءات الطلب (Request Actions)

**⚠️ CRITICAL REQUIREMENT**: Case Type (نوع الدعوى) is **ALWAYS MANDATORY** for ALL decision types - Register, SendToJudge, Reject, and RequestCompletion.

---

## Overview

Implement a new tab "إنهاء الطلب" that allows the Registration Clerk to make final decisions on case registration requests. This tab provides a form to select a decision type, specify case type (ALWAYS REQUIRED), add notes, and approve the decision.

---

## UI Requirements (Based on Screenshot)

### Layout
The tab contains:

1. **Green Header Section**
   - Title: "إنهاء الطلب"
   - Primary Action Button: "اعتماد القرار" (Approve Decision) - positioned on the left

2. **Main Form Section** with two rows:
   - **Row 1 (Two columns)**:
     - **القرار** (Decision): Dropdown - required field
     - **نوع الدعوى** (Case Type): Dropdown - **ALWAYS REQUIRED** (for all decision types)

   - **Row 2 (Full width)**:
     - **ملاحظات** (Notes): Large text area - optional field

3. **Right Side Navigation**
   - Shows active tab indicator for "إنهاء الطلب"

---

## Business Requirements

### Decision Options (القرار Dropdown)

Based on user input, the dropdown should contain:

1. **قيد الدعوى** (Register Case)
   - Transitions request to **Registered** status (ID: 6)
   - Generates **RegistrationNumber** (ONLY if not already created) using format: `CourtName-CurrentYear(Hijri)-SequentialNumber(AutoIncrement)`
   - Example: "المحكمة العامة-1446-001"
   - Requires external case management system integration
   - Returns: CaseNumber, RegistrationNumber, RegistrationDate
   - **Requires Case Type** (validated for all decisions)

2. **العرض على رئيس المحكمة** (Present to Court President)
   - Transitions request to **OnJudgeDesk** status (ID: 5)
   - **Requires Case Type** (validated for all decisions)

3. **التوجيه بعدم قيد الطلب** (Directive Not to Register)
   - Transitions request to **Rejected** status (ID: 10)
   - Requires rejection reason in Notes field
   - **Sends rejection notification:**
     - **Email Subject**: "اشعار نظام معين للدعوى رقم {رقم الدعوى}"
     - **Email Body**: "تم حفظ الطلب {رقم الطلب} لعدم استيفاء متطلبات القيد و مرور المدة القانونية"
     - **SMS Message**: "تم حفظ الطلب {رقم الطلب} لعدم استيفاء متطلبات القيد و مرور المدة القانونية"
     - Note: {رقم الطلب} = request.RequestNumber, {رقم الدعوى} = request.CaseNumber (if available)
   - **Requires Case Type** (validated for all decisions)

4. **استكمال النواقص** (Complete Deficiencies)
   - Transitions request to **PendingCompletion** status (ID: 8)
   - Sets 30-day deadline for completion
   - **Requires Case Type** (validated for all decisions)
   - **After 30 days expiry**:
     - Status changes to "لم يتم استكمال النواقص" (Deficiencies Not Completed)
     - Workflow record created with Notes: "SYSTEM"
     - Background service performs this automatically

### Case Type (نوع الدعوى Dropdown)

This field should be:
- **Visible**: Always visible
- **Required**: Always required (for all decision types)
- **Values**: Only 2 options:
  1. **إداري** (Administrative)
  2. **تأديبي** (Disciplinary)
- **Implementation**: Add `CaseTypeId` field to `CaseRegistrationRequest` entity with lookup to new `CaseType` table

### Notes (ملاحظات Field)

- **Optional**: Always optional for all decisions
- **Max Length**: 4000 characters
- **Multi-line**: Text area with minimum 3 rows

### Validations

Before allowing ANY decision, validate:

- **ERR_CASE_TYPE**: نوع الدعوى مطلوب (Case Type is ALWAYS required for all decision types)
- **ERR003**: جميع المرفقات الإلزامية موجودة (All mandatory attachments present) - for "قيد الدعوى" or "العرض على رئيس المحكمة"
- **ERR005**: يجب إضافة مدعى عليه واحد على الأقل (At least one defendant must be added)
- **ERR010**: يجب إضافة مرفق واحد على الأقل (At least one attachment must be added)

### Confirmation Dialog

- **CON02**: Before submitting any decision, show confirmation dialog
- **Message**: "هل أنت متأكد من الحفظ؟" (Are you sure you want to save?)
- **Buttons**: نعم (Yes) / لا (No)

### Workflow History Tracking

All actions must be saved in **CaseRequestWorkflow** entity with:
- Action date (timestamp)
- Notes (from user input)
- New status (after transition)
- User ID (who performed the action)
- Previous status (for audit trail)

### Permissions

- **User Role**: موظف القيد (Registration Clerk) only
- **Request Status**: Available when status is **Draft (1)** or **New (3)**

---

## Current System Analysis

### Existing Backend Implementation

The backend already has most of the required functionality:

**File**: `src/Backend/BOG.BL/Services/CaseRegistration/RequestActionBL.cs`

Existing methods that match our decisions:
- `RegisterCaseAsync()` → For "قيد الدعوى"
- `SendToJudgeDeskAsync()` → For "العرض على رئيس المحكمة"
- `RejectRequestAsync()` → For "التوجيه بعدم قيد الطلب"
- `RequestCompletionAsync()` → For "استكمال النواقص"

### Missing Backend Components

1. **Unified Endpoint**: Need a single endpoint that accepts decision type and routes to appropriate method
2. **Case Type Validation**: Ensure case type is always provided (required for all decisions)
3. **Notes Handling**: Pass notes to all action methods
4. **Workflow History Entity**: CaseRequestWorkflow entity to track all status changes with date, user, notes
5. **Validation Checks**: ERR003 (attachments), ERR005 (defendants), ERR010 (claims) before registration/judge desk
6. **Confirmation Dialog**: Frontend confirmation before submission (CON02)

### Existing Frontend Patterns

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/request-actions/request-actions.component.ts`

This component shows how to:
- Check permissions based on status
- Call action APIs
- Handle validation
- Display success/error messages

---

## Implementation Plan

### Phase 0: Database/Entity Changes

#### Step 0.1: Create CaseType Lookup Entity

**File**: `src/Backend/BOG.DbModel/Entities/Lookups/CaseType.cs` (NEW FILE)

```csharp
namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup table for case types (Administrative or Disciplinary)
/// </summary>
public class CaseType : BaseEntity
{
    /// <summary>
    /// English name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Arabic name (إداري or تأديبي)
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Active status
    /// </summary>
    public bool IsActive { get; set; } = true;
}
```

Add to `ApplicationDbContext.cs`:
```csharp
public DbSet<CaseType> CaseTypes { get; set; }
```

**Seed Data** (in migration or ApplicationDbContext OnModelCreating):
```csharp
modelBuilder.Entity<CaseType>().HasData(
    new CaseType
    {
        Id = 1,
        Name = "Administrative",
        NameAr = "إداري",
        Description = "Administrative case type",
        IsActive = true,
        CreatedDate = DateTime.UtcNow,
        ModifiedDate = DateTime.UtcNow
    },
    new CaseType
    {
        Id = 2,
        Name = "Disciplinary",
        NameAr = "تأديبي",
        Description = "Disciplinary case type",
        IsActive = true,
        CreatedDate = DateTime.UtcNow,
        ModifiedDate = DateTime.UtcNow
    }
);
```

#### Step 0.2: Add CaseTypeId to CaseRegistrationRequest Entity

**File**: `src/Backend/BOG.DbModel/Entities/CaseRegistration/CaseRegistrationRequest.cs`

Add property with default value:
```csharp
/// <summary>
/// Case Type ID (إداري or تأديبي) - ALWAYS REQUIRED
/// Default: 1 (إداري - Administrative)
/// </summary>
public int CaseTypeId { get; set; } = 1; // Default to إداري (Administrative)

/// <summary>
/// Navigation property to CaseType
/// </summary>
public virtual CaseType? CaseType { get; set; }
```

**Migration Command**:
```bash
dotnet ef migrations add AddCaseTypeEntity --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
dotnet ef database update --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
```

**Update ApplicationDbContext** to configure the relationship:
```csharp
// In OnModelCreating method
modelBuilder.Entity<CaseRegistrationRequest>()
    .HasOne(r => r.CaseType)
    .WithMany()
    .HasForeignKey(r => r.CaseTypeId)
    .OnDelete(DeleteBehavior.Restrict);
```

#### Step 0.3: Create CaseRequestWorkflow Entity (if not exists)

**File**: `src/Backend/BOG.DbModel/Entities/CaseRegistration/CaseRequestWorkflow.cs` (NEW FILE)

```csharp
namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Tracks workflow history for case registration requests.
/// Records all status transitions and actions taken.
/// </summary>
public class CaseRequestWorkflow : BaseEntity
{
    /// <summary>
    /// Case registration request ID
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Previous status ID (before transition)
    /// </summary>
    public int PreviousStatusId { get; set; }

    /// <summary>
    /// New status ID (after transition)
    /// </summary>
    public int NewStatusId { get; set; }

    /// <summary>
    /// User ID who performed the action
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// User's full name (from User.FullName field)
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Notes/remarks for the action
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Action timestamp (should be same as CreatedDate)
    /// </summary>
    public DateTime ActionDate { get; set; }

    // Navigation
    public virtual CaseRegistrationRequest? CaseRegistrationRequest { get; set; }
}
```

Add to `ApplicationDbContext.cs`:
```csharp
public DbSet<CaseRequestWorkflow> CaseRequestWorkflows { get; set; }
```

Create migration for this entity.

---

### Phase 1: Backend Implementation

#### Step 1.1: Update DTOs to Include CaseTypeId

**File**: `src/Backend/BOG.DTO/CaseRegistration/CaseRegistrationCreateDTO.cs`

Add property if not exists:
```csharp
/// <summary>
/// Case Type ID (إداري or تأديبي) - Default: 1 (إداري)
/// </summary>
[Required(ErrorMessage = "نوع الدعوى مطلوب")]
[Range(1, 2, ErrorMessage = "نوع دعوى غير صالح")]
public int CaseTypeId { get; set; } = 1; // Default to إداري
```

**File**: `src/Backend/BOG.DTO/CaseRegistration/CaseRegistrationUpdateDTO.cs`

Add property if not exists:
```csharp
/// <summary>
/// Case Type ID (إداري or تأديبي)
/// </summary>
[Range(1, 2, ErrorMessage = "نوع دعوى غير صالح")]
public int? CaseTypeId { get; set; }
```

#### Step 1.2: Create Decision DTO

**File**: `src/Backend/BOG.DTO/CaseRegistration/RequestDecisionDTO.cs` (NEW FILE)

```csharp
using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// DTO for making final decision on case registration request.
/// Used in POST /api/case-requests/{id}/complete
/// </summary>
public class RequestDecisionDTO
{
    /// <summary>
    /// Decision type
    /// </summary>
    [Required(ErrorMessage = "نوع القرار مطلوب")]
    [RegularExpression("^(Register|SendToJudge|Reject|RequestCompletion)$",
        ErrorMessage = "نوع قرار غير صالح")]
    public string DecisionType { get; set; } = "";

    /// <summary>
    /// Case type ID (ALWAYS REQUIRED for all decisions)
    /// Valid values: 1 (إداري) or 2 (تأديبي)
    /// </summary>
    [Required(ErrorMessage = "نوع الدعوى مطلوب")]
    [Range(1, 2, ErrorMessage = "نوع دعوى غير صالح")]
    public int CaseTypeId { get; set; } = 1; // Default to إداري

    /// <summary>
    /// Notes/remarks (optional)
    /// </summary>
    [StringLength(4000, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 4000 حرف")]
    public string? Notes { get; set; }

    /// <summary>
    /// Deficiencies list (for RequestCompletion decision)
    /// </summary>
    public List<string>? Deficiencies { get; set; }
}
```

---

#### Step 1.2: Add Complete Request Method to BL

**File**: `src/Backend/BOG.BL/Interfaces/CaseRegistration/IRequestActionBL.cs`

Add method signature:

```csharp
/// <summary>
/// Completes request processing with final decision.
/// Used by Registration Clerk (موظف القيد) to finalize request.
/// </summary>
Task<CaseRegistrationRequestVM> CompleteRequestAsync(
    int requestId,
    RequestDecisionDTO decision,
    CancellationToken cancellationToken = default);
```

**File**: `src/Backend/BOG.BL/Services/CaseRegistration/RequestActionBL.cs`

Add implementation method (after existing action methods):

```csharp
public async Task<CaseRegistrationRequestVM> CompleteRequestAsync(
    int requestId,
    RequestDecisionDTO decision,
    CancellationToken cancellationToken = default)
{
    if (requestId <= 0)
        throw new ArgumentException("Invalid request ID.", nameof(requestId));

    if (decision == null)
        throw new ArgumentNullException(nameof(decision));

    // Load request
    var request = await _requestRepository.GetWithDetailsAsync(requestId, cancellationToken);
    if (request == null || request.IsDeleted)
        throw new InvalidOperationException($"الطلب {requestId} غير موجود");

    // Validate request is in valid state for completion
    if (request.RequestStatusId != 1 && request.RequestStatusId != 3)
        throw new InvalidOperationException(
            "لا يمكن إنهاء الطلب. الحالة الحالية لا تسمح بهذا الإجراء");

    // Store previous status for workflow history
    int previousStatus = request.RequestStatusId;

    // ERR_CASE_TYPE: Case Type ALWAYS required (for ALL decision types)
    if (decision.CaseTypeId <= 0 || decision.CaseTypeId > 2)
        throw new InvalidOperationException("نوع الدعوى مطلوب");

    // **CRITICAL**: Update request with CaseTypeId before executing action
    request.CaseTypeId = decision.CaseTypeId;
    request.ModifiedDate = DateTime.UtcNow;
    await _requestRepository.UpdateAsync(request, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // Validate for Register and SendToJudge decisions
    if (decision.DecisionType == "Register" || decision.DecisionType == "SendToJudge")
    {
        // ERR003: Check mandatory attachments
        var attachmentErrors = await ValidateMandatoryAttachmentsAsync(requestId, cancellationToken);
        if (attachmentErrors.Any())
            throw new InvalidOperationException("جميع المرفقات الإلزامية مطلوبة");

        // ERR005: Check at least one defendant
        if (!request.Defendants?.Any(d => !d.IsDeleted) ?? true)
            throw new InvalidOperationException("يجب إضافة مدعى عليه واحد على الأقل");

        // ERR010: Check at least one attachment
        if (!request.Attachments?.Any(a => !a.IsDeleted) ?? true)
            throw new InvalidOperationException("يجب إضافة مرفق واحد على الأقل");
    }

    // Route to appropriate action method based on decision type
    CaseRegistrationRequestVM result = null!;
    switch (decision.DecisionType)
    {
        case "Register":
            // Call register method
            result = await RegisterCaseAsync(requestId, decision.CaseTypeId,
                decision.Notes, cancellationToken);
            break;

        case "SendToJudge":
            // Call send to judge method
            result = await SendToJudgeDeskAsync(requestId, decision.Notes, cancellationToken);
            break;

        case "Reject":
            // Call reject method
            result = await RejectRequestAsync(requestId, decision.Notes, cancellationToken);
            break;

        case "RequestCompletion":
            // Call request completion method
            result = await RequestCompletionAsync(requestId, decision.Notes,
                decision.Deficiencies, cancellationToken);
            break;

        default:
            throw new InvalidOperationException($"نوع قرار غير صالح: {decision.DecisionType}");
    }

    // Save workflow history for audit trail
    await SaveWorkflowHistoryAsync(
        requestId,
        previousStatus,
        result.RequestStatusId,
        decision.Notes,
        cancellationToken);

    return result;
}

/// <summary>
/// Save workflow history record for audit trail
/// </summary>
private async Task SaveWorkflowHistoryAsync(
    int requestId,
    int previousStatusId,
    int newStatusId,
    string? notes,
    CancellationToken cancellationToken)
{
    var workflow = new CaseRequestWorkflow
    {
        CaseRegistrationRequestId = requestId,
        PreviousStatusId = previousStatusId,
        NewStatusId = newStatusId,
        Notes = notes,
        ActionDate = DateTime.UtcNow,
        CreatedDate = DateTime.UtcNow,
        ModifiedDate = DateTime.UtcNow
    };

    // Note: Get UserId and FullName from current user context
    // var currentUser = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
    // workflow.UserId = currentUserId;
    // workflow.FullName = currentUser?.FullName;

    await _workflowRepository.AddAsync(workflow, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

---

#### Step 1.3: Update RegisterCaseAsync to Accept CaseTypeId and Generate RegistrationNumber

**File**: `src/Backend/BOG.BL/Services/CaseRegistration/RequestActionBL.cs`

Modify the `RegisterCaseAsync` method signature (currently line 85):

```csharp
public async Task<CaseRegistrationRequestVM> RegisterCaseAsync(
    int requestId,
    int caseTypeId,  // ADD THIS PARAMETER
    string? notes = null,
    CancellationToken cancellationToken = default)
{
    // ... existing validation code ...

    // Update request with case type
    request.CaseTypeId = caseTypeId;

    // Generate RegistrationNumber ONLY if it doesn't already exist
    if (string.IsNullOrEmpty(request.RegistrationNumber))
    {
        var registrationNumber = await GenerateRegistrationNumberAsync(
            request.CourtId,
            cancellationToken);

        request.RegistrationNumber = registrationNumber;
        request.RegistrationDate = DateTime.UtcNow;
    }

    // ... rest of existing implementation (external system integration) ...
}

/// <summary>
/// Generate registration number in format: CourtName-HijriYear-SequentialNumber
/// Example: "المحكمة العامة-1446-001"
/// Note: Only called if RegistrationNumber doesn't already exist
/// </summary>
private async Task<string> GenerateRegistrationNumberAsync(
    int courtId,
    CancellationToken cancellationToken)
{
    // Get court name
    var court = await _courtRepository.GetByIdAsync(courtId, cancellationToken);
    if (court == null)
        throw new InvalidOperationException($"المحكمة {courtId} غير موجودة");

    // Get current Hijri year
    var hijriCalendar = new System.Globalization.HijriCalendar();
    var currentHijriYear = hijriCalendar.GetYear(DateTime.Now);

    // Get next sequential number for this court and year
    var lastRegistration = await _requestRepository
        .FindAsync(
            r => r.CourtId == courtId &&
                 r.RegistrationNumber != null &&
                 r.RegistrationNumber.Contains($"-{currentHijriYear}-"),
            cancellationToken)
        .OrderByDescending(r => r.RegistrationNumber)
        .FirstOrDefaultAsync(cancellationToken);

    int sequentialNumber = 1;
    if (lastRegistration != null && !string.IsNullOrEmpty(lastRegistration.RegistrationNumber))
    {
        // Extract sequential number from last registration
        var parts = lastRegistration.RegistrationNumber.Split('-');
        if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
        {
            sequentialNumber = lastNumber + 1;
        }
    }

    // Format: CourtName-HijriYear-SequentialNumber (padded to 3 digits)
    return $"{court.NameAr}-{currentHijriYear}-{sequentialNumber:D3}";
}
```

**Update**: `CaseRegistrationRequest` entity already has `CaseTypeId`, `RegistrationNumber`, and `RegistrationDate` fields.

---

#### Step 1.4: Update Background Service for Expired Completion Requests

**File**: `src/Backend/BOG.API/BackgroundServices/CompletionDeadlineCheckerService.cs`

Update the background service to:
1. Change status to "لم يتم استكمال النواقص" (instead of Rejected)
2. Create workflow history record with Notes: "SYSTEM"

```csharp
// In ProcessExpiredCompletionRequestsAsync method:

foreach (var expiredRequest in expiredRequests)
{
    var previousStatusId = expiredRequest.RequestStatusId;

    // Change status to "لم يتم استكمال النواقص"
    // Note: Determine the correct status ID for this status
    expiredRequest.RequestStatusId = [DEFICIENCIES_NOT_COMPLETED_STATUS_ID];
    expiredRequest.ModifiedDate = DateTime.UtcNow;

    await _requestRepository.UpdateAsync(expiredRequest, cancellationToken);

    // Create workflow history record with SYSTEM as user
    var workflow = new CaseRequestWorkflow
    {
        CaseRegistrationRequestId = expiredRequest.Id,
        PreviousStatusId = previousStatusId,
        NewStatusId = expiredRequest.RequestStatusId,
        UserId = null, // No user - system action
        FullName = "SYSTEM", // System-generated action
        Notes = "SYSTEM",
        ActionDate = DateTime.UtcNow,
        CreatedDate = DateTime.UtcNow,
        ModifiedDate = DateTime.UtcNow
    };

    await _workflowRepository.AddAsync(workflow, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // Send notification about deficiencies not completed
    // ...existing notification code...
}
```

**Note**: You'll need to add the new status "لم يتم استكمال النواقص" to the RequestStatus lookup table if it doesn't exist.

---

#### Step 1.5: Add Controller Endpoint

**File**: `src/Backend/BOG.API/Controllers/CaseRegistrationController.cs`

Add new endpoint (after existing action endpoints):

```csharp
/// <summary>
/// Complete request processing with final decision.
/// POST /api/case-requests/{id}/complete
/// </summary>
[HttpPost("{id}/complete")]
[ProducesResponseType(typeof(CaseRegistrationRequestVM), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> CompleteRequest(
    int id,
    [FromBody] RequestDecisionDTO decision,
    CancellationToken cancellationToken)
{
    try
    {
        var result = await _requestActionBL.CompleteRequestAsync(id, decision, cancellationToken);
        _logger.LogInformation("Request {RequestId} completed with decision: {Decision}",
            id, decision.DecisionType);
        return Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Cannot complete request {RequestId}", id);
        return BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error completing request {RequestId}", id);
        return StatusCode(500, new { message = "حدث خطأ أثناء معالجة الطلب" });
    }
}
```

---

#### Step 1.5: Update Rejection Notification Messages

**File**: `src/Backend/BOG.BL/Services/CaseRegistration/RequestActionBL.cs`

Update the `SendRejectionNotificationsAsync()` method (or equivalent) to use the specified notification content:

```csharp
private async Task SendRejectionNotificationsAsync(
    CaseRegistrationRequest request,
    CancellationToken cancellationToken = default)
{
    var requestNumber = request.RequestNumber;
    var caseNumber = request.CaseNumber ?? "غير محدد";

    // Email notification
    var emailDto = new EmailNotificationDTO
    {
        To = request.CaseRequestPlaintiffs?.FirstOrDefault()?.Plaintiff?.Email,
        Subject = $"اشعار نظام معين للدعوى رقم {caseNumber}",
        Body = $"تم حفظ الطلب {requestNumber} لعدم استيفاء متطلبات القيد و مرور المدة القانونية",
        Priority = NotificationPriority.High
    };

    await _emailService.SendEmailAsync(emailDto, cancellationToken);

    // SMS notification
    var smsDto = new SmsNotificationDTO
    {
        PhoneNumber = request.CaseRequestPlaintiffs?.FirstOrDefault()?.Plaintiff?.MobileNumber,
        Message = $"تم حفظ الطلب {requestNumber} لعدم استيفاء متطلبات القيد و مرور المدة القانونية",
        Priority = NotificationPriority.High
    };

    await _smsService.SendSmsAsync(smsDto, cancellationToken);
}
```

**Note**: Adjust the method signature and DTO structure based on existing implementation in the codebase.

---

#### Step 1.6: Add Case Type Lookup Endpoint

**File**: `src/Backend/BOG.API/Controllers/LookupsController.cs`

Add new endpoint to return the 2 case types (إداري, تأديبي):

```csharp
/// <summary>
/// GET /api/lookups/case-types
/// Returns all active case types (إداري, تأديبي)
/// </summary>
[HttpGet("case-types")]
[ProducesResponseType(typeof(List<CaseTypeVM>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetCaseTypes(CancellationToken cancellationToken)
{
    try
    {
        var caseTypes = await _context.CaseTypes
            .Where(ct => ct.IsActive && !ct.IsDeleted)
            .OrderBy(ct => ct.Id)
            .Select(ct => new
            {
                id = ct.Id,
                name = ct.Name,
                nameAr = ct.NameAr,
                description = ct.Description
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} case types", caseTypes.Count);
        return Ok(caseTypes);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving case types");
        return StatusCode(500, new { message = "حدث خطأ أثناء تحميل أنواع الدعاوى" });
    }
}
```

**Expected Response**:
```json
[
  {
    "id": 1,
    "name": "Administrative",
    "nameAr": "إداري",
    "description": "Administrative case type"
  },
  {
    "id": 2,
    "name": "Disciplinary",
    "nameAr": "تأديبي",
    "description": "Disciplinary case type"
  }
]
```

---

### Phase 2: Frontend Implementation

#### Step 2.1: Create TypeScript Interfaces

**File**: `src/Frontend/bog-app/src/app/features/case-registration/models/enums.ts`

Add decision type enum:

```typescript
export enum DecisionType {
  Register = 'Register',
  SendToJudge = 'SendToJudge',
  Reject = 'Reject',
  RequestCompletion = 'RequestCompletion'
}
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/models/case-request.model.ts`

Add interfaces:

```typescript
export interface RequestDecisionDTO {
  decisionType: string;
  caseTypeId?: number;
  notes?: string;
  deficiencies?: string[];
}

export interface CaseTypeVM {
  id: number;
  name: string;
  nameAr: string;
  description?: string;
}
```

---

#### Step 2.2: Update API Service

**File**: `src/Frontend/bog-app/src/app/features/case-registration/services/case-registration-api.service.ts`

Add methods:

```typescript
/**
 * Complete request with final decision
 */
completeRequest(requestId: number, decision: RequestDecisionDTO): Observable<CaseRequestVM> {
  return this.http.post<CaseRequestVM>(
    `${this.apiUrl}/${requestId}/complete`,
    decision
  );
}

/**
 * Get case types lookup
 */
getCaseTypes(): Observable<CaseTypeVM[]> {
  return this.http.get<CaseTypeVM[]>(`${this.lookupsUrl}/case-types`);
}
```

---

#### Step 2.3: Create Request Completion Component

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/request-completion/request-completion.component.ts` (NEW FILE)

```typescript
import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CaseRegistrationApiService } from '../../services/case-registration-api.service';
import { RequestStateService } from '../../services/request-state.service';
import { DecisionType } from '../../models/enums';
import { CaseTypeVM, RequestDecisionDTO } from '../../models/case-request.model';

@Component({
  selector: 'app-request-completion',
  templateUrl: './request-completion.component.html',
  styleUrls: ['./request-completion.component.scss']
})
export class RequestCompletionComponent implements OnInit {
  @Input() requestId!: number;
  @Input() currentStatus!: number;

  completionForm!: FormGroup;
  caseTypes: CaseTypeVM[] = [];
  isSubmitting = false;

  // Decision options with Arabic labels
  decisionOptions = [
    { value: DecisionType.Register, label: 'قيد الدعوى' },
    { value: DecisionType.SendToJudge, label: 'العرض على رئيس المحكمة' },
    { value: DecisionType.Reject, label: 'التوجيه بعدم قيد الطلب' },
    { value: DecisionType.RequestCompletion, label: 'استكمال النواقص' }
  ];

  constructor(
    private fb: FormBuilder,
    private apiService: CaseRegistrationApiService,
    private requestState: RequestStateService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.initializeForm();
    this.loadCaseTypes();
    this.setupConditionalValidation();
  }

  private initializeForm() {
    this.completionForm = this.fb.group({
      decisionType: ['', Validators.required],
      caseTypeId: [1, Validators.required],  // Default to 1 (إداري - Administrative)
      notes: ['', Validators.maxLength(4000)]
    });
  }

  private loadCaseTypes() {
    this.apiService.getCaseTypes().subscribe({
      next: (types) => {
        this.caseTypes = types;
      },
      error: (error) => {
        console.error('Failed to load case types', error);
        this.snackBar.open('فشل تحميل أنواع الدعاوى', 'إغلاق', { duration: 3000 });
      }
    });
  }

  private setupConditionalValidation() {
    // Notes is always optional - no conditional validation needed
    // Just maintain maxLength validation
  }

  get canComplete(): boolean {
    // Only allow completion if status is Draft (1) or New (3)
    return this.currentStatus === 1 || this.currentStatus === 3;
  }

  onSubmit() {
    if (!this.completionForm.valid) {
      this.snackBar.open('يرجى تعبئة جميع الحقول المطلوبة', 'إغلاق', { duration: 3000 });
      return;
    }

    if (!this.canComplete) {
      this.snackBar.open('لا يمكن إنهاء الطلب في هذه الحالة', 'إغلاق', { duration: 3000 });
      return;
    }

    // Show confirmation dialog (CON02)
    const dialogRef = this.dialog.open(ConfirmationDialog, {
      width: '400px',
      data: {
        title: 'تأكيد العملية',
        message: 'هل أنت متأكد من الحفظ؟',
        confirmText: 'نعم',
        cancelText: 'لا'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return; // User clicked Cancel
      }

      const decision: RequestDecisionDTO = {
        decisionType: this.completionForm.value.decisionType,
        caseTypeId: this.completionForm.value.caseTypeId,
        notes: this.completionForm.value.notes || undefined
      };

      this.isSubmitting = true;

      this.apiService.completeRequest(this.requestId, decision).subscribe({
      next: (result) => {
        this.isSubmitting = false;

        // Update request state
        this.requestState.updateRequestStatus(this.requestId, result.requestStatusId);

        // Show success message based on decision
        const messages = {
          [DecisionType.Register]: 'تم قيد الدعوى بنجاح',
          [DecisionType.SendToJudge]: 'تم العرض على رئيس المحكمة بنجاح',
          [DecisionType.Reject]: 'تم رفض الطلب',
          [DecisionType.RequestCompletion]: 'تم طلب استكمال النواقص'
        };

        this.snackBar.open(
          messages[decision.decisionType as keyof typeof messages],
          'إغلاق',
          { duration: 5000 }
        );

        // Reset form
        this.completionForm.reset();
      },
      error: (error) => {
        this.isSubmitting = false;
        console.error('Failed to complete request', error);

        const errorMessage = error.error?.message || 'فشل إنهاء الطلب';
        this.snackBar.open(errorMessage, 'إغلاق', { duration: 5000 });
      }
      });
    });
  }
}
```

---

#### Step 2.4: Create Component Template

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/request-completion/request-completion.component.html` (NEW FILE)

```html
<app-section-container
  title="إنهاء الطلب"
  sectionId="completion"
  icon="check_circle">

  <div class="completion-section">
    <!-- Warning if cannot complete -->
    <app-validation-message
      *ngIf="!canComplete"
      type="warning"
      message="لا يمكن إنهاء الطلب في الحالة الحالية">
    </app-validation-message>

    <!-- Completion Form -->
    <form [formGroup]="completionForm" (ngSubmit)="onSubmit()" *ngIf="canComplete">

      <!-- Green Header with Action Button -->
      <div class="form-header">
        <h3>إنهاء الطلب</h3>
        <button mat-raised-button
                color="primary"
                type="submit"
                [disabled]="!completionForm.valid || isSubmitting">
          <mat-icon *ngIf="!isSubmitting">check</mat-icon>
          <mat-spinner *ngIf="isSubmitting" diameter="20"></mat-spinner>
          {{ isSubmitting ? 'جاري المعالجة...' : 'اعتماد القرار' }}
        </button>
      </div>

      <!-- Form Fields -->
      <div class="form-content">

        <!-- Row 1: Decision and Case Type -->
        <div class="form-row">

          <!-- Decision Dropdown -->
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>القرار *</mat-label>
            <mat-select formControlName="decisionType" required>
              <mat-option *ngFor="let option of decisionOptions" [value]="option.value">
                {{ option.label }}
              </mat-option>
            </mat-select>
            <mat-error *ngIf="completionForm.get('decisionType')?.hasError('required')">
              نوع القرار مطلوب
            </mat-error>
          </mat-form-field>

          <!-- Case Type Dropdown (always required) -->
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>نوع الدعوى *</mat-label>
            <mat-select formControlName="caseTypeId" required>
              <mat-option *ngFor="let type of caseTypes" [value]="type.id">
                {{ type.nameAr }}
              </mat-option>
            </mat-select>
            <mat-error *ngIf="completionForm.get('caseTypeId')?.hasError('required')">
              نوع الدعوى مطلوب
            </mat-error>
          </mat-form-field>

        </div>

        <!-- Row 2: Notes -->
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>ملاحظات</mat-label>
            <textarea matInput
                      formControlName="notes"
                      rows="4"
                      maxlength="4000"
                      placeholder="أدخل ملاحظاتك هنا..."></textarea>
            <mat-hint align="end">
              {{ completionForm.get('notes')?.value?.length || 0 }} / 4000
            </mat-hint>
            <mat-error *ngIf="completionForm.get('notes')?.hasError('required')">
              الملاحظات مطلوبة لهذا النوع من القرار
            </mat-error>
            <mat-error *ngIf="completionForm.get('notes')?.hasError('maxlength')">
              الملاحظات لا يمكن أن تتجاوز 4000 حرف
            </mat-error>
          </mat-form-field>
        </div>

      </div>
    </form>

  </div>

</app-section-container>
```

---

#### Step 2.5: Create Component Styles

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/request-completion/request-completion.component.scss` (NEW FILE)

```scss
.completion-section {
  padding: 16px 0;
}

.form-header {
  background-color: #2e7d32; // Green background
  color: white;
  padding: 16px 24px;
  border-radius: 4px 4px 0 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;

  h3 {
    margin: 0;
    font-size: 18px;
    font-weight: 500;
  }

  button {
    background-color: white;
    color: #2e7d32;

    &:hover:not(:disabled) {
      background-color: #f5f5f5;
    }
  }
}

.form-content {
  padding: 0 24px 24px 24px;
}

.form-row {
  display: flex;
  gap: 16px;
  margin-bottom: 16px;

  &:last-child {
    margin-bottom: 0;
  }
}

.full-width {
  width: 100%;
}

.half-width {
  flex: 1;
  min-width: 0;
}

// RTL Support
:host-context([dir="rtl"]) {
  .form-row {
    flex-direction: row-reverse;
  }
}

// Responsive design
@media (max-width: 768px) {
  .form-row {
    flex-direction: column;
    gap: 0;
  }

  .half-width {
    width: 100%;
  }
}
```

---

#### Step 2.6: Register Component in Module

**File**: `src/Frontend/bog-app/src/app/features/case-registration/case-registration.module.ts`

Add imports and declarations:

```typescript
import { RequestCompletionComponent } from './components/request-completion/request-completion.component';

@NgModule({
  declarations: [
    // ... existing components
    RequestCompletionComponent
  ],
  // ... rest of module
})
```

---

#### Step 2.7: Integrate into Request Details Page

**File**: `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.html`

Add the completion tab to the navigation menu and content area.

**In the tabs navigation** (after إجراءات الطلب):

```html
<li (click)="activeTab = 'completion'" [class.active]="activeTab === 'completion'">
  <mat-icon>check_circle</mat-icon>
  <span>إنهاء الطلب</span>
</li>
```

**In the tab content area** (after request-actions section):

```html
<section *ngSwitchCase="'completion'" class="tab-pane">
  <app-request-completion
    [requestId]="requestId"
    [currentStatus]="request?.requestStatusId || 0">
  </app-request-completion>
</section>
```

---

### Phase 2.5: Update View Models to Include CaseType

**File**: `src/Backend/BOG.VM/CaseRegistration/CaseRegistrationRequestVM.cs`

Add properties if not exists:
```csharp
/// <summary>
/// Case Type ID
/// </summary>
public int CaseTypeId { get; set; }

/// <summary>
/// Case Type Name in Arabic (إداري or تأديبي)
/// </summary>
public string? CaseTypeName { get; set; }
```

**File**: `src/Backend/BOG.VM/CaseRegistration/CaseRegistrationRequestDetailsVM.cs`

Add properties if not exists:
```csharp
/// <summary>
/// Case Type ID
/// </summary>
public int CaseTypeId { get; set; }

/// <summary>
/// Case Type Name in Arabic (إداري or تأديبي)
/// </summary>
public string? CaseTypeName { get; set; }
```

**Update Mapping Logic** in BL services to include CaseType when returning view models:
```csharp
// In CaseRegistrationBL.cs or RequestActionBL.cs when mapping to VM
CaseTypeId = request.CaseTypeId,
CaseTypeName = request.CaseType?.NameAr
```

---

### Phase 3: Testing Plan

#### Backend Testing

**Test 1: Complete with Register Decision**
```bash
# Prerequisites: Request with ID 1410 in New (3) or UnderReview (9) status

curl -X POST http://localhost:5002/api/case-requests/1410/complete \
  -H "Content-Type: application/json" \
  -d '{
    "decisionType": "Register",
    "caseTypeId": 1,
    "notes": "تم قيد الدعوى حسب الأصول"
  }'
```

**Expected**:
- Response 200 OK
- Request status changed to Registered (6)
- CaseNumber, RegistrationNumber assigned
- Notification sent

**Test 2: Complete with SendToJudge Decision**
```bash
curl -X POST http://localhost:5002/api/case-requests/1410/complete \
  -H "Content-Type: application/json" \
  -d '{
    "decisionType": "SendToJudge",
    "notes": "للعرض على رئيس المحكمة للنظر"
  }'
```

**Expected**:
- Response 200 OK
- Request status changed to OnJudgeDesk (5)
- Notification sent

**Test 3: Complete with Reject Decision**
```bash
curl -X POST http://localhost:5002/api/case-requests/1410/complete \
  -H "Content-Type: application/json" \
  -d '{
    "decisionType": "Reject",
    "notes": "عدم استيفاء الشروط النظامية"
  }'
```

**Expected**:
- Response 200 OK
- Request status changed to Rejected (10)
- Rejection notification sent

**Test 4: Complete with RequestCompletion Decision**
```bash
curl -X POST http://localhost:5002/api/case-requests/1410/complete \
  -H "Content-Type: application/json" \
  -d '{
    "decisionType": "RequestCompletion",
    "notes": "يرجى استكمال المرفقات المطلوبة"
  }'
```

**Expected**:
- Response 200 OK
- Request status changed to PendingCompletion (8)
- 30-day deadline set
- Completion notification sent

**Test 5: Validation Errors**
```bash
# Missing case type for Register
curl -X POST http://localhost:5002/api/case-requests/1410/complete \
  -H "Content-Type: application/json" \
  -d '{
    "decisionType": "Register"
  }'
```

**Expected**: 400 Bad Request with "نوع الدعوى مطلوب عند قيد الدعوى"

---

#### Frontend Testing

**Test 1: UI Display**
- Open request in New or UnderReview status
- Navigate to "إنهاء الطلب" tab
- Verify:
  - Green header displays
  - Decision dropdown shows 4 options
  - Notes text area displays
  - "اعتماد القرار" button visible

**Test 2: Conditional Case Type Field**
- Select "قيد الدعوى" decision
- Verify: Case Type dropdown appears and is required
- Select different decision
- Verify: Case Type field disappears

**Test 3: Notes Validation**
- Select "التوجيه بعدم قيد الطلب"
- Try to submit without notes
- Verify: Error message "الملاحظات مطلوبة لهذا النوع من القرار"
- Enter notes and submit
- Verify: Success

**Test 4: Complete Flow**
1. Select "قيد الدعوى"
2. Select case type
3. Enter notes
4. Click "اعتماد القرار"
5. Verify:
   - Loading spinner appears
   - Success message displays
   - Request status updates
   - Form resets

**Test 5: Permission Check**
- Open request in Draft (1) status
- Navigate to "إنهاء الطلب" tab
- Verify: Warning message "لا يمكن إنهاء الطلب في الحالة الحالية"
- Verify: Form is hidden

---

### Phase 4: Database Verification

**Query to check status changes:**
```sql
SELECT
    Id,
    RequestNumber,
    Subject,
    RequestStatusId,
    CaseNumber,
    RegistrationNumber,
    RegistrationDate,
    ModifiedDate
FROM CaseRegistrationRequests
WHERE Id = 1410;
```

**Query to check notifications sent:**
```sql
-- If notification tracking is implemented
SELECT
    NotificationType,
    Recipient,
    SentDate,
    Status
FROM Notifications
WHERE EntityId = 1410
  AND EntityType = 'CaseRequest'
ORDER BY SentDate DESC;
```

---

## Success Criteria

### Backend
- ✅ `RequestDecisionDTO` created with validation
- ✅ `CompleteRequestAsync()` method implemented in BL
- ✅ `/api/case-requests/{id}/complete` endpoint added
- ✅ Case type validation enforced for Register decision
- ✅ Notes validation enforced for Reject/RequestCompletion
- ✅ Status transitions work correctly
- ✅ Notifications sent for each decision type
- ✅ Solution builds without errors

### Frontend
- ✅ Request Completion component created
- ✅ UI matches screenshot design
- ✅ Decision dropdown displays 4 options in Arabic
- ✅ Case Type field conditionally displayed
- ✅ Notes field with character counter
- ✅ Form validation works correctly
- ✅ Submit button shows loading state
- ✅ Success/error messages display
- ✅ Tab integrated into request details page
- ✅ Permission checks prevent unauthorized access

### Integration
- ✅ API calls succeed from frontend
- ✅ Request status updates in real-time
- ✅ External case management integration works (for Register)
- ✅ Notifications sent successfully
- ✅ No console errors
- ✅ RTL layout works correctly

---

## Architecture Layer Changes Summary

### Database/Entity Layer (BOG.DbModel)
**New Entity:**
- `CaseRequestWorkflow` - Track workflow history with PreviousStatusId, NewStatusId, Notes, ActionDate, UserId, FullName (stores User.FullName)

**Modified Entity:**
- `CaseRegistrationRequest` - Ensure fields exist: CaseTypeId, RegistrationNumber, RegistrationDate, LastModifiedByUser

**New Status:**
- Add "لم يتم استكمال النواقص" to RequestStatus lookup table

**Migration:**
- Create migration for CaseRequestWorkflow entity

### Data Access Layer (BOG.DAL)
**New Repository:**
- `IWorkflowRepository` interface
- `WorkflowRepository` implementation for CaseRequestWorkflow

**Modified:**
- Register workflow repository in DI container

### Business Logic Layer (BOG.BL)
**New DTO:**
- `RequestDecisionDTO` - With DecisionType, CaseTypeId (required), Notes (optional, 4000 chars)

**New Interface Method:**
- `IRequestActionBL.CompleteRequestAsync()` - Unified endpoint for all decisions

**New Implementation:**
- `CompleteRequestAsync()` - Routes to appropriate action, validates ERR003/ERR005/ERR010, saves workflow history
- `GenerateRegistrationNumberAsync()` - Creates CourtName-HijriYear-Sequential format (only if not already exists)
- `SaveWorkflowHistoryAsync()` - Records status transitions

**Modified:**
- `RegisterCaseAsync()` - Accept caseTypeId parameter, generate RegistrationNumber (only if not exists), update LastModifiedByUser
- Background service - Create workflow record with "SYSTEM" notes on 30-day expiry

### API Layer (BOG.API)
**New Controller Endpoint:**
- `POST /api/case-requests/{id}/complete` - Accept RequestDecisionDTO, return CaseRegistrationRequestVM

**Modified Controller:**
- Add case types lookup endpoint if not exists: `GET /api/lookups/case-types`

**Modified Background Service:**
- `CompletionDeadlineCheckerService` - Create workflow record when expiring requests

### Frontend Layer (bog-app)
**New Components:**
- `RequestCompletionComponent` (.ts/.html/.scss)
- Confirmation dialog component (or use existing)

**New Interfaces:**
- `DecisionType` enum
- `RequestDecisionDTO` interface
- `CaseTypeVM` interface
- `CaseRequestWorkflow` interface (for display if needed)

**New Services:**
- Add methods to `CaseRegistrationApiService`:
  - `completeRequest()`
  - `getCaseTypes()`

**Modified Components:**
- `RequestDetailsComponent` - Add "إنهاء الطلب" tab navigation and content area
- Register new component in `CaseRegistrationModule`

**UI Features:**
- Decision dropdown (4 options)
- Case type dropdown (always required)
- Notes textarea (always optional, 4000 chars)
- CON02 confirmation dialog
- Validation display for ERR003/ERR005/ERR010
- Status-based permission check (Draft or New only)

---

## Files to Create/Modify

### Backend (6 new, 5 modified)

**New Files:**
1. `src/Backend/BOG.DTO/CaseRegistration/RequestDecisionDTO.cs`

**Modified Files:**
2. `src/Backend/BOG.BL/Interfaces/CaseRegistration/IRequestActionBL.cs` - Add CompleteRequestAsync signature
3. `src/Backend/BOG.BL/Services/CaseRegistration/RequestActionBL.cs` - Add CompleteRequestAsync implementation, modify RegisterCaseAsync
4. `src/Backend/BOG.API/Controllers/CaseRegistrationController.cs` - Add /complete endpoint
5. `src/Backend/BOG.API/Controllers/LookupsController.cs` - Add /case-types endpoint (if not exists)

### Frontend (3 new, 3 modified)

**New Files:**
6. `src/Frontend/bog-app/src/app/features/case-registration/components/request-completion/request-completion.component.ts`
7. `src/Frontend/bog-app/src/app/features/case-registration/components/request-completion/request-completion.component.html`
8. `src/Frontend/bog-app/src/app/features/case-registration/components/request-completion/request-completion.component.scss`

**Modified Files:**
9. `src/Frontend/bog-app/src/app/features/case-registration/models/enums.ts` - Add DecisionType enum
10. `src/Frontend/bog-app/src/app/features/case-registration/models/case-request.model.ts` - Add RequestDecisionDTO, CaseTypeVM interfaces
11. `src/Frontend/bog-app/src/app/features/case-registration/services/case-registration-api.service.ts` - Add completeRequest, getCaseTypes methods
12. `src/Frontend/bog-app/src/app/features/case-registration/case-registration.module.ts` - Register new component
13. `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.html` - Add completion tab

---

## Rollback Plan

If issues occur:
1. Remove the new `/complete` endpoint from controller
2. Remove the `CompleteRequestAsync` method from BL
3. Hide the "إنهاء الطلب" tab in frontend by commenting out the navigation item
4. Users can still use individual action buttons (Register, Reject, etc.) as before

---

## Notes

- **⚠️ CRITICAL**: Case Type (نوع الدعوى) is **ALWAYS MANDATORY** for ALL decision types (Register, SendToJudge, Reject, RequestCompletion)
  - This is validated on backend before any action execution
  - Frontend form has required validator on caseTypeId field
  - User cannot submit form without selecting Case Type
- The "إنهاء الطلب" feature consolidates multiple actions into one unified interface
- Backend already has all required methods - we're just routing through a single endpoint
- Permission enforcement should be added using `[Authorize]` attributes in future phase
- Consider adding audit log for all decisions made through this tab
- The 30-day auto-rejection for "استكمال النواقص" is already handled by background service
- Case Type has only 2 values: إداري (Administrative) and تأديبي (Disciplinary)
- Case Type is stored in CaseRegistrationRequest.CaseTypeId field
- CaseType lookup table should be seeded in migration with these 2 values

---

## Summary of Case Type Requirement Changes

This plan has been updated to ensure **Case Type is ALWAYS MANDATORY** for all decision types:

### Case Type Implementation:
- **Values**: Only 2 options - **إداري** (Administrative) or **تأديبي** (Disciplinary)
- **Storage**: New `CaseTypeId` field in `CaseRegistrationRequest` entity
- **Lookup Table**: New `CaseType` entity with seeded data for the 2 values
- **Endpoint**: `GET /api/lookups/case-types` returns the 2 case types
- **Frontend**: Dropdown with 2 options, always required for all decisions

### Changes Made:
1. **Database**: Created CaseType lookup entity with 2 seeded values
2. **Entity**: Added CaseTypeId to CaseRegistrationRequest
3. **Overview**: Clarified that Case Type is always required
4. **UI Requirements**: Changed from "conditionally required" to "ALWAYS REQUIRED"
5. **Decision Options**: Added "Requires Case Type" note to all 4 decision types
6. **Validations**: Added ERR_CASE_TYPE validation check before other validations
7. **Backend DTO**: Added comment emphasizing CaseTypeId is always required
8. **Backend Logic**: Added validation check `if (decision.CaseTypeId <= 0)` at start of CompleteRequestAsync
9. **Lookup Controller**: Added GET /api/lookups/case-types endpoint
10. **Frontend Form**: caseTypeId field has `Validators.required` and is always visible
11. **Frontend Template**: Case Type dropdown shows required asterisk (*) and validation error with 2 options

### Validation Flow:
```
User clicks "اعتماد القرار" (Approve Decision)
  ↓
Frontend validates form (caseTypeId required)
  ↓
Backend receives RequestDecisionDTO with CaseTypeId
  ↓
Backend validates CaseTypeId (1-2 range) (ERR_CASE_TYPE)
  ↓
Backend updates request.CaseTypeId and saves to database
  ↓
Backend validates other requirements (ERR003, ERR005, ERR010)
  ↓
Action executes successfully
  ↓
Backend returns updated VM with CaseTypeId and CaseTypeName
```

---

## CaseTypeId Implementation Across All Layers

### ✅ Complete Layer-by-Layer Implementation:

#### 1. **Database Layer** (`BOG.DbModel`)
- ✅ **New Entity**: `CaseType` lookup table with 2 seeded values
  - ID: 1, NameAr: "إداري" (Administrative)
  - ID: 2, NameAr: "تأديبي" (Disciplinary)
- ✅ **Updated Entity**: `CaseRegistrationRequest.CaseTypeId = 1` (default)
- ✅ **Migration**: `dotnet ef migrations add AddCaseTypeEntity`
- ✅ **Foreign Key**: Configured in ApplicationDbContext

#### 2. **DTO Layer** (`BOG.DTO`)
- ✅ **CaseRegistrationCreateDTO**: Added `CaseTypeId` with default = 1
- ✅ **CaseRegistrationUpdateDTO**: Added `CaseTypeId` (nullable)
- ✅ **RequestDecisionDTO**: Added `CaseTypeId` with validation [Range(1,2)]

#### 3. **View Model Layer** (`BOG.VM`)
- ✅ **CaseRegistrationRequestVM**: Added `CaseTypeId` and `CaseTypeName`
- ✅ **CaseRegistrationRequestDetailsVM**: Added `CaseTypeId` and `CaseTypeName`
- ✅ **Mapping**: Include `request.CaseType?.NameAr` when mapping

#### 4. **Business Logic Layer** (`BOG.BL`)
- ✅ **CompleteRequestAsync**: Validates CaseTypeId (1-2 range)
- ✅ **CompleteRequestAsync**: Updates `request.CaseTypeId = decision.CaseTypeId`
- ✅ **CompleteRequestAsync**: Saves to database before executing action
- ✅ **RegisterCaseAsync**: Accepts CaseTypeId parameter
- ✅ **View Model Mapping**: Includes CaseTypeId and CaseTypeName

#### 5. **API Layer** (`BOG.API`)
- ✅ **LookupsController**: `GET /api/lookups/case-types` returns 2 options
- ✅ **CaseRegistrationController**: `POST /api/case-requests/{id}/complete` accepts CaseTypeId
- ✅ **Response**: Returns updated request with CaseTypeId

#### 6. **Frontend Layer** (`bog-app`)
- ✅ **Interface**: `CaseTypeVM { id, name, nameAr }`
- ✅ **Interface**: `RequestDecisionDTO { caseTypeId }`
- ✅ **API Service**: `getCaseTypes()` method
- ✅ **API Service**: `completeRequest(id, dto)` sends CaseTypeId
- ✅ **Component**: Form initialized with `caseTypeId: [1, Validators.required]`
- ✅ **Template**: Dropdown bound to caseTypeId, pre-selected to "إداري"
- ✅ **Validation**: Required validator, error message displayed

### Data Flow Example:

```
1. Page Load:
   GET /api/lookups/case-types
   → Returns: [{ id: 1, nameAr: "إداري" }, { id: 2, nameAr: "تأديبي" }]

2. Form Initialization:
   caseTypeId = 1 (default to إداري)
   Dropdown displays both options with "إداري" selected

3. User Selects Decision:
   User chooses "قيد الدعوى" and keeps "إداري" selected

4. Submit Form:
   POST /api/case-requests/123/complete
   Body: {
     "decisionType": "Register",
     "caseTypeId": 1,
     "notes": "..."
   }

5. Backend Processing:
   - Validates: caseTypeId is 1 or 2 ✓
   - Updates: request.CaseTypeId = 1
   - Saves to database
   - Executes Register action
   - Returns VM with caseTypeId and caseTypeName

6. Frontend Updates:
   Request object now has:
   - caseTypeId: 1
   - caseTypeName: "إداري"
```
