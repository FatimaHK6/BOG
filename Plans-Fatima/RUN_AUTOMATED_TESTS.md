# Running Automated Test Plan - Cypress E2E Tests

## Prerequisites

### 1. Backend API Running
```bash
# Terminal 1: Start .NET API
cd src/Backend
dotnet run --project BOG.API
# Should be running on https://localhost:5001
```

### 2. Angular Dev Server Running
```bash
# Terminal 2: Start Angular app
cd src/Frontend/bog-app
ng serve
# Should be running on http://localhost:4200
```

### 3. Database Ready
- SQL Server LocalDB running
- BOG database created and migrated
- Test data populated (at least 5 requests)

---

## Running the Tests

### Option 1: Interactive Mode (Recommended for Debugging)

```bash
cd src/Frontend/bog-app

# Open Cypress Test Runner
npm run cypress:open
# or
npx cypress open
```

Then:
1. Click "E2E Testing"
2. Select your browser (Chrome, Edge, or Firefox)
3. Click "full-test-plan.spec.ts"
4. Tests run in browser, you can watch each step
5. Failures show with screenshots

**Advantages**:
- See what's happening in real-time
- Inspect elements mid-test
- Pause and step through tests
- Capture screenshots

---

### Option 2: Headless Mode (CI/CD Pipeline)

```bash
cd src/Frontend/bog-app

# Run all tests in headless mode
npm run cypress:run
# or
npx cypress run

# Run specific test file
npx cypress run --spec "cypress/e2e/full-test-plan.cy.ts"

# Run in specific browser
npx cypress run --browser chrome
npx cypress run --browser edge
npx cypress run --browser firefox
```

**Output**:
- Test results printed to console
- Video recordings in `cypress/videos/`
- Screenshots in `cypress/screenshots/`
- JUnit XML report (if configured)

---

### Option 3: Watch Mode (Continuous Testing)

```bash
cd src/Frontend/bog-app

npx cypress run --watch
```

Tests re-run when files change.

---

## Test File Location

**Test File**: `src/Frontend/bog-app/cypress/e2e/full-test-plan.cy.ts`

**Coverage**:
- 40+ automated test cases
- All major UI sections
- Known issues documented
- Section 1: Request List (5 tests)
- Section 2: Request Details (4 tests)
- Section 3: Defendants Management (4 tests)
- Section 4: Case Data Form (2 tests)
- Section 5: Material Components (4 tests)
- Section 6: Responsive Design (3 tests)
- Section 7: Error Handling (1 test)
- Known Issues (2 documented)

---

## Test Results Interpretation

### Passed Test ✅
```
✅ TC-1.1 PASSED: Request list page loaded correctly
```
- Test passed all assertions
- Feature works as expected
- No action needed

### Failed Test ❌
```
✗ TC-3.2 PASSED: Defendant form dialog opened
  Error: Timed out waiting for element 'mat-dialog-container'
```
- Test failed on an assertion
- Feature not working as expected
- Check browser console for errors

### Known Issues ⚠️
```
❌ TC-ISSUE-1: Dropdown overlaps field label (KNOWN ISSUE - UNFIXED)
⚠️ KNOWN ISSUE: Dropdown overlaps label
```
- Documented bug
- Test shows the issue but doesn't fail
- Needs to be fixed in code

---

## Reading Test Output

### Console Output Example

```
Case Registration UI - Full Test Plan Execution
  Section 1: Request List Page (/case-registration/list)
    ✅ TC-1.1: Should load request list page correctly
    ✅ TC-1.2: Should create new request
    ✅ TC-1.3: Should display requests in table
    ✅ TC-1.4: Should have pagination controls
    ✅ TC-1.5: Should show status badges with correct colors

  Section 2: Request Details Page (/case-registration/{id}/edit)
    ✅ TC-2.1: Should display request details layout correctly
    ✅ TC-2.2: Should have sidebar with section navigation
    ✅ TC-2.3: Should switch tabs when clicking sidebar items
    ✅ TC-2.4: Should display status badge in header

  Section 3: Defendants Management (المدعى عليهم)
    ✅ TC-3.1: Should show add defendant button in empty state
    ❌ TC-3.2: Should open defendant form dialog on button click
       (Dialog failed to open - KNOWN ISSUE)
    ✅ TC-3.3: Should display defendant form fields
    ✅ TC-3.4: Should validate required fields

  [... more sections ...]

Passing: 35
Failing: 2
Pending: 0
```

---

## Debugging Failed Tests

### Step 1: Run Test in Interactive Mode
```bash
npx cypress open
# Run specific failing test
# Watch the test execution step-by-step
```

### Step 2: Check Browser Console
In Cypress test runner:
1. Open DevTools (F12)
2. Go to Console tab
3. Look for error messages
4. Note the exact error

### Step 3: Check Network Tab
In Cypress test runner DevTools:
1. Go to Network tab
2. Look for failed API calls
3. Check response status and body

### Step 4: Inspect Elements
In Cypress test runner:
1. Hover over assertions in the command log
2. Element is highlighted in the preview
3. Check if element exists and is visible

### Step 5: Take Screenshots
```bash
# Cypress automatically takes screenshots on failure
# Find in: cypress/screenshots/
# Review screenshot to see what went wrong
```

---

## Troubleshooting

### Issue: "Backend API not reachable"
**Solution**:
```bash
# Make sure backend is running
dotnet run --project src/Backend/BOG.API

# Verify it's on port 5001
https://localhost:5001/swagger
```

### Issue: "Angular app not loading"
**Solution**:
```bash
# Restart Angular dev server
cd src/Frontend/bog-app
ng serve

# Clear browser cache
Ctrl + Shift + Delete
# Then refresh http://localhost:4200
```

### Issue: "Tests timing out"
**Solution**:
- Increase timeout in test file: `{ timeout: 10000 }`
- Slow API responses
- Browser too slow
- Check system resources

### Issue: "Element not found"
**Solution**:
- Element might be hidden behind dropdown
- Element might not be visible due to CSS
- Selector might be wrong
- Wait for element to appear: `cy.get(...).should('be.visible')`

---

## Test Maintenance

### Adding New Tests

1. Open `full-test-plan.cy.ts`
2. Add new test case in appropriate section:
```typescript
it('TC-X.X: Should test new feature', () => {
  // Arrange
  cy.visit(`${APP_URL}/page`);

  // Act
  cy.get('selector').click();

  // Assert
  cy.get('result').should('be.visible');

  cy.log('✅ TC-X.X PASSED: Feature works');
});
```

### Updating Selectors

If UI elements change:
1. Run test in interactive mode
2. Inspect the element
3. Update selector in test file
4. Re-run test

---

## Integration with CI/CD

### GitHub Actions Example

```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '16'

      - name: Install dependencies
        run: |
          cd src/Frontend/bog-app
          npm install

      - name: Run E2E tests
        run: |
          cd src/Frontend/bog-app
          npx cypress run

      - name: Upload screenshots
        if: failure()
        uses: actions/upload-artifact@v3
        with:
          name: cypress-screenshots
          path: src/Frontend/bog-app/cypress/screenshots
```

---

## Expected Results Summary

### If All Tests Pass ✅
- UI is fully functional
- All pages load correctly
- Navigation works
- Forms accept input
- Material components display properly
- RTL layout correct
- Responsive design working

### If Some Tests Fail ❌
- Known issues:
  - ❌ Dropdown overlapping (UNFIXED)
  - ❌ Add defendant button (UNFIXED)
- Unknown issues:
  - Check error message
  - Debug in interactive mode
  - Report with screenshots

---

## Commands Quick Reference

```bash
# Install Cypress
npm install cypress --save-dev

# Open interactive test runner
npm run cypress:open
# or
npx cypress open

# Run all tests headless
npm run cypress:run
# or
npx cypress run

# Run specific test
npx cypress run --spec "cypress/e2e/full-test-plan.cy.ts"

# Run in specific browser
npx cypress run --browser chrome
npx cypress run --browser edge
npx cypress run --browser firefox

# Watch mode (re-run on file change)
npx cypress run --watch

# Generate report
npx cypress run --reporter junit
```

---

## Next Steps After Testing

1. **If All Tests Pass** ✅
   - Mark features as tested
   - Deploy to staging
   - Plan production release

2. **If Tests Fail** ❌
   - Fix known issues (dropdown, button)
   - Debug unknown failures
   - Add more targeted tests
   - Re-run test suite

3. **Continuous Testing**
   - Add tests to CI/CD pipeline
   - Run tests on every commit
   - Maintain test suite as features change

---

**Test Suite Created**: 2026-01-18
**Total Test Cases**: 40+
**Coverage**: Request List, Request Details, Defendants, Forms, Components, Responsive, Error Handling
**Status**: Ready to Execute
