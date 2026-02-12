# Fix "Save Failed" Error - Partial Data Save Issue

## Problem Statement

When clicking the Save button on case registration request updates:
- ❌ Error message "An error occurred while updating the request" is displayed
- ✅ Some data IS being saved to the database
- ❌ **Subject field** is NOT saved
- ❌ **Classifications** are NOT saved
- ❌ **Contact information** (email, mobile) is NOT saved
- ✅ Other fields (evidence, court, case type, etc.) ARE saved successfully

## Root Cause Analysis

### Critical Issue: Conditional Dictionary Building in Backend

**Location:** `src/Backend/BOG.BL/Services/CaseRegistration/CaseRegistrationBL.cs` (Lines 151-168)

**The Problem:**
```csharp
if (requestData is CaseRegistrationUpdateDTO updateDto)
{
    requestDict = new Dictionary<string, object>();

    // PROBLEM: These conditions filter out empty values
    if (!string.IsNullOrWhiteSpace(updateDto.Subject))
        requestDict["subject"] = updateDto.Subject;

    if (!string.IsNullOrWhiteSpace(updateDto.Evidence))
        requestDict["evidence"] = updateDto.Evidence;

    if (!string.IsNullOrWhiteSpace(updateDto.PrimaryMobile))
        requestDict["primaryMobile"] = updateDto.PrimaryMobile;

    if (!string.IsNullOrWhiteSpace(updateDto.SecondaryMobile))
        requestDict["secondaryMobile"] = updateDto.SecondaryMobile;

    if (!string.IsNullOrWhiteSpace(updateDto.Email))
        requestDict["email"] = updateDto.Email;

    if (updateDto.ClassificationIds != null)
        requestDict["classificationIds"] = updateDto.ClassificationIds;
}
```

**Why This Breaks:**
1. Frontend sends complete payload with all fields (including empty strings for unfilled fields)
2. Backend filters these out with `!string.IsNullOrWhiteSpace()` checks
3. Filtered fields are **NOT added** to `requestDict`
4. Update logic (lines 180-284) uses `if (requestDict.ContainsKey(...))` to decide what to update
5. Since keys don't exist in dictionary, the update code is **never executed**
6. Database fields remain unchanged

**Example Flow:**
```
User fills: Subject = "My Case"
Frontend sends: { subject: "My Case", evidence: "", email: "", ... }

Backend processing:
- Line 154: !string.IsNullOrWhiteSpace("My Case") = TRUE → subject added to dict ✓
- Line 156: !string.IsNullOrWhiteSpace("") = FALSE → evidence NOT added ✗
- Line 164: !string.IsNullOrWhiteSpace("") = FALSE → email NOT added ✗

Update logic:
- Line 180: requestDict.ContainsKey("subject") = TRUE → Subject UPDATED ✓
- Line 183: requestDict.ContainsKey("evidence") = FALSE → Evidence SKIPPED ✗
- Line 270: requestDict.ContainsKey("email") = FALSE → Email SKIPPED ✗

Result: Subject saved, but Evidence and Email not saved
```

### Secondary Issues

**Issue 2: Frontend State Update Conditional**
- **File:** `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/contact-info/contact-info-form.component.ts`
- **Line:** 71
- **Problem:** Contact info only updates state if form is valid OR completely empty
- **Impact:** Invalid data prevents state from updating, causing stale values to be sent on Save

**Issue 3: Debounce Race Condition**
- **Files:** `subject-evidence-form.component.ts` (debounce 1000ms), `contact-info-form.component.ts` (debounce 500ms)
- **Problem:** If user clicks Save immediately after typing, debounce may not have fired yet
- **Impact:** Outdated/empty values sent in Save request

**Issue 4: Classifications Empty Array Handling**
- **Line:** 160-161 in BL
- **Problem:** Empty array `[]` should clear classifications, but may not if frontend sends `null` or omits field
- **Impact:** Classifications can't be cleared by user

## Solution Plan

### Phase 1: Fix Backend Dictionary Building (CRITICAL)

**File to Modify:** `src/Backend/BOG.BL/Services/CaseRegistration/CaseRegistrationBL.cs`

**Line Range:** 151-168

**Change:** Remove conditional checks and always add DTO fields to dictionary

**Before:**
```csharp
if (!string.IsNullOrWhiteSpace(updateDto.Subject))
    requestDict["subject"] = updateDto.Subject;

if (!string.IsNullOrWhiteSpace(updateDto.Evidence))
    requestDict["evidence"] = updateDto.Evidence;

if (!string.IsNullOrWhiteSpace(updateDto.PrimaryMobile))
    requestDict["primaryMobile"] = updateDto.PrimaryMobile;

if (!string.IsNullOrWhiteSpace(updateDto.SecondaryMobile))
    requestDict["secondaryMobile"] = updateDto.SecondaryMobile;

if (!string.IsNullOrWhiteSpace(updateDto.Email))
    requestDict["email"] = updateDto.Email;

if (updateDto.ClassificationIds != null)
    requestDict["classificationIds"] = updateDto.ClassificationIds;
```

**After:**
```csharp
// Always add fields to dictionary - let the update logic handle null/empty
requestDict["subject"] = updateDto.Subject;
requestDict["evidence"] = updateDto.Evidence;
requestDict["primaryMobile"] = updateDto.PrimaryMobile;
requestDict["secondaryMobile"] = updateDto.SecondaryMobile;
requestDict["email"] = updateDto.Email;
requestDict["classificationIds"] = updateDto.ClassificationIds ?? new List<int>();
requestDict["courtId"] = updateDto.CourtId;
requestDict["caseTypeId"] = updateDto.CaseTypeId;
requestDict["notes"] = updateDto.Notes;
```

**Rationale:**
- The update logic (lines 180-284) already handles null/empty values appropriately
- Removing the conditional checks ensures all fields are considered for update
- If a field is null/empty, the database will handle it according to constraints
- This allows users to clear fields by sending empty values

### Phase 2: Update Field Processing Logic

**File to Modify:** `src/Backend/BOG.BL/Services/CaseRegistration/CaseRegistrationBL.cs`

**Section 1: Subject and Evidence Updates (Lines 180-189)**

Keep existing logic but ensure it executes now that keys are always in dictionary:
```csharp
// Subject - validate length if provided
if (requestDict.ContainsKey("subject"))
{
    var subject = requestDict["subject"]?.ToString();
    if (!string.IsNullOrWhiteSpace(subject))
    {
        if (subject.Length > 4000)
            throw new ArgumentException("Subject cannot exceed 4000 characters");
        request.Subject = subject;
    }
    // If empty, keep existing value (don't clear required field)
}

// Evidence - validate length if provided
if (requestDict.ContainsKey("evidence"))
{
    var evidence = requestDict["evidence"]?.ToString();
    if (!string.IsNullOrWhiteSpace(evidence))
    {
        if (evidence.Length > 4000)
            throw new ArgumentException("Evidence cannot exceed 4000 characters");
        request.Evidence = evidence;
    }
    // If empty, keep existing value (don't clear required field)
}
```

**Section 2: Contact Information Updates (Lines 268-284)**

Add validation and handle empty values:
```csharp
// Primary Mobile
if (requestDict.ContainsKey("primaryMobile"))
{
    var mobile = requestDict["primaryMobile"]?.ToString();
    if (!string.IsNullOrWhiteSpace(mobile))
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(mobile, @"^05\d{8}$"))
            throw new ArgumentException("Primary mobile must be 10 digits starting with 05");
        request.PrimaryMobile = mobile;
    }
    else
    {
        request.PrimaryMobile = null; // Allow clearing optional field
    }
}

// Secondary Mobile
if (requestDict.ContainsKey("secondaryMobile"))
{
    var mobile = requestDict["secondaryMobile"]?.ToString();
    if (!string.IsNullOrWhiteSpace(mobile))
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(mobile, @"^05\d{8}$"))
            throw new ArgumentException("Secondary mobile must be 10 digits starting with 05");
        request.SecondaryMobile = mobile;
    }
    else
    {
        request.SecondaryMobile = null; // Allow clearing optional field
    }
}

// Email
if (requestDict.ContainsKey("email"))
{
    var email = requestDict["email"]?.ToString();
    if (!string.IsNullOrWhiteSpace(email))
    {
        if (email.Length > 255)
            throw new ArgumentException("Email cannot exceed 255 characters");
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            throw new ArgumentException("Invalid email format");
        request.Email = email;
    }
    else
    {
        request.Email = null; // Allow clearing optional field
    }
}
```

**Section 3: CourtId and CaseTypeId Updates (NEW - Add after line 189)**

These fields are currently NOT being updated from the DTO:
```csharp
// CourtId - validate court exists
if (requestDict.ContainsKey("courtId"))
{
    if (int.TryParse(requestDict["courtId"]?.ToString(), out int courtId) && courtId > 0)
    {
        // Validate court exists
        var courtExists = await _unitOfWork.Courts.AnyAsync(
            c => c.Id == courtId && c.IsActive && !c.IsDeleted,
            cancellationToken);

        if (!courtExists)
            throw new ArgumentException($"Court with ID {courtId} not found");

        request.CourtId = courtId;
    }
}

// CaseTypeId - validate case type exists
if (requestDict.ContainsKey("caseTypeId"))
{
    if (int.TryParse(requestDict["caseTypeId"]?.ToString(), out int caseTypeId) && caseTypeId > 0)
    {
        // Validate case type exists (1=إداري, 2=تأديبي)
        var caseTypeExists = await _unitOfWork.CaseTypes.AnyAsync(
            ct => ct.Id == caseTypeId && ct.IsActive && !ct.IsDeleted,
            cancellationToken);

        if (!caseTypeExists)
            throw new ArgumentException($"Case type with ID {caseTypeId} not found");

        request.CaseTypeId = caseTypeId;
    }
}

// Notes - optional field
if (requestDict.ContainsKey("notes"))
{
    var notes = requestDict["notes"]?.ToString();
    if (!string.IsNullOrWhiteSpace(notes))
    {
        if (notes.Length > 2000)
            throw new ArgumentException("Notes cannot exceed 2000 characters");
        request.Notes = notes;
    }
    else
    {
        request.Notes = null; // Allow clearing
    }
}
```

**Section 4: Classifications Update (Lines 190-265)**

The existing logic should work correctly now that `classificationIds` is always in the dictionary:
```csharp
// Ensure empty array clears classifications
if (requestDict.ContainsKey("classificationIds"))
{
    var classificationIds = requestDict["classificationIds"] as System.Collections.IEnumerable;
    if (classificationIds != null)
    {
        var idList = new List<int>();
        foreach (var classId in classificationIds)
        {
            if (int.TryParse(classId?.ToString(), out int id) && id > 0)
                idList.Add(id);
        }

        if (idList.Any())
        {
            // Existing validation and save logic (lines 207-246)
        }
        else
        {
            // Empty array - soft delete all classifications (lines 250-257)
            // This should now work correctly
        }
    }
}
```

### Phase 3: Improve Error Handling in Controller

**File to Modify:** `src/Backend/BOG.API/Controllers/CaseRegistrationController.cs`

**Line Range:** 147-181 (UpdateRequest method)

**Changes:**

1. Add specific exception handling for ArgumentException:
```csharp
catch (ArgumentException argEx)
{
    _logger.LogWarning("Validation failed for request {RequestId}: {Message}", id, argEx.Message);
    return BadRequest(new { message = argEx.Message });
}
```

2. Add specific exception handling for DbUpdateException:
```csharp
catch (DbUpdateException dbEx)
{
    _logger.LogError(dbEx, "Database error updating request {RequestId}", id);
    var innerMessage = dbEx.InnerException?.Message ?? "";

    if (innerMessage.Contains("FK_"))
        return BadRequest(new { message = "خطأ في البيانات المرجعية: تأكد من صحة المحكمة ونوع الدعوى" });
    if (innerMessage.Contains("String or binary data would be truncated"))
        return BadRequest(new { message = "البيانات المدخلة تتجاوز الحد المسموح" });

    return StatusCode(500, new { message = "خطأ في قاعدة البيانات" });
}
```

3. Improve generic exception logging:
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error updating request {RequestId}: {ExceptionType}",
        id, ex.GetType().Name);
    return StatusCode(500, new {
        message = "حدث خطأ أثناء تحديث الطلب",
        details = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            ? ex.Message : null
    });
}
```

### Phase 4: Fix Frontend State Update Issue

**File to Modify:** `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/contact-info/contact-info-form.component.ts`

**Line:** 71

**Change:** Always update state regardless of validation (let backend validate)

**Before:**
```typescript
.subscribe(values => {
  if (this.contactForm.valid || this.isFormEmpty()) {
    this.caseDataState.updateContactInfo(
      values.primaryMobile || '',
      values.secondaryMobile || '',
      values.email || ''
    );
  }
});
```

**After:**
```typescript
.subscribe(values => {
  // Always update state - backend will validate
  this.caseDataState.updateContactInfo(
    values.primaryMobile || '',
    values.secondaryMobile || '',
    values.email || ''
  );
});
```

### Phase 5: Add Validation to DAL

**File to Modify:** `src/Backend/BOG.DAL/Interfaces/IUnitOfWork.cs`

Add property for Courts repository:
```csharp
IRepository<Court> Courts { get; }
IRepository<CaseType> CaseTypes { get; }
```

**File to Modify:** `src/Backend/BOG.DAL/Repositories/UnitOfWork.cs`

Implement the properties:
```csharp
public IRepository<Court> Courts => _courtRepository ??= new Repository<Court>(_context);
public IRepository<CaseType> CaseTypes => _caseTypeRepository ??= new Repository<CaseType>(_context);

private IRepository<Court> _courtRepository;
private IRepository<CaseType> _caseTypeRepository;
```

### Phase 6: Improve Frontend Error Display

**File to Modify:** `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.ts`

**Line Range:** 153-159 (error handling)

**Change:** Extract and display detailed error messages

**Before:**
```typescript
this.saveError = error?.error?.message || 'حدث خطأ أثناء حفظ البيانات';
console.error('Error saving case data:', error);
this.snackBar.open(this.saveError || 'حدث خطأ أثناء حفظ البيانات', 'إغلاق', { duration: 5000 });
```

**After:**
```typescript
// Extract detailed error message
let errorMessage = 'حدث خطأ أثناء حفظ البيانات';

if (error?.error?.message) {
    errorMessage = error.error.message;
}

// Log full error for debugging
console.error('Error saving case data:', error);
console.error('Error response:', error?.error);

this.saveError = errorMessage;
this.snackBar.open(errorMessage, 'إغلاق', {
    duration: 7000,  // Longer for detailed messages
    panelClass: ['error-snackbar']
});

this.saveComplete.emit({ success: false, error: errorMessage });
```

## Critical Files to Modify

### Backend Files:

1. **src/Backend/BOG.BL/Services/CaseRegistration/CaseRegistrationBL.cs**
   - Lines 151-168: Remove conditional checks in dictionary building
   - Lines 180-189: Add validation for subject/evidence
   - After line 189: Add CourtId, CaseTypeId, Notes handling
   - Lines 268-284: Add validation for contact info
   - Ensure classifications logic works with empty arrays

2. **src/Backend/BOG.API/Controllers/CaseRegistrationController.cs**
   - Lines 147-181: Add specific exception handlers for ArgumentException, DbUpdateException
   - Improve generic exception logging

3. **src/Backend/BOG.DAL/Interfaces/IUnitOfWork.cs**
   - Add Courts and CaseTypes repository properties

4. **src/Backend/BOG.DAL/Repositories/UnitOfWork.cs**
   - Implement Courts and CaseTypes repository properties

### Frontend Files:

5. **src/Frontend/bog-app/src/app/features/case-registration/components/case-data/contact-info/contact-info-form.component.ts**
   - Line 71: Remove validation condition, always update state

6. **src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.ts**
   - Lines 153-159: Improve error message extraction and display

## Testing Strategy

### Test Case 1: Subject Field Update
1. Create a draft request
2. Fill in Subject field: "Test Subject"
3. Click Save
4. **Expected:** Subject saved successfully, no error
5. **Verify:** Database shows Subject = "Test Subject"

### Test Case 2: Contact Information Update
1. Edit existing request
2. Fill in Email: "test@example.com"
3. Fill in Primary Mobile: "0512345678"
4. Click Save
5. **Expected:** Contact info saved, no error
6. **Verify:** Database shows updated email and mobile

### Test Case 3: Classifications Update
1. Edit existing request
2. Select 2 classifications
3. Click Save
4. **Expected:** Classifications saved
5. **Verify:** RequestClassification table has 2 active records

### Test Case 4: Clear Classifications
1. Edit request with existing classifications
2. Remove all classifications (empty array)
3. Click Save
4. **Expected:** All classifications cleared (soft deleted)
5. **Verify:** RequestClassification records have IsDeleted = true

### Test Case 5: Invalid Data Validation
1. Fill in Email: "invalid-email"
2. Click Save
3. **Expected:** Error message "Invalid email format"
4. **Verify:** Data not saved

### Test Case 6: String Length Validation
1. Fill in Subject with 5000 characters
2. Click Save
3. **Expected:** Error "Subject cannot exceed 4000 characters"
4. **Verify:** Data not saved

### Test Case 7: Invalid Court ID
1. Manually send API request with CourtId: 999
2. **Expected:** Error "Court with ID 999 not found"
3. **Verify:** Request not updated

### Test Case 8: All Fields Together
1. Update Subject, Evidence, Classifications, Contact Info, CourtId, CaseTypeId, Notes
2. Click Save
3. **Expected:** All fields saved successfully
4. **Verify:** Database shows all updated values

## Verification Steps

### Step 1: Build and Test Backend
```bash
cd C:\Users\Lenovo\Desktop\Claude\BOG\src\Backend
dotnet build BOG.sln
dotnet run --project BOG.API
```
- Verify no compilation errors
- Check API starts successfully on port 5001

### Step 2: Test API Endpoints
```bash
# Create a test request
curl -X POST http://localhost:5001/api/case-requests \
  -H "Content-Type: application/json" \
  -d '{
    "courtId": 1,
    "subject": "Test Subject",
    "evidence": "Test Evidence",
    "caseTypeId": 1
  }'

# Update the request (replace {id} with returned ID)
curl -X PUT http://localhost:5001/api/case-requests/{id} \
  -H "Content-Type: application/json" \
  -d '{
    "subject": "Updated Subject",
    "evidence": "Updated Evidence",
    "primaryMobile": "0512345678",
    "email": "test@example.com",
    "classificationIds": [1, 2],
    "courtId": 2,
    "caseTypeId": 1,
    "notes": "Test notes"
  }'
```
- Verify 200 OK response
- Verify all fields updated in database

### Step 3: Test Frontend
```bash
cd C:\Users\Lenovo\Desktop\Claude\BOG\src\Frontend\bog-app
ng serve
```
- Navigate to http://localhost:4200/case-registration/{id}
- Edit Subject, Evidence, Contact Info, Classifications
- Click Save
- Verify no error message
- Verify success message displayed
- Refresh page and verify data persisted

### Step 4: Test Error Scenarios
1. Enter invalid email → Verify error message
2. Enter invalid mobile → Verify error message
3. Enter subject > 4000 chars → Verify error message
4. All scenarios should show specific error, not generic "Save failed"

### Step 5: Check Logs
```bash
# Backend should log:
- "Received update DTO with fields: ..."
- "Request dict keys: subject, evidence, email, ..."
- Validation errors as warnings
- Database errors as errors
```

## Success Criteria

✅ **Subject field saves correctly**
✅ **Classifications save and can be cleared**
✅ **Contact information (email, mobile) saves correctly**
✅ **No false "Save failed" error when save actually succeeds**
✅ **Specific validation error messages displayed instead of generic error**
✅ **All DTO fields (Subject, Evidence, Contact, CourtId, CaseTypeId, Notes, Classifications) are processed**
✅ **Empty values can clear optional fields**
✅ **Invalid data shows helpful error messages**
✅ **Backend logs show detailed information for debugging**

## Rollback Plan

If issues occur after deployment:
1. Revert `CaseRegistrationBL.cs` lines 151-168 to conditional checks
2. Restart API
3. Previous partial-save behavior restored (not ideal but functional)

## Estimated Implementation Time

- Phase 1 (Backend dictionary fix): 10 minutes
- Phase 2 (Field processing): 30 minutes
- Phase 3 (Error handling): 20 minutes
- Phase 4 (Frontend state): 5 minutes
- Phase 5 (DAL validation): 15 minutes
- Phase 6 (Frontend errors): 10 minutes
- Testing: 30 minutes

**Total:** ~2 hours
