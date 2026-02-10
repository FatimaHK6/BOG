# Classification Deletion Fix - Testing Guide

## Status: ✅ READY FOR TESTING

Both backend and frontend are running and ready for testing:
- **Backend API:** http://localhost:5001
- **Frontend App:** http://localhost:4300

---

## Code Changes Implemented

### 1. Frontend Fix (RequestDetailsComponent)
**File:** `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.ts`

**Line 425-442:** Removed unnecessary `this.loadRequest()` call from `onSaveComplete()` method

```typescript
// BEFORE (line 433):
this.loadRequest();  // ← DELETED

// AFTER (lines 431-434):
// NOTE: No need to reload from backend - the save response already updated the state
// via CaseDataStateService in the CaseDataContainerComponent.
// Removing this reload prevents unnecessary API calls and eliminates race conditions
// that could overwrite unsaved data (like classifications).
```

**Impact:**
- ✅ Eliminates race condition causing classifications to be overwritten
- ✅ Removes 3 unnecessary sequential API calls after save
- ✅ State updates directly from save response instead

### 2. Backend Enhancement (CaseRegistrationRequestRepository)
**File:** `src/Backend/BOG.DAL/Repositories/CaseRegistrationRequestRepository.cs`

**Lines 47-48:** Added RelatedCases to GetWithDetailsAsync query

```csharp
// ADDED:
.Include(r => r.RelatedCases.Where(rc => !rc.IsDeleted))
    .ThenInclude(rc => rc.Court)
```

**Impact:**
- ✅ Makes GetWithDetailsAsync truly load ALL details
- ✅ Ensures consistency if reload ever needed in future
- ✅ No breaking changes

---

## Manual Testing Steps

### Test Case 1: Add Classifications, Save Other Data (PRIMARY TEST)

This is the exact scenario that was failing before the fix.

**Setup:**
1. Open http://localhost:4300 in your browser
2. Navigate to an existing Draft case (or create a new one)

**Steps:**
1. Navigate to **"بيانات الدعوى"** → **"تصنيف الدعوى"** (Classifications tab)
2. Click **"إضافة تصنيف"** (Add Classification button)
3. Select 2-3 classifications from the dialog
4. Click **"تأكيد"** (Confirm button)
   - Classifications are now in memory/localStorage but NOT saved to backend
5. Navigate to **"موضوع وأسانيد الدعوى"** (Subject & Evidence tab)
6. Modify the subject text (e.g., add a word)
7. Click **"حفظ كمسودة"** (Save as Draft button) in the header
8. **CRITICAL VERIFICATION:**
   - Open DevTools: Press F12
   - Go to **Network** tab
   - Look for API calls after clicking Save
   - **Expected:** Only ONE call `PUT /api/case-requests/{id}`
   - **NOT Expected:** No `GET /api/case-requests/{id}` after save
9. Wait for save to complete (success message appears)
10. Navigate to **"المرفقات"** (Attachments tab)
11. Navigate back to **"تصنيفات"** (Classifications tab)

**Expected Result (FIX IS WORKING):**
- ✅ Classifications are still visible and intact
- ✅ Both saved classifications display correctly
- ✅ No validation errors
- ✅ Network tab shows NO reload API call after save

**Result If Bug Still Exists:**
- ❌ Classifications would be empty/cleared
- ❌ You would see a GET request after the save
- ❌ Data would be overwritten from backend reload

---

### Test Case 2: Multiple Tab Navigation Without Save

Verify classifications persist when navigating between tabs without saving.

1. Open a Draft case
2. Add 1-2 classifications (don't save)
3. Navigate through all tabs:
   - Subject & Evidence
   - Claims
   - Related Cases
   - Attachments
   - Contact Info
4. Return to Classifications tab

**Expected Result:**
- ✅ Classifications still visible (persisted in localStorage)
- ✅ No data loss across tab navigation

---

### Test Case 3: Multiple Saves with Classifications

Test that multiple saves in sequence preserve unsaved classifications.

1. Open Draft case
2. Add classification A
3. Save via Classifications tab button
4. Navigate to Contact Info
5. Change mobile number
6. Save via header "حفظ كمسودة"
7. Navigate back to Classifications
8. Add classification B (don't save)
9. Navigate to Claims tab
10. Add a claim
11. Save via header
12. Navigate back to Classifications

**Expected Result:**
- ✅ Classification A visible (was saved)
- ✅ Classification B visible (not yet saved, but not lost)
- ✅ All data preserved after multiple saves

---

### Test Case 4: Browser Refresh with Unsaved Data

Test that unsaved classifications survive browser refresh via localStorage.

1. Open Draft case
2. Add classifications
3. DO NOT SAVE
4. Press F5 to refresh browser
5. Navigate to Classifications tab

**Expected Result:**
- ✅ Classifications loaded from localStorage
- ✅ Unsaved data survives refresh

---

### Test Case 5: Save and Reopen Request

Test that saved classifications persist when reopening the case.

1. Open Draft case
2. Add classifications
3. Save via "حفظ كمسودة"
4. Navigate back to case list
5. Reopen the same case
6. Navigate to Classifications

**Expected Result:**
- ✅ Classifications loaded from backend
- ✅ Saved data persists across sessions

---

### Test Case 6: Verify No Reload API Calls

Test the most critical aspect: verify that no reload occurs after save.

1. Open DevTools (F12) → **Network** tab
2. Open a Draft case
3. Add classifications
4. Modify subject text
5. Click "حفظ كمسودة" and observe network traffic

**Expected API Calls in Network Tab:**
```
PUT /api/case-requests/[id]          ← Only this should appear
                                      (NO GET call after this)
```

**NOT Expected:**
```
GET /api/case-requests/[id]          ← Should NOT appear after save
GET /api/case-requests/[id]/claims   ← Should NOT appear after save
```

---

## Build Status

### Frontend Build ✅
```
Build at: 2026-02-09T11:50:34.344Z
Hash: dec2ec36aa63727b
Time: 14.253 seconds
Result: SUCCESS - No TypeScript compilation errors
```

### Backend Build ✅
```
Build: SUCCESSFUL
Errors: 0
Warnings: 15 (pre-existing null reference checks)
Time: 9.05 seconds
```

---

## Servers Running

Both servers are confirmed running:

```
Backend API:  http://localhost:5001
              Swagger UI: http://localhost:5001/swagger

Frontend App: http://localhost:4300
```

Test by checking:
- `curl http://localhost:5001/api/case-requests` (Backend API)
- `curl http://localhost:4300` (Frontend serving)

Both return 200 OK responses with HTML/JSON content.

---

## Fix Summary

**Problem:** Classifications were deleted when:
1. User adds classifications (stored in state + localStorage)
2. User saves other data (subject, evidence, etc.)
3. Save triggers unnecessary reload from backend
4. Backend returns empty classifications
5. Unsaved classifications in state get overwritten

**Root Cause:** `this.loadRequest()` in `onSaveComplete()` creates race condition

**Solution:** Remove the unnecessary reload
- Save response already updates state via CaseDataStateService
- No need to reload from backend
- Eliminates race condition entirely
- Improves performance

**Why It Works:**
- The save endpoint returns complete updated request data
- `CaseDataContainerComponent` already updates state from save response
- State management is reactive - UI updates automatically
- No backend modifications happen during save that require reload

---

## Performance Improvement

**Before Fix:**
```
1. User saves form data
2. PUT /api/case-requests/{id}     (1 API call)
3. onSaveComplete() triggers
4. loadRequest() calls:
   - GET /api/case-requests/{id}   (2nd API call)
   - getClaims() API call           (3rd API call)
   - getRelatedCases() API call     (4th API call)
Total: 4 API calls per save
```

**After Fix:**
```
1. User saves form data
2. PUT /api/case-requests/{id}     (1 API call)
3. onSaveComplete() triggers
4. No reload - reuse save response
Total: 1 API call per save
⚡ 75% reduction in API calls
```

---

## Rollback Plan (If Needed)

If issues arise, the change is trivial to revert:

**File:** `request-details.component.ts` (line 433)

```typescript
// Simply add back the line:
this.loadRequest();
```

Then rebuild:
```bash
cd src/Frontend/bog-app
npm install
ng build
```

---

## Questions to Answer During Testing

1. **Are classifications visible after saving other data?**
   - Yes = Fix working ✅
   - No = Issue remains ❌

2. **Is there a GET API call after the PUT save?**
   - No = Fix working ✅
   - Yes = Issue remains ❌

3. **Do classifications persist across tab navigation?**
   - Yes = Fix working ✅
   - No = Issue remains ❌

4. **Do multiple saves preserve unsaved classifications?**
   - Yes = Fix working ✅
   - No = Issue remains ❌

---

## Success Criteria

✅ All test cases pass
✅ No unnecessary API reload after save
✅ Classifications persist across navigation
✅ Browser console shows no errors
✅ Network tab shows single PUT call (no GET reload)

---

## Next Steps

1. Execute Test Case 1 (Primary Test) manually
2. Verify Network tab shows no GET reload call
3. Verify classifications are visible after save and navigation
4. Run remaining test cases if Test Case 1 passes
5. Document any issues found

---

Generated: 2026-02-09
Fix Status: Ready for Manual Testing
Servers: Both Running and Responding
