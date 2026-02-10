# Implementation Plan: Deficiencies Tab (UC 6.5.1.1.18 & 6.5.1.1.19)

## Context

This plan implements two use cases from the SRS for the BOG (Legal Case Management) system:

- **UC 6.5.1.1.18 - استكمال نواقص (Complete Deficiencies)**: Allows موظف القيد (Registration Officer) to add deficiencies to a case registration request
- **UC 6.5.1.1.19 - استعراض نواقص (View Deficiencies)**: Allows viewing of recorded deficiencies

### Why This Change Is Needed

Currently, the deficiencies tab exists as a placeholder stub showing only a deadline warning. When موظف القيد reviews a case registration request and finds missing or incomplete information, they need to:

1. Document specific deficiencies (what's missing and where)
2. Save them to the system (NO status change, NO notifications)

This implementation completes the deficiency management workflow by:
- Allowing موظف القيد to add/edit/delete deficiencies
- Providing a dedicated tab for viewing and managing deficiencies
- Following the existing architectural patterns used by Claims and Related Cases tabs

### SRS Requirements

**Deficiency Data Model:**
- **Deficiency Location** (required): One of 5 types
  - الموضوع (Subject)
  - الطلبات (Claims)
  - البيّنات (Evidence)
  - الدعاوى المرتبطة (Related Cases)
  - المرفقات (Attachments)
- **Description** (required): Max 2000 characters describing what's missing

**Business Rules:**
- Only موظف القيد can add/edit/delete deficiencies
- Adding deficiencies does NOT change request status or send notifications
- Deficiencies are independent records

---

## Implementation Approach

### Pattern Selection: Batch Update (Pattern A)

Following the existing Claims and Related Cases implementation pattern:
- Component manages local state via `CaseDataStateService`
- Single GET endpoint to retrieve deficiencies
- Single PUT endpoint for batch update (replaces all deficiencies)
- Changes saved when user clicks "Save"

**Justification:**
- موظف القيد typically adds all deficiencies at once during review
- Reduces API calls compared to individual POST/PUT/DELETE endpoints
- Consistent with similar features (claims, related cases)

### Architecture Layers

```
Frontend (Angular)
├── deficiency.model.ts (interfaces)
├── deficiency-api.service.ts (HTTP client)
├── case-data-state.service.ts (state management) [UPDATE]
├── deficiencies-list.component (main display)
└── deficiency-form-dialog.component (add/edit form)

Backend (.NET)
├── RequestDeficiency entity (database model)
├── DeficiencyType lookup entity (5 types)
├── DeficiencyRepository (data access)
├── DeficiencyBL (business logic)
├── DeficienciesController (API endpoints)
└── RequestActionBL [UPDATE] (integration)
```

---

## Database Schema Design

### New Entities

**DeficiencyType (Lookup Table)**
```
Id: int (PK)
Name: string (Subject, Claims, Evidence, RelatedCases, Attachments)
NameAr: string (Arabic names)
Description: string (nullable)
DisplayOrder: int
IsActive: bool
+ BaseEntity fields (IsDeleted, CreatedDate, ModifiedDate)
```

**RequestDeficiency**
```
Id: int (PK)
CaseRegistrationRequestId: int (FK)
DeficiencyTypeId: int (FK)
Description: string (max 2000 chars)
DisplayOrder: int
+ BaseEntity fields (IsDeleted, CreatedDate, ModifiedDate)
```

**Seed Data for DeficiencyType:**
1. Subject / الموضوع
2. Claims / الطلبات
3. Evidence / البيّنات
4. RelatedCases / الدعاوى المرتبطة
5. Attachments / المرفقات

### Migration

```bash
dotnet ef migrations add AddDeficienciesSupport \
  --project src/Backend/BOG.DbModel \
  --startup-project src/Backend/BOG.API
```

---

## API Endpoints

### DeficienciesController

**GET /api/case-requests/{requestId}/deficiencies**
- Returns: `IEnumerable<DeficiencyVM>`
- Purpose: Retrieve all deficiencies for a request
- Access: موظف القيد only

**PUT /api/case-requests/{requestId}/deficiencies**
- Body: `DeficienciesBatchUpdateDTO { deficiencies: DeficiencyDTO[] }`
- Returns: `IEnumerable<DeficiencyVM>`
- Purpose: Batch update deficiencies (replaces all)
- Access: موظف القيد only
- Validation: No status change

### LookupsController

**GET /api/lookups/deficiency-types**
- Returns: `IEnumerable<DeficiencyTypeVM>`
- Purpose: Get dropdown options for deficiency types
- Access: موظف القيد

---

## Frontend Components

### deficiency-form-dialog.component

**Purpose:** Dialog for adding/editing a single deficiency

**Form Fields:**
- Deficiency Type (mat-select dropdown) - 5 options
- Description (textarea) - with character counter (X/2000)

**Modes:**
- `create`: Add new deficiency to local state
- `edit`: Update existing deficiency in local state
- `view`: Read-only display

**Validation:**
- Required: deficiencyTypeId, description
- Max length: 2000 characters

### deficiencies-list.component (Update Existing Stub)

**Purpose:** Main tab component for viewing/managing deficiencies

**Current State:** Shows only deadline warning
**New Features:**
- Table/list of deficiencies with columns:
  - Deficiency Type (Arabic name)
  - Description (truncated with "show more")
  - Created date
  - Actions (edit/delete)
- "Add Deficiency" floating action button
- Empty state message when no deficiencies

**Inputs:**
- `@Input() requestId: number`
- `@Input() canEdit: boolean` (true only for موظف القيد)
- `@Input() currentStatus: number`

**Data Source:** `CaseDataStateService.getDeficiencies()`

### Integration Points

**request-details.component**
- Show deficiencies tab for موظف القيد
- Pass `canEdit = true` only for موظف القيد
- Pass `currentStatus` for context

---

## Critical Files

### Backend (Create New)

1. **src/Backend/BOG.DbModel/Entities/Lookups/DeficiencyType.cs**
   - Lookup entity for 5 deficiency types

2. **src/Backend/BOG.DbModel/Entities/CaseRegistration/RequestDeficiency.cs**
   - Entity storing deficiency records

3. **src/Backend/BOG.DAL/Interfaces/IDeficiencyRepository.cs**
   - Repository interface

4. **src/Backend/BOG.DAL/Repositories/DeficiencyRepository.cs**
   - Repository implementation with soft delete

5. **src/Backend/BOG.DTO/CaseRegistration/DeficienciesBatchUpdateDTO.cs**
   - DTO for batch update

6. **src/Backend/BOG.VM/CaseRegistration/DeficiencyVM.cs**
   - View model for responses

7. **src/Backend/BOG.VM/Lookups/DeficiencyTypeVM.cs**
   - View model for lookup data

8. **src/Backend/BOG.BL/Interfaces/CaseRegistration/IDeficiencyBL.cs**
   - Business logic interface

9. **src/Backend/BOG.BL/Services/CaseRegistration/DeficiencyBL.cs**
   - Business logic implementation

10. **src/Backend/BOG.API/Controllers/DeficienciesController.cs**
    - API controller (GET, PUT endpoints)

### Backend (Modify Existing)

11. **src/Backend/BOG.DbModel/ApplicationDbContext.cs**
    - Add DbSets and seed data

12. **src/Backend/BOG.DbModel/Entities/CaseRegistration/CaseRegistrationRequest.cs**
    - Add navigation property

13. **src/Backend/BOG.DAL/Interfaces/IUnitOfWork.cs**
    - Register repository

14. **src/Backend/BOG.DAL/Repositories/UnitOfWork.cs**
    - Implement repository registration

15. **src/Backend/BOG.DTO/CaseRegistration/RequestDeficiencyDTO.cs**
    - Add validation attributes

16. **src/Backend/BOG.API/Controllers/LookupsController.cs**
    - Add GetDeficiencyTypes endpoint

17. **src/Backend/BOG.API/Extensions/ServiceCollectionExtensions.cs**
    - Register DI services

### Frontend (Create New)

18. **src/Frontend/bog-app/src/app/features/case-registration/models/deficiency.model.ts**
    - TypeScript interfaces

19. **src/Frontend/bog-app/src/app/features/case-registration/services/deficiency-api.service.ts**
    - HTTP service

20. **src/Frontend/bog-app/src/app/features/case-registration/components/deficiencies/deficiency-form-dialog/deficiency-form-dialog.component.ts**
    - Dialog component for add/edit/view

21. **src/Frontend/bog-app/src/app/features/case-registration/components/deficiencies/deficiency-form-dialog/deficiency-form-dialog.component.html**
    - Dialog template

22. **src/Frontend/bog-app/src/app/features/case-registration/components/deficiencies/deficiency-form-dialog/deficiency-form-dialog.component.scss**
    - Dialog styles

### Frontend (Modify Existing)

23. **src/Frontend/bog-app/src/app/features/case-registration/services/case-data-state.service.ts**
    - Add deficiencies state management

24. **src/Frontend/bog-app/src/app/features/case-registration/components/deficiencies/deficiencies-list.component.ts**
    - Full implementation

25. **src/Frontend/bog-app/src/app/features/case-registration/components/deficiencies/deficiencies-list.component.html**
    - Table/list display

26. **src/Frontend/bog-app/src/app/features/case-registration/components/deficiencies/deficiencies-list.component.scss**
    - Table styles

27. **src/Frontend/bog-app/src/app/features/case-registration/case-registration.module.ts**
    - Declare components

---

## Implementation Sequence

### Phase 1: Database Foundation (1 day)
1. Create `DeficiencyType.cs` entity
2. Create `RequestDeficiency.cs` entity
3. Update `ApplicationDbContext.cs`
4. Update `CaseRegistrationRequest.cs`
5. Create migration and update database
6. Verify seed data

### Phase 2: Data Access Layer (0.5 day)
7. Create `IDeficiencyRepository.cs`
8. Create `DeficiencyRepository.cs`
9. Update `IUnitOfWork.cs` and `UnitOfWork.cs`

### Phase 3: DTOs and View Models (0.5 day)
10. Update `RequestDeficiencyDTO.cs`
11. Create `DeficienciesBatchUpdateDTO.cs`
12. Create `DeficiencyVM.cs`
13. Create `DeficiencyTypeVM.cs`

### Phase 4: Business Logic (1 day)
14. Create `IDeficiencyBL.cs`
15. Create `DeficiencyBL.cs`

### Phase 5: API Layer (0.5 day)
16. Create `DeficienciesController.cs`
17. Update `LookupsController.cs`
18. Update `ServiceCollectionExtensions.cs`

### Phase 6: Frontend Models & Services (0.5 day)
19. Create `deficiency.model.ts`
20. Create `deficiency-api.service.ts`
21. Update `case-data-state.service.ts`

### Phase 7: Frontend Components (1.5 days)
22. Create `deficiency-form-dialog.component`
23. Update `deficiencies-list.component`
24. Update `case-registration.module.ts`

### Phase 8: Integration & Testing (1 day)
25. End-to-end testing
26. Test add/edit/delete deficiencies
27. Test batch update
28. UI/UX refinements

**Total Estimated Time: 6 days**

---

## Reusable Existing Code

### Backend Patterns
- **Repository Pattern**: `src/Backend/BOG.DAL/Repositories/ClaimRepository.cs`
- **Business Logic Service**: `src/Backend/BOG.BL/Services/CaseRegistration/ClaimBL.cs`
- **Controller**: `src/Backend/BOG.API/Controllers/ClaimsController.cs`

### Frontend Patterns
- **Form Dialog**: `src/Frontend/bog-app/src/app/features/case-registration/components/claims/claim-form-dialog/claim-form-dialog.component.ts`
- **List Component**: `src/Frontend/bog-app/src/app/features/case-registration/components/claims/claims-list/claims-list.component.ts`
- **State Service**: `src/Frontend/bog-app/src/app/features/case-registration/services/case-data-state.service.ts`
- **API Service**: `src/Frontend/bog-app/src/app/features/case-registration/services/claims-api.service.ts`

---

## Validation & Error Handling

### Backend Validations
- Required fields: `DeficiencyTypeId`, `Description`
- Max length: 2000 characters for description
- Valid deficiency type ID (1-5)

### Frontend Validations
- Form-level required field validation
- Character counter with max 2000 enforcement
- Inline error messages
- Disabled save button until valid

### Business Rules
- Only موظف القيد can add/edit deficiencies (check user permissions)
- NO status changes
- NO notifications

---

## Verification

### How to Test End-to-End

**As موظف القيد (Registration Officer):**
1. Navigate to a case registration request
2. Click "Deficiencies" tab (or access via dedicated interface)
3. Click "Add Deficiency" button
4. Add deficiencies:
   - Select deficiency type (e.g., "الموضوع")
   - Enter description (e.g., "الموضوع غير واضح، يرجى إضافة تفاصيل أكثر")
   - Add multiple deficiencies
5. Save deficiencies
6. Verify no status change occurs
7. Edit deficiency → Modify description → Save
8. Delete deficiency → Confirm deletion → Verify removed

**Database Verification:**
```sql
-- Check DeficiencyTypes table has 5 seed records
SELECT * FROM DeficiencyTypes WHERE IsDeleted = 0;

-- Check RequestDeficiencies table has records
SELECT rd.*, dt.NameAr, crr.RequestNumber
FROM RequestDeficiencies rd
JOIN DeficiencyTypes dt ON rd.DeficiencyTypeId = dt.Id
JOIN CaseRegistrationRequests crr ON rd.CaseRegistrationRequestId = crr.Id
WHERE rd.IsDeleted = 0;

-- Check soft delete works
SELECT * FROM RequestDeficiencies WHERE IsDeleted = 1;
```

**API Verification:**
```bash
# Get deficiencies for request
GET /api/case-requests/123/deficiencies

# Update deficiencies (موظف القيد only)
PUT /api/case-requests/123/deficiencies
{
  "deficiencies": [
    { "deficiencyTypeId": 1, "description": "الموضوع غير واضح" },
    { "deficiencyTypeId": 3, "description": "البيّنات ناقصة" }
  ]
}

# Get deficiency types
GET /api/lookups/deficiency-types
```

---

## Notes

- This implementation follows the existing Claims/Related Cases pattern for consistency
- Deficiencies are stored separately in their own table for flexibility
- Soft delete is used throughout for audit trail preservation
- **NO automatic status changes or notifications** - deficiencies are added independently
- Only موظف القيد can manage deficiencies (role-based access control)
