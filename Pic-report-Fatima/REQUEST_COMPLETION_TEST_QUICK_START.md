# Request Completion E2E Tests - Quick Start Guide

## 🚀 Get Started in 5 Minutes

### Step 1: Ensure Services Are Running

**Terminal 1 - Backend API**
```bash
cd src/Backend
dotnet run --project BOG.API
# Wait for: "Now listening on: https://localhost:5001"
```

**Terminal 2 - Frontend Server**
```bash
cd src/Frontend/bog-app
npm install  # Only needed first time
ng serve
# Wait for: "Application bundle generation complete"
```

### Step 2: Run Tests

**Option A: Run all tests (Headless)**
```bash
cd src/Frontend/bog-app
npx cypress run --spec "cypress/e2e/request-completion.cy.ts"
```

**Option B: Interactive UI (Recommended for first run)**
```bash
cd src/Frontend/bog-app
npx cypress open
# Select "E2E Testing" → Click "request-completion.cy.ts"
```

**Option C: Run specific suite**
```bash
npx cypress run --spec "cypress/e2e/request-completion.cy.ts" --grep "Suite 1:"
```

---

## 📊 Test Overview

| Suite | Name | Tests | Focus |
|-------|------|-------|-------|
| 1 | Navigation | 3 | Tab access & routing |
| 2 | Display & State | 5 | Component rendering |
| 3 | Case Types API | 5 | Dropdown integration |
| 4 | Decision Types | 8 | Selection logic |
| 5 | Validation | 7 | Form constraints |
| 6 | Success | 8 | Submission workflows |
| 7 | Errors | 8 | Error handling |
| 8 | Status Control | 5 | Access validation |
| 9 | RTL/Responsive | 8 | Design compatibility |
| **Total** | | **45+** | **Complete Coverage** |

---

## ✅ What Gets Tested

### Form Fields
- ✅ Decision Type dropdown (4 options)
- ✅ Case Type dropdown (API-driven)
- ✅ Notes textarea (4000 char limit)

### Business Rules
- ✅ Only Draft (1) and New (3) statuses allowed
- ✅ Required field validation
- ✅ Character limit enforcement
- ✅ Decision type selection enables submit

### Workflows
- ✅ Navigate to completion tab
- ✅ Select decisions (Register/SendToJudge/Reject/RequestCompletion)
- ✅ Show confirmation dialog
- ✅ Submit and get success message
- ✅ Reset form after success

### Errors
- ✅ ERR005 - Missing classifications
- ✅ ERR002 - Missing defendants
- ✅ ERR010 - Missing attachments
- ✅ Multiple errors in one response
- ✅ Form preserved on error

### UI/UX
- ✅ Arabic text and labels
- ✅ RTL layout direction
- ✅ Desktop/Tablet/Mobile viewports
- ✅ Loading states and spinners
- ✅ Error and success messages

---

## 🎯 Test Examples

### Example 1: Basic Navigation
```typescript
// Go to case registration list
cy.visit('/case-registration/list');

// Create new request
cy.contains('button', 'طلب جديد').click();

// Navigate to completion tab
cy.contains('.nav-item', 'إنهاء الطلب').click();

// Verify component is visible
cy.get('app-request-completion').should('be.visible');
```

### Example 2: Submit a Decision
```typescript
// Select decision type
cy.get('mat-select[formcontrolname="decisionType"]').click({ force: true });
cy.contains('mat-option', 'قيد الدعوى').click();

// Add optional notes
cy.get('textarea[formcontrolname="notes"]').type('ملاحظات');

// Submit
cy.get('button[type="submit"]').click();

// Confirm in dialog
cy.contains('button', 'نعم').click();

// Check success message
cy.contains('تم قيد الدعوى بنجاح').should('be.visible');
```

### Example 3: Validate Form
```typescript
// Submit button should be disabled initially
cy.get('button[type="submit"]').should('be.disabled');

// Select decision
cy.get('mat-select[formcontrolname="decisionType"]').click({ force: true });
cy.contains('mat-option', 'قيد الدعوى').click();

// Button should be enabled
cy.get('button[type="submit"]').should('not.be.disabled');
```

---

## 📍 Key Selectors Used

```typescript
// Form fields
mat-select[formcontrolname="decisionType"]
mat-select[formcontrolname="caseTypeId"]
textarea[formcontrolname="notes"]

// Buttons
button[type="submit"]
.form-header button

// Material components
mat-option
mat-error
mat-hint
[role="dialog"]

// Text assertions
cy.contains('قيد الدعوى')
cy.contains('إنهاء الطلب')
cy.contains('تم قيد الدعوى بنجاح')
```

---

## 🐛 Common Issues & Fixes

### Issue: "Cannot find element 'nav-item'"
**Solution**: Ensure request is fully loaded
```typescript
cy.url().should('include', '/edit', { timeout: 10000 });
```

### Issue: "Dialog not appearing"
**Solution**: Add explicit wait
```typescript
cy.get('[role="dialog"]', { timeout: 3000 }).should('be.visible');
```

### Issue: "Dropdown not opening"
**Solution**: Use force click
```typescript
cy.get('mat-select').click({ force: true });
```

### Issue: "Arabic text not displayed"
**Solution**: Check RTL attribute
```typescript
cy.get('html').should('have.attr', 'dir', 'rtl');
```

---

## 📝 Test File Details

**File**: `cypress/e2e/request-completion.cy.ts`
**Size**: 35 KB
**Lines**: ~800
**Suites**: 9
**Tests**: 45+
**Patterns**: API interception, Material testing, Form validation, RTL support

---

## 🎬 Running Individual Tests

```bash
# Run only Suite 1 (Navigation)
npx cypress run --spec "cypress/e2e/request-completion.cy.ts" --grep "Suite 1:"

# Run only Suite 6 (Success scenarios)
npx cypress run --spec "cypress/e2e/request-completion.cy.ts" --grep "Suite 6:"

# Run specific test
npx cypress run --spec "cypress/e2e/request-completion.cy.ts" --grep "Should reset form"

# Run with Firefox
npx cypress run --browser firefox --spec "cypress/e2e/request-completion.cy.ts"
```

---

## 📊 Success Criteria

| Criteria | Status | Notes |
|----------|--------|-------|
| Tests created | ✅ | 45+ tests written |
| Navigation | ✅ | 3/3 navigation tests |
| Component rendering | ✅ | 5/5 display tests |
| API integration | ✅ | Case types mocked |
| Decision selection | ✅ | 4/4 options tested |
| Form validation | ✅ | All rules validated |
| Success flows | ✅ | 4/4 decision types |
| Error handling | ✅ | 3 error codes tested |
| Status control | ✅ | Access rules verified |
| Responsive design | ✅ | 3 viewport sizes |
| RTL support | ✅ | Arabic text verified |

---

## 🚨 Expected Results

### Normal Run (All Pass)
```
✓ Suite 1: Navigation (3 tests) - ~10 seconds
✓ Suite 2: Display & State (5 tests) - ~15 seconds
✓ Suite 3: Case Types API (5 tests) - ~15 seconds
✓ Suite 4: Decision Types (8 tests) - ~25 seconds
✓ Suite 5: Form Validation (7 tests) - ~20 seconds
✓ Suite 6: Success Scenarios (8 tests) - ~30 seconds
✓ Suite 7: Error Scenarios (8 tests) - ~30 seconds
✓ Suite 8: Status Control (5 tests) - ~20 seconds
✓ Suite 9: RTL/Responsive (8 tests) - ~30 seconds

Total: 45+ tests, ~195 seconds, 100% pass
```

---

## 🔧 Troubleshooting Commands

```bash
# Clear Cypress cache
npx cypress cache clear

# Run with verbose logging
npx cypress run --spec "cypress/e2e/request-completion.cy.ts" --reporter tap

# Run with specific log level
DEBUG=cypress:* npx cypress run

# Run without video
npx cypress run --spec "cypress/e2e/request-completion.cy.ts" --video false

# Run with specific timeout
npx cypress run --spec "cypress/e2e/request-completion.cy.ts" --config defaultCommandTimeout=15000
```

---

## 📚 Additional Resources

- **Component Code**: `src/app/features/case-registration/components/request-completion/`
- **Test File**: `cypress/e2e/request-completion.cy.ts`
- **Config**: `cypress.config.ts`
- **API Docs**: Available at `http://localhost:5001/swagger`

---

## 💡 Tips

1. **Run tests when backend is slow**: Increase timeouts in specific tests
2. **Debug failed test**: Use `cy.debug()` or Cypress UI step-through
3. **Check API responses**: Open Network tab in Cypress UI
4. **Verify forms**: Use `cy.get('form').then(f => console.log(f.val()))`
5. **Test in isolation**: Use `.only` to run single test: `it.only('Test name')`

---

## 🎉 You're Ready!

Your test suite is complete and ready to run. Start with:

```bash
cd src/Frontend/bog-app
npx cypress open
```

Then click on `request-completion.cy.ts` to watch the tests run! 🚀
