# Deficiencies Tab - Integration & Testing Plan

## Phase 8: Integration & Testing

### Test Coverage Overview

This document outlines the comprehensive testing strategy for the Deficiencies Tab feature (نواقص الدعوى).

---

## 1. Backend API Tests

### 1.1 GET /api/case-requests/{requestId}/deficiencies

**Test Case 1.1.1: Retrieve Deficiencies for Valid Request**
- **Precondition**: Request exists with ID 1
- **Action**: GET /api/case-requests/1/deficiencies
- **Expected Result**:
  - Status: 200 OK
  - Response: List of DeficiencyVM objects with full joined data
  - Each object includes: Id, DeficiencyTypeId, DeficiencyTypeName, DescriptionAr, etc.

**Test Case 1.1.2: Retrieve Deficiencies for Non-existent Request**
- **Precondition**: Request ID 9999 does not exist
- **Action**: GET /api/case-requests/9999/deficiencies
- **Expected Result**:
  - Status: 200 OK (returns empty list)
  - Response: Empty array

**Test Case 1.1.3: Handle Soft-Deleted Deficiencies**
- **Precondition**: Request has soft-deleted deficiencies (IsDeleted = true)
- **Action**: GET /api/case-requests/1/deficiencies
- **Expected Result**:
  - Status: 200 OK
  - Response: Only non-deleted deficiencies are returned

### 1.2 PUT /api/case-requests/{requestId}/deficiencies

**Test Case 1.2.1: Batch Update with Valid Deficiencies**
- **Precondition**: Request exists in Draft status (RequestStatusId = 1)
- **Payload**:
  ```json
  {
    "deficiencies": [
      { "deficiencyDescriptionId": 1 },
      { "deficiencyDescriptionId": 3 },
      { "deficiencyDescriptionId": 5 }
    ]
  }
  ```
- **Expected Result**:
  - Status: 200 OK
  - Response: Updated list of 3 DeficiencyVM objects
  - Old deficiencies are soft-deleted
  - New deficiencies created with correct DisplayOrder (0, 1, 2)

**Test Case 1.2.2: Clear All Deficiencies**
- **Precondition**: Request has existing deficiencies
- **Payload**:
  ```json
  {
    "deficiencies": []
  }
  ```
- **Expected Result**:
  - Status: 200 OK
  - Response: Empty array
  - All previous deficiencies are soft-deleted

**Test Case 1.2.3: Update with Invalid Status**
- **Precondition**: Request in Registered status (RequestStatusId = 2)
- **Action**: PUT /api/case-requests/1/deficiencies
- **Expected Result**:
  - Status: 400 BadRequest
  - Message: "Request cannot be edited in current status"

**Test Case 1.2.4: Update Non-existent Request**
- **Precondition**: Request ID 9999 does not exist
- **Action**: PUT /api/case-requests/9999/deficiencies
- **Expected Result**:
  - Status: 404 NotFound
  - Message: "Case registration request 9999 not found"

**Test Case 1.2.5: Invalid Deficiency Description ID**
- **Precondition**: DeficiencyDescription ID 9999 does not exist
- **Payload**:
  ```json
  {
    "deficiencies": [
      { "deficiencyDescriptionId": 9999 }
    ]
  }
  ```
- **Expected Result**:
  - Status: 400 BadRequest or handling per FK constraint

### 1.3 GET /api/lookups/deficiency-types

**Test Case 1.3.1: Retrieve All Types**
- **Action**: GET /api/lookups/deficiency-types
- **Expected Result**:
  - Status: 200 OK
  - Response: Array of 6 DeficiencyTypeVM objects
  - Ordered by DisplayOrder
  - Fields: Id, Name, NameAr, DisplayOrder

**Test Case 1.3.2: Verify All Types Present**
- **Expected Types**:
  - Id 1: المستندات (Documents)
  - Id 2: التقارير الطبية (Medical Reports)
  - Id 3: الأوراق القانونية (Legal Documents)
  - Id 4: الوثائق المالية (Financial Documents)
  - Id 5: وثائق الهوية (Identity Documents)
  - Id 6: أخرى (Other)

### 1.4 GET /api/lookups/deficiency-descriptions

**Test Case 1.4.1: Retrieve All Descriptions**
- **Action**: GET /api/lookups/deficiency-descriptions
- **Expected Result**:
  - Status: 200 OK
  - Response: Array of all DeficiencyDescriptionVM objects
  - Count: 16 descriptions (varies per seed data)

**Test Case 1.4.2: Filter by Type**
- **Action**: GET /api/lookups/deficiency-descriptions?typeId=1
- **Expected Result**:
  - Status: 200 OK
  - Response: Only descriptions with DeficiencyTypeId = 1
  - Ordered by DisplayOrder

**Test Case 1.4.3: Invalid Type ID**
- **Action**: GET /api/lookups/deficiency-descriptions?typeId=9999
- **Expected Result**:
  - Status: 200 OK
  - Response: Empty array

---

## 2. Frontend Component Tests

### 2.1 DeficienciesListComponent

**Test Case 2.1.1: Load and Display Deficiencies**
- **Setup**: Component with requestId = 1, canEdit = false
- **Expected**:
  - Deficiencies loaded from API
  - Displayed in list format with type badges
  - Delete buttons hidden (canEdit = false)

**Test Case 2.1.2: Edit Mode - Delete Deficiency**
- **Setup**: Component with requestId = 1, canEdit = true
- **Action**: Click delete button on deficiency
- **Expected**:
  - Confirmation dialog appears
  - If confirmed: Deficiency removed from local array
  - Count badge updated

**Test Case 2.1.3: Empty State**
- **Setup**: Request with no deficiencies
- **Expected**:
  - "لا توجد نواقص" message displayed
  - Deadline warning visible

**Test Case 2.1.4: Type Color Coding**
- **Expected**: Each type has correct color:
  - Type 1 (Documents): #FF5722 (Orange-Red)
  - Type 2 (Medical): #2196F3 (Blue)
  - Type 3 (Legal): #4CAF50 (Green)
  - Type 4 (Financial): #FF9800 (Orange)
  - Type 5 (ID): #9C27B0 (Purple)
  - Type 6 (Other): #00BCD4 (Cyan)

### 2.2 DeficienciesSelectionDialogComponent

**Test Case 2.2.1: Open Dialog and Load Types**
- **Setup**: Dialog opens for RequestCompletion action
- **Expected**:
  - All 6 deficiency types loaded
  - First type expanded by default
  - Loading spinner appears briefly

**Test Case 2.2.2: Select Deficiencies**
- **Setup**: Dialog open with types loaded
- **Action**: Check multiple deficiencies across types
- **Expected**:
  - Checkboxes update visually
  - Count badges show selections per type
  - Summary shows total count

**Test Case 2.2.3: Confirm Selection**
- **Setup**: 3 deficiencies selected
- **Action**: Click "تأكيد" button
- **Expected**:
  - Dialog closes
  - Returns array of RequestDeficiencyDTO objects
  - Each object: { deficiencyDescriptionId: number }

**Test Case 2.2.4: Cancel Dialog**
- **Setup**: Dialog open with some selections
- **Action**: Click "إلغاء" button
- **Expected**:
  - Dialog closes without changes
  - No deficiencies passed to parent

### 2.3 RequestCompletionComponent Integration

**Test Case 2.3.1: Trigger Dialog on RequestCompletion Action**
- **Setup**: Component with form
- **Action**: Select "استكمال النواقص" from decision type dropdown
- **Expected**:
  - DeficienciesSelectionDialog opens automatically
  - Dialog shows existing selections (if any)

**Test Case 2.3.2: Display Selected Deficiencies**
- **Setup**: Dialog confirms with 3 deficiencies
- **Expected**:
  - Green "النواقص المختارة" box appears
  - Shows count: "3 نقص مختار"
  - Check icon visible

**Test Case 2.3.3: Submit Form with Deficiencies**
- **Setup**: RequestCompletion action with 3 selected deficiencies
- **Action**: Fill form and submit
- **Expected**:
  - API request includes: `deficiencies: [{ deficiencyDescriptionId: X }, ...]`
  - Request sent to POST /api/case-requests/{id}/action
  - Success message shown: "تم طلب استكمال النواقص"

**Test Case 2.3.4: Clear Selections on Action Change**
- **Setup**: RequestCompletion selected with deficiencies
- **Action**: Change decision type to "قيد الدعوى"
- **Expected**:
  - selectedDeficiencies array cleared
  - Selection box disappears
  - Dialog does not appear again

---

## 3. End-to-End Workflow Tests

### 3.1 Complete Deficiencies Request Workflow

**Scenario 3.1.1: موظف القيد Requests Completion**
1. User navigates to case registration request (status: Draft)
2. User goes to "Completion" section
3. User selects "استكمال النواقص" action
4. Dialog opens, user selects 3 deficiencies
5. User enters notes
6. User submits form
7. **Verification**:
   - API endpoint called with deficiencies payload
   - Request status updated to PendingCompletion (6)
   - Deficiencies stored in database
   - List reflects in DeficienciesListComponent

### 3.1.2: Verify Deficiencies Persistence
1. User navigates back to case request
2. User views Deficiencies tab
3. **Verification**:
   - Previously added deficiencies are displayed
   - Type colors match their types
   - Count matches submitted count

### 3.1.3: Update Deficiencies
1. Request status is PendingCompletion (6)
2. User selects "استكمال النواقص" again
3. Dialog opens with previous selections pre-checked
4. User modifies selection (add/remove items)
5. User submits
6. **Verification**:
   - Old deficiencies soft-deleted
   - New deficiencies stored
   - DisplayOrder updated correctly

### 3.1.4: Clear All Deficiencies
1. User selects RequestCompletion action
2. User clears all selections in dialog
3. User submits
4. **Verification**:
   - Dialog allows submission with empty array
   - All previous deficiencies soft-deleted
   - DeficienciesListComponent shows empty state

---

## 4. Data Integrity Tests

### 4.1 Soft Delete Verification
- Deleted deficiencies have IsDeleted = true
- ModifiedDate updated on soft delete
- Queries filter out soft-deleted records
- Hard delete never occurs

### 4.2 Audit Trail
- CreatedDate preserved on original creation
- ModifiedDate updated on each edit
- DisplayOrder correct (0-based index)

### 4.3 Foreign Key Constraints
- Cannot add deficiency with invalid DeficiencyDescriptionId
- Cascading deletes handled properly
- Data integrity maintained

---

## 5. Error Handling Tests

### 5.1 Network Errors
- API timeout handled with snackbar message
- Dialog loading state shown
- Retry capability

### 5.2 Validation Errors
- Empty deficiencies array accepted
- Required fields in submission checked
- Error messages displayed in Arabic

### 5.3 Authorization
- Only موظف القيد can take RequestCompletion action
- Non-authorized users see disabled button/message
- Proper 403 Forbidden response on unauthorized access

---

## 6. Performance Tests

### 6.1 Large Dataset Handling
- Load time with all 16 deficiency descriptions
- Dialog performance with accordion expansion
- Batch update performance with multiple selections

### 6.2 Memory Management
- Proper subscription cleanup (takeUntil)
- No memory leaks on component destroy
- Dialog cleanup after close

---

## 7. UI/UX Tests

### 7.1 RTL Layout
- All text properly right-aligned
- Icons and buttons properly positioned
- Dialog direction attribute set correctly

### 7.2 Responsive Design
- Dialog fits on mobile devices
- Checkboxes properly spaced
- Text readable on all breakpoints

### 7.3 Accessibility
- Proper ARIA labels on form elements
- Keyboard navigation supported
- Color coding not sole indicator (text labels present)

---

## 8. Browser Compatibility

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

---

## Test Execution Checklist

### Backend Tests
- [ ] API endpoint tests pass
- [ ] Database integrity verified
- [ ] Soft delete working correctly
- [ ] Eager loading functioning

### Frontend Tests
- [ ] Component unit tests pass
- [ ] Dialog opens/closes correctly
- [ ] Form submission includes deficiencies
- [ ] State management working

### Integration Tests
- [ ] End-to-end workflow successful
- [ ] API calls match expected format
- [ ] Database state correct after submission
- [ ] UI reflects backend state

### Performance Tests
- [ ] Load times acceptable
- [ ] No console errors
- [ ] Memory usage stable

### User Acceptance Tests
- [ ] Workflow intuitive
- [ ] Error messages clear
- [ ] Success feedback provided
- [ ] Data persists correctly

---

## Known Limitations & Future Enhancements

1. **Bulk Deficiency Management**: Currently deficiencies can only be edited via RequestCompletion action
2. **Deficiency Templates**: Could add ability to save/load deficiency sets
3. **Notification System**: Could notify plaintiff when deficiencies are added
4. **Deficiency Status Tracking**: Could track completion status of each deficiency
5. **Reporting**: Could generate deficiency reports

---

## Regression Test Points

- Ensure other case registration features still work:
  - Adding plaintiffs/defendants
  - Adding claims
  - Adding related cases
  - Completing request without deficiencies

---

## Sign-off Criteria

✅ All test cases pass
✅ No critical bugs found
✅ Code review completed
✅ Performance acceptable
✅ Documentation complete
✅ Ready for production deployment
