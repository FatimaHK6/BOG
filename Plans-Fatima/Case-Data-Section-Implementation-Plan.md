# Implementation Plan: Enhanced بيانات الدعوى (Case Data) Section
## Vertical Tabs Design Implementation

## Overview

Transform the current basic case-data-form into a comprehensive 6-tab module with **vertical tabs positioned on the right** (matching the HTML mockup design):

### 6 Subsections:
1. **موضوع وأسانيد الدعوى** - Rich text editors for موضوع وأسانيد الدعوى
2. **طلبات الدعوى** - Claims table list (gold "إضافة طلب" button)
3. **الدعاوى المرتبطة** - Related cases table list
4. **تصنيف الدعوى** - Classifications (4-level hierarchical, multi-select searchable table dialog)
5. **بيانات التواصل** - Contact information (3 fields: primary mobile, secondary mobile, email)
6. **المرفقات** - Attachments (existing component, moved to tab)

### Design Specifications (from HTML mockup):
- **Layout**: Content area on left, vertical tabs on right
- **Tabs**: Right-side vertical navigation with icons and labels
- **Active Tab Color**: Teal/cyan (#2c7a7b) with left border indicator
- **Hover State**: Light teal background (#e8f4f8)
- **Add Buttons**: Gold color (#d4a017) for "إضافة طلب", "إضافة دعوى مرتبطة"
- **Action Buttons**: Fixed عودة (Back) and حفظ كمسودة (Save Draft) buttons visible across all tabs
- **Client-side Storage**: All form data stored in client-side state (BehaviorSubject) until user clicks save button
- **RTL Support**: Full Arabic RTL layout

---

## Current State Analysis

### Backend:
- ✅ **Entities Exist**: `Claim` (ClaimText, DisplayOrder)
- ⚠️ **RelatedCase Needs Migration**: Current fields (CaseNumber, Notes) → Required fields (CourtId, CaseNumber, CaseYear)
- ❌ **Missing**: DTOs, ViewModels, Repositories, Business Logic services, API Controllers
- ✅ **Subject/Evidence**: Already stored in `CaseRegistrationRequest.Subject` and `.Evidence`
- ✅ **Court Entity**: Already exists at `BOG.DbModel.Entities.Identity.Court`
- ✅ **Court Seeding**: 6 courts seeded (3 regions: Riyadh, Jeddah, Dammam) with 2 court types each (General + Commercial)
- ❌ **Missing**: API endpoint for fetching courts list

### Frontend:
- ✅ **Current**: Basic `case-data-form` with plain textareas for subject/evidence
- ✅ **Material Tabs Module**: Already imported but unused
- ❌ **Missing**: Rich text editor (ngx-editor), vertical tabs layout, Claims/RelatedCases components
- ✅ **Attachments Component**: Exists at `attachments-list.component.ts`

---

## PHASE 0: Database Seeding for Courts

### Court Seeding (COMPLETED)

**Status**: ✅ **COMPLETED** - Seed data has been added to `ApplicationDbContext.cs`

**Seeded Data**:
- **6 Courts** (Riyadh, Jeddah, Dammam - each with General + Commercial courts)
  - Court ID 1: Riyadh General Court (المحكمة العامة بالرياض) - Region 1, City 1
  - Court ID 2: Riyadh Commercial Court (المحكمة التجارية بالرياض) - Region 1, City 1
  - Court ID 3: Jeddah General Court (المحكمة العامة بجدة) - Region 2, City 2
  - Court ID 4: Jeddah Commercial Court (المحكمة التجارية بجدة) - Region 2, City 2
  - Court ID 5: Dammam General Court (المحكمة العامة بالدمام) - Region 5, City 5
  - Court ID 6: Dammam Commercial Court (المحكمة التجارية بالدمام) - Region 5, City 5

**Default Behavior**:
- When creating a new case request, CourtId defaults to **1** (Riyadh General Court - first court in seeding order)
- Users can change the court by selecting from the court dropdown

**Migration Required**:
```bash
dotnet ef migrations add "SeedCourtsAndDepartments" --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
dotnet ef database update --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
```

**Files Modified**:
- `src/Backend/BOG.DbModel/ApplicationDbContext.cs` - Added `SeedCourtsAndDepartments()` call in `SeedLookupData()` method

### Entity Updates for Contact Information

**File**: `src/Backend/BOG.DbModel/Entities/CaseRegistration/CaseRegistrationRequest.cs`

Add these fields to the entity to support contact information:
```csharp
// Add to CaseRegistrationRequest entity:

/// <summary>
/// Primary mobile phone number (رقم الجوال الأساسي).
/// Must be 10 digits starting with "05" or null.
/// </summary>
[StringLength(10, MinimumLength = 10, ErrorMessage = "رقم الجوال يجب أن يكون 10 أرقام")]
[RegularExpression(@"^05\d{8}$", ErrorMessage = "رقم الجوال يجب أن يبدأ بـ 05 ويكون 10 أرقام")]
public string? PrimaryMobile { get; set; }

/// <summary>
/// Secondary mobile phone number (رقم الجوال الثانوي).
/// Must be 10 digits starting with "05" or null.
/// </summary>
[StringLength(10, MinimumLength = 10, ErrorMessage = "رقم الجوال يجب أن يكون 10 أرقام")]
[RegularExpression(@"^05\d{8}$", ErrorMessage = "رقم الجوال يجب أن يبدأ بـ 05 ويكون 10 أرقام")]
public string? SecondaryMobile { get; set; }

/// <summary>
/// Email address (البريد الإلكتروني).
/// Must be valid email format or null.
/// </summary>
[EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
[StringLength(255)]
public string? Email { get; set; }
```

**Migration Required**:
```bash
dotnet ef migrations add "AddContactFieldsToRequest" --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
dotnet ef database update --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
```

**Update Frontend Request Details**:

Update `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.ts` to use Court ID 1 by default:
```typescript
createNewRequest() {
  this.loading = true;
  const dto: CaseRequestCreateDTO = {
    courtId: 1,  // Default to Riyadh General Court (first seeded court)
    subject: '',
    evidence: ''
  };

  this.caseRegistrationApi.create(dto).subscribe({
    next: (request) => {
      this.requestId = request.id;
      this.requestState.updateRequest(request);
      this.loading = false;
      this.router.navigate(['/case-registration', request.id, 'edit'], { replaceUrl: true });
    },
    error: (error) => {
      this.loading = false;
      console.error('Error creating request:', error);
    }
  });
}
```

---

## PHASE 0: Database Setup - Classification Entity (4-Level Hierarchy)

### 0.1 Modify Classification Entity

**File**: `src/Backend/BOG.DbModel/Entities/Lookups/Classification.cs`

**Current Entity**: Has only `Name`, `NameAr`, `Description`, `IsActive` (flat structure)

**Update**: Add 4 new required level fields:

```csharp
namespace BOG.DbModel.Entities.Lookups;

public class Classification : BaseEntity
{
    /// <summary>
    /// التصنيف الأول (First level classification)
    /// </summary>
    public string Level1 { get; set; } = null!;

    /// <summary>
    /// التصنيف الثاني (Second level classification)
    /// </summary>
    public string Level2 { get; set; } = null!;

    /// <summary>
    /// التصنيف الثالث (Third level classification)
    /// </summary>
    public string Level3 { get; set; } = null!;

    /// <summary>
    /// التصنيف الرابع (Fourth level classification)
    /// </summary>
    public string Level4 { get; set; } = null!;

    // Keep existing fields for backward compatibility
    public string Name { get; set; } = null!;           // Legacy
    public string NameAr { get; set; } = null!;         // Legacy
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### 0.2 Update ApplicationDbContext Configuration

**File**: `src/Backend/BOG.DbModel/ApplicationDbContext.cs`

In the `ConfigureLookupEntities` method (around line 509), update Classification configuration:

```csharp
modelBuilder.Entity<Classification>(entity =>
{
    entity.HasKey(e => e.Id);

    // New 4-level structure
    entity.Property(e => e.Level1).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Level2).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Level3).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Level4).IsRequired().HasMaxLength(200);

    // Legacy fields
    entity.Property(e => e.Name).HasMaxLength(100);
    entity.Property(e => e.NameAr).HasMaxLength(100);
    entity.Property(e => e.Description).HasMaxLength(500);

    entity.Property(e => e.IsActive).HasDefaultValue(true);
    entity.Property(e => e.IsDeleted).HasDefaultValue(false);

    // Index for search performance across all 4 levels
    entity.HasIndex(e => new { e.Level1, e.Level2, e.Level3, e.Level4 });
});
```

### 0.3 Add Database Migration

**Command**:
```bash
dotnet ef migrations add AddFourLevelClassificationStructure --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
dotnet ef database update --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
```

### 0.4 Update Seed Data

**File**: `src/Backend/BOG.DbModel/ApplicationDbContext.cs`

In the `SeedLookupData` method (around line 1107), replace current classification seeds with 4-level hierarchical data:

```csharp
var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

modelBuilder.Entity<Classification>().HasData(
    // Civil Cases - Property Rights
    new Classification {
        Id = 1,
        Level1 = "دعاوى مدنية",
        Level2 = "دعاوى الملكية",
        Level3 = "منازعات الأراضي",
        Level4 = "نزاع حدود",
        Name = "Civil-Property-Land-Boundary",
        NameAr = "دعاوى مدنية",
        IsActive = true, IsDeleted = false,
        CreatedDate = now, ModifiedDate = now
    },

    // Civil Cases - Contracts
    new Classification {
        Id = 2,
        Level1 = "دعاوى مدنية",
        Level2 = "دعاوى العقود",
        Level3 = "عقود البيع",
        Level4 = "إخلال بالعقد",
        Name = "Civil-Contracts-Sales-Breach",
        NameAr = "دعاوى مدنية",
        IsActive = true, IsDeleted = false,
        CreatedDate = now, ModifiedDate = now
    },

    // Commercial Cases - Company Disputes
    new Classification {
        Id = 3,
        Level1 = "دعاوى تجارية",
        Level2 = "منازعات الشركات",
        Level3 = "نزاعات الشركاء",
        Level4 = "توزيع الأرباح",
        Name = "Commercial-Company-Partner-Profit",
        NameAr = "دعاوى تجارية",
        IsActive = true, IsDeleted = false,
        CreatedDate = now, ModifiedDate = now
    },

    // Add 10+ more examples covering different hierarchies
    // (Commercial checks, Labor, Family, Administrative cases, etc.)
);
```

### 0.5 Create Classification ViewModel

**New File**: `src/Backend/BOG.VM/Lookups/ClassificationVM.cs`

```csharp
namespace BOG.VM.Lookups;

/// <summary>
/// View Model for Classification lookup with 4-level hierarchy.
/// </summary>
public class ClassificationVM
{
    public int Id { get; set; }

    public string Level1 { get; set; } = null!;  // التصنيف الأول
    public string Level2 { get; set; } = null!;  // التصنيف الثاني
    public string Level3 { get; set; } = null!;  // التصنيف الثالث
    public string Level4 { get; set; } = null!;  // التصنيف الرابع

    /// <summary>
    /// Combined text for display: "Level1 - Level2 - Level3 - Level4"
    /// </summary>
    public string FullText => $"{Level1} - {Level2} - {Level3} - {Level4}";

    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
```

### 0.6 Update LookupsController

**File**: `src/Backend/BOG.API/Controllers/LookupsController.cs`

Update the `GetClassifications` method (around line 41):

```csharp
[HttpGet("classifications")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<IEnumerable<ClassificationVM>>> GetClassifications(CancellationToken cancellationToken)
{
    try
    {
        var classifications = await _classificationRepository.FindAsync(
            c => c.IsActive && !c.IsDeleted,
            cancellationToken);

        var result = classifications
            .Select(c => new ClassificationVM
            {
                Id = c.Id,
                Level1 = c.Level1,
                Level2 = c.Level2,
                Level3 = c.Level3,
                Level4 = c.Level4,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .OrderBy(c => c.Level1)
            .ThenBy(c => c.Level2)
            .ThenBy(c => c.Level3)
            .ThenBy(c => c.Level4)
            .ToList();

        _logger.LogInformation("Retrieved {Count} classifications", result.Count);
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving classifications");
        return StatusCode(StatusCodes.Status500InternalServerError,
            new { message = "An error occurred while retrieving classifications." });
    }
}
```

Add using: `using BOG.VM.Lookups;`

---

## PHASE 1: Backend Implementation (Claims & RelatedCases)

### 1.0 Database Migration for RelatedCase Entity

**File**: `src/Backend/BOG.DbModel/Entities/CaseRegistration/RelatedCase.cs`

**Modify entity** to have exactly 3 fields (remove Notes, add CourtId and CaseYear):
```csharp
namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for related cases (الدعاوى المرتبطة) - one-to-many with CaseRegistrationRequest.
/// </summary>
public class RelatedCase : BaseEntity
{
    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Foreign key to Court (المحكمة).
    /// </summary>
    public int? CourtId { get; set; }

    /// <summary>
    /// Related case number (رقم الدعوى) - max 11 digits.
    /// </summary>
    public int CaseNumber { get; set; }

    /// <summary>
    /// Case year (عام الدعوى).
    /// </summary>
    public int CaseYear { get; set; }

    /// <summary>
    /// Navigation property for the request.
    /// </summary>
    public virtual CaseRegistrationRequest Request { get; set; } = null!;

    /// <summary>
    /// Navigation property for the court.
    /// </summary>
    public virtual Court? Court { get; set; }
}
```

**Create Migration**:
```bash
dotnet ef migrations add UpdateRelatedCaseFields --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
```

The migration will:
- Add `CourtId` column (int, nullable, FK to Court)
- Modify `CaseNumber` column (change from string to int, max 11 digits)
- Add `CaseYear` column (int, not null, default 0)
- Drop `Notes` column

**Apply Migration**:
```bash
dotnet ef database update --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
```

### 1.1 Create DTOs

**File**: `src/Backend/BOG.DTO/CaseRegistration/ClaimDTO.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

public class ClaimDTO
{
    [Required(ErrorMessage = "نص الطلب مطلوب")]
    [StringLength(2000, ErrorMessage = "لا يمكن أن يتجاوز نص الطلب 2000 حرف")]
    public string ClaimText { get; set; } = null!;
}

public class ClaimsBatchUpdateDTO
{
    public List<ClaimDTO> Claims { get; set; } = new();
}
```

**File**: `src/Backend/BOG.DTO/CaseRegistration/RelatedCaseDTO.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

public class RelatedCaseDTO
{
    public int? CourtId { get; set; }

    [Required(ErrorMessage = "رقم الدعوى مطلوب")]
    [Range(1, 99999999999, ErrorMessage = "رقم الدعوى يجب أن يكون بين 1 و 99999999999 (11 رقم كحد أقصى)")]
    public int CaseNumber { get; set; }

    [Required(ErrorMessage = "عام الدعوى مطلوب")]
    [Range(1900, 2100, ErrorMessage = "يجب أن يكون عام الدعوى بين 1900 و 2100")]
    public int CaseYear { get; set; }
}

public class RelatedCasesBatchUpdateDTO
{
    public List<RelatedCaseDTO> RelatedCases { get; set; } = new();
}
```

### 1.2 Create View Models

**File**: `src/Backend/BOG.VM/CaseRegistration/ClaimVM.cs`
```csharp
namespace BOG.VM.CaseRegistration;

public class ClaimVM
{
    public int Id { get; set; }
    public int CaseRegistrationRequestId { get; set; }
    public string ClaimText { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

**File**: `src/Backend/BOG.VM/CaseRegistration/RelatedCaseVM.cs`
```csharp
namespace BOG.VM.CaseRegistration;

public class RelatedCaseVM
{
    public int Id { get; set; }
    public int CaseRegistrationRequestId { get; set; }
    public int? CourtId { get; set; }
    public string? CourtName { get; set; } // For display purposes
    public int CaseNumber { get; set; }
    public int CaseYear { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

### 1.3 Create Repository Interfaces & Implementations

**File**: `src/Backend/BOG.DAL/Interfaces/IClaimRepository.cs`
```csharp
using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

public interface IClaimRepository : IRepository<Claim>
{
    Task<IEnumerable<Claim>> GetByRequestIdAsync(int requestId, CancellationToken ct = default);
    Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default);
}
```

**File**: `src/Backend/BOG.DAL/Repositories/ClaimRepository.cs`
```csharp
using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

public class ClaimRepository : Repository<Claim>, IClaimRepository
{
    public ClaimRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Claim>> GetByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.CaseRegistrationRequestId == requestId && !c.IsDeleted)
            .OrderBy(c => c.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        var claims = await _dbSet
            .Where(c => c.CaseRegistrationRequestId == requestId && !c.IsDeleted)
            .ToListAsync(ct);

        foreach (var claim in claims)
        {
            claim.IsDeleted = true;
            claim.ModifiedDate = DateTime.UtcNow;
        }
    }
}
```

**File**: `src/Backend/BOG.DAL/Interfaces/IRelatedCaseRepository.cs`
```csharp
using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

public interface IRelatedCaseRepository : IRepository<RelatedCase>
{
    Task<IEnumerable<RelatedCase>> GetByRequestIdAsync(int requestId, CancellationToken ct = default);
    Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default);
}
```

**File**: `src/Backend/BOG.DAL/Repositories/RelatedCaseRepository.cs`
```csharp
using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

public class RelatedCaseRepository : Repository<RelatedCase>, IRelatedCaseRepository
{
    public RelatedCaseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<RelatedCase>> GetByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(rc => rc.Court) // Include Court for display
            .Where(rc => rc.CaseRegistrationRequestId == requestId && !rc.IsDeleted)
            .OrderByDescending(rc => rc.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        var relatedCases = await _dbSet
            .Where(rc => rc.CaseRegistrationRequestId == requestId && !rc.IsDeleted)
            .ToListAsync(ct);

        foreach (var relatedCase in relatedCases)
        {
            relatedCase.IsDeleted = true;
            relatedCase.ModifiedDate = DateTime.UtcNow;
        }
    }
}
```

### 1.4 Create Business Logic Services

**File**: `src/Backend/BOG.BL/Interfaces/CaseRegistration/IClaimBL.cs`
```csharp
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Interfaces.CaseRegistration;

public interface IClaimBL
{
    Task<IEnumerable<ClaimVM>> GetClaimsAsync(int requestId, CancellationToken ct = default);
    Task<IEnumerable<ClaimVM>> UpdateClaimsAsync(int requestId, ClaimsBatchUpdateDTO dto, CancellationToken ct = default);
}
```

**File**: `src/Backend/BOG.BL/Services/CaseRegistration/ClaimBL.cs`
```csharp
using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Services.CaseRegistration;

public class ClaimBL : IClaimBL
{
    private readonly IClaimRepository _claimRepository;
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClaimBL(
        IClaimRepository claimRepository,
        ICaseRegistrationRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _claimRepository = claimRepository;
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ClaimVM>> GetClaimsAsync(int requestId, CancellationToken ct = default)
    {
        var claims = await _claimRepository.GetByRequestIdAsync(requestId, ct);
        return claims.Select(c => new ClaimVM
        {
            Id = c.Id,
            CaseRegistrationRequestId = c.CaseRegistrationRequestId,
            ClaimText = c.ClaimText,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate
        });
    }

    public async Task<IEnumerable<ClaimVM>> UpdateClaimsAsync(int requestId, ClaimsBatchUpdateDTO dto, CancellationToken ct = default)
    {
        // Validate request exists and is editable
        var request = await _requestRepository.GetByIdAsync(requestId, ct);
        if (request == null || request.IsDeleted)
            throw new KeyNotFoundException("Case registration request not found");

        // Check if request is in editable status (1=Draft, 8=Deficiencies)
        if (request.RequestStatusId != 1 && request.RequestStatusId != 8)
            throw new InvalidOperationException("Request cannot be edited in current status");

        // Soft delete existing claims
        await _claimRepository.DeleteByRequestIdAsync(requestId, ct);

        // Create new claims from DTO
        var newClaims = dto.Claims.Select(c => new Claim
        {
            CaseRegistrationRequestId = requestId,
            ClaimText = c.ClaimText,
            DisplayOrder = 0, // Not used for ordering, kept for entity compatibility
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        }).ToList();

        foreach (var claim in newClaims)
        {
            await _claimRepository.AddAsync(claim, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Return saved claims
        var savedClaims = await _claimRepository.GetByRequestIdAsync(requestId, ct);
        return savedClaims.Select(c => new ClaimVM
        {
            Id = c.Id,
            CaseRegistrationRequestId = c.CaseRegistrationRequestId,
            ClaimText = c.ClaimText,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate
        });
    }
}
```

**File**: `src/Backend/BOG.BL/Interfaces/CaseRegistration/IRelatedCaseBL.cs`
```csharp
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Interfaces.CaseRegistration;

public interface IRelatedCaseBL
{
    Task<IEnumerable<RelatedCaseVM>> GetRelatedCasesAsync(int requestId, CancellationToken ct = default);
    Task<IEnumerable<RelatedCaseVM>> UpdateRelatedCasesAsync(int requestId, RelatedCasesBatchUpdateDTO dto, CancellationToken ct = default);
}
```

**File**: `src/Backend/BOG.BL/Services/CaseRegistration/RelatedCaseBL.cs`
```csharp
using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Services.CaseRegistration;

public class RelatedCaseBL : IRelatedCaseBL
{
    private readonly IRelatedCaseRepository _relatedCaseRepository;
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RelatedCaseBL(
        IRelatedCaseRepository relatedCaseRepository,
        ICaseRegistrationRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _relatedCaseRepository = relatedCaseRepository;
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<RelatedCaseVM>> GetRelatedCasesAsync(int requestId, CancellationToken ct = default)
    {
        var relatedCases = await _relatedCaseRepository.GetByRequestIdAsync(requestId, ct);
        return relatedCases.Select(rc => new RelatedCaseVM
        {
            Id = rc.Id,
            CaseRegistrationRequestId = rc.CaseRegistrationRequestId,
            CourtId = rc.CourtId,
            CourtName = rc.Court?.NameAr, // Arabic court name for display
            CaseNumber = rc.CaseNumber,
            CaseYear = rc.CaseYear,
            CreatedDate = rc.CreatedDate,
            ModifiedDate = rc.ModifiedDate
        });
    }

    public async Task<IEnumerable<RelatedCaseVM>> UpdateRelatedCasesAsync(int requestId, RelatedCasesBatchUpdateDTO dto, CancellationToken ct = default)
    {
        // Validate request exists and is editable
        var request = await _requestRepository.GetByIdAsync(requestId, ct);
        if (request == null || request.IsDeleted)
            throw new KeyNotFoundException("Case registration request not found");

        if (request.RequestStatusId != 1 && request.RequestStatusId != 8)
            throw new InvalidOperationException("Request cannot be edited in current status");

        // Soft delete existing related cases
        await _relatedCaseRepository.DeleteByRequestIdAsync(requestId, ct);

        // Create new related cases from DTO
        var newRelatedCases = dto.RelatedCases.Select(rc => new RelatedCase
        {
            CaseRegistrationRequestId = requestId,
            CourtId = rc.CourtId,
            CaseNumber = rc.CaseNumber,
            CaseYear = rc.CaseYear,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        }).ToList();

        foreach (var relatedCase in newRelatedCases)
        {
            await _relatedCaseRepository.AddAsync(relatedCase, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Return saved related cases
        var savedRelatedCases = await _relatedCaseRepository.GetByRequestIdAsync(requestId, ct);
        return savedRelatedCases.Select(rc => new RelatedCaseVM
        {
            Id = rc.Id,
            CaseRegistrationRequestId = rc.CaseRegistrationRequestId,
            CourtId = rc.CourtId,
            CourtName = rc.Court?.NameAr,
            CaseNumber = rc.CaseNumber,
            CaseYear = rc.CaseYear,
            CreatedDate = rc.CreatedDate,
            ModifiedDate = rc.ModifiedDate
        });
    }
}
```

### 1.5 Create API Controllers

**File**: `src/Backend/BOG.API/Controllers/ClaimsController.cs`
```csharp
using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

[ApiController]
[Route("api/case-requests/{requestId}/claims")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimBL _claimBL;

    public ClaimsController(IClaimBL claimBL)
    {
        _claimBL = claimBL;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClaimVM>>> GetClaims([FromRoute] int requestId, CancellationToken ct = default)
    {
        try
        {
            var claims = await _claimBL.GetClaimsAsync(requestId, ct);
            return Ok(claims);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطأ في الحصول على الطلبات", error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult<IEnumerable<ClaimVM>>> UpdateClaims(
        [FromRoute] int requestId,
        [FromBody] ClaimsBatchUpdateDTO dto,
        CancellationToken ct = default)
    {
        try
        {
            var claims = await _claimBL.UpdateClaimsAsync(requestId, dto, ct);
            return Ok(claims);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطأ في تحديث الطلبات", error = ex.Message });
        }
    }
}
```

**File**: `src/Backend/BOG.API/Controllers/RelatedCasesController.cs`
```csharp
using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

[ApiController]
[Route("api/case-requests/{requestId}/related-cases")]
public class RelatedCasesController : ControllerBase
{
    private readonly IRelatedCaseBL _relatedCaseBL;

    public RelatedCasesController(IRelatedCaseBL relatedCaseBL)
    {
        _relatedCaseBL = relatedCaseBL;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RelatedCaseVM>>> GetRelatedCases([FromRoute] int requestId, CancellationToken ct = default)
    {
        try
        {
            var relatedCases = await _relatedCaseBL.GetRelatedCasesAsync(requestId, ct);
            return Ok(relatedCases);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطأ في الحصول على الدعاوى المرتبطة", error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult<IEnumerable<RelatedCaseVM>>> UpdateRelatedCases(
        [FromRoute] int requestId,
        [FromBody] RelatedCasesBatchUpdateDTO dto,
        CancellationToken ct = default)
    {
        try
        {
            var relatedCases = await _relatedCaseBL.UpdateRelatedCasesAsync(requestId, dto, ct);
            return Ok(relatedCases);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطأ في تحديث الدعاوى المرتبطة", error = ex.Message });
        }
    }
}
```

### 1.6 Register Services in DI

**File**: `src/Backend/BOG.API/Extensions/ServiceCollectionExtensions.cs`

Add to existing service registration:
```csharp
// Claim services
services.AddScoped<IClaimRepository, ClaimRepository>();
services.AddScoped<IClaimBL, ClaimBL>();

// Related Case services
services.AddScoped<IRelatedCaseRepository, RelatedCaseRepository>();
services.AddScoped<IRelatedCaseBL, RelatedCaseBL>();
```

---

## PHASE 2: Frontend - Package Installation

### 2.1 Install ngx-editor

**Command**:
```bash
cd src/Frontend/bog-app
npm install ngx-editor@12.2.1 --save
```

**Why ngx-editor v12.2.1?**
- Angular 13 compatible
- RTL support for Arabic
- Lightweight (no jQuery)
- Built-in formatting toolbar
- Character limit support

---

## PHASE 3: Frontend - State Management Service (NEW - HIGH PRIORITY)

### 3.0 Create CaseDataStateService (NEW)

**File**: `src/Frontend/bog-app/src/app/features/case-registration/services/case-data-state.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ClaimVM } from '../models/claim.model';
import { RelatedCaseVM } from '../models/related-case.model';

export interface CaseDataState {
  subject: string;
  evidence: string;
  claims: ClaimVM[];
  relatedCases: RelatedCaseVM[];
  classificationIds: number[];
  primaryMobile: string;
  secondaryMobile: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class CaseDataStateService {
  private initialState: CaseDataState = {
    subject: '',
    evidence: '',
    claims: [],
    relatedCases: [],
    classificationIds: [],
    primaryMobile: '',
    secondaryMobile: '',
    email: ''
  };

  private stateSubject = new BehaviorSubject<CaseDataState>(this.initialState);
  public state$: Observable<CaseDataState> = this.stateSubject.asObservable();

  constructor() {
    // Optionally load from localStorage on service creation
    this.loadFromLocalStorage();
  }

  // Getters
  getState(): CaseDataState {
    return this.stateSubject.value;
  }

  getSubject(): string {
    return this.stateSubject.value.subject;
  }

  getEvidence(): string {
    return this.stateSubject.value.evidence;
  }

  getClaims(): ClaimVM[] {
    return this.stateSubject.value.claims;
  }

  getRelatedCases(): RelatedCaseVM[] {
    return this.stateSubject.value.relatedCases;
  }

  getClassificationIds(): number[] {
    return this.stateSubject.value.classificationIds;
  }

  getContactInfo(): { primary: string; secondary: string; email: string } {
    const state = this.stateSubject.value;
    return {
      primary: state.primaryMobile,
      secondary: state.secondaryMobile,
      email: state.email
    };
  }

  // Setters - update state WITHOUT saving to database
  updateSubject(value: string): void {
    this.updateState({ ...this.stateSubject.value, subject: value });
  }

  updateEvidence(value: string): void {
    this.updateState({ ...this.stateSubject.value, evidence: value });
  }

  updateClaims(claims: ClaimVM[]): void {
    this.updateState({ ...this.stateSubject.value, claims });
  }

  addClaim(claim: ClaimVM): void {
    const claims = [...this.stateSubject.value.claims, claim];
    this.updateState({ ...this.stateSubject.value, claims });
  }

  removeClaim(index: number): void {
    const claims = this.stateSubject.value.claims.filter((_, i) => i !== index);
    this.updateState({ ...this.stateSubject.value, claims });
  }

  updateRelatedCases(relatedCases: RelatedCaseVM[]): void {
    this.updateState({ ...this.stateSubject.value, relatedCases });
  }

  addRelatedCase(relatedCase: RelatedCaseVM): void {
    const relatedCases = [...this.stateSubject.value.relatedCases, relatedCase];
    this.updateState({ ...this.stateSubject.value, relatedCases });
  }

  removeRelatedCase(index: number): void {
    const relatedCases = this.stateSubject.value.relatedCases.filter((_, i) => i !== index);
    this.updateState({ ...this.stateSubject.value, relatedCases });
  }

  updateClassifications(classificationIds: number[]): void {
    this.updateState({ ...this.stateSubject.value, classificationIds });
  }

  updateContactInfo(primary: string, secondary: string, email: string): void {
    this.updateState({
      ...this.stateSubject.value,
      primaryMobile: primary,
      secondaryMobile: secondary,
      email
    });
  }

  // Internal update method
  private updateState(newState: CaseDataState): void {
    this.stateSubject.next(newState);
    // Optional: Save to localStorage for backup
    this.saveToLocalStorage(newState);
  }

  // Load from localStorage (for browser refresh scenarios)
  private loadFromLocalStorage(): void {
    const stored = localStorage.getItem('caseDataState');
    if (stored) {
      try {
        const state = JSON.parse(stored);
        this.stateSubject.next(state);
      } catch (e) {
        console.error('Failed to load state from localStorage:', e);
      }
    }
  }

  // Save to localStorage (for browser refresh scenarios)
  private saveToLocalStorage(state: CaseDataState): void {
    try {
      localStorage.setItem('caseDataState', JSON.stringify(state));
    } catch (e) {
      console.error('Failed to save state to localStorage:', e);
    }
  }

  // Reset state
  resetState(): void {
    this.stateSubject.next(this.initialState);
    localStorage.removeItem('caseDataState');
  }

  // Load existing request data from backend
  loadFromRequest(request: any): void {
    const state: CaseDataState = {
      subject: request.subject || '',
      evidence: request.evidence || '',
      claims: request.claims || [],
      relatedCases: request.relatedCases || [],
      classificationIds: request.classificationIds || [],
      primaryMobile: request.primaryMobile || '',
      secondaryMobile: request.secondaryMobile || '',
      email: request.email || ''
    };
    this.stateSubject.next(state);
    this.saveToLocalStorage(state);
  }
}
```

---

## PHASE 3: Frontend - Models & Services

### 3.1 Create TypeScript Models

**File**: `src/Frontend/bog-app/src/app/features/case-registration/models/claim.model.ts`
```typescript
export interface ClaimVM {
  id: number;
  caseRegistrationRequestId: number;
  claimText: string;
  createdDate: Date;
  modifiedDate: Date;
}

export interface ClaimDTO {
  claimText: string;
}

export interface ClaimsBatchUpdateDTO {
  claims: ClaimDTO[];
}
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/models/related-case.model.ts`
```typescript
export interface RelatedCaseVM {
  id: number;
  caseRegistrationRequestId: number;
  courtId?: number;
  courtName?: string; // For display
  caseNumber: number;
  caseYear: number;
  createdDate: Date;
  modifiedDate: Date;
}

export interface RelatedCaseDTO {
  courtId?: number;
  caseNumber: number;
  caseYear: number;
}

export interface RelatedCasesBatchUpdateDTO {
  relatedCases: RelatedCaseDTO[];
}

// For Court lookup dropdown
export interface CourtLookup {
  id: number;
  nameAr: string;
  name: string;
}
```

### 3.2 Create Lookups API Service

**File**: `src/Frontend/bog-app/src/app/features/case-registration/services/lookups-api.service.ts`
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface CourtLookup {
  id: number;
  nameAr: string;
  name: string;
  regionId: number;
  cityId: number;
}

@Injectable({
  providedIn: 'root'
})
export class LookupsApiService {
  private baseUrl = `${environment.apiUrl}/lookups`;

  constructor(private http: HttpClient) {}

  /**
   * Get all active courts
   */
  getCourts(): Observable<CourtLookup[]> {
    return this.http.get<CourtLookup[]>(`${this.baseUrl}/courts`);
  }
}
```

**Note**: The `/api/lookups/courts` endpoint should be created in the backend LookupsController (see PHASE 2 section).

### 3.2 Create CaseDataStateService (Client-side State Management)

**File**: `src/Frontend/bog-app/src/app/features/case-registration/services/case-data-state.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ClaimVM } from '../models/claim.model';
import { RelatedCaseVM } from '../models/related-case.model';

interface ContactInfo {
  primaryMobile?: string;
  secondaryMobile?: string;
  email?: string;
}

interface CaseDataState {
  subject: string;
  evidence: string;
  claims: ClaimVM[];
  relatedCases: RelatedCaseVM[];
  classificationIds: number[];
  primaryMobile: string;
  secondaryMobile: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class CaseDataStateService {
  private initialState: CaseDataState = {
    subject: '',
    evidence: '',
    claims: [],
    relatedCases: [],
    classificationIds: [],
    primaryMobile: '',
    secondaryMobile: '',
    email: ''
  };

  private stateSubject = new BehaviorSubject<CaseDataState>(this.initialState);
  public state$: Observable<CaseDataState> = this.stateSubject.asObservable();

  constructor() {
    // Load from localStorage on service initialization (optional backup)
    this.loadFromLocalStorage();
  }

  /**
   * Update subject field and save to state + localStorage
   */
  updateSubject(value: string): void {
    this.updateState({ ...this.stateSubject.value, subject: value });
  }

  /**
   * Update evidence field and save to state + localStorage
   */
  updateEvidence(value: string): void {
    this.updateState({ ...this.stateSubject.value, evidence: value });
  }

  /**
   * Update claims array and save to state + localStorage
   */
  updateClaims(claims: ClaimVM[]): void {
    this.updateState({ ...this.stateSubject.value, claims });
  }

  /**
   * Update related cases array and save to state + localStorage
   */
  updateRelatedCases(relatedCases: RelatedCaseVM[]): void {
    this.updateState({ ...this.stateSubject.value, relatedCases });
  }

  /**
   * Update classification IDs array and save to state + localStorage
   */
  updateClassifications(classificationIds: number[]): void {
    this.updateState({ ...this.stateSubject.value, classificationIds });
  }

  /**
   * Update contact information and save to state + localStorage
   */
  updateContactInfo(primaryMobile: string, secondaryMobile: string, email: string): void {
    this.updateState({
      ...this.stateSubject.value,
      primaryMobile: primaryMobile || '',
      secondaryMobile: secondaryMobile || '',
      email: email || ''
    });
  }

  /**
   * Get current subject value
   */
  getSubject(): string {
    return this.stateSubject.value.subject;
  }

  /**
   * Get current evidence value
   */
  getEvidence(): string {
    return this.stateSubject.value.evidence;
  }

  /**
   * Get current claims array
   */
  getClaims(): ClaimVM[] {
    return this.stateSubject.value.claims;
  }

  /**
   * Get current related cases array
   */
  getRelatedCases(): RelatedCaseVM[] {
    return this.stateSubject.value.relatedCases;
  }

  /**
   * Get current classification IDs array
   */
  getClassificationIds(): number[] {
    return this.stateSubject.value.classificationIds;
  }

  /**
   * Get contact information
   */
  getContactInfo(): ContactInfo {
    const state = this.stateSubject.value;
    return {
      primaryMobile: state.primaryMobile,
      secondaryMobile: state.secondaryMobile,
      email: state.email
    };
  }

  /**
   * Get all data as a single object (for saving to database)
   */
  getAllData(): CaseDataState {
    return { ...this.stateSubject.value };
  }

  /**
   * Load existing request data from backend response
   */
  loadFromRequest(request: any): void {
    const newState: CaseDataState = {
      subject: request.subject || '',
      evidence: request.evidence || '',
      claims: request.claims || [],
      relatedCases: request.relatedCases || [],
      classificationIds: request.classificationIds || [],
      primaryMobile: request.primaryMobile || '',
      secondaryMobile: request.secondaryMobile || '',
      email: request.email || ''
    };
    this.updateState(newState);
  }

  /**
   * Reset state to initial values
   */
  resetState(): void {
    this.updateState({ ...this.initialState });
  }

  /**
   * Internal method to update state and save to localStorage
   */
  private updateState(newState: CaseDataState): void {
    this.stateSubject.next(newState);
    this.saveToLocalStorage(newState);
  }

  /**
   * Save state to localStorage as backup (optional)
   */
  private saveToLocalStorage(state: CaseDataState): void {
    try {
      localStorage.setItem('case-data-state', JSON.stringify(state));
    } catch (error) {
      console.warn('Failed to save state to localStorage:', error);
    }
  }

  /**
   * Load state from localStorage (optional backup recovery)
   */
  private loadFromLocalStorage(): void {
    try {
      const saved = localStorage.getItem('case-data-state');
      if (saved) {
        const state = JSON.parse(saved) as CaseDataState;
        this.stateSubject.next(state);
      }
    } catch (error) {
      console.warn('Failed to load state from localStorage:', error);
    }
  }
}
```

### 3.3 Create API Services

**File**: `src/Frontend/bog-app/src/app/features/case-registration/services/claim-api.service.ts`
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ClaimVM, ClaimsBatchUpdateDTO } from '../models/claim.model';

@Injectable()
export class ClaimApiService {
  private baseUrl = `${environment.apiUrl}/case-requests`;

  constructor(private http: HttpClient) {}

  getClaims(requestId: number): Observable<ClaimVM[]> {
    return this.http.get<ClaimVM[]>(`${this.baseUrl}/${requestId}/claims`);
  }

  updateClaims(requestId: number, dto: ClaimsBatchUpdateDTO): Observable<ClaimVM[]> {
    return this.http.put<ClaimVM[]>(`${this.baseUrl}/${requestId}/claims`, dto);
  }
}
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/services/related-case-api.service.ts`
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RelatedCaseVM, RelatedCasesBatchUpdateDTO } from '../models/related-case.model';

@Injectable()
export class RelatedCaseApiService {
  private baseUrl = `${environment.apiUrl}/case-requests`;

  constructor(private http: HttpClient) {}

  getRelatedCases(requestId: number): Observable<RelatedCaseVM[]> {
    return this.http.get<RelatedCaseVM[]>(`${this.baseUrl}/${requestId}/related-cases`);
  }

  updateRelatedCases(requestId: number, dto: RelatedCasesBatchUpdateDTO): Observable<RelatedCaseVM[]> {
    return this.http.put<RelatedCaseVM[]>(`${this.baseUrl}/${requestId}/related-cases`, dto);
  }
}
```

---

## PHASE 4: Frontend - Components with Vertical Tabs Design

### Component Structure
```
components/case-data/
├── case-data-container/
│   ├── case-data-container.component.ts (NEW - parent with vertical tabs)
│   ├── case-data-container.component.html
│   └── case-data-container.component.scss
├── subject-evidence/
│   ├── subject-evidence-form.component.ts (NEW - rich text editors)
│   ├── subject-evidence-form.component.html
│   └── subject-evidence-form.component.scss
├── claims-list/
│   ├── claims-list.component.ts (NEW - table with drag-drop)
│   ├── claims-list.component.html
│   ├── claims-list.component.scss
│   └── claim-form-dialog.component.ts (NEW - dialog)
├── related-cases-list/
│   ├── related-cases-list.component.ts (NEW - table with inline form)
│   ├── related-cases-list.component.html
│   ├── related-cases-list.component.scss
│   └── related-case-form-dialog.component.ts (NEW - dialog)
└── classifications-tab/
    ├── classifications-tab.component.ts (NEW - Tab 4 wrapper)
    ├── classifications-tab.component.html
    ├── classifications-tab.component.scss
    └── classification-selection-dialog/
        ├── classification-selection-dialog.component.ts (NEW - Dialog)
        ├── classification-selection-dialog.component.html
        └── classification-selection-dialog.component.scss
```

### 4.1 Case Data Container (Vertical Tabs Layout)

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.ts`

```typescript
import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CaseRegistrationApiService } from '../../services/case-registration-api.service';

@Component({
  selector: 'app-case-data-container',
  templateUrl: './case-data-container.component.html',
  styleUrls: ['./case-data-container.component.scss']
})
export class CaseDataContainerComponent implements OnInit {
  @Input() requestId!: number;
  @Input() canEdit = false;

  request: any; // CaseRegistrationRequest data for contact info display
  isSaving = false;
  saveSuccess = false;

  constructor(
    private caseRegistrationApi: CaseRegistrationApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.requestId) {
      console.error('requestId is required for CaseDataContainerComponent');
      return;
    }

    // Load request data for contact info display
    this.caseRegistrationApi.getRequest(this.requestId).subscribe({
      next: (request) => {
        this.request = request;
      },
      error: (err) => {
        console.error('Error loading request data:', err);
      }
    });
  }

  /**
   * Navigate back to requests list
   * Shows confirmation if there are unsaved changes
   */
  goBack(): void {
    // Check if there are unsaved changes
    const hasUnsavedChanges = this.checkForUnsavedChanges();

    if (hasUnsavedChanges && this.canEdit) {
      const confirmLeave = confirm(
        'هل تريد العودة إلى قائمة الطلبات؟ سيتم فقدان أي تغييرات غير محفوظة.'
      );

      if (!confirmLeave) {
        return;
      }
    }

    // Navigate to requests list
    this.router.navigate(['/case-registration/requests']);
  }

  /**
   * Check if there are unsaved changes
   * Compare current state with last saved state
   */
  private checkForUnsavedChanges(): boolean {
    // TODO: Implement logic to detect unsaved changes
    // This could compare current state with last saved state
    // For now, return false to allow navigation without warning
    return false;
  }

  saveAllData(): void {
    // TODO: Implement save logic that aggregates all state data
    // This method should be called from all tab components
  }
}
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.html`

```html
<app-section-container title="بيانات الدعوى" sectionId="case-data" icon="description">
  <!-- FIXED ACTION BAR - Always visible across all tabs -->
  <div class="fixed-action-bar">
    <!-- Back Button -->
    <button
      mat-raised-button
      class="secondary-button"
      (click)="goBack()"
      [disabled]="isSaving">
      <mat-icon>arrow_forward</mat-icon>
      عودة
    </button>

    <!-- Save Draft Button -->
    <button
      mat-raised-button
      color="primary"
      (click)="saveAllData()"
      [disabled]="!canEdit || isSaving">
      <mat-icon>save</mat-icon>
      حفظ كمسودة
    </button>
    <span *ngIf="isSaving" class="save-indicator">
      <mat-spinner diameter="20"></mat-spinner>
      <span class="saving-text">جاري الحفظ...</span>
    </span>
    <span *ngIf="saveSuccess" class="success-indicator">
      <mat-icon>check_circle</mat-icon>
      <span>تم الحفظ بنجاح</span>
    </span>
  </div>

  <div class="case-data-layout">
    <!-- Content Area (Left in RTL) -->
    <div class="content-area">
      <mat-tab-group class="vertical-tabs" [selectedIndex]="0">
        <!-- Tab 1: موضوع وأسانيد الدعوى -->
        <mat-tab>
          <ng-template mat-tab-label>
            <mat-icon>description</mat-icon>
            <span>موضوع وأسانيد الدعوى</span>
          </ng-template>
          <app-subject-evidence-form
            [requestId]="requestId"
            [canEdit]="canEdit">
          </app-subject-evidence-form>
        </mat-tab>

        <!-- Tab 2: طلبات الدعوى -->
        <mat-tab>
          <ng-template mat-tab-label>
            <mat-icon>list_alt</mat-icon>
            <span>طلبات الدعوى</span>
          </ng-template>
          <app-claims-list
            [requestId]="requestId"
            [canEdit]="canEdit">
          </app-claims-list>
        </mat-tab>

        <!-- Tab 3: الدعاوى المرتبطة -->
        <mat-tab>
          <ng-template mat-tab-label>
            <mat-icon>link</mat-icon>
            <span>الدعاوى المرتبطة</span>
          </ng-template>
          <app-related-cases-list
            [requestId]="requestId"
            [canEdit]="canEdit">
          </app-related-cases-list>
        </mat-tab>

        <!-- Tab 4: تصنيف الدعوى -->
        <mat-tab>
          <ng-template mat-tab-label>
            <mat-icon>category</mat-icon>
            <span>تصنيف الدعوى</span>
          </ng-template>
          <app-classifications-tab
            [requestId]="requestId"
            [canEdit]="canEdit">
          </app-classifications-tab>
        </mat-tab>

        <!-- Tab 5: بيانات التواصل -->
        <mat-tab>
          <ng-template mat-tab-label>
            <mat-icon>contact_mail</mat-icon>
            <span>بيانات التواصل</span>
          </ng-template>
          <div class="contact-info-display">
            <h3>بيانات التواصل</h3>

            <div class="info-field">
              <label>رقم الجوال الأساسي:</label>
              <p>{{ request?.primaryMobile || '-' }}</p>
            </div>

            <div class="info-field">
              <label>رقم الجوال الثانوي:</label>
              <p>{{ request?.secondaryMobile || '-' }}</p>
            </div>

            <div class="info-field">
              <label>البريد الإلكتروني:</label>
              <p>{{ request?.email || '-' }}</p>
            </div>
          </div>
        </mat-tab>

        <!-- Tab 6: المرفقات -->
        <mat-tab>
          <ng-template mat-tab-label>
            <mat-icon>attach_file</mat-icon>
            <span>المرفقات</span>
          </ng-template>
          <app-attachments-list
            [requestId]="requestId"
            [canEdit]="canEdit">
          </app-attachments-list>
        </mat-tab>
      </mat-tab-group>
    </div>
  </div>
</app-section-container>
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.scss`

```scss
.case-data-layout {
  display: flex;
  min-height: 600px;
}

.content-area {
  flex: 1;
  width: 100%;
}

// Vertical Tabs Styling (matching HTML mockup)
::ng-deep .vertical-tabs {
  display: flex;
  flex-direction: row-reverse; // Tabs on right in RTL

  // Tab navigation container
  .mat-tab-header {
    flex-direction: column;
    border-left: 1px solid #e0e0e0;
    background: #f8f9fa;
    min-width: 280px;
    max-width: 280px;
    padding: 16px 0;
  }

  .mat-tab-labels {
    flex-direction: column;
    width: 100%;
  }

  .mat-tab-label {
    padding: 14px 20px;
    height: auto;
    min-height: 48px;
    justify-content: flex-end;
    text-align: right;
    border-radius: 4px;
    margin: 2px 8px;
    opacity: 1;
    color: #555;
    font-weight: 400;
    position: relative;

    mat-icon {
      margin-left: 12px;
      margin-right: 0;
      color: #777;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    span {
      font-size: 14px;
    }

    &:hover {
      background: #e8f4f8;
      color: #2c7a7b;

      mat-icon {
        color: #2c7a7b;
      }
    }

    &.mat-tab-label-active {
      color: #2c7a7b;
      font-weight: 600;
      background: #d1ecf1;

      mat-icon {
        color: #2c7a7b;
      }

      // Left border indicator
      &::before {
        content: '';
        position: absolute;
        right: 0;
        top: 50%;
        transform: translateY(-50%);
        width: 4px;
        height: 70%;
        background: #2c7a7b;
        border-radius: 2px 0 0 2px;
      }
    }
  }

  // Tab content area
  .mat-tab-body-wrapper {
    flex: 1;
    padding: 24px;
  }

  .mat-ink-bar {
    display: none; // Hide default indicator
  }
}

.fixed-action-bar {
  position: sticky;
  top: 0;
  z-index: 100;
  background: white;
  padding: 16px 24px;
  border-bottom: 1px solid #e0e0e0;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

  button {
    display: flex;
    align-items: center;
    gap: 8px;

    mat-icon {
      margin-left: 8px;
      margin-right: 0; // RTL adjustment
    }

    // Secondary button styling (Back button)
    &.secondary-button {
      background: white;
      color: #1976d2;
      border: 1px solid #1976d2;

      &:hover:not(:disabled) {
        background: #f5f5f5;
        border-color: #1565c0;
        color: #1565c0;
      }
    }
  }

  .save-indicator {
    display: flex;
    align-items: center;
    gap: 8px;
    color: #1976d2;
    font-size: 14px;

    mat-spinner {
      margin: 0;
    }

    .saving-text {
      margin-right: 8px; // RTL adjustment
    }
  }

  .success-indicator {
    display: flex;
    align-items: center;
    gap: 8px;
    color: #4caf50;
    font-size: 14px;
    font-weight: 500;

    mat-icon {
      color: #4caf50;
    }

    span {
      margin-right: 8px; // RTL adjustment
    }
  }
}

.contact-info-display {
  padding: 24px;

  h3 {
    margin-bottom: 24px;
    font-weight: 600;
    color: #333;
  }

  .info-field {
    margin-bottom: 20px;
    padding: 16px;
    background: #f8f9fa;
    border-radius: 4px;

    label {
      display: block;
      font-weight: 600;
      color: #666;
      margin-bottom: 8px;
      font-size: 14px;
    }

    p {
      margin: 0;
      font-size: 16px;
      color: #333;
      direction: rtl;
    }
  }
}
```

### 4.2 موضوع وأسانيد الدعوى Form (Rich Text Editors)

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/subject-evidence/subject-evidence-form.component.ts`

```typescript
import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Editor, Toolbar } from 'ngx-editor';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CaseRegistrationApiService } from '../../../services/case-registration-api.service';

@Component({
  selector: 'app-subject-evidence-form',
  templateUrl: './subject-evidence-form.component.html',
  styleUrls: ['./subject-evidence-form.component.scss']
})
export class SubjectEvidenceFormComponent implements OnInit, OnDestroy {
  @Input() requestId!: number;
  @Input() canEdit = false;

  form!: FormGroup;
  subjectEditor!: Editor;
  evidenceEditor!: Editor;
  isSaving = false;

  toolbar: Toolbar = [
    ['bold', 'italic', 'underline'],
    ['ordered_list', 'bullet_list'],
    ['text_color', 'background_color'],
    ['align_left', 'align_center', 'align_right', 'align_justify']
  ];

  constructor(
    private fb: FormBuilder,
    private caseDataState: CaseDataStateService
  ) {}

  ngOnInit(): void {
    // Initialize editors
    this.subjectEditor = new Editor({
      attributes: { dir: 'rtl', lang: 'ar' }
    });
    this.evidenceEditor = new Editor({
      attributes: { dir: 'rtl', lang: 'ar' }
    });

    // Initialize form
    this.form = this.fb.group({
      subject: ['', [Validators.required, Validators.maxLength(4000)]],
      evidence: ['', [Validators.required, Validators.maxLength(4000)]]
    });

    // Load existing data from state service
    this.loadData();

    // Subscribe to form changes and update state service (NO database save)
    if (this.canEdit) {
      this.form.valueChanges
        .pipe(distinctUntilChanged())
        .subscribe((values) => {
          // Update client-side state ONLY - NO database save
          this.caseDataState.updateSubject(values.subject);
          this.caseDataState.updateEvidence(values.evidence);
        });
    } else {
      this.form.disable();
    }
  }

  ngOnDestroy(): void {
    this.subjectEditor.destroy();
    this.evidenceEditor.destroy();
  }

  loadData(): void {
    const subject = this.caseDataState.getSubject();
    const evidence = this.caseDataState.getEvidence();

    this.form.patchValue({
      subject: subject,
      evidence: evidence
    }, { emitEvent: false });
  }

  getCharCount(fieldName: string): number {
    const html = this.form.get(fieldName)?.value || '';
    return this.stripHtml(html).length;
  }

  private stripHtml(html: string): string {
    const tmp = document.createElement('DIV');
    tmp.innerHTML = html;
    return tmp.textContent || tmp.innerText || '';
  }
}
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/subject-evidence/subject-evidence-form.component.html`

```html
<div class="subject-evidence-container">
  <form [formGroup]="form">
    <!-- Subject Editor -->
    <div class="editor-wrapper">
      <label class="editor-label">
        موضوع الدعوى <span class="required">*</span>
      </label>
      <div class="ngx-editor-container">
        <ngx-editor-menu [editor]="subjectEditor" [toolbar]="toolbar"></ngx-editor-menu>
        <ngx-editor
          [editor]="subjectEditor"
          formControlName="subject"
          placeholder="أدخل موضوع الدعوى بالتفصيل...">
        </ngx-editor>
      </div>
      <div class="char-counter">{{ getCharCount('subject') }}/4000</div>
      <mat-error *ngIf="form.get('subject')?.hasError('required') && form.get('subject')?.touched">
        موضوع الدعوى مطلوب (ERR006)
      </mat-error>
      <mat-error *ngIf="form.get('subject')?.hasError('maxlength')">
        تجاوز الحد الأقصى للحروف (4000 حرف)
      </mat-error>
    </div>

    <!-- Evidence Editor -->
    <div class="editor-wrapper">
      <label class="editor-label">
        أسانيد الدعوى <span class="required">*</span>
      </label>
      <div class="ngx-editor-container">
        <ngx-editor-menu [editor]="evidenceEditor" [toolbar]="toolbar"></ngx-editor-menu>
        <ngx-editor
          [editor]="evidenceEditor"
          formControlName="evidence"
          placeholder="أدخل الأسانيد القانونية للدعوى...">
        </ngx-editor>
      </div>
      <div class="char-counter">{{ getCharCount('evidence') }}/4000</div>
      <mat-error *ngIf="form.get('evidence')?.hasError('required') && form.get('evidence')?.touched">
        أسانيد الدعوى مطلوبة (ERR007)
      </mat-error>
      <mat-error *ngIf="form.get('evidence')?.hasError('maxlength')">
        تجاوز الحد الأقصى للحروف (4000 حرف)
      </mat-error>
    </div>
  </form>
</div>
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/subject-evidence/subject-evidence-form.component.scss`

```scss
.subject-evidence-container {
  padding: 16px 0;
}

.editor-wrapper {
  margin-bottom: 32px;
}

.editor-label {
  display: block;
  font-weight: 600;
  margin-bottom: 8px;
  color: #333;
  font-size: 14px;

  .required {
    color: #f44336;
  }
}

.ngx-editor-container {
  border: 1px solid #ccc;
  border-radius: 4px;
  overflow: hidden;

  ::ng-deep .NgxEditor {
    min-height: 200px;
    padding: 12px;

    &__Content {
      min-height: 200px;
      direction: rtl;
      text-align: right;
    }
  }

  ::ng-deep .NgxEditor__MenuBar {
    background: #f9f9f9;
    border-bottom: 1px solid #ccc;
    padding: 8px;
  }
}

.char-counter {
  text-align: left;
  font-size: 12px;
  color: #666;
  margin-top: 4px;
}

mat-error {
  font-size: 12px;
  margin-top: 4px;
}
```

### 4.3 Claims List Component (Simple Table)

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/claims-list/claims-list.component.ts`

```typescript
import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ClaimApiService } from '../../../services/claim-api.service';
import { ClaimVM } from '../../../models/claim.model';
import { ClaimFormDialogComponent } from './claim-form-dialog.component';

@Component({
  selector: 'app-claims-list',
  templateUrl: './claims-list.component.html',
  styleUrls: ['./claims-list.component.scss']
})
export class ClaimsListComponent implements OnInit {
  @Input() requestId!: number;
  @Input() canEdit = false;

  claims: ClaimVM[] = [];
  displayedColumns = ['index', 'claimText', 'actions'];

  constructor(
    private caseDataState: CaseDataStateService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadClaimsFromState();
  }

  loadClaimsFromState(): void {
    this.claims = this.caseDataState.getClaims();
  }

  openAddClaim(): void {
    const dialogRef = this.dialog.open(ClaimFormDialogComponent, {
      width: '600px',
      direction: 'rtl',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.claims.push({ ...result, id: 0, caseRegistrationRequestId: this.requestId });
        // Update state service ONLY - NO database save
        this.caseDataState.updateClaims(this.claims);
      }
    });
  }

  editClaim(claim: ClaimVM, index: number): void {
    const dialogRef = this.dialog.open(ClaimFormDialogComponent, {
      width: '600px',
      direction: 'rtl',
      data: { mode: 'edit', claim: { ...claim } }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.claims[index] = { ...claim, claimText: result.claimText };
        // Update state service ONLY - NO database save
        this.caseDataState.updateClaims(this.claims);
      }
    });
  }

  deleteClaim(index: number): void {
    if (confirm('هل أنت متأكد من حذف هذا الطلب؟')) {
      this.claims.splice(index, 1);
      // Update state service ONLY - NO database save
      this.caseDataState.updateClaims(this.claims);
    }
  }
}
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/claims-list/claims-list.component.html`

```html
<div class="claims-container">
  <!-- Header with Add Button (Gold color) -->
  <div class="list-header">
    <h3>قائمة الطلبات</h3>
    <button
      mat-raised-button
      class="add-btn-gold"
      (click)="openAddClaim()"
      [disabled]="!canEdit">
      <mat-icon>add</mat-icon>
      إضافة طلب
    </button>
  </div>

  <!-- Loading Spinner -->
  <div *ngIf="isLoading" class="loading-container">
    <mat-spinner diameter="40"></mat-spinner>
  </div>

  <!-- Claims Table -->
  <div *ngIf="!isLoading && claims.length > 0" class="table-container">
    <table mat-table [dataSource]="claims">
      <!-- Index Column -->
      <ng-container matColumnDef="index">
        <th mat-header-cell *matHeaderCellDef style="width: 150px; text-align: center;">رقم الطلب</th>
        <td mat-cell *matCellDef="let claim; let i = index" style="text-align: center; font-weight: 600;">
          {{ i + 1 }}
        </td>
      </ng-container>

      <!-- Claim Text Column -->
      <ng-container matColumnDef="claimText">
        <th mat-header-cell *matHeaderCellDef>تفاصيل الطلب</th>
        <td mat-cell *matCellDef="let claim">{{ claim.claimText }}</td>
      </ng-container>

      <!-- Actions Column -->
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef style="width: 80px;"></th>
        <td mat-cell *matCellDef="let claim; let i = index" style="text-align: center;">
          <button mat-icon-button [matMenuTriggerFor]="menu" *ngIf="canEdit">
            <mat-icon>more_vert</mat-icon>
          </button>
          <mat-menu #menu="matMenu">
            <button mat-menu-item (click)="editClaim(claim, i)">
              <mat-icon>edit</mat-icon>
              <span>تعديل</span>
            </button>
            <button mat-menu-item (click)="deleteClaim(i)">
              <mat-icon>delete</mat-icon>
              <span>حذف</span>
            </button>
          </mat-menu>
        </td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    </table>
  </div>

  <!-- Empty State -->
  <div *ngIf="!isLoading && claims.length === 0" class="empty-state">
    <mat-icon>inbox</mat-icon>
    <p>لا توجد طلبات مضافة</p>
  </div>
</div>
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/claims-list/claims-list.component.scss`

```scss
.claims-container {
  padding: 16px 0;
}

.list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;

  h3 {
    margin: 0;
    font-weight: 600;
  }
}

// Gold button styling (matching HTML mockup)
.add-btn-gold {
  background-color: #d4a017;
  color: white;
  font-weight: 500;

  &:hover:not([disabled]) {
    background-color: #b8890f;
  }

  mat-icon {
    margin-left: 8px;
  }
}

.loading-container {
  display: flex;
  justify-content: center;
  padding: 40px;
}

.table-container {
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;

  table {
    width: 100%;
  }
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #999;

  mat-icon {
    font-size: 64px;
    width: 64px;
    height: 64px;
    color: #ccc;
    margin-bottom: 16px;
  }

  p {
    font-size: 16px;
  }
}
```

### 4.4 Claim Form Dialog

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/claims-list/claim-form-dialog.component.ts`

```typescript
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-claim-form-dialog',
  template: `
    <h2 mat-dialog-title>{{ data.mode === 'create' ? 'إضافة طلب' : 'تعديل طلب' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>نص الطلب *</mat-label>
          <textarea
            matInput
            formControlName="claimText"
            rows="5"
            maxlength="2000"
            placeholder="أدخل نص الطلب..."></textarea>
          <mat-hint align="end">{{ charCount }}/2000</mat-hint>
          <mat-error *ngIf="form.get('claimText')?.hasError('required')">
            نص الطلب مطلوب
          </mat-error>
          <mat-error *ngIf="form.get('claimText')?.hasError('maxlength')">
            تجاوز الحد الأقصى للحروف (2000 حرف)
          </mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancel()">إلغاء</button>
      <button
        mat-raised-button
        color="primary"
        (click)="save()"
        [disabled]="form.invalid">
        {{ data.mode === 'create' ? 'إضافة' : 'حفظ' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
    }
    mat-dialog-content {
      padding: 20px 24px;
    }
  `]
})
export class ClaimFormDialogComponent implements OnInit {
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<ClaimFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      claimText: [
        this.data.claim?.claimText || '',
        [Validators.required, Validators.maxLength(2000)]
      ]
    });
  }

  get charCount(): number {
    return this.form.get('claimText')?.value?.length || 0;
  }

  save(): void {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
```

### 4.5 Related Cases List Component

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/related-cases-list/related-cases-list.component.ts`

```typescript
import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RelatedCaseApiService } from '../../../services/related-case-api.service';
import { RelatedCaseVM } from '../../../models/related-case.model';
import { RelatedCaseFormDialogComponent } from './related-case-form-dialog.component';

@Component({
  selector: 'app-related-cases-list',
  templateUrl: './related-cases-list.component.html',
  styleUrls: ['./related-cases-list.component.scss']
})
export class RelatedCasesListComponent implements OnInit {
  @Input() requestId!: number;
  @Input() canEdit = false;

  relatedCases: RelatedCaseVM[] = [];
  displayedColumns = ['courtName', 'caseNumber', 'caseYear', 'actions'];

  constructor(
    private caseDataState: CaseDataStateService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadRelatedCasesFromState();
  }

  loadRelatedCasesFromState(): void {
    this.relatedCases = this.caseDataState.getRelatedCases();
  }

  openAddRelatedCase(): void {
    const dialogRef = this.dialog.open(RelatedCaseFormDialogComponent, {
      width: '600px',
      direction: 'rtl',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.relatedCases.push({ ...result, id: 0, caseRegistrationRequestId: this.requestId });
        // Update state service ONLY - NO database save
        this.caseDataState.updateRelatedCases(this.relatedCases);
      }
    });
  }

  editRelatedCase(relatedCase: RelatedCaseVM, index: number): void {
    const dialogRef = this.dialog.open(RelatedCaseFormDialogComponent, {
      width: '600px',
      direction: 'rtl',
      data: { mode: 'edit', relatedCase: { ...relatedCase } }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.relatedCases[index] = { ...relatedCase, ...result };
        // Update state service ONLY - NO database save
        this.caseDataState.updateRelatedCases(this.relatedCases);
      }
    });
  }

  deleteRelatedCase(index: number): void {
    if (confirm('هل أنت متأكد من حذف هذه الدعوى المرتبطة؟')) {
      this.relatedCases.splice(index, 1);
      // Update state service ONLY - NO database save
      this.caseDataState.updateRelatedCases(this.relatedCases);
    }
  }
}
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/related-cases-list/related-cases-list.component.html`

```html
<div class="related-cases-container">
  <!-- Header with Add Button (Gold color) -->
  <div class="list-header">
    <h3>قائمة الدعاوى المرتبطة</h3>
    <button
      mat-raised-button
      class="add-btn-gold"
      (click)="openAddRelatedCase()"
      [disabled]="!canEdit">
      <mat-icon>add</mat-icon>
      إضافة دعوى مرتبطة
    </button>
  </div>

  <!-- Loading Spinner -->
  <div *ngIf="isLoading" class="loading-container">
    <mat-spinner diameter="40"></mat-spinner>
  </div>

  <!-- Related Cases Table -->
  <div *ngIf="!isLoading && relatedCases.length > 0" class="table-container">
    <table mat-table [dataSource]="relatedCases">
      <!-- Court Column -->
      <ng-container matColumnDef="courtName">
        <th mat-header-cell *matHeaderCellDef>المحكمة</th>
        <td mat-cell *matCellDef="let relatedCase">{{ relatedCase.courtName || '-' }}</td>
      </ng-container>

      <!-- Case Number Column -->
      <ng-container matColumnDef="caseNumber">
        <th mat-header-cell *matHeaderCellDef style="width: 200px; text-align: center;">رقم الدعوى</th>
        <td mat-cell *matCellDef="let relatedCase" style="text-align: center; font-weight: 600;">
          {{ relatedCase.caseNumber }}
        </td>
      </ng-container>

      <!-- Case Year Column -->
      <ng-container matColumnDef="caseYear">
        <th mat-header-cell *matHeaderCellDef style="width: 150px; text-align: center;">عام الدعوى</th>
        <td mat-cell *matCellDef="let relatedCase" style="text-align: center;">
          {{ relatedCase.caseYear }}
        </td>
      </ng-container>

      <!-- Actions Column -->
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef style="width: 80px;"></th>
        <td mat-cell *matCellDef="let relatedCase; let i = index" style="text-align: center;">
          <button mat-icon-button [matMenuTriggerFor]="menu" *ngIf="canEdit">
            <mat-icon>more_vert</mat-icon>
          </button>
          <mat-menu #menu="matMenu">
            <button mat-menu-item (click)="editRelatedCase(relatedCase, i)">
              <mat-icon>edit</mat-icon>
              <span>تعديل</span>
            </button>
            <button mat-menu-item (click)="deleteRelatedCase(i)">
              <mat-icon>delete</mat-icon>
              <span>حذف</span>
            </button>
          </mat-menu>
        </td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    </table>
  </div>

  <!-- Empty State -->
  <div *ngIf="!isLoading && relatedCases.length === 0" class="empty-state">
    <mat-icon>folder_open</mat-icon>
    <p>لا يوجد اي سجلات</p>
  </div>
</div>
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/related-cases-list/related-cases-list.component.scss`

```scss
.related-cases-container {
  padding: 16px 0;
}

.list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;

  h3 {
    margin: 0;
    font-weight: 600;
  }
}

.add-btn-gold {
  background-color: #d4a017;
  color: white;
  font-weight: 500;

  &:hover:not([disabled]) {
    background-color: #b8890f;
  }

  mat-icon {
    margin-left: 8px;
  }
}

.loading-container {
  display: flex;
  justify-content: center;
  padding: 40px;
}

.table-container {
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;

  table {
    width: 100%;
  }
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #999;

  mat-icon {
    font-size: 64px;
    width: 64px;
    height: 64px;
    color: #ccc;
    margin-bottom: 16px;
  }

  p {
    font-size: 16px;
  }
}
```

### 4.6 Related Case Form Dialog

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/related-cases-list/related-case-form-dialog.component.ts`

```typescript
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { LookupsApiService } from '../../../services/lookups-api.service';
import { CourtLookup } from '../../../models/related-case.model';

@Component({
  selector: 'app-related-case-form-dialog',
  template: `
    <h2 mat-dialog-title>{{ data.mode === 'create' ? 'إضافة دعوى مرتبطة' : 'تعديل دعوى مرتبطة' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>المحكمة</mat-label>
          <mat-select formControlName="courtId">
            <mat-option [value]="null">اختر المحكمة</mat-option>
            <mat-option *ngFor="let court of courts" [value]="court.id">
              {{ court.nameAr }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>رقم الدعوى *</mat-label>
          <input
            matInput
            type="number"
            formControlName="caseNumber"
            min="1"
            max="99999999999"
            placeholder="أدخل رقم الدعوى المرتبطة">
          <mat-error *ngIf="form.get('caseNumber')?.hasError('required')">
            رقم الدعوى مطلوب
          </mat-error>
          <mat-error *ngIf="form.get('caseNumber')?.hasError('max')">
            رقم الدعوى لا يمكن أن يتجاوز 11 رقم
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>عام الدعوى *</mat-label>
          <input
            matInput
            type="number"
            formControlName="caseYear"
            min="1900"
            max="2100"
            placeholder="2025">
          <mat-error *ngIf="form.get('caseYear')?.hasError('required')">
            عام الدعوى مطلوب
          </mat-error>
          <mat-error *ngIf="form.get('caseYear')?.hasError('min') || form.get('caseYear')?.hasError('max')">
            يجب أن يكون العام بين 1900 و 2100
          </mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancel()">إلغاء</button>
      <button
        mat-raised-button
        color="primary"
        (click)="save()"
        [disabled]="form.invalid">
        {{ data.mode === 'create' ? 'إضافة' : 'حفظ' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    mat-dialog-content {
      padding: 20px 24px;
    }
  `]
})
export class RelatedCaseFormDialogComponent implements OnInit {
  form!: FormGroup;
  courts: CourtLookup[] = [];

  constructor(
    private fb: FormBuilder,
    private lookupsApi: LookupsApiService,
    public dialogRef: MatDialogRef<RelatedCaseFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      courtId: [this.data.relatedCase?.courtId || null],
      caseNumber: [
        this.data.relatedCase?.caseNumber || null,
        [Validators.required, Validators.min(1), Validators.max(99999999999)]
      ],
      caseYear: [
        this.data.relatedCase?.caseYear || new Date().getFullYear(),
        [Validators.required, Validators.min(1900), Validators.max(2100)]
      ]
    });

    // Load courts dropdown
    this.lookupsApi.getCourts().subscribe({
      next: (courts) => {
        this.courts = courts;
      },
      error: (err) => {
        console.error('Error loading courts:', err);
      }
    });
  }

  save(): void {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
```

### 4.7 Classifications Tab Component (Tab 4 - 4-Level Hierarchical)

**Classification Structure**: Each classification has 4 hierarchical levels:
- **التصنيف الأول** (Level 1) - Primary category (e.g., "دعاوى مدنية", "دعاوى تجارية")
- **التصنيف الثاني** (Level 2) - Secondary category (e.g., "دعاوى الملكية", "منازعات الشركات")
- **التصنيف الثالث** (Level 3) - Tertiary category (e.g., "منازعات الأراضي", "نزاعات الشركاء")
- **التصنيف الرابع** (Level 4) - Quaternary category (e.g., "نزاع حدود", "توزيع الأرباح")

**Behavior**:
1. Tab displays button "إضافة تصنيف" at the top
2. Below button: Selected classifications shown in a table with rows (each row has 5 columns: 4 levels + remove button)
3. When "إضافة تصنيف" clicked → Opens `classification-selection-dialog` (see UPDATE section for dialog details)
4. Dialog shows searchable table with 4-level structure + checkbox column
5. User selects multiple classifications, clicks "حفظ الاختيار"
6. Selected items display in a table format with each classification as a row showing all 4 levels
7. Selection/removal updates client-side state only (saved to database when "حفظ كمسودة" button is clicked)

**Note**: This component is a wrapper for Tab 4. The dialog implementation is detailed in the "UPDATE: 4-Level Classification System" section below (lines 3471+).

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/classifications-tab/classifications-tab.component.ts`

```typescript
import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ClassificationApiService, ClassificationVM } from '../../../services/classification-api.service';
import { CaseRegistrationApiService } from '../../../services/case-registration-api.service';
import { ClassificationSelectionDialogComponent } from './classification-selection-dialog/classification-selection-dialog.component';

@Component({
  selector: 'app-classifications-tab',
  templateUrl: './classifications-tab.component.html',
  styleUrls: ['./classifications-tab.component.scss']
})
export class ClassificationsTabComponent implements OnInit {
  @Input() requestId!: number;
  @Input() canEdit = false;

  allClassifications: ClassificationVM[] = [];
  selectedClassifications: ClassificationVM[] = [];

  constructor(
    private classificationApi: ClassificationApiService,
    private caseDataState: CaseDataStateService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.loadClassifications();
  }

  loadClassifications() {
    this.classificationApi.getAll().subscribe({
      next: (classifications) => {
        this.allClassifications = classifications;
        this.loadSelectedClassificationsFromState();
      },
      error: (error) => {
        console.error('Failed to load classifications', error);
      }
    });
  }

  loadSelectedClassificationsFromState() {
    const selectedIds = this.caseDataState.getClassificationIds();
    this.selectedClassifications = this.allClassifications.filter(c => selectedIds.includes(c.id));
  }

  openClassificationDialog() {
    const dialogRef = this.dialog.open(ClassificationSelectionDialogComponent, {
      width: '90%',
      maxWidth: '1000px',
      data: {
        classifications: this.allClassifications,
        selectedIds: this.selectedClassifications.map(c => c.id)
      },
      dir: 'rtl'
    });

    dialogRef.afterClosed().subscribe((result: ClassificationVM[]) => {
      if (result) {
        this.selectedClassifications = result;
        const selectedIds = result.map(c => c.id);
        // Update state service ONLY - NO database save
        this.caseDataState.updateClassifications(selectedIds);
      }
    });
  }

  removeClassification(classification: ClassificationVM) {
    this.selectedClassifications = this.selectedClassifications.filter(c => c.id !== classification.id);
    const updatedIds = this.selectedClassifications.map(c => c.id);
    // Update state service ONLY - NO database save
    this.caseDataState.updateClassifications(updatedIds);
  }
}
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/classifications-tab/classifications-tab.component.html`

```html
<div class="classifications-tab">
  <!-- Add Button -->
  <div class="tab-header">
    <button
      mat-raised-button
      color="primary"
      (click)="openClassificationDialog()"
      [disabled]="!canEdit">
      <mat-icon>add</mat-icon>
      إضافة تصنيف
    </button>
  </div>

  <!-- Selected Classifications Table -->
  <table mat-table [dataSource]="selectedClassifications" *ngIf="selectedClassifications.length > 0">
    <!-- Level 1 Column -->
    <ng-container matColumnDef="level1">
      <th mat-header-cell *matHeaderCellDef>التصنيف الأول</th>
      <td mat-cell *matCellDef="let c">{{ c.level1 }}</td>
    </ng-container>

    <!-- Level 2 Column -->
    <ng-container matColumnDef="level2">
      <th mat-header-cell *matHeaderCellDef>التصنيف الثاني</th>
      <td mat-cell *matCellDef="let c">{{ c.level2 }}</td>
    </ng-container>

    <!-- Level 3 Column -->
    <ng-container matColumnDef="level3">
      <th mat-header-cell *matHeaderCellDef>التصنيف الثالث</th>
      <td mat-cell *matCellDef="let c">{{ c.level3 }}</td>
    </ng-container>

    <!-- Level 4 Column -->
    <ng-container matColumnDef="level4">
      <th mat-header-cell *matHeaderCellDef>التصنيف الرابع</th>
      <td mat-cell *matCellDef="let c">{{ c.level4 }}</td>
    </ng-container>

    <!-- Actions Column -->
    <ng-container matColumnDef="actions">
      <th mat-header-cell *matHeaderCellDef style="width: 80px;">إجراءات</th>
      <td mat-cell *matCellDef="let c">
        <button
          mat-icon-button
          (click)="removeClassification(c)"
          [disabled]="!canEdit"
          matTooltip="حذف التصنيف">
          <mat-icon>close</mat-icon>
        </button>
      </td>
    </ng-container>

    <tr mat-header-row *matHeaderRowDef="['level1', 'level2', 'level3', 'level4', 'actions']"></tr>
    <tr mat-row *matRowDef="let row; columns: ['level1', 'level2', 'level3', 'level4', 'actions'];"></tr>
  </table>

  <!-- Empty State -->
  <div *ngIf="selectedClassifications.length === 0" class="empty-state">
    <mat-icon>category</mat-icon>
    <p>لم يتم اختيار أي تصنيف</p>
  </div>
</div>
```

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/classifications-tab/classifications-tab.component.scss`

```scss
.classifications-tab {
  padding: 24px;

  .tab-header {
    margin-bottom: 20px;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    background-color: white;
    border: 1px solid #e0e0e0;
    border-radius: 4px;
    overflow: hidden;

    thead {
      background-color: #f5f5f5;

      th {
        padding: 12px;
        text-align: right;
        border-bottom: 2px solid #e0e0e0;
        color: #666;
        font-weight: 600;
        font-size: 13px;
      }
    }

    tbody {
      tr {
        border-bottom: 1px solid #e0e0e0;

        &:hover {
          background-color: #f9f9f9;
        }

        &:last-child {
          border-bottom: none;
        }

        td {
          padding: 12px;
          text-align: right;
          color: #333;
          font-size: 14px;
          word-break: break-word;
        }
      }
    }
  }

  .empty-state {
    text-align: center;
    padding: 40px 20px;
    color: #999;

    mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      opacity: 0.5;
      display: block;
      margin: 0 auto 8px;
    }

    p {
      margin: 8px 0 0;
    }
  }
}
```

### 4.8 Contact Information Component (Tab 5 - User-Editable Form)

**Note**: Tab 5 "بيانات التواصل" (Contact Information) is **user-editable**. Users can enter phone numbers and email. Updates are saved to client-side state only until "حفظ كمسودة" button is clicked.

**Includes**: Contact information fields (all optional with validation):
- **Primary Mobile**: Max 10 digits, must start with "05" (e.g., 0512345678)
- **Secondary Mobile**: Max 10 digits, must start with "05" (e.g., 0512345678)
- **Email**: Valid email format (e.g., user@example.com)

**Location**: Shown in `case-data-container.component.html` section 4.1:
```html
<!-- Tab 5: بيانات التواصل -->
<mat-tab>
  <ng-template mat-tab-label>
    <mat-icon>contact_mail</mat-icon>
    <span>بيانات التواصل</span>
  </ng-template>
  <form [formGroup]="contactForm">
    <div class="contact-info-form">
      <div class="contact-header">
        <h3>بيانات التواصل</h3>
      </div>

      <!-- Primary Mobile -->
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>رقم الجوال الأساسي</mat-label>
        <input matInput formControlName="primaryMobile" maxlength="10" [disabled]="!canEdit">
        <mat-error *ngIf="contactForm.get('primaryMobile')?.hasError('pattern')">
          رقم الجوال يجب أن يبدأ بـ 05 ويكون 10 أرقام
        </mat-error>
      </mat-form-field>

      <!-- Secondary Mobile -->
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>رقم الجوال الثانوي</mat-label>
        <input matInput formControlName="secondaryMobile" maxlength="10" [disabled]="!canEdit">
        <mat-error *ngIf="contactForm.get('secondaryMobile')?.hasError('pattern')">
          رقم الجوال يجب أن يبدأ بـ 05 ويكون 10 أرقام
        </mat-error>
      </mat-form-field>

      <!-- Email -->
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>البريد الإلكتروني</mat-label>
        <input matInput formControlName="email" type="email" [disabled]="!canEdit">
        <mat-error *ngIf="contactForm.get('email')?.hasError('email')">
          البريد الإلكتروني غير صحيح
        </mat-error>
      </mat-form-field>
    </div>
  </form>
</mat-tab>
```

**Required Changes to Container Component**:

Update `case-data-container.component.ts` to handle contact form:
```typescript
import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CaseRegistrationApiService } from '../../services/case-registration-api.service';
import { RequestStateService } from '../../services/request-state.service';

export class CaseDataContainerComponent implements OnInit {
  @Input() requestId!: number;
  @Input() canEdit = false;

  contactForm!: FormGroup;
  isSaving = false;
  saveSuccess = false;

  constructor(
    private fb: FormBuilder,
    private caseApi: CaseRegistrationApiService,
    private caseDataState: CaseDataStateService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.buildContactForm();
    this.loadContactDataFromState();

    // Subscribe to form changes and update state service (NO database save)
    if (this.canEdit) {
      this.contactForm.valueChanges
        .pipe(distinctUntilChanged())
        .subscribe((values) => {
          // Update client-side state ONLY - NO database save
          this.caseDataState.updateContactInfo(
            values.primaryMobile,
            values.secondaryMobile,
            values.email
          );
        });
    } else {
      this.contactForm.disable();
    }
  }

  buildContactForm(): void {
    this.contactForm = this.fb.group({
      primaryMobile: ['', [Validators.pattern(/^05\d{8}$|^$/)]],
      secondaryMobile: ['', [Validators.pattern(/^05\d{8}$|^$/)]],
      email: ['', [Validators.email]]
    });
  }

  loadContactDataFromState(): void {
    const contactInfo = this.caseDataState.getContactInfo();

    this.contactForm.patchValue({
      primaryMobile: contactInfo.primaryMobile || '',
      secondaryMobile: contactInfo.secondaryMobile || '',
      email: contactInfo.email || ''
    }, { emitEvent: false });
  }

  saveAllData(): void {
    // Collect all data from state service
    const allData = this.caseDataState.getAllData();

    // Validate contact form if present
    if (!this.contactForm.valid) {
      this.snackBar.open('يرجى التحقق من بيانات التواصل', 'إغلاق', { duration: 3000 });
      return;
    }

    this.isSaving = true;
    this.saveSuccess = false;

    // Single API call to save everything
    this.caseApi.saveDraft(this.requestId, allData).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.saveSuccess = true;
        this.snackBar.open('تم حفظ المسودة بنجاح', 'إغلاق', { duration: 3000 });

        // Clear success message after 3 seconds
        setTimeout(() => this.saveSuccess = false, 3000);
      },
      error: (err) => {
        this.isSaving = false;
        console.error('خطأ في الحفظ:', err);
        this.snackBar.open('خطأ في حفظ المسودة', 'إغلاق', { duration: 5000 });
      }
    });
  }
}
```

**Add to SCSS**:
```scss
.contact-info-form {
  padding: 24px;

  .contact-header {
    margin-bottom: 24px;

    h3 {
      margin: 0;
      font-weight: 600;
      color: #333;
    }
  }

  mat-form-field {
    display: block;
    margin-bottom: 20px;
  }

  .full-width {
    width: 100%;
  }
}
```

### 4.9 Attachments Component (Tab 6 - Existing Component)

**Note**: Tab 6 "المرفقات" (Attachments) uses the **existing** `AttachmentsListComponent` that is already implemented at:
- `src/Frontend/bog-app/src/app/features/case-registration/components/attachments/attachments-list.component.ts`

**No changes needed** - this component already exists and handles:
- File upload (PDF only, max 4MB)
- Attachment type selection
- Description field
- List of attachments with download/delete
- Empty state display

The component is already integrated in the container HTML (section 4.1):
```html
<!-- Tab 6: المرفقات -->
<mat-tab>
  <ng-template mat-tab-label>
    <mat-icon>attach_file</mat-icon>
    <span>المرفقات</span>
  </ng-template>
  <app-attachments-list
    [requestId]="requestId"
    [canEdit]="canEdit">
  </app-attachments-list>
</mat-tab>
```

### 4.10 Update Module Registration

**File**: `src/Frontend/bog-app/src/app/features/case-registration/case-registration.module.ts`

Add imports:
```typescript
import { NgxEditorModule } from 'ngx-editor';

// New components
import { CaseDataContainerComponent } from './components/case-data/case-data-container/case-data-container.component';
import { SubjectEvidenceFormComponent } from './components/case-data/subject-evidence/subject-evidence-form.component';
import { ClaimsListComponent } from './components/case-data/claims-list/claims-list.component';
import { ClaimFormDialogComponent } from './components/case-data/claims-list/claim-form-dialog.component';
import { RelatedCasesListComponent } from './components/case-data/related-cases-list/related-cases-list.component';
import { RelatedCaseFormDialogComponent } from './components/case-data/related-cases-list/related-case-form-dialog.component';
import { ClassificationsTabComponent } from './components/case-data/classifications-tab/classifications-tab.component';
import { ClassificationSelectionDialogComponent } from './components/case-data/classifications-tab/classification-selection-dialog/classification-selection-dialog.component';

// New services
import { ClaimApiService } from './services/claim-api.service';
import { RelatedCaseApiService } from './services/related-case-api.service';
import { LookupsApiService } from './services/lookups-api.service';

@NgModule({
  declarations: [
    // ... existing
    CaseDataContainerComponent,
    SubjectEvidenceFormComponent,
    ClaimsListComponent,
    ClaimFormDialogComponent,
    RelatedCasesListComponent,
    RelatedCaseFormDialogComponent,
    ClassificationsTabComponent,
    ClassificationSelectionDialogComponent
  ],
  imports: [
    // ... existing
    NgxEditorModule
  ],
  providers: [
    // ... existing
    ClaimApiService,
    RelatedCaseApiService,
    LookupsApiService
  ]
})
export class CaseRegistrationModule { }
```

### 4.11 Replace Old Component in Request Details

**File**: `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.html`

Change:
```html
<!-- OLD -->
<app-case-data-form [requestId]="requestId" [canEdit]="canEdit"></app-case-data-form>

<!-- NEW -->
<app-case-data-container [requestId]="requestId" [canEdit]="canEdit"></app-case-data-container>
```

---

## PHASE 5: Testing & Verification

### 5.1 Backend API Testing (Swagger/Postman)

**Test Claims Endpoints:**
```
GET /api/case-requests/1408/claims
Expected: [] or [{ id, claimText, displayOrder, ... }]

PUT /api/case-requests/1408/claims
Body: {
  "claims": [
    { "claimText": "طلب الحكم بإلزام المدعى عليه بالتعويض", "displayOrder": 0 },
    { "claimText": "طلب رد الاعتبار", "displayOrder": 1 }
  ]
}
Expected: 200 OK with saved claims array
```

**Test Related Cases Endpoints:**
```
GET /api/case-requests/1408/related-cases
Expected: [] or [{ id, caseNumber, notes, ... }]

PUT /api/case-requests/1408/related-cases
Body: {
  "relatedCases": [
    { "courtId": 1, "caseNumber": 12345, "caseYear": 2025 }
  ]
}
Expected: 200 OK with saved related cases array

GET /api/lookups/courts
Expected: [{ id: 1, nameAr: "المحكمة الإدارية بالرياض", name: "..." }]
```

**Add to existing**: `src/Backend/BOG.API/Controllers/LookupsController.cs`

Add this new endpoint to retrieve courts:
```csharp
[ApiController]
[Route("api/lookups")]
public class LookupsController : ControllerBase
{
    private readonly IRepository<Court> _courtRepository;
    private readonly ILogger<LookupsController> _logger;
    // ... other repositories ...

    public LookupsController(
        IRepository<Court> courtRepository,
        ILogger<LookupsController> logger
        // ... other repositories ...)
    {
        _courtRepository = courtRepository ?? throw new ArgumentNullException(nameof(courtRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // ... other initializations ...
    }

    /// <summary>
    /// Gets all active courts.
    /// Used by the case registration form for court selection.
    /// </summary>
    [HttpGet("courts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<object>>> GetCourts(CancellationToken cancellationToken)
    {
        try
        {
            var courts = await _courtRepository.FindAsync(
                c => c.IsActive && !c.IsDeleted,
                cancellationToken);

            var result = courts
                .Select(c => new
                {
                    id = c.Id,
                    nameAr = c.NameAr,
                    name = c.Name,
                    regionId = c.RegionId,
                    cityId = c.CityId
                })
                .OrderBy(c => c.nameAr)
                .ToList();

            _logger.LogInformation("Retrieved {Count} courts", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courts");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving courts." });
        }
    }

    // ... existing endpoints (classifications, attachment-types, etc.) ...
}
```

### 5.2 Frontend Component Testing

**Test Case 1: Vertical Tabs Layout**
1. Navigate to http://localhost:4200/case-registration/1408/edit
2. Open بيانات الدعوى section
3. **Verify**: Tabs appear vertically on the right side
4. **Verify**: Active tab has teal color (#2c7a7b) with left border indicator
5. **Verify**: Hover shows light teal background
6. Click each tab
7. **Verify**: Content changes, active indicator moves

**Test Case 2: Rich Text موضوع وأسانيد الدعوى**
1. Go to "موضوع وأسانيد الدعوى" tab
2. Type text in Subject editor
3. Format text (bold, italic, list, color)
4. Wait 2 seconds (auto-save with debounce)
5. Refresh page
6. **Verify**: Formatted text persists with all formatting

**Test Case 3: Add/Edit/Delete Claims**
1. Go to "طلبات الدعوى" tab
2. Click gold "إضافة طلب" button
3. Enter claim text
4. Click "إضافة"
5. **Verify**: Claim appears in table
6. Add 2 more claims
7. Edit claim #2
8. **Verify**: Changes save
9. Delete claim #3
10. **Verify**: Table updates

**Test Case 4: Related Cases**
1. Go to "الدعاوى المرتبطة" tab
2. Click gold "إضافة دعوى مرتبطة" button
3. **Verify**: Dialog opens with المحكمة ورقم الدعوى وعام الدعوى fields
4. Select a court from dropdown
5. Enter case number (e.g., 12345)
6. Enter case year (e.g., 2025)
7. Click "إضافة"
8. **Verify**: Related case appears in table with court name, case number, and year
9. Edit related case
10. **Verify**: Dialog opens with pre-filled values
11. Delete related case
12. **Verify**: Table updates

**Test Case 5: Character Limits**
- Subject/Evidence: Type 4001 characters → Verify error
- Claim text: Cannot type beyond 2000 (maxlength)
- Case Number: Enter 100000000000 (12 digits) → Verify error "رقم الدعوى لا يمكن أن يتجاوز 11 رقم"

**Test Case 6: Contact Information Validation**
1. Go to "بيانات التواصل" tab
2. **Phone Validation**:
   - Enter "0512345678" (valid: starts with 05, 10 digits) → Verify no error
   - Enter "1234567890" (invalid: doesn't start with 05) → Verify error "رقم الجوال يجب أن يبدأ بـ 05 ويكون 10 أرقام"
   - Enter "051234567" (invalid: only 9 digits) → Verify error
   - Enter "05123456789" (field shows only 10 chars due to maxlength)
   - Leave phone blank → Verify no error (optional field)
3. **Email Validation**:
   - Enter "user@example.com" (valid format) → Verify no error
   - Enter "invalidemail" (missing @) → Verify error "البريد الإلكتروني غير صحيح"
   - Enter "user@" (incomplete) → Verify error
   - Leave email blank → Verify no error (optional field)
4. **Auto-Save**:
   - Enter valid phone and email → Wait 2 seconds → Verify auto-save triggers
   - Refresh page → Verify contact info persists

**Test Case 7: Classifications Grid Dialog**
1. Go to "تصنيف الدعوى" tab
2. Click gold "إضافة تصنيف" button
3. **Verify**: Dialog opens with:
   - Title: "تحديد تصنيفات الدعوى"
   - Search field at top (RTL)
   - Grid layout displaying classifications (4 columns per row)
   - Each card shows checkbox, Arabic name, description
4. **Search Functionality**:
   - Type a classification name in search → Verify grid filters in real-time
   - Search for Arabic text → Verify results update
   - Search for non-existent term → Verify "لا توجد تصنيفات مطابقة" message
   - Clear search → Verify all classifications reappear
5. **Selection**:
   - Click on a classification card → Verify checkbox toggles
   - Click multiple cards → Verify multiple selections possible
   - Verify selected cards have teal border and background (#e8f4f8)
6. **Dialog Actions**:
   - Click "تأكيد التحديد" → Dialog closes and selected classifications appear in list below button
   - Click "إلغاء" → Dialog closes without saving changes
7. **Selected Classifications Display**:
   - Verify selected items show in flex row below button
   - Each item shows name and × (close) button
   - Click × button → Verify item is removed and saved
   - Verify empty state shows "لم يتم تحديد أي تصنيفات" when no items selected

### 5.3 RTL & Arabic Testing

- **Verify**: Rich text editors display RTL
- **Verify**: Toolbar buttons aligned correctly
- **Verify**: Dialog directions are RTL
- **Verify**: Table text aligned right
- **Verify**: Icons positioned correctly in tabs

---

## Critical Files Summary

### Backend (16+ new/modified files)
1. `src/Backend/BOG.DbModel/Entities/CaseRegistration/RelatedCase.cs` (MODIFY)
2. **Database Migration**: `dotnet ef migrations add UpdateRelatedCaseFields`
3. `src/Backend/BOG.DTO/CaseRegistration/ClaimDTO.cs`
4. `src/Backend/BOG.DTO/CaseRegistration/RelatedCaseDTO.cs`
5. `src/Backend/BOG.VM/CaseRegistration/ClaimVM.cs`
6. `src/Backend/BOG.VM/CaseRegistration/RelatedCaseVM.cs`
7. `src/Backend/BOG.DAL/Interfaces/IClaimRepository.cs`
8. `src/Backend/BOG.DAL/Interfaces/IRelatedCaseRepository.cs`
9. `src/Backend/BOG.DAL/Repositories/ClaimRepository.cs`
10. `src/Backend/BOG.DAL/Repositories/RelatedCaseRepository.cs`
11. `src/Backend/BOG.BL/Interfaces/CaseRegistration/IClaimBL.cs`
12. `src/Backend/BOG.BL/Interfaces/CaseRegistration/IRelatedCaseBL.cs`
13. `src/Backend/BOG.BL/Services/CaseRegistration/ClaimBL.cs`
14. `src/Backend/BOG.BL/Services/CaseRegistration/RelatedCaseBL.cs`
15. `src/Backend/BOG.API/Controllers/ClaimsController.cs`
16. `src/Backend/BOG.API/Controllers/RelatedCasesController.cs`
17. `src/Backend/BOG.API/Controllers/LookupsController.cs` (NEW - if not exists)
18. `src/Backend/BOG.API/Extensions/ServiceCollectionExtensions.cs` (MODIFY)

### Frontend (18 new files + modifications)
1. `src/Frontend/bog-app/package.json` (MODIFY - add ngx-editor)
2. `src/Frontend/bog-app/src/app/features/case-registration/models/claim.model.ts`
3. `src/Frontend/bog-app/src/app/features/case-registration/models/related-case.model.ts`
4. `src/Frontend/bog-app/src/app/features/case-registration/services/claim-api.service.ts`
5. `src/Frontend/bog-app/src/app/features/case-registration/services/related-case-api.service.ts`
6. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.ts`
7. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.html`
8. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.scss`
9. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/subject-evidence/subject-evidence-form.component.ts`
10. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/subject-evidence/subject-evidence-form.component.html`
11. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/subject-evidence/subject-evidence-form.component.scss`
12. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/claims-list/claims-list.component.ts`
13. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/claims-list/claims-list.component.html`
14. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/claims-list/claims-list.component.scss`
15. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/claims-list/claim-form-dialog.component.ts`
16. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/related-cases-list/related-cases-list.component.ts`
17. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/related-cases-list/related-cases-list.component.html`
18. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/related-cases-list/related-cases-list.component.scss`
19. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/related-cases-list/related-case-form-dialog.component.ts`
20. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/classifications-tab/classifications-tab.component.ts`
21. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/classifications-tab/classifications-tab.component.html`
22. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/classifications-tab/classifications-tab.component.scss`
23. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/classifications-tab/classification-selection-dialog/classification-selection-dialog.component.ts`
24. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/classifications-tab/classification-selection-dialog/classification-selection-dialog.component.html`
25. `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/classifications-tab/classification-selection-dialog/classification-selection-dialog.component.scss`
26. `src/Frontend/bog-app/src/app/features/case-registration/case-registration.module.ts` (MODIFY)
27. `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.html` (MODIFY)

---

## Migration Strategy

1. **Phase 1**: Backend implementation (DTOs → Repos → BL → Controllers → DI)
2. **Phase 2**: Install ngx-editor package
3. **Phase 3**: Frontend models and services
4. **Phase 4**: Build components incrementally:
   - CaseDataContainerComponent with vertical tabs
   - SubjectEvidenceFormComponent with rich text editors
   - ClaimsListComponent with drag-drop
   - RelatedCasesListComponent
   - ClassificationsFormComponent (extracted)
5. **Phase 5**: Replace old case-data-form in request-details
6. **Phase 6**: Test all features end-to-end
7. **Phase 7**: Remove old case-data-form component

---

## Design Specifications Checklist

✅ Vertical tabs on right side (RTL layout)
✅ Active tab color: #2c7a7b with left border indicator
✅ Hover state: #e8f4f8 background
✅ Gold buttons: #d4a017 for "Add" actions
✅ Rich text editors with RTL support
✅ Character counters (4000 for subject/evidence, 2000 for claims)
✅ Auto-save with 2-second debounce
✅ Drag-drop for claims reordering
✅ Empty states for lists
✅ Loading spinners
✅ Material icons matching mockup

---

## Success Criteria

✅ Vertical tabs layout matches HTML mockup exactly
✅ Rich text editors work with formatting (bold, italic, lists, colors)
✅ Character limits enforced and displayed
✅ Claims support Add/Edit/Delete
✅ Related Cases support Add/Edit/Delete
✅ All data auto-saves with debounce
✅ RTL rendering correct for all Arabic content
✅ API endpoints return proper HTTP status codes
✅ Validation errors display in UI
✅ Page refresh preserves all data including formatting
✅ Gold buttons and teal active tabs match mockup colors
✅ No minimum character validation enforced
✅ RelatedCase has exactly 3 fields: Court (dropdown), Case Number (integer, max 11 digits), Case Year
✅ Court dropdown loads from /api/lookups/courts endpoint
✅ Database migration successfully modifies CaseNumber to int, adds CourtId and CaseYear, removes Notes
✅ CaseNumber validation enforces integer type with max value 99999999999 (11 digits)

---

## UPDATE: Contact Information Tab (NEWLY ADDED)

### Changes Summary

Based on user feedback, the following enhancements have been added to the plan:

#### 1. **Contact Information Tab (Tab 5) - 3 Fields Only** ✅
   - Tab 5 "بيانات التواصل" contains exactly 3 editable contact fields:
     - Primary Mobile (رقم الجوال الأساسي)
     - Secondary Mobile (رقم الجوال الثانوي)
     - Email (البريد الإلكتروني)
   - All fields are optional
   - Auto-saves with 2-second debounce

#### 2. **Phone Number Validation** ✅
   - Must start with "05" and be exactly 10 digits (pattern: 05XXXXXXXX)
   - Examples: 0512345678, 0598765432
   - Field maxlength="10" to prevent exceeding limit
   - Error message: "رقم الجوال يجب أن يبدأ بـ 05 ويكون 10 أرقام"

#### 3. **Email Validation** ✅
   - Must be valid email format (e.g., user@example.com)
   - Standard email validation using Validators.email
   - Error message: "البريد الإلكتروني غير صحيح"

### Implementation Details

**Backend Changes**:
1. Contact information fields added to `CaseRegistrationRequest` entity:
   - `PrimaryMobile` (nullable string, max 10 chars, pattern: 05 + 8 digits)
   - `SecondaryMobile` (nullable string, max 10 chars, pattern: 05 + 8 digits)
   - `Email` (nullable string, valid email format)

**Frontend Changes**:
1. Contact Information Tab (Tab 5) now contains editable form with contact fields (all optional):
   - Primary Mobile: Pattern `/^05\d{8}$|^$/` (10 digits starting with "05" or empty), maxlength="10"
   - Secondary Mobile: Pattern `/^05\d{8}$|^$/` (10 digits starting with "05" or empty), maxlength="10"
   - Email: Standard email validation
2. Component handles auto-save with 2-second debounce

### Files to Create/Modify

**Backend**:
- ⏳ `src/Backend/BOG.DbModel/Entities/CaseRegistration/CaseRegistrationRequest.cs` - Add contact fields (PrimaryMobile, SecondaryMobile, Email)
- ⏳ Database migrations: `AddContactFieldsToRequest`

**Frontend**:
- ⏳ Contact Information form in `case-data-container.component.html` - 3 fields (primaryMobile, secondaryMobile, email)
- ⏳ `case-data-container.component.ts` - Form with auto-save (2-second debounce)

### Success Criteria for This Update

✅ Contact information form contains 3 fields: Primary Mobile, Secondary Mobile, Email
✅ Contact information auto-saves after 2-second debounce
✅ Phone validation: Starts with "05" and exactly 10 digits total
✅ Phone field: maxlength="10" attribute prevents exceeding limit
✅ Email validation: Valid email format (user@example.com)
✅ All fields are optional (optional fields accept empty values)
✅ RTL layout maintained for all form fields

---

## UPDATE: 4-Level Classification System (NEWLY ADDED)

### Overview

Transform the current flat Classification system into a 4-level hierarchical structure. Classifications will be pre-populated by admins with 4 text fields (التصنيف الأول، التصنيف الثاني، التصنيف الثالث، التصنيف الرابع). Users select multiple classifications from a searchable dialog table displaying all 4 levels.

**Current State**:
- Classification entity has only Name, NameAr, Description, IsActive (flat structure)
- Frontend uses simple multi-select dropdown
- Many-to-many relationship via RequestClassification entity

**New State**:
- Classification entity will have 4 level fields (Level1, Level2, Level3, Level4)
- Admin seeds pre-populated classification data (10+ examples)
- Users open searchable dialog with 4-column table to select multiple classifications
- Dialog filters across all 4 fields simultaneously
- Selected classifications display in card format showing all 4 levels

---

## PHASE 0: Backend Implementation - Classification Entity

### 0.1 Modify Classification Entity

**File**: `src/Backend/BOG.DbModel/Entities/Lookups/Classification.cs`

Add 4 new required properties to the Classification entity:

```csharp
using BOG.DbModel.Entities.Base;

namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Classification entity representing a 4-level hierarchical classification system.
/// Used for case categorization: Level1 (main category) → Level2 (subcategory) → Level3 → Level4 (specific)
/// </summary>
public class Classification : BaseEntity
{
    /// <summary>
    /// First level classification (التصنيف الأول)
    /// Example: "دعاوى مدنية" (Civil cases)
    /// </summary>
    public string Level1 { get; set; } = null!;

    /// <summary>
    /// Second level classification (التصنيف الثاني)
    /// Example: "دعاوى الملكية" (Property cases)
    /// </summary>
    public string Level2 { get; set; } = null!;

    /// <summary>
    /// Third level classification (التصنيف الثالث)
    /// Example: "منازعات الأراضي" (Land disputes)
    /// </summary>
    public string Level3 { get; set; } = null!;

    /// <summary>
    /// Fourth level classification (التصنيف الرابع)
    /// Example: "نزاع حدود" (Boundary dispute)
    /// </summary>
    public string Level4 { get; set; } = null!;
}
```

### 0.2 Update Database Configuration

**File**: `src/Backend/BOG.DbModel/ApplicationDbContext.cs`

Locate the `ConfigureLookupEntities` method (around line 509) and add configuration for Classification properties:

```csharp
private void ConfigureLookupEntities(ModelBuilder builder)
{
    // ... existing code ...

    // Configure Classification entity
    var classificationEntity = builder.Entity<Classification>();

    classificationEntity.HasKey(c => c.Id);

    // Configure 4-level classification properties
    classificationEntity.Property(c => c.Level1)
        .IsRequired()
        .HasMaxLength(200)
        .HasColumnType("nvarchar(200)");

    classificationEntity.Property(c => c.Level2)
        .IsRequired()
        .HasMaxLength(200)
        .HasColumnType("nvarchar(200)");

    classificationEntity.Property(c => c.Level3)
        .IsRequired()
        .HasMaxLength(200)
        .HasColumnType("nvarchar(200)");

    classificationEntity.Property(c => c.Level4)
        .IsRequired()
        .HasMaxLength(200)
        .HasColumnType("nvarchar(200)");

    // Add composite index for optimized search across all 4 levels
    classificationEntity.HasIndex(c => new { c.Level1, c.Level2, c.Level3, c.Level4 })
        .HasDatabaseName("IX_Classification_Levels");

    // ... rest of existing code ...
}
```

### 0.3 Update Seed Data

**File**: `src/Backend/BOG.DbModel/ApplicationDbContext.cs`

Locate the `SeedLookupData` method (around line 1107) and update the Classification seeds with 10+ examples:

```csharp
// Clear existing classifications to prevent duplicates
context.Classifications.RemoveRange(context.Classifications);
await context.SaveChangesAsync();

var now = DateTime.UtcNow;

var classifications = new List<Classification>
{
    // Civil Cases - Property Disputes
    new Classification { Id = 1, Level1 = "دعاوى مدنية", Level2 = "دعاوى الملكية", Level3 = "منازعات الأراضي", Level4 = "نزاع حدود", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 2, Level1 = "دعاوى مدنية", Level2 = "دعاوى الملكية", Level3 = "منازعات العقارات", Level4 = "نزاع الملكية", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 3, Level1 = "دعاوى مدنية", Level2 = "دعاوى الملكية", Level3 = "منازعات العقارات", Level4 = "نزاع التقسيم", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 4, Level1 = "دعاوى مدنية", Level2 = "دعاوى العقود", Level3 = "منازعات التنفيذ", Level4 = "عدم التزام المتعاقد", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 5, Level1 = "دعاوى مدنية", Level2 = "دعاوى العقود", Level3 = "منازعات الدفع", Level4 = "تأخر السداد", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 6, Level1 = "دعاوى تجارية", Level2 = "منازعات التجار", Level3 = "منازعات البيع والشراء", Level4 = "سلع معيبة", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 7, Level1 = "دعاوى تجارية", Level2 = "منازعات البنوك", Level3 = "منازعات القروض", Level4 = "عدم السداد", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 8, Level1 = "دعاوى عمالية", Level2 = "منازعات الأجور", Level3 = "تأخر الأجور", Level4 = "عدم دفع كامل المستحقات", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 9, Level1 = "دعاوى عمالية", Level2 = "منازعات الفصل", Level3 = "فصل تعسفي", Level4 = "فصل بلا سبب", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 10, Level1 = "دعاوى إدارية", Level2 = "منازعات القرارات الإدارية", Level3 = "قرارات التعيين", Level4 = "طعن في التعيين", CreatedDate = now, ModifiedDate = now },
    new Classification { Id = 11, Level1 = "دعاوى إدارية", Level2 = "منازعات الخدمات", Level3 = "منازعات الترقيات", Level4 = "عدم الترقية", CreatedDate = now, ModifiedDate = now }
};

context.Classifications.AddRange(classifications);
await context.SaveChangesAsync();
```

### 0.4 Create Database Migration

Run the following command from the solution root:

```bash
# Add migration for 4-level classification structure
dotnet ef migrations add AddFourLevelClassificationStructure --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API

# Update database
dotnet ef database update --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
```

The migration will automatically:
- Add Level1, Level2, Level3, Level4 columns (nvarchar(200), NOT NULL)
- Create composite index on all 4 levels
- Seed the 10+ classification examples

### 0.5 Create Classification ViewModel

**New File**: `src/Backend/BOG.VM/Lookups/ClassificationVM.cs`

```csharp
namespace BOG.VM.Lookups;

/// <summary>
/// View Model for Classification with 4-level hierarchical structure
/// Used for API responses to frontend
/// </summary>
public class ClassificationVM
{
    public int Id { get; set; }

    /// <summary>
    /// First level classification (التصنيف الأول)
    /// </summary>
    public string Level1 { get; set; } = null!;

    /// <summary>
    /// Second level classification (التصنيف الثاني)
    /// </summary>
    public string Level2 { get; set; } = null!;

    /// <summary>
    /// Third level classification (التصنيف الثالث)
    /// </summary>
    public string Level3 { get; set; } = null!;

    /// <summary>
    /// Fourth level classification (التصنيف الرابع)
    /// </summary>
    public string Level4 { get; set; } = null!;

    /// <summary>
    /// Computed property: Full text representation of all 4 levels
    /// Format: "Level1 - Level2 - Level3 - Level4"
    /// </summary>
    public string FullText => $"{Level1} - {Level2} - {Level3} - {Level4}";
}
```

### 0.6 Update Lookups Controller

**File**: `src/Backend/BOG.API/Controllers/LookupsController.cs`

Update the GetClassifications method to return the new 4-level structure:

```csharp
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.Lookups;
using BOG.VM.Lookups;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// Lookup data management API controller.
/// </summary>
[ApiController]
[Route("api/lookups")]
public class LookupsController : ControllerBase
{
    private readonly IRepository<Classification> _classificationRepository;
    private readonly ILogger<LookupsController> _logger;

    public LookupsController(
        IRepository<Classification> classificationRepository,
        ILogger<LookupsController> logger)
    {
        _classificationRepository = classificationRepository ?? throw new ArgumentNullException(nameof(classificationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all active classifications with 4-level hierarchy.
    /// Returns classifications sorted by all 4 levels for consistent ordering.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active classifications with all 4 levels</returns>
    [HttpGet("classifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ClassificationVM>>> GetClassifications(CancellationToken cancellationToken)
    {
        try
        {
            var classifications = await _classificationRepository.FindAsync(
                c => !c.IsDeleted,
                cancellationToken);

            var result = classifications
                .Select(c => new ClassificationVM
                {
                    Id = c.Id,
                    Level1 = c.Level1,
                    Level2 = c.Level2,
                    Level3 = c.Level3,
                    Level4 = c.Level4
                })
                .OrderBy(c => c.Level1)
                .ThenBy(c => c.Level2)
                .ThenBy(c => c.Level3)
                .ThenBy(c => c.Level4)
                .ToList();

            _logger.LogInformation("Retrieved {Count} classifications with 4-level structure", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classifications");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving classifications." });
        }
    }
}
```

---

## PHASE 2: Frontend Implementation - Classification Dialog

### 2.1 Update Classification Service Interface

**File**: `src/Frontend/bog-app/src/app/features/case-registration/services/classification-api.service.ts`

Update the ClassificationVM interface to include 4 levels:

```typescript
export interface ClassificationVM {
  id: number;
  level1: string;
  level2: string;
  level3: string;
  level4: string;
  fullText?: string;
}
```

### 2.2 Frontend Classification Components

**Note**: The frontend components for Tab 4 (classifications) are fully implemented in **PHASE 4, Section 4.7** above:

- `ClassificationsTabComponent` - Wrapper for Tab 4 (lines 2396+)
- `ClassificationSelectionDialogComponent` - Dialog for selecting classifications (nested within classifications-tab)

**No additional implementation needed** - refer to section 4.7 for complete code and details.

---

## Testing & Verification

### Backend Verification

1. **Check Database**:
   ```sql
   SELECT Id, Level1, Level2, Level3, Level4, IsActive FROM Classifications WHERE IsDeleted = 0 ORDER BY Level1;
   ```
   Expected: 11 active classifications with all 4 levels populated

2. **Test API in Swagger**:
   - Navigate to `https://localhost:5001/swagger`
   - Call `GET /api/lookups/classifications`
   - Verify response includes level1, level2, level3, level4 fields
   - Verify data is sorted by Level1, Level2, Level3, Level4

### Frontend Verification

1. **Dialog Opens**:
   - Navigate to case registration form
   - Click "إضافة تصنيف" button
   - Dialog opens with table showing 4 columns + checkbox
   - All 11 classifications display in table

2. **Search Functionality**:
   - Type Arabic text in search field (e.g., "مدنية")
   - Table filters in real-time across all 4 levels
   - Try searching different level 2/3/4 terms

3. **Multi-Select**:
   - Select multiple classifications using checkboxes
   - Click "حفظ الاختيار"
   - Dialog closes
   - Selected items appear as cards below the button
   - Each card shows all 4 levels with labels

4. **Remove Functionality**:
   - Click × button on a selected classification card
   - Card disappears
   - Classification removed from form
   - Auto-save triggers

5. **Validation**:
   - Remove all classifications
   - Try to proceed
   - Error "يجب تحديد تصنيف واحد على الأقل (ERR005)" appears

6. **RTL Layout**:
   - Verify table columns are right-to-left
   - Arabic text displays correctly
   - Dialog mirror-layout works properly

### End-to-End Test

1. Create new case request
2. Add subject and evidence
3. Add 3 different classifications
4. Save case
5. Reload page
6. Verify all 3 classifications persist
7. Modify selection (remove 1, add another)
8. Save
9. Verify persistence

---

## Success Criteria

✅ Classification entity has 4 level fields in database
✅ Migration applied without errors
✅ 11 realistic classification examples seeded
✅ API endpoint returns all 4 levels for each classification
✅ Classifications sorted by Level1, Level2, Level3, Level4
✅ Dialog opens with searchable table (4 columns + checkbox)
✅ Search filters across all 4 levels
✅ Multi-select with checkboxes works
✅ Selected items display all 4 levels with labels
✅ Remove button removes individual item
✅ Selection/removal updates client-side state only
✅ Validation shows error if no classification selected
✅ RTL layout displays correctly
✅ No console errors
✅ Existing case registration flow not broken

---

## CRITICAL UPDATE: Save Strategy Correction (Implemented)

### ⚠️ ISSUE IDENTIFIED AND RESOLVED

**Original Issue**: The implementation plan had auto-save logic that immediately saved data to the database on field changes using a 2-second debounce. This conflicted with the actual requirements.

**Correct Behavior**: All form data (subject, evidence, claims, related cases, classifications, contact info) should be stored **client-side only** until the user explicitly clicks the fixed **"حفظ كمسودة"** button, which saves ALL data to the database at once.

### CHANGES IMPLEMENTED

#### 1. ✅ Design Specifications Updated (Line 22-24)
- **Removed**: "Auto-save: 2-second debounce with visual indicator"
- **Added**: "Save Button: Fixed حفظ كمسودة button visible across all tabs"
- **Added**: "Client-side Storage: All form data stored in client-side state until user clicks save button"

#### 2. ✅ CaseDataStateService Implemented (New Section 3.2, Lines 1240-1447)
**Purpose**: Central state management service using RxJS BehaviorSubject for all form data
**Key Methods**:
- `updateSubject()`, `updateEvidence()` - Update text fields
- `updateClaims()`, `updateRelatedCases()` - Update list data
- `updateClassifications()` - Update selected classification IDs
- `updateContactInfo()` - Update phone and email
- `getAllData()` - Get all state for saving to database
- `loadFromRequest()` - Load existing data from backend
- Optional localStorage backup for browser refresh scenarios

**Storage**: BehaviorSubject (required) + localStorage (optional backup)
**No API calls**: Service is pure state management only

#### 3. ✅ SubjectEvidenceFormComponent Updated (Lines 1622-1747)
**Changes**:
- ❌ Removed: apiService dependency
- ❌ Removed: debounceTime(2000) auto-save logic
- ❌ Removed: saveForm() method with API call
- ✅ Added: caseDataState service injection
- ✅ Updated: Form valueChanges pipes only distinctUntilChanged() → updates state only
- ✅ Updated: loadData() reads from state service instead of API
- ✅ Added: Error codes ERR006 and ERR007 to validation messages
- **Result**: Pure local state updates, NO database saves

#### 4. ✅ ClaimsListComponent Updated (Lines 1825-1884)
**Changes**:
- ❌ Removed: claimApi service dependency
- ❌ Removed: isLoading, snackBar dependencies
- ❌ Removed: loadClaims() API call
- ❌ Removed: saveClaims() method with API call
- ✅ Added: caseDataState service injection
- ✅ Updated: Add/Edit/Delete operations now only call `caseDataState.updateClaims()`
- ✅ Fixed: Changed deprecated `dialogRef.afterإغلاقd()` to `dialogRef.afterClosed()`
- **Result**: All operations update state only, NO database saves

#### 5. ✅ RelatedCasesListComponent Updated (Lines 2132-2191)
**Changes**:
- ❌ Removed: relatedCaseApi service dependency
- ❌ Removed: isLoading, snackBar dependencies
- ❌ Removed: loadRelatedCases() API call
- ❌ Removed: saveRelatedCases() method with API call
- ✅ Added: caseDataState service injection
- ✅ Updated: Add/Edit/Delete operations now only call `caseDataState.updateRelatedCases()`
- ✅ Fixed: Changed deprecated `dialogRef.afterإغلاقd()` to `dialogRef.afterClosed()`
- **Result**: All operations update state only, NO database saves

#### 6. ✅ ClassificationsTabComponent Updated (Lines 2792-2853)
**Changes**:
- ❌ Removed: caseApi service dependency
- ❌ Removed: saving, snackBar dependencies
- ❌ Removed: loadSelectedClassifications() API call
- ❌ Removed: saveClassifications() method with API call
- ✅ Added: caseDataState service injection
- ✅ Updated: Dialog selection/removal now only call `caseDataState.updateClassifications()`
- **Result**: All operations update state only, NO database saves

#### 7. ✅ CaseDataContainerComponent Updated (Lines 2807-2891)
**Major Changes**:
- ❌ Removed: debounceTime(2000) auto-save logic
- ❌ Removed: setupAutoSave() method
- ❌ Removed: saveContactInfo() method with API call
- ❌ Removed: requestState service dependency
- ✅ Added: caseDataState service injection
- ✅ Updated: Form valueChanges pipes only distinctUntilChanged() → updates state only
- ✅ Updated: loadRequestData() → loadContactDataFromState()
- ✅ **NEW**: `saveAllData()` method that aggregates ALL state data and sends single API call
- **Result**:
  - Contact form updates state only (no auto-save)
  - All data saved at once via saveAllData() method

#### 8. ✅ Fixed Action Buttons Added (HTML Template, Lines 1616-1640)
**Location**: case-data-container.component.html (top of component, always sticky)
**Features**:

**Back Button ("عودة")**:
- Icon: arrow_forward (RTL-appropriate for right-to-left language)
- Style: Secondary (outlined button with blue border, white background)
- Position: Left side of action bar (displayed first in flex layout)
- Function: Navigate back to /case-registration/requests
- Shows confirmation if unsaved changes exist: "هل تريد العودة إلى قائمة الطلبات؟ سيتم فقدان أي تغييرات غير محفوظة."
- Disabled during save operation (isSaving=true) to prevent conflicts
- Always enabled for read-only views (canEdit=false allowed to click)

**Save Draft Button ("حفظ كمسودة")**:
- Icon: save (standard save icon)
- Style: Primary (filled blue button)
- Position: Right side of action bar (displayed second in flex layout)
- Function: Aggregates all state data and saves to database atomically
- Disabled when canEdit=false or isSaving=true
- Shows spinner during save: "جاري الحفظ..." with rotating spinner
- Shows success indicator for 3 seconds: "تم الحفظ بنجاح" with green checkmark
- Always visible (sticky positioning: top: 0, z-index: 100)
- Visible across all 6 tabs without page reload

**Shared Features**:
- Fixed positioning with `position: sticky; top: 0; z-index: 100`
- White background with bottom border for separation
- Horizontal flex layout with 16px gap between elements
- Box shadow for elevation effect
- Both buttons disabled simultaneously during save operation
- RTL-aware spacing and alignment

**Markup**:
```html
<div class="fixed-action-bar">
  <!-- Back Button -->
  <button mat-raised-button class="secondary-button"
          (click)="goBack()"
          [disabled]="isSaving">
    <mat-icon>arrow_forward</mat-icon>
    عودة
  </button>

  <!-- Save Draft Button -->
  <button mat-raised-button color="primary"
          (click)="saveAllData()"
          [disabled]="!canEdit || isSaving">
    <mat-icon>save</mat-icon>
    حفظ كمسودة
  </button>

  <span *ngIf="isSaving" class="save-indicator">
    <mat-spinner diameter="20"></mat-spinner>
    <span class="saving-text">جاري الحفظ...</span>
  </span>
  <span *ngIf="saveSuccess" class="success-indicator">
    <mat-icon>check_circle</mat-icon>
    <span>تم الحفظ بنجاح</span>
  </span>
</div>
```

#### 9. ✅ Fixed Action Bar SCSS Added (Lines 1586-1641)
**Styling**:
- `position: sticky; top: 0; z-index: 100` - Always visible above tabs
- `display: flex; gap: 16px` - Horizontal layout
- RTL adjustments: `margin-right` for Arabic text alignment
- Save indicator: Blue spinner with text
- Success indicator: Green checkmark with text
- Box shadow for elevation

#### 10. ✅ Documentation Updates
- Line 2773: Updated Classifications behavior documentation
- Line 3001: Updated Contact Info note to reflect state-based saving
- All auto-save references removed/corrected

### REQUIRED BACKEND IMPLEMENTATION

**New API Endpoint Required**:
```
POST /api/case-requests/{requestId}/save-draft
Content-Type: application/json

Request Body:
{
  "subject": "string",
  "evidence": "string",
  "claims": [
    { "claimText": "string" }
  ],
  "relatedCases": [
    {
      "courtId": number,
      "caseNumber": number,
      "caseYear": number
    }
  ],
  "classificationIds": [number],
  "primaryMobile": "string",
  "secondaryMobile": "string",
  "email": "string"
}

Response:
{
  "id": number,
  "subject": "string",
  "evidence": "string",
  // ... rest of updated request
}
```

**Implementation Location**: `CaseRegistrationRequest` controller in `BOG.API/Controllers/`

**Implementation Steps**:
1. Create DTO for batch save operation (includes all data)
2. Add method to CaseRegistrationBL to save all data atomically
3. Add controller endpoint POST `/case-requests/{id}/save-draft`
4. Transaction: Save subject/evidence, claims, related cases, classifications, contact info
5. Return updated CaseRegistrationRequest

### FILES MODIFIED IN PLAN

1. **Line 22-24**: Design specifications
2. **Section 3.2 (Lines 1240-1447)**: NEW CaseDataStateService
3. **Lines 1622-1747**: SubjectEvidenceFormComponent (removed auto-save, use state)
4. **Lines 1825-1884**: ClaimsListComponent (removed auto-save, use state)
5. **Lines 2132-2191**: RelatedCasesListComponent (removed auto-save, use state)
6. **Lines 2792-2853**: ClassificationsTabComponent (removed auto-save, use state)
7. **Lines 2807-2891**: CaseDataContainerComponent (added saveAllData() method)
8. **Lines 1368-1386**: HTML template with fixed save button
9. **Lines 1586-1641**: SCSS for fixed-action-bar styling
10. **Lines 2773, 3001**: Documentation corrections

### TESTING CHECKLIST FOR SAVE STRATEGY

#### Frontend Testing
- [ ] Navigate to case registration form
- [ ] Enter data in Subject field → Verify no API call made
- [ ] Enter data in Evidence field → Verify no API call made
- [ ] Add claim → Verify no API call made
- [ ] Add related case → Verify no API call made
- [ ] Select classification → Verify no API call made
- [ ] Enter phone/email → Verify no API call made
- [ ] Refresh page → Verify data still present (localStorage backup)
- [ ] Click "حفظ كمسودة" button → Verify single API call with all data
- [ ] Verify success message appears
- [ ] Refresh page → Verify all data persisted from database

#### Network Monitor (Developer Tools - Network Tab)
- [ ] While editing: NO requests to `/api/case-requests/{id}` (until save button clicked)
- [ ] On button click: ONE request to `/api/case-requests/{id}/save-draft` with complete payload
- [ ] Payload includes: subject, evidence, claims[], relatedCases[], classificationIds[], contact info

#### Backend Testing
- [ ] Verify saveDraft() endpoint implemented
- [ ] Test with valid data
- [ ] Test with invalid data (validation errors)
- [ ] Test concurrent save attempts
- [ ] Verify transaction rolls back on error
- [ ] Verify all data saves atomically

### SUMMARY

✅ **Save strategy corrected**: Auto-save → Manual save with "حفظ كمسودة" button
✅ **Client-side state management**: CaseDataStateService with BehaviorSubject
✅ **All components updated**: SubjectEvidence, Claims, RelatedCases, Classifications, ContactInfo
✅ **Fixed save button**: Sticky position, visible across all tabs
✅ **Single API call**: All data saved atomically on button click
✅ **Error codes added**: ERR006 and ERR007 for validation messages
✅ **Documentation updated**: Auto-save references removed/corrected

**Status**: Ready for implementation. All UI/UX layer complete. Awaiting backend `saveDraft()` endpoint implementation.
