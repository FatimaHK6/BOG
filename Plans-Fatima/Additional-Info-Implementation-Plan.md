# Plan: Implement Additional Info with Conditional Forms (UC 6.5.1.1.15)

## Problem Statement

**User Report:** "Priority is not saved" + UI doesn't match Adobe XD design

**Root Cause:**
1. Current implementation has GENERIC fields (Notes, Priority, etc.) but SRS requires CONDITIONAL forms based on case type
2. No save functionality implemented
3. Database schema doesn't match SRS requirements

**Actual Requirements (from SRS UC 6.5.1.1.15):**
The Additional Info section must display **different fields** based on the type of dispute:

### Type 1: إلغاء قرار إداري (Management Decision Cancellation) - 5 Fields
1. **رقم القرار** (Decision Number)
2. **تاريخ القرار** (Decision Date)
3. **تاريخ العلم بالقرار** (Date of Notification of Decision)
4. **طريقة العلم بالقرار** (Method of Notification of Decision)
5. **جهة إصدار القرار** (Decision Issuing Authority)

### Type 2: حقوق خدمة/تقاعدية (Service/Retirement Rights) - 6 Fields
1. **يوجد تظلم** (Has Complaint - Yes/No)
2. **رقم التظلم** (Complaint Number)
3. **تاريخ التظلم** (Complaint Date)
4. **الجهة المتظلم لها** (Authority Complained To)
5. **تاريخ البت في التظلم** (Date of Decision on Complaint)
6. **نتيجة النظام** (System Result)

### Type 3: نزاع علامة تجارية (Trademark Dispute) - 2 Fields
1. **رقم الطلب** (Request Number)
2. **تاريخه** (Its Date / Request Date)

---

## Current State Analysis

### ✅ What Exists:
1. **Backend Business Logic**: `AdditionalInfoBL.cs` (but with wrong schema)
2. **Backend Repository**: `AdditionalInfoRepository.cs`
3. **Database**: `AdditionalInfos` table (but with generic columns)
4. **Frontend UI**: Basic form (but doesn't match SRS)

### ❌ What's WRONG:
1. **Database Schema Mismatch**: Current table has generic fields (Notes, Priority, RelatedCaseNumber) instead of type-specific fields
2. **No Type Selection**: No way to determine which form to display
3. **No API Controller**: No endpoints
4. **No Frontend Service**: No API integration
5. **No Conditional UI**: Frontend shows fixed fields instead of conditional forms
6. **No Grid Layout**: Current UI is single column, SRS requires grid layout

---

## Implementation Strategy

### ⚠️ DATABASE SCHEMA DECISION

**✅ SELECTED: Option B - Separate Tables per Type**

**Database Structure:**
1. **Base Table**: `AdditionalInfos` (keeps existing table, add TypeId)
   - Id, CaseRegistrationRequestId, AdditionalInfoTypeId, IsDeleted, timestamps

2. **Type-Specific Tables:**
   - `AdditionalInfo_ManagementDecision` (FK to AdditionalInfos)
   - `AdditionalInfo_ServiceRights` (FK to AdditionalInfos)
   - `AdditionalInfo_Trademark` (FK to AdditionalInfos)

3. **Lookup Table**: `AdditionalInfoTypes` (3 types)

**Pros:**
- ✅ Clean schema, no nullable columns
- ✅ Type-safe, can't mix field types
- ✅ Easy to add constraints per type
- ✅ Better performance (no scanning nullable columns)

**Cons:**
- ❌ More complex queries (need JOINs)
- ❌ More entities to manage
- ❌ More tables in database

**Query Pattern:**
```sql
-- Get Management Decision info
SELECT ai.*, aid.*
FROM AdditionalInfos ai
LEFT JOIN AdditionalInfo_ManagementDecision aid ON ai.Id = aid.AdditionalInfoId
WHERE ai.CaseRegistrationRequestId = @id AND ai.AdditionalInfoTypeId = 1
```

---

## Implementation Plan

### Phase 0: Database Entities & Migration (Separate Tables Approach)

#### 0.1 Update Base AdditionalInfo Entity

**File:** `src/Backend/BOG.DbModel/Entities/CaseRegistration/AdditionalInfo.cs`

```csharp
/// <summary>
/// Base table for Additional Info - can have one, two, or all three types
/// </summary>
public class AdditionalInfo : BaseEntity
{
    public int CaseRegistrationRequestId { get; set; }

    // Navigation properties
    public virtual CaseRegistrationRequest CaseRegistrationRequest { get; set; } = null!;

    // One-to-one relationships with type-specific tables (all optional)
    public virtual AdditionalInfoManagementDecision? ManagementDecision { get; set; }
    public virtual AdditionalInfoServiceRights? ServiceRights { get; set; }
    public virtual AdditionalInfoTrademark? Trademark { get; set; }
}
```

Mark old columns as obsolete (will be dropped in migration):
```csharp
[Obsolete("Being replaced by separate tables")]
public string? Notes { get; set; }
// ... other old fields
```

#### 0.2 Create Type-Specific Entities

**New File:** `src/Backend/BOG.DbModel/Entities/CaseRegistration/AdditionalInfoManagementDecision.cs`
```csharp
/// <summary>
/// Type 1: إلغاء قرار إداري (Management Decision Cancellation)
/// </summary>
public class AdditionalInfoManagementDecision : BaseEntity
{
    public int AdditionalInfoId { get; set; }  // FK to AdditionalInfos

    /// <summary>رقم القرار - Decision Number</summary>
    [Required, MaxLength(50)]
    public string DecisionNumber { get; set; } = null!;

    /// <summary>تاريخ القرار - Decision Date</summary>
    [Required]
    public DateTime DecisionDate { get; set; }

    /// <summary>تاريخ العلم بالقرار - Date of Notification of Decision</summary>
    [Required]
    public DateTime NotificationDate { get; set; }

    /// <summary>طريقة العلم بالقرار - Method of Notification (FK to NotificationMethods lookup)</summary>
    [Required]
    public int NotificationMethodId { get; set; }

    /// <summary>جهة إصدار القرار - Decision Issuing Authority (FK to GovernmentEntities lookup)</summary>
    [Required]
    public int IssuingAuthorityId { get; set; }

    // Navigation properties
    public virtual AdditionalInfo AdditionalInfo { get; set; } = null!;
    public virtual NotificationMethod NotificationMethod { get; set; } = null!;
    public virtual GovernmentEntity IssuingAuthority { get; set; } = null!;
}
```

**New File:** `src/Backend/BOG.DbModel/Entities/CaseRegistration/AdditionalInfoServiceRights.cs`
```csharp
/// <summary>
/// Type 2: حقوق خدمة/تقاعدية (Service/Retirement Rights)
/// </summary>
public class AdditionalInfoServiceRights : BaseEntity
{
    public int AdditionalInfoId { get; set; }

    /// <summary>يوجد تظلم - Has Complaint (Dropdown: Yes/No)</summary>
    public bool? HasComplaint { get; set; }

    /// <summary>رقم التظلم - Complaint Number</summary>
    [MaxLength(50)]
    public string? ComplaintNumber { get; set; }

    /// <summary>تاريخ التظلم - Complaint Date</summary>
    public DateTime? ComplaintDate { get; set; }

    /// <summary>الجهة المتظلم لها - Authority Complained To (FK to GovernmentEntities lookup)</summary>
    public int? ComplaintAuthorityId { get; set; }

    /// <summary>تاريخ البت في التظلم - Date of Decision on Complaint</summary>
    public DateTime? ComplaintDecisionDate { get; set; }

    /// <summary>نتيجة النظام - System Result (free text)</summary>
    [MaxLength(500)]
    public string? SystemResult { get; set; }

    // Navigation properties
    public virtual AdditionalInfo AdditionalInfo { get; set; } = null!;
    public virtual GovernmentEntity? ComplaintAuthority { get; set; }
}
```

**New File:** `src/Backend/BOG.DbModel/Entities/CaseRegistration/AdditionalInfoTrademark.cs`
```csharp
/// <summary>
/// Type 3: نزاع علامة تجارية (Trademark Dispute)
/// </summary>
public class AdditionalInfoTrademark : BaseEntity
{
    public int AdditionalInfoId { get; set; }

    /// <summary>رقم الطلب - Request Number</summary>
    [Required, MaxLength(50)]
    public string RequestNumber { get; set; } = null!;

    /// <summary>تاريخه - Its Date (Request Date)</summary>
    [Required]
    public DateTime RequestDate { get; set; }

    public virtual AdditionalInfo AdditionalInfo { get; set; } = null!;
}
```

#### 0.2 Create Lookup Entities

**New File:** `src/Backend/BOG.DbModel/Entities/Lookups/NotificationMethod.cs`
```csharp
/// <summary>
/// Lookup for طريقة العلم بالقرار (Method of Notification)
/// </summary>
public class NotificationMethod : BaseEntity
{
    public string Name { get; set; } = null!;           // English code
    public string NameAr { get; set; } = null!;         // Arabic name
    public bool IsActive { get; set; } = true;
}
```

Seed data:
1. الإبلاغ بالقرار (Notification of Decision)
2. العلم به (Knowledge of It)
3. الجريدة الرسمية (Official Gazette)

---

**New File:** `src/Backend/BOG.DbModel/Entities/Lookups/GovernmentEntity.cs`
```csharp
/// <summary>
/// Lookup for government entities (جهة إصدار القرار / الجهة المتظلم لها)
/// Used in both Type 1 and Type 2
/// </summary>
public class GovernmentEntity : BaseEntity
{
    public string Name { get; set; } = null!;           // English name
    public string NameAr { get; set; } = null!;         // Arabic name
    public string? Code { get; set; }                    // Entity code
    public bool IsActive { get; set; } = true;
}
```

Seed data: (To be provided by business - examples below)
- وزارة العدل (Ministry of Justice)
- وزارة الداخلية (Ministry of Interior)
- etc.

#### 0.3 Update DbContext

**File:** `src/Backend/BOG.DbModel/ApplicationDbContext.cs`

Add DbSets:
```csharp
// Additional Info lookup tables
public DbSet<NotificationMethod> NotificationMethods { get; set; } = null!;
public DbSet<GovernmentEntity> GovernmentEntities { get; set; } = null!;

// Additional Info type-specific tables
public DbSet<AdditionalInfoManagementDecision> AdditionalInfoManagementDecisions { get; set; } = null!;
public DbSet<AdditionalInfoServiceRights> AdditionalInfoServiceRights { get; set; } = null!;
public DbSet<AdditionalInfoTrademark> AdditionalInfoTrademarks { get; set; } = null!;
```

Configure relationships in `OnModelCreating`:
```csharp
// AdditionalInfo -> ManagementDecision (One-to-One, optional)
modelBuilder.Entity<AdditionalInfo>()
    .HasOne(e => e.ManagementDecision)
    .WithOne(e => e.AdditionalInfo)
    .HasForeignKey<AdditionalInfoManagementDecision>(e => e.AdditionalInfoId)
    .OnDelete(DeleteBehavior.Cascade);

// AdditionalInfo -> ServiceRights (One-to-One)
modelBuilder.Entity<AdditionalInfo>()
    .HasOne(e => e.ServiceRights)
    .WithOne(e => e.AdditionalInfo)
    .HasForeignKey<AdditionalInfoServiceRights>(e => e.AdditionalInfoId)
    .OnDelete(DeleteBehavior.Cascade);

// AdditionalInfo -> Trademark (One-to-One)
modelBuilder.Entity<AdditionalInfo>()
    .HasOne(e => e.Trademark)
    .WithOne(e => e.AdditionalInfo)
    .HasForeignKey<AdditionalInfoTrademark>(e => e.AdditionalInfoId)
    .OnDelete(DeleteBehavior.Cascade);
```

#### 0.4 Create EF Migration

```bash
dotnet ef migrations add AddAdditionalInfoTypes --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
```

#### 0.5 Seed Lookup Data

**Migration file - Up() method:**
```csharp
migrationBuilder.InsertData(
    table: "AdditionalInfoTypes",
    columns: new[] { "Id", "Name", "NameAr", "IsActive", "IsDeleted", "CreatedDate", "ModifiedDate" },
    values: new object[,]
    {
        { 1, "ManagementDecision", "إلغاء قرار إداري", true, false, DateTime.UtcNow, DateTime.UtcNow },
        { 2, "ServiceRights", "حقوق خدمة/تقاعدية", true, false, DateTime.UtcNow, DateTime.UtcNow },
        { 3, "Trademark", "نزاع علامة تجارية", true, false, DateTime.UtcNow, DateTime.UtcNow }
    });
```

---

### Phase 1: Backend View Models & DTOs

#### 1.1 Create AdditionalInfoVM

**File:** `src/Backend/BOG.VM/AdditionalInfo/AdditionalInfoVM.cs`

```csharp
namespace BOG.VM.AdditionalInfo;

public class AdditionalInfoVM
{
    public int Id { get; set; }
    public int AdditionalInfoTypeId { get; set; }
    public string AdditionalInfoTypeName { get; set; } = null!;

    // Type 1: إلغاء قرار إداري (5 fields)
    public string? DecisionNumber { get; set; }                // رقم القرار
    public DateTime? DecisionDate { get; set; }                 // تاريخ القرار
    public DateTime? NotificationDate { get; set; }             // تاريخ العلم بالقرار
    public string? NotificationMethod { get; set; }             // طريقة العلم بالقرار
    public string? IssuingAuthority { get; set; }               // جهة إصدار القرار

    // Type 2: حقوق خدمة/تقاعدية (6 fields)
    public bool? HasComplaint { get; set; }                     // يوجد تظلم
    public string? ComplaintNumber { get; set; }                // رقم التظلم
    public DateTime? ComplaintDate { get; set; }                // تاريخ التظلم
    public string? ComplaintAuthority { get; set; }             // الجهة المتظلم لها
    public DateTime? ComplaintDecisionDate { get; set; }        // تاريخ البت في التظلم
    public string? SystemResult { get; set; }                   // نتيجة النظام

    // Type 3: نزاع علامة تجارية (2 fields)
    public string? RequestNumber { get; set; }                  // رقم الطلب
    public DateTime? RequestDate { get; set; }                  // تاريخه
}
```

#### 1.2 Create DTOs

**File:** `src/Backend/BOG.DTO/AdditionalInfo/AdditionalInfoUpdateDTO.cs`

```csharp
public class AdditionalInfoUpdateDTO
{
    public int AdditionalInfoTypeId { get; set; }

    // Type 1: إلغاء قرار إداري (5 fields)
    public string? DecisionNumber { get; set; }                // رقم القرار
    public DateTime? DecisionDate { get; set; }                 // تاريخ القرار
    public DateTime? NotificationDate { get; set; }             // تاريخ العلم بالقرار
    public string? NotificationMethod { get; set; }             // طريقة العلم بالقرار
    public string? IssuingAuthority { get; set; }               // جهة إصدار القرار

    // Type 2: حقوق خدمة/تقاعدية (6 fields)
    public bool? HasComplaint { get; set; }                     // يوجد تظلم
    public string? ComplaintNumber { get; set; }                // رقم التظلم
    public DateTime? ComplaintDate { get; set; }                // تاريخ التظلم
    public string? ComplaintAuthority { get; set; }             // الجهة المتظلم لها
    public DateTime? ComplaintDecisionDate { get; set; }        // تاريخ البت في التظلم
    public string? SystemResult { get; set; }                   // نتيجة النظام

    // Type 3: نزاع علامة تجارية (2 fields)
    public string? RequestNumber { get; set; }                  // رقم الطلب
    public DateTime? RequestDate { get; set; }                  // تاريخه
}
```

#### 1.3 Update AdditionalInfoBL

**File:** `src/Backend/BOG.BL/Services/CaseRegistration/AdditionalInfoBL.cs`

**Critical Changes for Separate Tables:**

Update `AddOrUpdateAsync` to:
1. **Detect which types have data** (can be zero, one, two, or all three types - ALL OPTIONAL)
2. Create/update base AdditionalInfo record (or return empty if no data)
3. **Save ALL types that have data:**
   - If Type 1 has data: Create/update AdditionalInfoManagementDecision
   - If Type 2 has data: Create/update AdditionalInfoServiceRights
   - If Type 3 has data: Create/update AdditionalInfoTrademark
4. **Delete type-specific records** for types that no longer have data
5. Use eager loading to fetch all type-specific data: `.Include(x => x.ManagementDecision).Include(x => x.ServiceRights).Include(x => x.Trademark)`
6. Map to VM with all type-specific fields (can be all null)

**Example Logic:**
```csharp
// 1. Detect which types have data
bool hasType1Data = !string.IsNullOrEmpty(dto.DecisionNumber) || dto.DecisionDate.HasValue;
bool hasType2Data = dto.HasComplaint.HasValue || !string.IsNullOrEmpty(dto.ComplaintNumber);
bool hasType3Data = !string.IsNullOrEmpty(dto.RequestNumber) || dto.RequestDate.HasValue;

// Note: All types are OPTIONAL - no validation required if none are filled

// 2. Get or create base record
var additionalInfo = await GetOrCreateBaseRecord(requestId);

// If no data in any type, just return empty VM (all types optional)
if (!hasType1Data && !hasType2Data && !hasType3Data)
{
    return MapToViewModel(additionalInfo); // Returns with all null fields
}

// 3. Save each type that has data
if (hasType1Data)
{
    await SaveManagementDecisionAsync(additionalInfo.Id, dto);
}
else
{
    // Delete if exists
    await DeleteManagementDecisionIfExists(additionalInfo.Id);
}

if (hasType2Data)
{
    await SaveServiceRightsAsync(additionalInfo.Id, dto);
}
else
{
    await DeleteServiceRightsIfExists(additionalInfo.Id);
}

if (hasType3Data)
{
    await SaveTrademarkAsync(additionalInfo.Id, dto);
}
else
{
    await DeleteTrademarkIfExists(additionalInfo.Id);
}

// 4. Reload with all relationships
var result = await _repository.GetWithDetailsAsync(additionalInfo.Id);
return MapToViewModel(result);
```

---

### Phase 2: Backend API Controller & Lookups

#### 2.1 Create AdditionalInfoController
**New File:** `src/Backend/BOG.API/Controllers/AdditionalInfoController.cs`

Endpoints:
- `GET /api/case-requests/{requestId}/additional-info` - Get existing data
- `PUT /api/case-requests/{requestId}/additional-info` - Save/update data

#### 2.2 Add Lookups Endpoint for Types
**File:** `src/Backend/BOG.API/Controllers/LookupsController.cs`

Add method:
```csharp
[HttpGet("additional-info-types")]
public async Task<ActionResult<IEnumerable<AdditionalInfoTypeVM>>> GetAdditionalInfoTypes()
{
    // Return active types for dropdown
}
```

---

### Phase 3: Frontend Services

#### 3.1 Create AdditionalInfoApiService
**New File:** `src/Frontend/bog-app/src/app/features/case-registration/services/additional-info-api.service.ts`

**Key Changes:**
- Interface includes `additionalInfoTypeId` field
- Separate optional fields for each type (Type 1, 2, 3)
- Methods: `get(requestId)`, `save(requestId, data)`

#### 3.2 Update LookupsApiService
**File:** `src/Frontend/bog-app/src/app/features/case-registration/services/lookups-api.service.ts` (or create if missing)

Add method:
```typescript
getAdditionalInfoTypes(): Observable<AdditionalInfoTypeVM[]> {
  return this.http.get<AdditionalInfoTypeVM[]>(`${this.baseUrl}/additional-info-types`);
}
```

---

### Phase 4: Frontend Component - ALL TYPES VISIBLE ⚠️

#### 4.1 Update AdditionalInfoFormComponent
**File:** `src/Frontend/bog-app/src/app/features/case-registration/components/additional-info/additional-info-form.component.ts`

**Critical Changes:**
1. **NO Type Dropdown**: Remove type selector - all sections visible at once
2. **All Forms Visible**: Display Type 1, Type 2, AND Type 3 sections simultaneously
3. **Single FormGroup**: One form with all fields from all types
4. **Grid Layout**: Use CSS Grid (2 columns for some fields)
5. **Auto-save**: 2-second debounce on form changes
6. **Smart Save**: Backend determines which type to save based on filled fields

**Component Structure:**
```typescript
export class AdditionalInfoFormComponent implements OnInit {
  infoForm: FormGroup;
  saving = false;
  saveSuccess = false;

  constructor(
    private fb: FormBuilder,
    private additionalInfoApi: AdditionalInfoApiService,
    private snackBar: MatSnackBar
  ) {
    this.infoForm = this.fb.group({
      // Type 1: Management Decision fields
      decisionNumber: ['', Validators.maxLength(50)],
      decisionDate: [null],
      notificationDate: [null],
      notificationMethodId: [null],
      issuingAuthorityId: [null],

      // Type 2: Service/Retirement Rights fields
      hasComplaint: [false],
      complaintNumber: ['', Validators.maxLength(50)],
      complaintDate: [null],
      complaintAuthorityId: [null],
      complaintDecisionDate: [null],
      systemResult: ['', Validators.maxLength(500)],

      // Type 3: Trademark fields
      requestNumber: ['', Validators.maxLength(50)],
      requestDate: [null]
    });
  }

  ngOnInit() {
    this.loadAdditionalInfo();
    if (this.canEdit) {
      this.setupAutoSave();
    }
  }
}
```

#### 4.2 Update Component Template - ALL SECTIONS VISIBLE

**CSS Styling (matching Adobe XD design):**
```css
.grid-2col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.section-box {
  border: 1px solid #c8e6c9;
  border-radius: 4px;
  padding: 20px;
  margin-bottom: 24px;
  background: #ffffff;
}

.section-header {
  color: #2e7d32;
  font-size: 16px;
  font-weight: 500;
  margin: 0 0 20px 0;
  padding: 0;
  text-align: right;
}

.full-width {
  grid-column: 1 / -1;
}

.save-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #666;
  font-size: 14px;
  margin-top: 16px;
}

mat-form-field {
  width: 100%;
}
```

**Template Structure (matching Adobe XD design - each type in separate container):**
```html
<form [formGroup]="infoForm">

  <!-- Type 1: دعاوى إلغاء القرارات الإدارية -->
  <app-section-container
    title="دعاوى إلغاء القرارات الإدارية"
    sectionId="additional-info-type1"
    icon="gavel">
    <div class="grid-2col">
        <mat-form-field appearance="outline">
          <mat-label>رقم القرار</mat-label>
          <input matInput formControlName="decisionNumber">
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>تاريخ القرار</mat-label>
          <input matInput [matDatepicker]="picker1" formControlName="decisionDate">
          <mat-datepicker-toggle matSuffix [for]="picker1"></mat-datepicker-toggle>
          <mat-datepicker #picker1></mat-datepicker>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>طريقة العلم بالقرار</mat-label>
          <mat-select formControlName="notificationMethodId">
            <mat-option [value]="null">اختر</mat-option>
            <mat-option *ngFor="let method of notificationMethods" [value]="method.id">
              {{ method.nameAr }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>تاريخ العلم بالقرار</mat-label>
          <input matInput [matDatepicker]="picker2" formControlName="notificationDate">
          <mat-datepicker-toggle matSuffix [for]="picker2"></mat-datepicker-toggle>
          <mat-datepicker #picker2></mat-datepicker>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>جهة إصدار القرار</mat-label>
          <mat-select formControlName="issuingAuthorityId">
            <mat-option [value]="null">اختر</mat-option>
            <mat-option *ngFor="let entity of governmentEntities" [value]="entity.id">
              {{ entity.nameAr }}
            </mat-option>
          </mat-select>
        </mat-form-field>
      </div>
  </app-section-container>

  <!-- Type 2: دعاوى الحقوق المتعلقة بالخدمة والتقاعد ودعاوى الإلغاء -->
  <app-section-container
    title="دعاوى الحقوق المتعلقة بالخدمة والتقاعد ودعاوى الإلغاء"
    sectionId="additional-info-type2"
    icon="work">
    <div class="grid-2col">
        <mat-form-field appearance="outline">
          <mat-label>يوجد تظلم</mat-label>
          <mat-select formControlName="hasComplaint">
            <mat-option [value]="null">اختر</mat-option>
            <mat-option [value]="true">نعم</mat-option>
            <mat-option [value]="false">لا</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>تاريخ التظلم</mat-label>
          <input matInput [matDatepicker]="picker3" formControlName="complaintDate">
          <mat-datepicker-toggle matSuffix [for]="picker3"></mat-datepicker-toggle>
          <mat-datepicker #picker3></mat-datepicker>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>الجهة المتظلم لها</mat-label>
          <mat-select formControlName="complaintAuthorityId">
            <mat-option [value]="null">اختر</mat-option>
            <mat-option *ngFor="let entity of governmentEntities" [value]="entity.id">
              {{ entity.nameAr }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>رقم التظلم</mat-label>
          <input matInput formControlName="complaintNumber">
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>تاريخ البت في التظلم</mat-label>
          <input matInput [matDatepicker]="picker4" formControlName="complaintDecisionDate">
          <mat-datepicker-toggle matSuffix [for]="picker4"></mat-datepicker-toggle>
          <mat-datepicker #picker4></mat-datepicker>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>نتيجة النظام</mat-label>
          <input matInput formControlName="systemResult">
        </mat-form-field>
      </div>
  </app-section-container>

  <!-- Type 3: نزاع علامة تجارية -->
  <app-section-container
    title="نزاع علامة تجارية"
    sectionId="additional-info-type3"
    icon="store">
    <div class="grid-2col">
        <mat-form-field appearance="outline">
          <mat-label>رقم الطلب</mat-label>
          <input matInput formControlName="requestNumber">
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>تاريخه</mat-label>
          <input matInput [matDatepicker]="picker5" formControlName="requestDate">
          <mat-datepicker-toggle matSuffix [for]="picker5"></mat-datepicker-toggle>
          <mat-datepicker #picker5></mat-datepicker>
        </mat-form-field>
      </div>
  </app-section-container>

  <!-- Auto-save indicator -->
  <div class="save-indicator" *ngIf="saving">
    <mat-spinner diameter="20"></mat-spinner>
    <span>جاري الحفظ التلقائي...</span>
  </div>

</form>
```

---

## Critical Files to Create/Modify

| Priority | File Path | Action | Impact |
|----------|-----------|--------|--------|
| 🔴 **BREAKING** | `BOG.DbModel/Entities/CaseRegistration/AdditionalInfo.cs` | **MODIFY** | Simplify to base table, add nav properties |
| 🔴 **NEW** | `BOG.DbModel/Entities/CaseRegistration/AdditionalInfoManagementDecision.cs` | **CREATE** | Type 1 entity |
| 🔴 **NEW** | `BOG.DbModel/Entities/CaseRegistration/AdditionalInfoServiceRights.cs` | **CREATE** | Type 2 entity |
| 🔴 **NEW** | `BOG.DbModel/Entities/CaseRegistration/AdditionalInfoTrademark.cs` | **CREATE** | Type 3 entity |
| 🔴 **NEW** | `BOG.DbModel/Entities/Lookups/AdditionalInfoType.cs` | **CREATE** | Lookup entity |
| 🔴 Critical | `BOG.DbModel/ApplicationDbContext.cs` | **MODIFY** | Add 4 DbSets, configure relationships |
| 🔴 Critical | **EF Migration** | **CREATE** | Create 3 new tables + seed types |
| 🔴 Critical | `BOG.VM/AdditionalInfo/AdditionalInfoVM.cs` | **MODIFY** | Update with type fields |
| 🔴 Critical | `BOG.DTO/AdditionalInfo/AdditionalInfoUpdateDTO.cs` | **MODIFY** | Update with type fields |
| 🔴 Critical | `BOG.BL/Services/CaseRegistration/AdditionalInfoBL.cs` | **MODIFY** | Type-based validation |
| 🔴 Critical | `BOG.API/Controllers/AdditionalInfoController.cs` | **CREATE** | API endpoints |
| 🔴 Critical | `BOG.API/Controllers/LookupsController.cs` | **MODIFY** | Add types endpoint |
| 🔴 Critical | `bog-app/services/additional-info-api.service.ts` | **CREATE** | Frontend API service |
| 🔴 Critical | `bog-app/components/additional-info/additional-info-form.component.ts` | **REWRITE** | Conditional forms |
| 🔴 Critical | `bog-app/components/additional-info/additional-info-form.component.html` | **REWRITE** | Grid layout, conditional UI |

---

## Implementation Sequence

1. ⚠️ **Phase 0: Database Migration** (BREAKING CHANGE)
   - Update AdditionalInfo entity
   - Create AdditionalInfoType lookup entity
   - Update DbContext
   - Create & run migration
   - Seed lookup data

2. **Phase 1: Backend Layer**
   - Update AdditionalInfoVM with type fields
   - Update AdditionalInfoUpdateDTO
   - Update AdditionalInfoBL with type validation
   - Create AdditionalInfoController
   - Add lookups endpoint for types

3. **Phase 2: Frontend Services**
   - Create AdditionalInfoApiService
   - Update LookupsApiService for types

4. **Phase 3: Frontend Component**
   - Rewrite AdditionalInfoFormComponent
   - Implement conditional forms
   - Add grid layout CSS
   - Implement auto-save

5. **Phase 4: Testing**
   - Test each type's form
   - Verify auto-save
   - Test validation
   - Database verification

---

## Testing & Verification

### Test Case 1: Type 1 - Administrative Decision
```
1. Open Additional Info section
2. Select "إلغاء قرار إداري" from dropdown
3. Fill: Decision Number="123", Decision Date=today, Notification Method="Email", Issuing Authority="وزارة"
4. Wait 2 seconds → Auto-save
5. Refresh page → Verify fields loaded correctly
6. Database check:
   SELECT AdditionalInfoTypeId, DecisionNumber, DecisionDate
   FROM AdditionalInfos
   WHERE CaseRegistrationRequestId = @id
   Expected: TypeId=1, all fields populated
```

### Test Case 2: Type 2 - Service/Retirement Rights
```
1. Select "حقوق خدمة/تقاعدية"
2. Fill complaint fields
3. Verify auto-save
4. Database: TypeId=2, complaint fields populated, decision fields NULL
```

### Test Case 3: Type 3 - Trademark
```
1. Select "نزاع علامة تجارية"
2. Fill trademark fields
3. Verify auto-save
4. Database: TypeId=3, trademark fields populated, others NULL
```

### Test Case 4: Type Switch
```
1. Select Type 1, fill fields, save
2. Switch to Type 2
3. Verify: Type 1 fields cleared, Type 2 form shown empty
4. Fill Type 2, save
5. Database: Should UPDATE (not INSERT), Type 1 fields NULL, Type 2 fields populated
```

### Database Verification Query:
```sql
SELECT
    ai.Id,
    ai.CaseRegistrationRequestId,
    ait.NameAr as TypeName,
    ai.AdditionalInfoTypeId,
    ai.DecisionNumber,
    ai.DecisionDate,
    ai.ComplaintNumber,
    ai.TrademarkRequestNumber
FROM AdditionalInfos ai
INNER JOIN AdditionalInfoTypes ait ON ai.AdditionalInfoTypeId = ait.Id
WHERE ai.CaseRegistrationRequestId = @requestId
  AND ai.IsDeleted = 0;
```

---

## Success Criteria

- ✅ Type dropdown loads 3 options from API
- ✅ Conditional forms display based on type selection
- ✅ Grid layout (2 columns) matches Adobe XD design
- ✅ Type-specific validation works (required fields enforced)
- ✅ Auto-save triggers 2 seconds after changes
- ✅ Switching types clears old form and shows new form
- ✅ Database stores type-specific fields correctly (nulls for inactive types)
- ✅ Page reload loads saved type and fields
- ✅ Arabic labels match SRS specification

---

## Risks & Mitigation

**Risk 1: Breaking Change to Database**
- **Impact**: Existing AdditionalInfo records have old schema
- **Mitigation**: Migration will add new columns as nullable, old data preserved but not usable
- **Recommendation**: Clear test data before migration

**Risk 2: Complex Frontend Logic**
- **Impact**: 3 separate form groups, conditional validation
- **Mitigation**: Use getter for activeForm, centralize validation logic

**Risk 3: User Confusion (Type Selection)**
- **Impact**: User doesn't know which type to select
- **Mitigation**: Add help text/tooltips explaining each type, default to most common type

---

## Estimated Timeline

- **Phase 0 (Database)**: 2 hours (entity, migration, testing)
- **Phase 1 (Backend)**: 3 hours (VM, DTO, BL validation, API)
- **Phase 2 (Frontend Services)**: 1 hour
- **Phase 3 (Frontend Component)**: 4 hours (complex conditional logic)
- **Phase 4 (Testing)**: 2 hours (all 3 types + switching)

**Total: 12 hours** (significant increase due to conditional forms complexity)
