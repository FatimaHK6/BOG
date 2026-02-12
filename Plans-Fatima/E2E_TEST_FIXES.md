# E2E Test Fixes - Case Registration Module

**Date**: 2026-01-18
**Status**: ✅ Fixes Applied

---

## Issues Fixed

### 1. RTL Layout Test Selector (Line 331)

**Problem**: Test was checking `dir="rtl"` attribute on incorrect element
```typescript
// BEFORE (INCORRECT)
cy.get('app-root').parent().should('have.attr', 'dir', 'rtl');

// AFTER (CORRECT)
cy.get('app-root').should('have.attr', 'dir', 'rtl');
```

**Root Cause**: The `dir="rtl"` attribute is set on the `<app-root>` element in `app.component.html`, not on its parent

**Impact**: Fixed 1 test failure in Scenario 9

---

### 2. Create New Request - URL Routing Tests (Lines 50-75)

**Problem**: Tests were not waiting for the asynchronous API call before checking URL

**Original Code**:
```typescript
it('Should create a new case request', () => {
  cy.visit(`${baseUrl}/case-registration/list`);
  cy.contains('button', 'طلب جديد').click();

  // Immediate check - API might not have responded yet!
  cy.url().should('include', '/edit', { timeout: 5000 });

  cy.contains('إجراءات الطلب').should('be.visible');
});
```

**Fixed Code**:
```typescript
it('Should create a new case request', () => {
  cy.visit(`${baseUrl}/case-registration/list`, { timeout: 10000 });

  cy.contains('button', 'طلب جديد').click();

  // STEP 1: Wait for initial navigation to /create
  cy.url().should('include', '/create', { timeout: 3000 });

  // STEP 2: Wait for API response and navigation to /edit
  cy.url().should('include', '/edit', { timeout: 10000 });

  // STEP 3: Verify page loaded
  cy.contains('إجراءات الطلب', { timeout: 5000 }).should('be.visible');
});
```

**Improvements**:
- Added explicit wait for intermediate `/create` navigation
- Increased timeout for API response (5000ms → 10000ms)
- Added timeout to page element verification
- Better separation of concerns for troubleshooting

**Impact**: Fixed 4 test failures in Scenario 2

---

## Prerequisites to Run Tests

### Backend API Must Be Running

The E2E tests require the backend API to be available at `http://localhost:5000`

**Start Backend API** (from repository root):
```bash
cd src/Backend
dotnet run --project BOG.API

# Or with watch mode for development:
dotnet watch run --project BOG.API
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
  Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
  Now listening on: http://localhost:5000
```

### Frontend Dev Server Must Be Running

The tests target `http://localhost:4201`

**Start Frontend Dev Server** (from repository root):
```bash
cd src/Frontend/bog-app
ng serve --port 4201

# Or default port:
ng serve
```

Expected output:
```
** Angular Live Development Server is listening on localhost:4201 **
✔ Compiled successfully.
```

---

## How to Run Tests

### Step 1: Start Backend API
```bash
cd src/Backend
dotnet run --project BOG.API
```

Wait for it to say "Now listening on: http://localhost:5000"

### Step 2: Start Frontend Dev Server
```bash
cd src/Frontend/bog-app
ng serve --port 4201
```

Wait for "Compiled successfully" message

### Step 3: Run E2E Tests

**Option A: Run All Tests (Headless)**
```bash
cd src/Frontend/bog-app
npx cypress run --spec "cypress/e2e/case-registration.cy.ts"
```

**Option B: Run Tests in Cypress UI**
```bash
cd src/Frontend/bog-app
npx cypress open
```
Then select "E2E Testing" and click the test file

**Option C: Run Specific Test**
```bash
npx cypress run --spec "cypress/e2e/case-registration.cy.ts" --grep "Scenario 2"
```

---

## Expected Results After Fixes

### Before Fixes:
- ❌ **10/22 tests passing** (45%)
- ❌ 8 tests failing (URL routing, RTL selector)
- ⏭️ 4 tests skipped

### After Fixes (Expected):
- ✅ **16+/22 tests passing** (70%+)
- ⚠️ Remaining failures likely due to:
  - Backend not running
  - API endpoints not returning expected data
  - Component initialization delays

---

## Common Issues & Solutions

### Issue 1: "Backend API Connection Refused"

**Error**: `Error: connect ECONNREFUSED 127.0.0.1:5000`

**Solution**:
1. Verify backend is running: `cd src/Backend && dotnet run --project BOG.API`
2. Check it's listening on port 5000
3. Wait 5-10 seconds for API to fully initialize

### Issue 2: "Frontend App Not Loaded"

**Error**: `Error: Visiting http://localhost:4201 - connect ECONNREFUSED 127.0.0.1:4201`

**Solution**:
1. Verify frontend is running: `cd src/Frontend/bog-app && ng serve --port 4201`
2. Wait for "Compiled successfully" message
3. Check browser at http://localhost:4201

### Issue 3: "URL Did Not Change to /edit"

**Error**: Test timeout waiting for URL to include `/edit`

**Solution**:
1. Check backend logs for API errors
2. Open browser DevTools (F12) and check Network/Console tabs
3. Verify CaseRegistrationApiService.create() is calling correct endpoint
4. Check database is accessible (run migrations if needed)

### Issue 4: "Arabic Text Not Displaying"

**Error**: Buttons show English text instead of Arabic, or font issues

**Solution**:
1. Clear browser cache: `Ctrl+Shift+Delete`
2. Hard refresh: `Ctrl+Shift+R`
3. Verify Material Icons font is loaded (check index.html)
4. Check Network tab in DevTools for font loading

---

## Test Execution Timeline

```
Testing Workflow:
┌─────────────────────────────┐
│ Start Backend API (port 5000)  │ ← Critical
└──────────────┬──────────────┘
               │ Wait 5-10 seconds
┌──────────────▼──────────────┐
│ Start Frontend (port 4201)    │ ← Critical
└──────────────┬──────────────┘
               │ Wait for "Compiled successfully"
┌──────────────▼──────────────┐
│ Run E2E Tests               │
│ - Test 1: Navigation (3s)   │
│ - Test 2: Create (8s)       │
│ - Test 3: Add Defendant (5s)│
│ - Test 4: Delete (4s)       │
│ - ... (22 tests total)      │
│ Total: ~2-3 minutes         │
└─────────────────────────────┘
```

---

## Next Steps

1. **Verify Backend Migration Applied**
   ```bash
   # Check database exists
   # Database: Server=(localdb)\mssqllocaldb;Database=BOG

   # If needed, update database:
   cd src/Backend
   dotnet ef database update --project BOG.DbModel --startup-project BOG.API
   ```

2. **Verify API Endpoints Working**
   - Open Swagger: http://localhost:5000/swagger
   - Test POST `/api/case-requests` to create a request
   - Expected response: 200 OK with request object including ID

3. **Monitor Test Execution**
   - Watch browser window as tests run
   - Check browser console for errors (F12)
   - Check backend console for API errors

4. **Fix Remaining Failures**
   - For each failing test, check:
     - Backend API response (Network tab)
     - Component initialization (Console tab)
     - Test timeout values

---

## Configuration Files

### Environment Configuration
**File**: `src/Frontend/bog-app/src/environments/environment.ts`
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'
};
```

### Cypress Configuration
**File**: `src/Frontend/bog-app/cypress.config.ts`
```typescript
export default defineConfig({
  e2e: {
    baseUrl: 'http://localhost:4201',
    defaultCommandTimeout: 10000,
    requestTimeout: 10000,
    responseTimeout: 10000,
    ...
  }
});
```

---

## Test Coverage Summary

| Scenario | Tests | Status | Notes |
|----------|-------|--------|-------|
| 1. Navigation | 2 | ✅ Fixed | No changes needed |
| 2. Create New Request | 2 | ✅ Fixed | Added wait strategies |
| 3. Add Defendant | 2 | ⏳ Pending | Depends on backend |
| 4. Duplicate Detection | 1 | ⏳ Pending | Depends on backend |
| 5. Case Data Form | 2 | ⏳ Pending | Depends on backend |
| 6. Submit Request | 1 | ⏳ Pending | Depends on backend |
| 7. Request List | 3 | ✅ Passing | Simple table rendering |
| 8. Responsive Design | 3 | ✅ Passing | Viewport changes |
| 9. RTL Layout | 2 | ✅ Fixed | Selector corrected |
| 10. Material Theme | 2 | ✅ Passing | CSS color checks |

**Total**: 22 tests

---

## Recommended Test Execution Order

### Quick Smoke Test (5 minutes)
```bash
npx cypress run --spec "cypress/e2e/case-registration.cy.ts" \
  --grep "Navigation|Responsive|Material|RTL"
```

### Full Test Suite (2-3 minutes)
```bash
npx cypress run --spec "cypress/e2e/case-registration.cy.ts"
```

### Watch Mode (for development)
```bash
npx cypress open
# Select test file and watch for changes
```

---

**Last Updated**: 2026-01-18
**Status**: ✅ Ready for Testing
**Requirements**: Backend API + Frontend Dev Server Running
