# Phase 2 Testing Guide - Claims & Related Cases

## Quick Start

### Prerequisites
- Node.js 14+ installed
- .NET 8 SDK installed
- SQL Server LocalDB installed (or connection string configured)

### Start Backend API

```bash
# Navigate to backend directory
cd C:\Users\Lenovo\Desktop\Claude\BOG\src\Backend

# Build solution
dotnet build BOG.sln

# Run API
dotnet run --project BOG.API

# Expected output:
# Now listening on: https://localhost:5001
# Application started. Press Ctrl+C to shut down.
```

### Start Frontend (in separate terminal)

```bash
# Navigate to frontend directory
cd C:\Users\Lenovo\Desktop\Claude\BOG\src\Frontend\bog-app

# Install dependencies (if needed)
npm install

# Start dev server
ng serve

# Expected output:
# ✔ Compiled successfully.
# ✔ Listening on localhost:4200
```

Open browser: http://localhost:4200

---

## Test Case 1: Create and Manage Claims

### Steps
1. Navigate to "Create New Case Request" or open existing case
2. Click **"طلبات الدعوى"** (Claims) in the left sidebar
3. Click **"إضافة طلب دعوى"** (Add Claim) button
4. Enter claim text in textarea (test with various lengths):
   - Short text (< 100 chars) - should show green progress bar
   - Long text (1500+ chars) - should show yellow warning progress bar
   - Max text (2000 chars exactly) - should show red error color
5. Click **Save** button

### Expected Results
- ✅ Dialog closes
- ✅ Snackbar shows "تم إضافة طلب الدعوى بنجاح" (Success message)
- ✅ Claim appears in list as a card
- ✅ Count badge "1" appears next to "طلبات الدعوى" in sidebar
- ✅ Card shows claim text, ID, and modification date
- ✅ Character counter below claim text shows correct count

### Edit Test
1. Click on claim card's menu (⋮) button
2. Click **"تعديل"** (Edit)
3. Modify the text
4. Click Save

### Expected Results
- ✅ Dialog closes
- ✅ Snackbar shows "تم تعديل طلب الدعوى بنجاح" (Updated message)
- ✅ Claim text updates in list

### Delete Test
1. Click claim card's menu (⋮) button
2. Click **"حذف"** (Delete)
3. Confirmation dialog appears
4. Click **"حذف"** (Delete) in dialog

### Expected Results
- ✅ Confirmation dialog appears with claim preview
- ✅ Snackbar shows "سيتم حذف طلب الدعوى عند الحفظ"
- ✅ After saving case, claim is removed

---

## Test Case 2: Create and Manage Related Cases

### Steps
1. In same case request, click **"الدعاوى المرتبطة"** (Related Cases) in sidebar
2. Click **"إضافة دعوى مرتبطة"** (Add Related Case) button
3. Optional: Select a court from dropdown
4. Enter case number (e.g., "12345")
5. Enter case year (e.g., "1445" - Hijri year format)
6. Click **Save** button

### Expected Results
- ✅ Dialog closes
- ✅ Snackbar shows "تم إضافة الدعوى المرتبطة بنجاح" (Success message)
- ✅ Related case appears in table
- ✅ Count badge "1" appears next to "الدعاوى المرتبطة" in sidebar
- ✅ Table shows columns: Court Name | Case Number | Year

### Form Validation Test
1. Try to save without court selected - should allow (optional field)
2. Try to save without case number - should show error "رقم الدعوى مطلوب"
3. Try entering letters in case number - should show error "يجب إدخال رقم صحيح فقط"
4. Try entering year with 3 digits - should show error "يجب إدخال 4 أرقام فقط"

### Edit Test
1. Click on related case row's menu (⋮) button
2. Click **"تعديل"** (Edit)
3. Modify court or numbers
4. Click Save

### Expected Results
- ✅ Dialog closes
- ✅ Table updates with new values

### Delete Test
1. Click related case row's menu (⋮) button
2. Click **"حذف"** (Delete)
3. Confirmation dialog appears
4. Click **"حذف"** (Delete)

### Expected Results
- ✅ Confirmation dialog shows case number/year
- ✅ Snackbar shows confirmation message

---

## Test Case 3: Court Dropdown Loading

### Steps
1. Open Related Case dialog (create or edit)
2. Observe court dropdown

### Expected Results
- ✅ Dropdown loads courts from API
- ✅ Shows "المحاكم" (Courts) optgroup
- ✅ Lists all active courts with Arabic names
- ✅ Shows "بدون محكمة" (No Court) option at top

### Troubleshooting
If courts don't load:
1. Check backend is running on https://localhost:5001
2. Check browser console for errors
3. Verify `GET /api/lookups/courts` returns data

---

## Test Case 4: Data Persistence

### Steps
1. Add 2-3 claims and 2-3 related cases
2. Click elsewhere or navigate to different section
3. Refresh browser (Ctrl+R or Cmd+R)
4. Navigate back to Claims section

### Expected Results
- ✅ All claims and related cases persist
- ✅ Data loads from localStorage
- ✅ Count badges show correct numbers
- ✅ No loss of data on page refresh

---

## Test Case 5: Character Counter Validation

### Steps
1. Open claim form dialog
2. Enter exactly 2000 characters
3. Try to type one more character

### Expected Results
- ✅ Cannot type more than 2000 chars
- ✅ Progress bar is red (error state)
- ✅ Character count shows "2000 / 2000"
- ✅ Save button shows red progress bar
- ✅ Cannot submit form with text > 2000 chars

---

## Test Case 6: UI/UX Tests

### RTL Support
- ✅ All text should be right-aligned
- ✅ Icons should appear on left (mirrored)
- ✅ Form labels right-aligned
- ✅ Checkboxes on left side

### Mobile Responsiveness
1. Open DevTools (F12)
2. Toggle device toolbar (Ctrl+Shift+M)
3. Test on various screen sizes (iPhone, iPad, etc.)

### Expected Results
- ✅ Sidebar becomes collapsible hamburger menu
- ✅ Dialogs become full-width on small screens
- ✅ Table becomes scrollable horizontally
- ✅ All buttons remain accessible
- ✅ Forms stack vertically on mobile

### Material Design
- ✅ All buttons use Material Design styling
- ✅ Cards have proper shadows and hover effects
- ✅ Form fields use outline appearance
- ✅ Icons render correctly
- ✅ Snackbars appear from bottom

---

## Test Case 7: Empty States

### Steps
1. Open new case request
2. Navigate to Claims section (no claims added)
3. Navigate to Related Cases section (no cases added)

### Expected Results
- ✅ Empty state message appears: "لم يتم إضافة أي طلب دعوى بعد" (Claims)
- ✅ Empty state message appears: "لم يتم إضافة أي دعوى مرتبطة بعد" (Related Cases)
- ✅ Large icon appears (description icon for claims, link_off icon for related cases)
- ✅ "Add" button appears in empty state
- ✅ Click empty state button opens appropriate dialog

---

## Test Case 8: Error Handling

### Test Server Error
1. Stop backend API
2. Try to open Related Case dialog
3. Try to load courts

### Expected Results
- ✅ Snackbar shows "خطأ في تحميل المحاكم" (Error message)
- ✅ Courts dropdown shows empty state
- ✅ Application remains responsive

### Test Invalid Input
1. Try to save claim with empty text - should show error
2. Try to save related case with invalid year - should show error

---

## Test Case 9: Navigation

### Steps
1. Add claims and related cases
2. Click different sections in sidebar:
   - Case Data
   - Claims
   - Related Cases
   - Attachments
   - Additional Info
   - Actions

### Expected Results
- ✅ Correct section displays
- ✅ Sidebar item highlights as active
- ✅ URL updates with tab parameter (?tab=claims, etc.)
- ✅ Back navigation preserves state
- ✅ Count badges update correctly for all sections

---

## Test Case 10: Form Validation

### Claims Dialog
1. Click Save without entering text
2. Should show error: "طلب الدعوى مطلوب"

### Related Cases Dialog
1. Leave case number empty, click Save
   - Should show: "رقم الدعوى مطلوب"
2. Leave year empty, click Save
   - Should show: "السنة مطلوبة"
3. Enter non-numeric case number
   - Should show: "يجب إدخال رقم صحيح فقط"
4. Enter 3-digit year
   - Should show: "يجب إدخال 4 أرقام فقط"

---

## Performance Testing

### Large Dataset Test
1. Add 50 claims and 50 related cases
2. Navigate between sections

### Expected Results
- ✅ Application remains responsive
- ✅ No noticeable lag when switching sections
- ✅ Scrolling is smooth
- ✅ localStorage doesn't exceed browser limits

---

## Browser Testing

Tested and recommended browsers:
- ✅ Chrome/Edge 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Mobile browsers (Chrome Android, Safari iOS)

---

## Common Issues & Solutions

### Issue: Courts dropdown is empty
**Solution**:
- Check backend is running
- Verify database has court data
- Check browser console for API errors

### Issue: Data not persisting after refresh
**Solution**:
- Check browser localStorage is enabled
- Clear browser cache and try again
- Check console for localStorage errors

### Issue: Dialog won't close after save
**Solution**:
- Check for JavaScript errors in console
- Try refreshing page
- Check form validation is passing

### Issue: Character counter not updating
**Solution**:
- Type slowly to ensure input registers
- Check textarea is focused
- Try in different browser

---

## Debugging Tips

### Check API Calls
1. Open DevTools (F12)
2. Go to Network tab
3. Look for `/api/lookups/courts` request
4. Check response status and body

### Check State
1. Open Console (F12)
2. Run: `localStorage.getItem('case-data-state')`
3. Should return JSON with claims and relatedCases arrays

### Check Component State
1. Install Angular DevTools extension
2. In DevTools, go to Components tab
3. Find ClaimsListComponent or RelatedCasesListComponent
4. Inspect properties in right panel

---

## Test Summary Checklist

- [ ] Claims can be created
- [ ] Claims can be edited
- [ ] Claims can be deleted
- [ ] Claims persist on refresh
- [ ] Character counter works (0-2000)
- [ ] Related cases can be created
- [ ] Related cases can be edited
- [ ] Related cases can be deleted
- [ ] Related cases persist on refresh
- [ ] Court dropdown populates
- [ ] Form validation works
- [ ] RTL layout is correct
- [ ] Mobile responsive
- [ ] Sidebar badges update
- [ ] Empty states display
- [ ] Error messages show correctly
- [ ] Snackbar notifications appear
- [ ] Navigation works smoothly
- [ ] No console errors
- [ ] Performance is acceptable

---

**All tests should pass for full Phase 2 completion** ✅
