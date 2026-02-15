# Deficiencies Tab - Frontend Testing Guide

## Manual Test Scenarios

### Prerequisites
- Backend API running on `http://localhost:5000` or `https://localhost:5001`
- Frontend running on `http://localhost:4200`
- Database seeded with deficiency data (6 types, 16 descriptions)
- Test case registration request with ID 1 in Draft status

---

## Test Scenario 1: View Deficiencies List

### Steps:
1. Navigate to case registration request details page
2. Click on "النواقص" (Deficiencies) in the sidebar
3. Verify the deficiencies tab loads

### Expected Results:
- [ ] Page loads without errors
- [ ] "الموعد النهائي للإكمال" (Auto-reject deadline) warning displays
- [ ] Shows "لا توجد نواقص" (No deficiencies) message if list is empty
- [ ] Deficiency list displays if there are existing deficiencies
- [ ] Each deficiency shows:
  - Type badge with color coding
  - Arabic description
  - English description (if available)
  - Created date
  - Delete button (if canEdit = true)

### Notes:
- First time viewing should show empty state
- Type colors should match: Documents=Orange, Medical=Blue, Legal=Green, Financial=Orange, ID=Purple, Other=Cyan

---

## Test Scenario 2: Open Deficiencies Selection Dialog

### Steps:
1. Navigate to "إنهاء الطلب" (Request Completion) section
2. Click on decision type dropdown
3. Select "استكمال النواقص" (Request Completion)

### Expected Results:
- [ ] DeficienciesSelectionDialog opens automatically
- [ ] Dialog title shows "اختيار النواقص (نواقص الدعوى)"
- [ ] All 6 deficiency types load successfully
- [ ] First type is expanded by default
- [ ] Loading spinner briefly appears while loading
- [ ] Dialog is properly RTL-aligned (Arabic text right-aligned)

### Notes:
- Dialog should appear immediately after selecting action
- No manual button click needed

---

## Test Scenario 3: Select Deficiencies

### Steps:
1. Dialog is open (from Scenario 2)
2. Expand different type sections (click type header)
3. Select multiple deficiencies from different types
4. Observe count badges update

### Expected Results:
- [ ] Type sections expand/collapse when clicked
- [ ] Checkboxes update visual state when clicked
- [ ] Count badge appears next to each type showing selected count
- [ ] Green summary box appears showing total selections
- [ ] "تأكيد" (Confirm) button remains enabled while selections exist

### Notes:
- Test selecting 1, 3, 5, and more deficiencies
- Test expanding all types
- Verify count badges update in real-time

---

## Test Scenario 4: Confirm Deficiency Selection

### Steps:
1. Select 3 deficiencies from the dialog
2. Click "تأكيد" (Confirm) button
3. Observe form state update

### Expected Results:
- [ ] Dialog closes
- [ ] Green "النواقص المختارة" (Selected Deficiencies) box appears in form
- [ ] Shows count: "3 نقص مختار"
- [ ] Check icon displays in the box
- [ ] Form is ready for submission

### Notes:
- Selected count should match number of checkboxes checked
- Dialog should not show error messages

---

## Test Scenario 5: Cancel Dialog

### Steps:
1. Open dialog for RequestCompletion action
2. Select some deficiencies
3. Click "إلغاء" (Cancel) button
4. Verify form state

### Expected Results:
- [ ] Dialog closes without saving
- [ ] Decision type field shows selected action still
- [ ] No green selection box appears if was previously empty
- [ ] If had previous selections, they are preserved

### Notes:
- Canceling should not affect form state
- Previous selections should persist

---

## Test Scenario 6: Submit Form with Deficiencies

### Steps:
1. Complete form fields:
   - Decision type: "استكمال النواقص"
   - Case type: Select any type
   - Notes: (optional)
2. Select 2-3 deficiencies via dialog
3. Click "حفظ" (Save) button
4. Confirm in confirmation dialog

### Expected Results:
- [ ] Form validation passes
- [ ] API request sent to POST /api/case-requests/{id}/action
- [ ] Request payload includes deficiencies array
- [ ] Success message shows: "تم طلب استكمال النواقص"
- [ ] Form resets after successful submission
- [ ] Selection box disappears
- [ ] Form ready for next action

### Notes:
- Monitor browser console for network requests
- Check Network tab to verify payload format
- Verify deficiencies are included in POST body

---

## Test Scenario 7: View Added Deficiencies

### Steps:
1. After submitting deficiencies (Scenario 6)
2. Navigate to "النواقص" tab
3. Observe the deficiencies list

### Expected Results:
- [ ] Previously added deficiencies display in list
- [ ] Correct count of deficiencies shown
- [ ] Each deficiency shows:
  - Correct type with color badge
  - Arabic description
  - Creation date
- [ ] No empty state message
- [ ] Delete buttons visible (if canEdit = true)

### Notes:
- Data should persist across page reloads
- List should be sorted by DisplayOrder

---

## Test Scenario 8: Edit Deficiencies (Update)

### Steps:
1. Request status is PendingCompletion (6)
2. Go to Request Completion section
3. Select "استكمال النواقص" again
4. Dialog opens with previous selections pre-checked
5. Modify selections:
   - Uncheck 1 item
   - Check 2 new items
6. Confirm and submit

### Expected Results:
- [ ] Dialog opens with previous selections already checked
- [ ] User can modify selections
- [ ] Old deficiencies are removed
- [ ] New deficiencies are added
- [ ] Deficiencies tab reflects new list
- [ ] DisplayOrder updated correctly (0, 1, 2, ...)

### Notes:
- Verify old deficiencies are gone from list
- Verify new ones are in correct order

---

## Test Scenario 9: Clear All Deficiencies

### Steps:
1. Request has existing deficiencies
2. Go to Request Completion section
3. Select "استكمال النواقص"
4. Dialog opens (with existing selections visible)
5. Uncheck all selections
6. Confirm dialog (should allow empty confirmation)
7. Submit form

### Expected Results:
- [ ] Dialog allows confirming with no selections
- [ ] "تأكيد" button enabled even with empty selections
- [ ] After submission, deficiencies list shows empty state
- [ ] "لا توجد نواقص" message displays

### Notes:
- Empty deficiencies array should be valid
- All previous items should be soft-deleted

---

## Test Scenario 10: Error Handling

### Test Case 10.1: Network Error
1. Disconnect internet while dialog loading
2. Observe error handling

Expected:
- [ ] Loading spinner shows temporarily
- [ ] Error snackbar appears: "فشل في تحميل النواقص"
- [ ] Dialog remains open
- [ ] User can retry

### Test Case 10.2: Invalid Request Status
1. Try to submit deficiencies for a Registered request
2. Observe error response

Expected:
- [ ] API returns 400 Bad Request
- [ ] Error message displays: "Request cannot be edited in current status"
- [ ] Form state preserved

### Test Case 10.3: API Timeout
1. Simulate slow API response
2. Submit form

Expected:
- [ ] Submit button shows spinner
- [ ] Timeout handled gracefully
- [ ] Error message displayed
- [ ] Form remains accessible for retry

---

## Test Scenario 11: Responsive Design

### Test Case 11.1: Mobile View
1. Open application in mobile viewport (375px width)
2. Navigate to deficiencies dialog
3. Test interactions

Expected:
- [ ] Dialog fits on mobile screen
- [ ] Checkboxes properly spaced
- [ ] Type headers readable
- [ ] Confirm button clickable
- [ ] No horizontal scroll needed

### Test Case 11.2: Tablet View
1. Open application in tablet viewport (768px width)
2. Navigate to deficiencies section

Expected:
- [ ] Layout adapts properly
- [ ] All elements accessible
- [ ] No overflow issues

---

## Test Scenario 12: RTL (Arabic) Layout

### Expected:
- [ ] All text right-aligned
- [ ] Dialog direction="rtl" applied
- [ ] Icons positioned correctly
- [ ] Checkboxes on right side
- [ ] Buttons properly aligned
- [ ] Scrollbars on correct side

### Notes:
- Inspect element styles
- Verify margin-right/margin-left direction

---

## Test Scenario 13: Accessibility

### Test Case 13.1: Keyboard Navigation
1. Open dialog
2. Use Tab key to navigate
3. Use Enter/Space to select items

Expected:
- [ ] Can tab through all interactive elements
- [ ] Can select/deselect with Space
- [ ] Can submit with Enter
- [ ] Focus visible on all elements

### Test Case 13.2: Screen Reader
1. Open dialog with screen reader enabled
2. Navigate through options

Expected:
- [ ] Dialog title announced
- [ ] Type headers announced
- [ ] Checkbox labels announced
- [ ] Button labels announced

---

## Test Scenario 14: Performance

### Test Case 14.1: Dialog Load Time
Expected:
- [ ] Dialog opens within 2 seconds
- [ ] Loading spinner not visible for long periods

### Test Case 14.2: Selection Performance
1. Select/deselect many items rapidly
2. Monitor browser performance

Expected:
- [ ] No lag or jank
- [ ] Count badges update instantly
- [ ] No memory issues

### Test Case 14.3: Form Submission
1. Submit form with multiple deficiencies

Expected:
- [ ] Submit completes within 5 seconds
- [ ] No timeouts
- [ ] Network payload reasonably sized

---

## Test Scenario 15: State Management

### Test Case 15.1: Component Cleanup
1. Open dialog
2. Close dialog via cancel
3. Open new dialog instance

Expected:
- [ ] No duplicate listeners
- [ ] Clean state in new dialog
- [ ] No memory leaks

### Test Case 15.2: Form Persistence
1. Fill form partially
2. Select deficiencies
3. Navigate away (without submitting)
4. Navigate back

Expected:
- [ ] Form state preserved (depends on implementation)
- [ ] Selections may or may not persist (document expected behavior)

---

## Browser Compatibility Tests

Test on:
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Edge (latest)

Expected:
- [ ] All features work identically
- [ ] No console errors
- [ ] Styling consistent

---

## Integration Tests

### Test 1: Complete Workflow
1. Create case registration request
2. Add plaintiffs/defendants
3. Add claims
4. Add classifications
5. Request completion with deficiencies
6. Verify all data persists

Expected:
- [ ] All features work together
- [ ] No conflicts
- [ ] Data integrity maintained

### Test 2: Multiple Actions
1. Submit RequestCompletion with deficiencies
2. View request (status = PendingCompletion)
3. Edit deficiencies
4. Verify changes reflected

Expected:
- [ ] Updates work correctly
- [ ] No data loss
- [ ] Audit trail maintained

---

## Sign-off Checklist

- [ ] All manual tests passed
- [ ] No critical bugs found
- [ ] No console errors
- [ ] Responsive design verified
- [ ] Accessibility checked
- [ ] Performance acceptable
- [ ] Browser compatibility verified
- [ ] Integration with other features confirmed
- [ ] Error handling tested
- [ ] RTL layout correct

---

## Known Issues / Observations

Document any issues found:

| Issue | Severity | Status | Notes |
|-------|----------|--------|-------|
|       |          |        |       |

---

## Test Execution Summary

- **Date**: _______________
- **Tester**: _______________
- **Total Tests**: 15 scenarios
- **Passed**: _____
- **Failed**: _____
- **Blocked**: _____
- **Overall Status**: ☐ Pass ☐ Fail

---

## Notes and Recommendations

[Add any additional notes or recommendations here]
