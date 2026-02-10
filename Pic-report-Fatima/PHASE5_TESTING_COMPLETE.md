# Phase 3: Testing & Validation Guide

## Request Completion Tab Implementation
**Feature**: إنهاء الطلب (Request Completion)
**Date**: 2026-02-08
**Status**: Ready for Testing

---

## Part 1: Database Setup (Prerequisites)

### Step 1: Run Entity Framework Migrations

Before testing, you must create and apply the database migrations for the new entities:

```bash
# Navigate to Backend directory
cd src/Backend

# Add migration for CaseType and CaseRequestWorkflow entities
dotnet ef migrations add AddCaseTypeAndWorkflow --project BOG.DbModel --startup-project BOG.API

# Update database
dotnet ef database update --project BOG.DbModel --startup-project BOG.API
```

**Expected Result:**
- Migration created successfully
- Database updated with new tables: CaseTypes, CaseRequestWorkflows
- 2 case types seeded: إداري (ID=1), تأديبي (ID=2)

### Step 2: Verify Database Setup

Run this SQL query to verify the setup:

```sql
-- Check if CaseType table exists and has 2 records
SELECT COUNT(*) as CaseTypeCount FROM CaseTypes WHERE IsActive = 1 AND IsDeleted = 0;
-- Expected: 2

-- Check if CaseRequestWorkflow table exists
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CaseRequestWorkflows';
-- Expected: CaseRequestWorkflows

-- Verify CaseRegistrationRequest has CaseTypeId column
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CaseRegistrationRequests' AND COLUMN_NAME = 'CaseTypeId';
-- Expected: CaseTypeId
```

---

## Part 2: Backend Testing

### Setup: Start the Backend API

```bash
cd src/Backend
dotnet run --project BOG.API
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to quit.
```

### Test 2.1: GET /api/lookups/case-types

**Purpose**: Verify case types lookup endpoint

```bash
curl -X GET https://localhost:5001/api/lookups/case-types \
  -H "Content-Type: application/json" \
  -k
```

**Expected Response** (Status: 200):
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

**Verification Checklist:**
- [ ] Status code is 200
- [ ] Returns 2 case types
- [ ] Both Arabic and English names present
- [ ] IDs are 1 and 2

---

### Test 2.2: POST /api/case-requests/{id}/complete - Register Decision

**Prerequisites:**
- Create a case request (or use existing ID, e.g., 1)
- Ensure request has: classifications, defendants, attachments, subject, evidence
- Ensure request is in Draft (1) or New (3) status

**Test Command:**
```bash
curl -X POST https://localhost:5001/api/case-requests/1/complete \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "decisionType": "Register",
    "caseTypeId": 1,
    "notes": "تم قيد الدعوى حسب الأصول والمتطلبات"
  }'
```

**Expected Response** (Status: 200):
```json
{
  "id": 1,
  "requestStatusId": 6,
  "requestStatusName": "Registered",
  "courtId": 1,
  "courtName": "المحكمة العامة بالرياض",
  "subject": "دعوى إدارية",
  "evidence": "أدلة كاملة ومؤيدة للدعوى",
  "caseNumber": "CASE-2026-00001",
  "registrationNumber": "المحكمة العامة-1446-001",
  "registrationDate": "2026-02-08T12:00:00Z",
  "caseTypeId": 1,
  "caseTypeName": "إداري",
  "createdDate": "2026-02-01T10:00:00Z",
  "modifiedDate": "2026-02-08T12:00:00Z"
}
```

**Verification Checklist:**
- [ ] Status code is 200
- [ ] Status changes to Registered (6)
- [ ] CaseTypeId is set to 1
- [ ] CaseTypeName is "إداري"
- [ ] RegistrationNumber is generated
- [ ] Success notification sent

**Workflow History Check:**
```sql
SELECT * FROM CaseRequestWorkflows
WHERE CaseRegistrationRequestId = 1
ORDER BY CreatedDate DESC;
-- Expected: Record with PreviousStatusId=3, NewStatusId=6, Notes="تم قيد الدعوى حسب الأصول والمتطلبات"
```

---

### Test 2.3: POST /api/case-requests/{id}/complete - SendToJudge Decision

**Test Command:**
```bash
curl -X POST https://localhost:5001/api/case-requests/2/complete \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "decisionType": "SendToJudge",
    "caseTypeId": 2,
    "notes": "للعرض على رئيس المحكمة للنظر"
  }'
```

**Expected Response** (Status: 200):
```json
{
  "id": 2,
  "requestStatusId": 5,
  "requestStatusName": "OnJudgeDesk",
  "caseTypeId": 2,
  "caseTypeName": "تأديبي",
  ...
}
```

**Verification Checklist:**
- [ ] Status changes to OnJudgeDesk (5)
- [ ] CaseTypeId is set to 2
- [ ] CaseTypeName is "تأديبي"
- [ ] Workflow history recorded

---

### Test 2.4: POST /api/case-requests/{id}/complete - Reject Decision

**Test Command:**
```bash
curl -X POST https://localhost:5001/api/case-requests/3/complete \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "decisionType": "Reject",
    "caseTypeId": 1,
    "notes": "عدم استيفاء الشروط النظامية والقانونية المطلوبة"
  }'
```

**Expected Response** (Status: 200):
```json
{
  "id": 3,
  "requestStatusId": 10,
  "requestStatusName": "Rejected",
  "rejectionReason": "عدم استيفاء الشروط النظامية والقانونية المطلوبة",
  "caseTypeId": 1,
  ...
}
```

**Verification Checklist:**
- [ ] Status changes to Rejected (10)
- [ ] RejectionReason populated
- [ ] CaseTypeId set correctly
- [ ] Rejection notification sent (SMS/Email)

---

### Test 2.5: POST /api/case-requests/{id}/complete - RequestCompletion Decision

**Test Command:**
```bash
curl -X POST https://localhost:5001/api/case-requests/4/complete \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "decisionType": "RequestCompletion",
    "caseTypeId": 2,
    "notes": "يرجى استكمال المرفقات والمستندات المطلوبة خلال 30 يوم"
  }'
```

**Expected Response** (Status: 200):
```json
{
  "id": 4,
  "requestStatusId": 8,
  "requestStatusName": "PendingCompletion",
  "completionDeadline": "2026-03-09T12:00:00Z",
  "caseTypeId": 2,
  ...
}
```

**Verification Checklist:**
- [ ] Status changes to PendingCompletion (8)
- [ ] CompletionDeadline set to 30 days from now
- [ ] CaseTypeId set correctly
- [ ] Completion notification sent

---

### Test 2.6: Validation Error - Missing CaseTypeId

**Purpose**: Verify CaseTypeId validation (ERR_CASE_TYPE)

**Test Command:**
```bash
curl -X POST https://localhost:5001/api/case-requests/5/complete \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "decisionType": "Register",
    "notes": "بدون نوع دعوى"
  }'
```

**Expected Response** (Status: 400):
```json
{
  "message": "نوع الدعوى مطلوب"
}
```

**Verification Checklist:**
- [ ] Status code is 400
- [ ] Error message about CaseTypeId
- [ ] Request not modified

---

### Test 2.7: Validation Error - Missing Classifications (ERR005)

**Test Command:**
```bash
curl -X POST https://localhost:5001/api/case-requests/6/complete \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "decisionType": "Register",
    "caseTypeId": 1
  }'
```

**Expected Response** (Status: 400):
```json
{
  "message": "ERR005: يجب تحديد تصنيف واحد على الأقل للدعوى"
}
```

**Verification Checklist:**
- [ ] Status code is 400
- [ ] Error includes ERR005
- [ ] Message about classifications

---

### Test 2.8: Validation Error - Missing Defendants (ERR002)

**Expected Response** (Status: 400):
```json
{
  "message": "ERR002: يجب تحديد مدعى عليه واحد على الأقل"
}
```

---

### Test 2.9: Validation Error - Missing Attachments (ERR010)

**Expected Response** (Status: 400):
```json
{
  "message": "ERR010: يجب إضافة مرفق واحد على الأقل"
}
```

---

### Test 2.10: Invalid CaseTypeId

**Test Command:**
```bash
curl -X POST https://localhost:5001/api/case-requests/7/complete \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "decisionType": "Register",
    "caseTypeId": 99
  }'
```

**Expected Response** (Status: 400):
```json
{
  "message": "نوع الدعوى مطلوب"
}
```

---

## Part 3: Frontend Testing

### Setup: Start the Frontend

```bash
cd src/Frontend/bog-app
ng serve
# Navigate to: http://localhost:4200
```

### Test 3.1: Load Request Details Page

1. Open browser: `http://localhost:4200`
2. Navigate to an existing case request (or create a new one)
3. Look for the sidebar navigation

**Expected Result:**
- ✅ Sidebar shows "إنهاء الطلب" option with `done_all` icon
- ✅ Option appears after "إجراءات الطلب"

---

### Test 3.2: Click on "إنهاء الطلب" Tab

**Steps:**
1. Click "إنهاء الطلب" in sidebar
2. Observe the page

**Expected Result:**
- ✅ Tab name changes to "إنهاء الطلب"
- ✅ Form appears with green header
- ✅ Button shows "اعتماد القرار"
- ✅ Two dropdowns visible: "القرار" and "نوع الدعوى"
- ✅ Notes textarea below

---

### Test 3.3: Load Case Types Dropdown

**Steps:**
1. Open "إنهاء الطلب" tab
2. Click on "نوع الدعوى" dropdown

**Expected Result:**
- ✅ Dropdown shows 2 options:
  - إداري
  - تأديبي
- ✅ "إداري" selected by default
- ✅ No loading spinner visible (case types loaded)

---

### Test 3.4: Select Decision Type - Register

**Steps:**
1. Open "إنهاء الطلب" tab
2. Select "قيد الدعوى" from "القرار" dropdown
3. Select "إداري" from "نوع الدعوى" dropdown
4. Type notes: "جاهز للقيد"

**Expected Result:**
- ✅ Form is valid
- ✅ "اعتماد القرار" button is enabled
- ✅ All fields show values

---

### Test 3.5: Submit Decision with Confirmation

**Steps:**
1. Fill form as above
2. Click "اعتماد القرار" button

**Expected Result:**
- ✅ Confirmation dialog appears:
  - Title: "تأكيد العملية"
  - Message: "هل أنت متأكد من الحفظ؟"
  - Buttons: "نعم" and "لا"

---

### Test 3.6: Click "نعم" in Confirmation

**Steps:**
1. In confirmation dialog, click "نعم"

**Expected Result:**
- ✅ Dialog closes
- ✅ Button shows loading spinner and "جاري المعالجة..."
- ✅ API call made to `/api/case-requests/{id}/complete`

---

### Test 3.7: Success Response

**Expected Result:**
- ✅ Button returns to normal state
- ✅ Success snackbar appears: "تم قيد الدعوى بنجاح"
- ✅ Form resets to default values
- ✅ Page status may update (if status changed)

---

### Test 3.8: Error Handling - Missing Classifications

**Setup:**
1. Create a request WITHOUT classifications
2. Try to submit "قيد الدعوى" decision

**Expected Result:**
- ✅ API returns 400 error
- ✅ Error snackbar appears: "ERR005: يجب تحديد تصنيف واحد على الأقل للدعوى"
- ✅ Snackbar visible for ~8 seconds
- ✅ Form remains intact

---

### Test 3.9: Decision Type - SendToJudge

**Steps:**
1. Select "العرض على رئيس المحكمة"
2. Select case type
3. Add notes (optional)
4. Click "اعتماد القرار"
5. Confirm

**Expected Result:**
- ✅ Success: "تم العرض على رئيس المحكمة بنجاح"
- ✅ Request status changes to "على مكتب القاضي"

---

### Test 3.10: Decision Type - Reject

**Steps:**
1. Select "التوجيه بعدم قيد الطلب"
2. Select case type
3. Add rejection notes
4. Submit

**Expected Result:**
- ✅ Success: "تم رفض الطلب"
- ✅ Request status changes to "مرفوض"
- ✅ Rejection reason saved

---

### Test 3.11: Decision Type - RequestCompletion

**Steps:**
1. Select "استكمال النواقص"
2. Select case type
3. Add notes about missing documents
4. Submit

**Expected Result:**
- ✅ Success: "تم طلب استكمال النواقص"
- ✅ Request status changes to "بانتظار الاستكمال"
- ✅ CompletionDeadline set to 30 days

---

### Test 3.12: RTL (Arabic) Layout

**Steps:**
1. Verify page direction: `<div dir="rtl">`
2. Check form alignment
3. Check button alignment in header

**Expected Result:**
- ✅ Form fields right-aligned
- ✅ Labels on right side
- ✅ Buttons on left side (RTL)
- ✅ Arabic text displays correctly
- ✅ No text wrapping issues

---

### Test 3.13: Responsive Design - Mobile

**Steps:**
1. Open DevTools (F12)
2. Toggle device toolbar (Ctrl+Shift+M)
3. Set to iPhone X (375px width)
4. Open "إنهاء الطلب" tab

**Expected Result:**
- ✅ Form stacks vertically
- ✅ "القرار" and "نوع الدعوى" in separate rows
- ✅ Notes textarea full width
- ✅ Button full width
- ✅ Green header adjusts properly
- ✅ Sidebar closes on selection

---

### Test 3.14: Form Validation - Notes Character Counter

**Steps:**
1. Click notes textarea
2. Type some Arabic text
3. Observe character counter

**Expected Result:**
- ✅ Counter shows: "X / 4000"
- ✅ Updates as you type
- ✅ Max 4000 characters enforced
- ✅ Cannot exceed 4000 chars

---

### Test 3.15: Permission Check - Wrong Status

**Setup:**
1. Create request with status "مسجل" (Registered - ID 6) or other non-eligible status

**Steps:**
1. Open "إنهاء الطلب" tab

**Expected Result:**
- ✅ Warning message appears: "لا يمكن إنهاء الطلب في الحالة الحالية"
- ✅ Form is hidden
- ✅ Cannot submit

---

### Test 3.16: Console Errors

**Steps:**
1. Open browser DevTools (F12)
2. Go to Console tab
3. Perform all above tests
4. Check for errors

**Expected Result:**
- ✅ No red error messages in console
- ✅ No 404 or 500 errors
- ✅ Network requests successful (green)

---

## Part 4: Integration Testing

### Test 4.1: End-to-End Flow

**Steps:**
1. Create new case request
2. Fill out all required sections:
   - Add defendants
   - Add classifications
   - Add attachments
   - Add subject and evidence
3. Submit request
4. Navigate to "إنهاء الطلب"
5. Select decision type
6. Select case type
7. Add notes
8. Submit decision
9. Verify success

**Expected Result:**
- ✅ All steps complete without errors
- ✅ Database updated correctly
- ✅ Workflow history recorded
- ✅ Status changed appropriately

---

### Test 4.2: Database Verification

After completing tests, run these SQL queries:

```sql
-- Verify CaseTypeId is set for requests
SELECT Id, RequestStatusId, CaseTypeId FROM CaseRegistrationRequests
WHERE Id IN (1, 2, 3, 4, 5)
ORDER BY Id;
-- Expected: All have CaseTypeId values

-- Verify Workflow History
SELECT CaseRegistrationRequestId, PreviousStatusId, NewStatusId, Notes, ActionDate
FROM CaseRequestWorkflows
ORDER BY ActionDate DESC
LIMIT 10;
-- Expected: Records for each decision made

-- Verify Status Transitions
SELECT Id, RequestStatusId FROM CaseRegistrationRequests
WHERE Id = 1; -- Should be 6 (Registered) if you tested Register decision
```

---

## Part 5: Success Criteria Validation

| Criterion | Status | Notes |
|-----------|--------|-------|
| Database migrations run successfully | ⬜ | Run `dotnet ef database update` |
| 2 case types seeded (إداري, تأديبي) | ⬜ | Verify in database |
| GET /api/lookups/case-types returns correct data | ⬜ | Test 2.1 |
| POST /api/case-requests/{id}/complete endpoint works | ⬜ | Tests 2.2-2.5 |
| CaseTypeId validation enforced | ⬜ | Test 2.6 |
| All validation errors return correct messages | ⬜ | Tests 2.6-2.9 |
| Frontend component displays correctly | ⬜ | Test 3.1-3.2 |
| Form fields work as expected | ⬜ | Tests 3.3-3.4 |
| Confirmation dialog works | ⬜ | Test 3.5-3.6 |
| All 4 decision types work | ⬜ | Tests 3.9-3.11 |
| Error messages display correctly | ⬜ | Test 3.8 |
| RTL layout works properly | ⬜ | Test 3.12 |
| Mobile responsive | ⬜ | Test 3.13 |
| Workflow history recorded | ⬜ | Test 4.2 |
| No console errors | ⬜ | Test 3.16 |

---

## Debugging Tips

### If Case Types Don't Load

**Browser Console Check:**
```javascript
// In browser console:
localStorage.getItem('case-request-context');
// Should show the current request ID
```

**Network Check (F12 → Network tab):**
- Look for request to `/api/lookups/case-types`
- Status should be 200
- Response should show 2 objects

**Backend Check:**
```sql
SELECT * FROM CaseTypes WHERE IsActive = 1 AND IsDeleted = 0;
```

---

### If Form Doesn't Submit

**Check Browser Console:**
- Look for XHR/Fetch errors
- Check CORS errors
- Verify API URL is correct

**Check Network Tab:**
- POST to `/api/case-requests/{id}/complete`
- Check request body (should have decisionType, caseTypeId)
- Check response status and message

**Backend Logs:**
- Check if endpoint is hit
- Look for validation errors

---

### If Status Doesn't Change

**Check Database:**
```sql
SELECT Id, RequestStatusId FROM CaseRegistrationRequests WHERE Id = [your-id];
```

**Check Response:**
- Verify API returns updated requestStatusId
- Check if frontend updates the status display

---

## Rollback Plan

If critical issues found:

1. **Backend Rollback:**
   ```bash
   dotnet ef database update [previous-migration]
   git revert [commit-hash]
   ```

2. **Frontend Rollback:**
   ```bash
   git revert [commit-hash]
   rm -rf src/Frontend/bog-app/src/app/features/case-registration/components/request-completion
   ```

---

## Test Completion Checklist

- [ ] Phase 3.1: Database migrations successful
- [ ] Phase 3.2: Backend case types endpoint works
- [ ] Phase 3.3-2.10: All 9 backend tests passed
- [ ] Phase 3.1-3.16: All 16 frontend tests passed
- [ ] Phase 4.1: End-to-end flow works
- [ ] Phase 4.2: Database verification successful
- [ ] All success criteria validated
- [ ] No console errors
- [ ] RTL layout correct
- [ ] Mobile responsive working

---

## Sign-Off

**Testing Date**: ___________
**Tester Name**: ___________
**All Tests Passed**: ☐ Yes ☐ No
**Issues Found**: ___________

---

*For detailed implementation context, see PHASE5_COMPLETION_REPORT.md*
