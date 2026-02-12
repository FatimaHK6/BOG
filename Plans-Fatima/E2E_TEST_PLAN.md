# End-to-End Test Plan - Case Registration Module (Developer-B)
## نظام إدارة الدعاوى - خطة اختبار شاملة

**Document Version:** 1.0
**Date Created:** 2026-01-18
**Last Updated:** 2026-01-18
**Project:** Legal Case Management System (BOG)
**Module:** Case Registration (UC 6.5.1.1.11 - 6.5.1.1.24)
**Scope:** Developer-B (Defendants, Case Data, Attachments, Request Actions)

---

## 1. Test Objectives

### Primary Objectives:
- ✅ Verify the complete Case Registration workflow from creation to completion
- ✅ Validate all CRUD operations (Create, Read, Update, Delete)
- ✅ Ensure proper data validation and error handling
- ✅ Confirm state transitions work correctly
- ✅ Verify API integration with backend
- ✅ Test file upload with BR04 validation (PDF, max 4MB)
- ✅ Validate all error codes (ERR001-013, BR04-05) display correctly

### Secondary Objectives:
- ✅ Ensure RTL (Arabic) layout works properly
- ✅ Test responsive design (Desktop, Tablet, Mobile)
- ✅ Verify accessibility (keyboard navigation, screen reader support)
- ✅ Performance testing (page load times, auto-save response)

---

## 2. Scope

### In Scope (Developer-B):
- ✅ Case Registration Request CRUD
- ✅ Defendants Management (Full CRUD)
- ✅ Case Data Form (Subject, Evidence, Classifications)
- ✅ Attachments Upload & Management
- ✅ Additional Info Form (Notes, Priority, Related Cases)
- ✅ Request Actions (Submit, Register, Reject, Request Completion)
- ✅ Request List with Search & Pagination
- ✅ State Transitions & Permissions

### Out of Scope (Developer-A):
- ❌ Plaintiffs Management
- ❌ Court Selection
- ❌ Judge Assignment
- ❌ Case Approval Workflow

---

## 3. Prerequisites

### System Requirements:
- **Browser**: Chrome/Edge (latest version)
- **Backend API**: Running on `http://localhost:5000`
- **Frontend Dev Server**: Running on `http://localhost:4201`
- **Database**: SQL Server LocalDB with migrations applied
- **Network**: Localhost connection available

### Test Data Setup:
```bash
# Backend must be running
cd src/Backend
dotnet run --project BOG.API

# Frontend must be running
cd src/Frontend/bog-app
ng serve --port 4201
```

### Test Users:
- **Default User**: System Administrator (full permissions)
- **Court ID**: 1 (Default court)

---

## 4. High-Level Test Scenarios

### **Scenario 1: Complete Case Registration Workflow**
**Goal**: Test the entire flow from creating a request to submitting it

#### Steps:
1. ✅ Navigate to Case Registration
2. ✅ Create new case request
3. ✅ Add defendants (2+ defendants)
4. ✅ Fill case data (subject, evidence)
5. ✅ Upload attachments (mandatory)
6. ✅ Add additional info
7. ✅ Submit request
8. ✅ Verify state transition to "Submitted"

#### Expected Results:
- All data saved correctly
- Auto-save works (2-second debounce)
- Request status changes to "Submitted"
- Success notifications appear
- User redirected to request details (view mode)

---

### **Scenario 2: Defendants Management**
**Goal**: Test full CRUD operations for defendants

#### Test Cases:

**TC2.1: Add Natural Person Defendant**
- Type: Natural (طبيعي)
- Full Name: أحمد محمد الشريف
- Identity Type: National ID (هوية وطنية)
- Identity Number: 1234567890
- Address: الرياض، حي النخيل
- Expected: Defendant appears in table, count badge increments

**TC2.2: Add Company Defendant**
- Type: Company (شركة)
- Full Name: شركة النور للتجارة
- Commercial Reg: 123456
- Expected: Company-specific fields visible, defendant added

**TC2.3: Add Government Entity Defendant**
- Type: Government Entity (جهة حكومية)
- Full Name: وزارة العدل
- Expected: Government-specific dropdown visible

**TC2.4: Duplicate Detection (ERR013)**
- Add defendant with identity number: 1234567890
- Try to add another with same number
- Expected: Error "ERR013: المدعى عليه موجود مسبقاً"

**TC2.5: Edit Defendant**
- Edit existing defendant
- Change name and address
- Save changes
- Expected: Changes persist in table

**TC2.6: Delete Defendant**
- Delete a defendant
- Confirm in dialog
- Expected: Defendant removed, count badge decrements

**TC2.7: Validation - Missing Required Fields**
- Try to add defendant without full name
- Expected: Error message "الاسم الكامل مطلوب"

**TC2.8: Validation - Maximum Defendants**
- Add 50+ defendants (if limit exists)
- Expected: Either succeeds or shows appropriate limit error

---

### **Scenario 3: Case Data Management**
**Goal**: Test case information form and validation

#### Test Cases:

**TC3.1: Auto-Save Functionality**
- Fill subject field
- Wait 2 seconds without clicking save
- Refresh page
- Expected: Data persists (was auto-saved)

**TC3.2: Subject Validation (ERR006)**
- Try to submit without subject
- Expected: Error "ERR006: موضوع الدعوى مطلوب"

**TC3.3: Evidence Validation (ERR007)**
- Try to submit without evidence
- Expected: Error "ERR007: الأدلة مطلوبة"

**TC3.4: Character Limits**
- Enter > 4000 characters in subject
- Expected: Input blocked at 4000 chars, hint shows count

**TC3.5: Classifications Field**
- Open classifications dropdown
- Expected: Dropdown opens with available options

---

### **Scenario 4: Attachments Management**
**Goal**: Test file upload with BR04 validation

#### Test Cases:

**TC4.1: Upload Valid PDF (< 4MB)**
- Select PDF file (2MB)
- Expected: File uploaded, appears in table

**TC4.2: File Size Validation (BR04)**
- Try to upload PDF > 4MB
- Expected: Error "BR04: حجم الملف لا يزيد عن 4 ميجابايت"

**TC4.3: File Type Validation (BR04)**
- Try to upload .doc or .txt file
- Expected: Error "BR04: يقبل فقط ملفات PDF"

**TC4.4: Multiple Files**
- Upload 3 different PDF files
- Expected: All files appear in table

**TC4.5: Delete Attachment**
- Delete an uploaded file
- Expected: File removed from list

**TC4.6: Mandatory Attachments (ERR003)**
- Try to submit without uploading required documents
- Expected: Error "ERR003: المرفقات الإلزامية غير مكتملة"

**TC4.7: Download Attachment**
- Click download icon on attachment
- Expected: File downloads to computer

---

### **Scenario 5: Request Actions & State Transitions**
**Goal**: Test submit, register, reject, and request completion actions

#### Test Cases:

**TC5.1: Submit Request (Draft → Submitted)**
- Create request with all required data
- Click "إرسال الطلب" (Submit)
- Expected: Status changes to "Submitted"
- Success message: "تم إرسال الطلب بنجاح"

**TC5.2: Register Case (New → Registered)**
- Request status must be "New" (3)
- Click "تسجيل الدعوى" (Register)
- Expected:
  - Status changes to "Registered"
  - Case number generated and displayed
  - Success message appears

**TC5.3: Reject Request**
- Request in appropriate status
- Click "رفض الطلب" (Reject)
- Dialog asks for rejection notes
- Expected:
  - Status changes to "Rejected"
  - Notes saved with request

**TC5.4: Request Completion (For Pending Completion)**
- Request status is "PendingCompletion" (8)
- Click "طلب إكمال" (Request Completion)
- Dialog asks for deficiencies
- Expected: Status updated, deficiencies noted

**TC5.5: Action Button Visibility by Status**

| Status | Submit | Register | Reject | ReqCompletion |
|--------|--------|----------|--------|---------------|
| Draft (1) | ✅ | ❌ | ❌ | ❌ |
| New (3) | ❌ | ✅ | ✅ | ✅ |
| Registered (6) | ❌ | ❌ | ❌ | ❌ |
| PendingCompletion (8) | ❌ | ❌ | ✅ | ✅ |

---

### **Scenario 6: Request List & Search**
**Goal**: Test request list page with search and pagination

#### Test Cases:

**TC6.1: View Request List**
- Navigate to Case Registration
- Expected: List shows all requests

**TC6.2: Search by Request Number**
- Enter request number in search field
- Click search button
- Expected: Only matching requests shown

**TC6.3: Filter by Status**
- Select status from dropdown (e.g., "Draft")
- Click search
- Expected: Only requests with that status shown

**TC6.4: Pagination**
- Change page size to 25 items
- Navigate to page 2
- Expected: Correct items displayed

**TC6.5: Sort by Date**
- Click on date column header
- Expected: List sorted by date

**TC6.6: View Request Details**
- Click "visibility" icon on request row
- Expected: Request details page loads in view mode

**TC6.7: Edit Request**
- Click "edit" icon on request row
- Expected: Request details page loads in edit mode

---

### **Scenario 7: Additional Info Form**
**Goal**: Test additional information form

#### Test Cases:

**TC7.1: Add Notes**
- Enter text in notes field
- Expected: Text saves with auto-save

**TC7.2: Set Priority**
- Select priority level (1-3)
- Expected: Priority saved

**TC7.3: Related Case Reference**
- Enter related case number
- Expected: Field saves value

**TC7.4: Character Limits**
- Enter > 2000 chars in notes
- Expected: Input blocked at 2000

---

### **Scenario 8: Validation & Error Handling**
**Goal**: Test all error codes display correctly

#### Error Code Coverage:

| Error Code | Trigger | Expected Display |
|-----------|---------|-----------------|
| ERR001 | Submit without plaintiffs | Toast: "ERR001: يجب إضافة مدعٍ واحد على الأقل" |
| ERR002 | Submit without defendants | Toast: "ERR002: يجب إضافة مدعى عليه واحد" |
| ERR003 | Submit without mandatory attachments | Toast: "ERR003: المرفقات الإلزامية غير مكتملة" |
| ERR005 | Submit without classifications | Toast: "ERR005: التصنيفات مطلوبة" |
| ERR006 | Submit without subject | Toast: "ERR006: موضوع الدعوى مطلوب" |
| ERR007 | Submit without evidence | Toast: "ERR007: الأدلة مطلوبة" |
| ERR013 | Add duplicate defendant | Dialog: "ERR013: المدعى عليه موجود مسبقاً" |
| BR04 | Upload non-PDF or > 4MB | Toast: "BR04: ملفات PDF فقط، أقصى حجم 4 ميجابايت" |
| BR05 | Auto-reject after 30 days | Warning: "سيتم رفض الطلب تلقائياً بعد 30 يوماً" |

---

### **Scenario 9: UI/UX Testing**
**Goal**: Verify UI elements and user experience

#### Test Cases:

**TC9.1: RTL Layout**
- Verify all text flows right-to-left
- Verify icons align correctly
- Expected: Proper RTL display

**TC9.2: Material Design Theme**
- Verify primary color is #1B5E20 (green)
- Check button styling
- Expected: Consistent green theme

**TC9.3: Sidebar Navigation**
- Click each sidebar item
- Expected: Page scrolls to correct section

**TC9.4: Mobile Responsive**
- Resize browser to mobile (375px)
- Expected: Layout adapts, sidebar becomes collapsible FAB

**TC9.5: Loading States**
- Observe loading spinner when data loads
- Expected: Spinner shows during API calls

**TC9.6: Success/Error Messages**
- Trigger success action
- Trigger error action
- Expected: Proper toast notifications appear

---

### **Scenario 10: Performance Testing**
**Goal**: Verify application performance

#### Test Cases:

**TC10.1: Page Load Time**
- Load Case Registration list
- Measure time to interactive
- Expected: < 3 seconds

**TC10.2: Auto-Save Response**
- Edit field and wait for auto-save
- Expected: Save completes within 2-5 seconds

**TC10.3: Large List Performance**
- Load request list with 1000+ items
- Expected: Page scrolls smoothly, pagination works

**TC10.4: File Upload Progress**
- Upload large PDF (3.9MB)
- Expected: Upload completes within 5 seconds

---

## 5. Test Execution Strategy

### Phase 1: Smoke Testing (Day 1)
- ✅ Verify app loads without errors
- ✅ Test basic navigation
- ✅ Test adding first defendant
- ✅ Test submitting request

### Phase 2: Functional Testing (Day 2-3)
- ✅ All CRUD operations for defendants
- ✅ Case data validation
- ✅ Attachments upload
- ✅ Request actions

### Phase 3: Validation & Error Testing (Day 4)
- ✅ All error codes appear
- ✅ Form validation works
- ✅ Duplicate detection (ERR013)
- ✅ File validation (BR04)

### Phase 4: UI/UX & Performance (Day 5)
- ✅ RTL layout
- ✅ Mobile responsiveness
- ✅ Performance benchmarks
- ✅ Accessibility

---

## 6. Test Data Requirements

### Sample Test Data:

**Defendants:**
```
1. Natural Person:
   Name: أحمد محمد الشريف
   Identity: 1234567890 (National ID)
   Address: الرياض، حي النخيل

2. Company:
   Name: شركة النور للتجارة
   Reg: 123456
   Address: جدة، المنطقة الصناعية

3. Government:
   Name: وزارة العدل
   Address: الرياض
```

**Case Data:**
```
Subject: دعوى استرجاع مبالغ مالية مستحقة
Evidence: عقود، فواتير، تحويلات بنكية
Classifications: مدني، مالي
```

**Attachments:**
```
- contract.pdf (2MB)
- invoice.pdf (1.5MB)
- bank_statement.pdf (3MB)
```

---

## 7. Success Criteria

### Functional Success (Must Pass):
- ✅ 100% of CRUD operations work
- ✅ All validation rules enforced
- ✅ All error codes display correctly
- ✅ State transitions work properly
- ✅ Auto-save functionality works
- ✅ File upload with BR04 validation works
- ✅ All API endpoints respond correctly

### UI/UX Success (Must Pass):
- ✅ RTL layout correct
- ✅ Green theme (#1B5E20) consistent
- ✅ No English text in buttons
- ✅ All icons render correctly
- ✅ Responsive design works

### Performance Success (Must Pass):
- ✅ Page load < 3 seconds
- ✅ Auto-save completes within 5 seconds
- ✅ File upload < 10 seconds
- ✅ No console errors

### Accessibility Success (Should Pass):
- ✅ Keyboard navigation works
- ✅ Form labels properly associated
- ✅ Error messages descriptive
- ✅ Color contrast WCAG AA compliant

---

## 8. Test Reporting

### Daily Report:
- Total tests executed
- Passed / Failed / Blocked
- Critical bugs
- Blockers
- Screenshots of failures

### Final Report:
- Executive summary
- Test coverage percentage
- Defect summary by severity
- Risk assessment
- Release readiness

---

## 9. Risk Assessment

### High Risk Areas:
1. **API Integration** - Backend communication failures
2. **File Upload** - BR04 validation edge cases
3. **State Transitions** - Incorrect permission checks
4. **Duplicate Detection (ERR013)** - Database constraint issues

### Mitigation:
- ✅ Daily backend status verification
- ✅ File validation testing with boundary values
- ✅ State transition matrix testing
- ✅ Database constraint verification

---

## 10. Entry & Exit Criteria

### Entry Criteria:
- ✅ Backend API running and tested
- ✅ Frontend application compiled without errors
- ✅ Test environment prepared
- ✅ Test data loaded
- ✅ Test users created

### Exit Criteria:
- ✅ All critical bugs fixed
- ✅ All high-priority tests passed
- ✅ 90%+ test coverage achieved
- ✅ No blocker defects remain
- ✅ Performance benchmarks met
- ✅ Product Owner approval obtained

---

## 11. Test Tools & Environment

### Tools:
- **Browser**: Chrome/Edge DevTools
- **API Testing**: Postman/curl
- **Performance**: Chrome Lighthouse
- **Accessibility**: WAVE, Axe DevTools
- **Automated**: Cypress (future implementation)

### Environment URLs:
- **Frontend**: http://localhost:4201
- **Backend API**: http://localhost:5000
- **API Docs**: http://localhost:5000/swagger

---

## 12. Contact & Support

**Test Lead**: [Your Name]
**QA Team**: [Team]
**Backend Contact**: [Developer-B]
**Frontend Contact**: [Developer-A/C]

---

**Document Status**: Ready for Testing
**Approval**: _______________
**Date**: _______________

