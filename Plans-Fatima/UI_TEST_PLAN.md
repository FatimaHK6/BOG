# Case Registration UI - Comprehensive Test Plan

**Project**: BOG (Legal Case Management System)
**Module**: Case Registration (UC 6.5.1)
**Scope**: Developer-B (Defendants Management)
**Date**: 2026-01-18
**Status**: Ready for Testing

---

## Test Plan Overview

### Objective
Verify that the Angular frontend for Case Registration module works correctly across all pages, components, and user interactions, following RTL Arabic layout and Material Design principles.

### Scope
- **In Scope**:
  - Request List Page (Search, Filter, Pagination)
  - Request Details Page (View/Edit/Create)
  - Defendants Management (CRUD operations)
  - Case Data Form
  - Tabbed Navigation
  - RTL Layout
  - Material Components (Dropdowns, Dialogs, Tables, Forms)

- **Out of Scope**:
  - Plaintiffs Management (Developer-A scope)
  - File Uploads/Attachments (incomplete)
  - API Backend (assumed working)

### Test Environment
- **URL**: `http://localhost:4200`
- **Browser**: Chrome/Edge (latest)
- **Backend API**: Running on `https://localhost:5001`
- **Database**: SQL Server LocalDB (BOG database)

---

## Test Cases

### Section 1: Request List Page (`/case-registration/list`)

#### TC-1.1: Page Load & Initial Display
**Precondition**: User is logged in and navigates to request list page

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to `/case-registration/list` | Page loads successfully |
| 2 | Wait for data to load | List of requests displays in table |
| 3 | Check page layout | Header shows "قائمة طلبات التسجيل" (Request List) |
| 4 | Check sidebar | Right-side navigation menu visible |
| 5 | Verify RTL layout | All text is right-aligned, Arabic correct direction |

**Expected**: ✅ All requests display with columns: رقم الطلب, الموضوع, الحالة, التاريخ, الإجراءات

---

#### TC-1.2: Create New Request Button
**Precondition**: User is on request list page

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "طلب جديد" (New Request) button | Dialog/page for creating new request appears |
| 2 | System creates new draft request | Request is created with Status = "مسودة" (Draft) |
| 3 | User is redirected | URL changes to `/case-registration/{id}/edit` |
| 4 | Sidebar displays | Request sections visible on right sidebar |

**Expected**: ✅ New request created and displayed in edit mode

---

#### TC-1.3: Search & Filter - Request Number
**Precondition**: At least 5 requests exist in database

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter request number in search field | Input accepts Arabic/English characters |
| 2 | Click search icon (magnifying glass) | Dropdown with matching requests displays OR table updates |
| 3 | Select a request | Table filters to show only that request |
| 4 | Clear search field | All requests display again |

**Expected**: ✅ Search filters requests by number correctly

---

#### TC-1.4: Filter by Status (الحالة)
**Precondition**: Requests with different statuses exist

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "الحالة" (Status) dropdown | ❌ **KNOWN ISSUE**: Dropdown overlaps field |
| 2 | Select "مسودة" (Draft) | Only draft requests display |
| 3 | Select "جديد" (New) | Only new requests display |
| 4 | Select "-- الكل --" (All) | All requests display |

**Expected**: ✅ Dropdown filters work (when dropdown issue is fixed)
**Current Status**: ❌ Dropdown overlapping - UNFIXED

---

#### TC-1.5: Pagination
**Precondition**: More than 10 requests exist

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Check page shows 10 items | Default page size is 10 |
| 2 | Click next page arrow | Page 2 displays |
| 3 | Change items per page to 25 | Table shows 25 items |
| 4 | Change items per page to 50 | Table shows 50 items |
| 5 | Navigate back to page 1 | First page displays |

**Expected**: ✅ Pagination works correctly (10, 25, 50 per page)

---

#### TC-1.6: View/Edit/Delete Actions
**Precondition**: At least one request exists

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click eye icon (عرض) | Request details page opens in view mode |
| 2 | Go back to list | Edit mode button (تعديل) shown in list |
| 3 | Click edit icon | Request details page opens in edit mode |
| 4 | Edit icon is disabled | Cannot edit if status is not Draft |

**Expected**: ✅ View/Edit actions work for appropriate statuses

---

### Section 2: Request Details Page (`/case-registration/{id}/edit`)

#### TC-2.1: Page Layout & Sidebar Navigation
**Precondition**: User has opened a request in edit mode

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Check header bar | Shows request number, status badge |
| 2 | Check sidebar | Lists sections: المدعون, المدعى عليهم, بيانات الدعوى, المرفقات, etc. |
| 3 | Check active section | Current section highlighted in sidebar |
| 4 | Check desktop layout | Sidebar sticky on right, content on left |
| 5 | Verify RTL layout | Sidebar on right side (Arabic RTL correct) |

**Expected**: ✅ Layout is correct with proper RTL positioning

---

#### TC-2.2: Tabbed Navigation (Clicking Sidebar Items)
**Precondition**: User is on request details page

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "المدعى عليهم" in sidebar | Defendants section displays (tab-based) |
| 2 | Click "بيانات الدعوى" | Case Data form displays |
| 3 | Check URL | URL includes `?tab=defendants` or similar |
| 4 | Click "المرفقات" | Attachments section displays |
| 5 | Go back in browser history | Previous tab is restored |

**Expected**: ✅ Tab navigation works with URL parameters

---

#### TC-2.3: Request Status Display
**Precondition**: Request with different status exists

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Check status badge in header | Shows current status (مسودة, جديد, مسجل, etc.) |
| 2 | Check status color | Color matches status: Gray=Draft, Blue=New, Green=Registered |
| 3 | Verify status text | Arabic text is correct |

**Expected**: ✅ Status displays with correct color and text

---

### Section 3: Defendants Management (المدعى عليهم)

#### TC-3.1: Add First Defendant (Empty State)
**Precondition**: New request created with no defendants yet

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Go to Defendants tab | Empty state message displays |
| 2 | See message: "لم يتم إضافة أي مدعى عليه بعد" | Message shown |
| 3 | Click "إضافة أول مدعى عليه" button | ❌ **KNOWN ISSUE**: Button click doesn't open dialog |
| 4 | Dialog should open | Form appears with fields |

**Expected**: ✅ Defendant form dialog opens
**Current Status**: ❌ Button not working - UNFIXED

---

#### TC-3.2: Add Defendant - Form Fields
**Precondition**: Defendant form dialog is open

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select "نوع المدعى عليه" (Defendant Type) | Dropdown shows: طبيعي, شركة, جهة حكومية, etc. |
| 2 | Enter "الاسم الكامل" (Full Name) | Text field accepts input, shows character count |
| 3 | Select "نوع الهوية" (Identity Type) | Dropdown shows: هوية وطنية, إقامة, جواز سفر, etc. |
| 4 | Enter "رقم الهوية" (Identity Number) | Number field accepts input |
| 5 | Enter "العنوان" (Address) | Text area accepts multiple lines |

**Expected**: ✅ All form fields work correctly

---

#### TC-3.3: Add Defendant - Form Validation
**Precondition**: Defendant form is open and empty

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "إضافة" (Add) button | Error: "الاسم الكامل مطلوب" (Name required) |
| 2 | Enter name, try to submit | Form validates before submitting |
| 3 | Enter 300 characters in name | Name field limited to 200 characters |
| 4 | Enter name with special chars | Special chars accepted |

**Expected**: ✅ Form validation works correctly

---

#### TC-3.4: Add Defendant - Submit & Save
**Precondition**: Form has valid data

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Fill form with valid data | All required fields completed |
| 2 | Click "إضافة" button | Loading spinner shows |
| 3 | Wait for API response | API call completes (or error shows) |
| 4 | Check success message | SnackBar shows: "تم إضافة المدعى عليه بنجاح" |
| 5 | Dialog closes | Form dialog disappears |
| 6 | Check defendants list | New defendant appears in table |

**Expected**: ✅ Defendant created and displayed in list

---

#### TC-3.5: View Defendants List
**Precondition**: At least one defendant exists

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Go to Defendants tab | Table displays with columns: النوع, الاسم الكامل, رقم الهوية, العنوان, الإجراءات |
| 2 | Check defendant type badge | Shows type in green badge (e.g., "طبيعي") |
| 3 | Check name column | Full name displays |
| 4 | Check identity column | Identity number and type show |
| 5 | Check count badge | Sidebar shows count (e.g., "2") |

**Expected**: ✅ Defendants list displays correctly

---

#### TC-3.6: Edit Defendant
**Precondition**: At least one defendant in list

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click three-dot menu on defendant row | Menu shows: تعديل (Edit), حذف (Delete) |
| 2 | Click "تعديل" | Edit form dialog opens with current data |
| 3 | Modify name | Name field is editable |
| 4 | Click "حفظ" (Save) | Changes saved, dialog closes |
| 5 | Check list | Updated data displays in table |

**Expected**: ✅ Defendant edit works

---

#### TC-3.7: Delete Defendant
**Precondition**: At least two defendants exist

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click three-dot menu | Menu appears |
| 2 | Click "حذف" (Delete) | Confirmation dialog shows |
| 3 | Message shows: "هل أنت متأكد من حذف المدعى عليه 'الاسم'" | Confirmation dialog with defendant name |
| 4 | Click "حذف" in confirmation | Defendant deleted from list |
| 5 | Check count | Sidebar count decreases |

**Expected**: ✅ Defendant deleted with confirmation

---

#### TC-3.8: Duplicate Defendant Validation (ERR013)
**Precondition**: Defendant with identity number exists

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Try to add defendant with same ID number | Form submitted |
| 2 | Server returns ERR013 error | Error message shows: "ERR013: المدعى عليه موجود مسبقاً" |
| 3 | Form stays open | User can modify and retry |

**Expected**: ✅ Duplicate validation works

---

### Section 4: Case Data Form (بيانات الدعوى)

#### TC-4.1: Case Data Form Fields
**Precondition**: Request is in edit mode

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Go to Case Data tab | Form displays with fields |
| 2 | Check "موضوع الدعوى" field | Text area with 4000 char limit, counter shows |
| 3 | Check "الأدلة والمستندات" field | Text area with 4000 char limit |
| 4 | Check "التصنيفات" dropdown | ❌ **KNOWN ISSUE**: Dropdown overlaps |

**Expected**: ✅ Form fields display (when dropdown fixed)

---

#### TC-4.2: Character Counter
**Precondition**: Case Data form is open

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Type 50 characters in subject field | Counter shows "50/4000" |
| 2 | Type 500 characters | Counter shows "500/4000" |
| 3 | Try typing past 4000 characters | Additional text not accepted |
| 4 | Delete characters | Counter updates in real-time |

**Expected**: ✅ Character counter works correctly

---

#### TC-4.3: Form Validation (ERR006, ERR007)
**Precondition**: Case Data form is open

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Leave subject field empty | Field is marked as required |
| 2 | Try to submit without subject | Error: "موضوع الدعوى مطلوب (ERR006)" |
| 3 | Leave evidence field empty | Field is marked as required |
| 4 | Try to submit without evidence | Error: "الأدلة مطلوبة (ERR007)" |

**Expected**: ✅ Validation errors display correctly

---

### Section 5: Material Components & Styling

#### TC-5.1: Dropdown Issues (Known Issue)
**Precondition**: Any dropdown in the app

| Status | Issue | Severity | Fix Status |
|--------|-------|----------|-----------|
| 🔴 | Dropdown overlaps field label | High | ❌ UNFIXED |
| 🔴 | Label not visible above dropdown | High | ❌ UNFIXED |
| 🔴 | Dropdown doesn't close on outside click | Medium | ❌ UNFIXED |

**Dropdowns Affected**:
- الحالة (Status) - Request List
- التصنيفات (Classifications) - Case Data
- نوع المدعى عليه (Defendant Type) - Defendant Form
- نوع الهوية (Identity Type) - Defendant Form
- الأولوية (Priority) - Various forms
- الجهة الحكومية (Government Agency) - Defendant Form

---

#### TC-5.2: RTL Layout Verification
**Precondition**: Page is fully loaded

| Element | Check | Expected |
|---------|-------|----------|
| Page direction | Text direction | Right-to-left (RTL) |
| Sidebar | Position | Right side of page |
| Buttons | Icon position | Icons on right, text on left |
| Tables | Header order | Columns in RTL order |
| Dialogs | Direction | Dialog respects RTL |
| Forms | Label position | Labels right of inputs |

**Expected**: ✅ All elements properly RTL-aligned

---

#### TC-5.3: Material Icons Display
**Precondition**: Page is loaded

| Icon | Location | Expected |
|------|----------|----------|
| add | New Request button | Icon displays |
| edit | Defendant edit button | Icon displays |
| delete | Defendant delete button | Icon displays |
| visibility | View request button | Icon displays |
| more_vert | Action menu button | Icon displays |

**Expected**: ✅ All Material Icons display correctly (not as boxes)

---

#### TC-5.4: Dialog Display
**Precondition**: Any dialog opens

| Element | Check |
|---------|-------|
| Dialog title | Arabic text displays correctly |
| Form fields | All fields visible |
| Buttons | Cancel and Save buttons visible |
| Scrolling | If form is long, scrolling works |
| Backdrop | Dark overlay behind dialog |
| Close button | X button in top-right of dialog |

**Expected**: ✅ Dialog displays correctly with RTL support

---

### Section 6: Responsive Design

#### TC-6.1: Desktop View (≥1200px)
**Precondition**: Browser width ≥ 1200px

| Element | Expected |
|---------|----------|
| Sidebar | Visible on right, sticky position |
| Content area | Takes up remaining space |
| Layout | Two-column layout |

**Expected**: ✅ Desktop layout displays correctly

---

#### TC-6.2: Tablet View (768-1199px)
**Precondition**: Browser width between 768-1199px

| Element | Expected |
|---------|----------|
| Sidebar | Collapsible or hidden |
| Toggle button | Menu button appears |
| Content | Takes full width when sidebar hidden |

**Expected**: ✅ Tablet layout works

---

#### TC-6.3: Mobile View (<768px)
**Precondition**: Browser width < 768px

| Element | Expected |
|---------|----------|
| Sidebar | Hidden by default |
| Mobile menu button | FAB button appears (bottom-right) |
| Content | Full width |
| Toggle | Click FAB to show/hide sidebar |

**Expected**: ✅ Mobile layout responsive

---

### Section 7: Auto-Save & State Management

#### TC-7.1: Form Auto-Save
**Precondition**: Form field has focus

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Type in case data subject field | Text input accepted |
| 2 | Wait 2 seconds | Auto-save indicator shows (if implemented) |
| 3 | Navigate away | Draft saved automatically |
| 4 | Return to form | Data is preserved |

**Expected**: ✅ Auto-save works (if implemented)

---

#### TC-7.2: State Preservation on Tab Switch
**Precondition**: Form has unsaved data

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Type in case data form | Data entered |
| 2 | Click another tab | Switch to defendants |
| 3 | Click back to case data | Data is still there |

**Expected**: ✅ Data preserved when switching tabs

---

### Section 8: Error Handling

#### TC-8.1: API Error Handling
**Precondition**: Backend API is down or returns error

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Try to load requests | Error SnackBar appears |
| 2 | Error message shown | "خطأ في تحميل الطلبات" (Error loading requests) |
| 3 | Retry button available | User can retry |

**Expected**: ✅ Errors handled gracefully

---

#### TC-8.2: Validation Error Messages
**Precondition**: Form validation fails

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Submit form with empty field | Inline error message shows |
| 2 | Error color is red | Visual feedback clear |
| 3 | Error explains issue | Message in Arabic is understandable |

**Expected**: ✅ Validation errors clear and helpful

---

## Known Issues Summary

| Issue | Severity | Status | Impact |
|-------|----------|--------|--------|
| Dropdown Overlapping | 🔴 High | ❌ UNFIXED | Cannot see field labels above dropdowns |
| Add Defendant Button | 🔴 High | ❌ UNFIXED | Cannot add defendants (click not working) |
| Dropdown Not Closing | 🟡 Medium | ❌ UNFIXED | Have to click elsewhere to close |
| CSS Positioning | 🔴 High | ❌ UNFIXED | Requires CDK strategy override or component redesign |

---

## Testing Checklist

### Pre-Test Setup
- [ ] Backend API running on https://localhost:5001
- [ ] Database (BOG) populated with test data
- [ ] Angular dev server running on http://localhost:4200
- [ ] Browser console cleared
- [ ] Browser cache cleared (Ctrl+Shift+Delete)

### Functional Testing
- [ ] Request List page loads
- [ ] Create new request works
- [ ] Search/filter works (when dropdown fixed)
- [ ] Pagination works
- [ ] Request details loads
- [ ] Sidebar navigation works
- [ ] Add defendant works (when button fixed)
- [ ] Edit defendant works
- [ ] Delete defendant works with confirmation
- [ ] Case data form displays
- [ ] Form validation works

### UI/UX Testing
- [ ] RTL layout correct
- [ ] Material icons display
- [ ] Buttons are clickable
- [ ] Form fields are accessible
- [ ] Dialogs open/close correctly
- [ ] SnackBar notifications show

### Responsive Testing
- [ ] Desktop layout (1200+px) works
- [ ] Tablet layout (768-1199px) works
- [ ] Mobile layout (<768px) works

### Browser Testing
- [ ] Chrome - works
- [ ] Edge - works
- [ ] Firefox - works (if required)

---

## Pass/Fail Criteria

### PASS
- All functional tests pass
- All UI elements display correctly
- RTL layout working
- No console errors
- Responsive design working

### FAIL
- Critical issues unfixed (dropdown overlapping, button not working)
- Console errors present
- RTL layout broken
- Non-responsive design

---

## Recommended Next Steps

1. **Fix Dropdown Positioning** (Priority 1)
   - Implement custom CDK positioning strategy
   - Or upgrade Angular Material
   - Or replace with custom component

2. **Fix Add Defendant Button** (Priority 1)
   - Debug JavaScript console errors
   - Check event handler binding
   - Verify DefendantFormDialogComponent is injectable

3. **Complete Missing Features** (Priority 2)
   - File uploads (Attachments)
   - Additional Info form
   - Request Actions

4. **Performance & Security** (Priority 3)
   - Security audit
   - Performance profiling
   - Load testing

---

**Document Version**: 1.0
**Last Updated**: 2026-01-18
**Test Status**: Ready for Execution
