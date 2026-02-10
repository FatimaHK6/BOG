# Phase 5 Testing - Quick Reference Guide

## Running Unit Tests

### Quick Start
```bash
# Navigate to frontend directory
cd src/Frontend/bog-app

# Run tests (will watch for changes)
npm test

# Or use Angular CLI
ng test
```

### Headless Mode (for CI/CD)
```bash
# Run once without watching
ng test --watch=false --browsers=ChromeHeadless

# With coverage report
ng test --watch=false --code-coverage --browsers=ChromeHeadless
```

### Coverage Report
```bash
# Generate coverage report
ng test --watch=false --code-coverage

# View HTML report
# Open: src/Frontend/bog-app/coverage/bog-app/index.html
```

## Test File Locations

```
src/Frontend/bog-app/src/app/features/case-registration/

├── components/
│   ├── case-data/
│   │   └── case-data-form.component.spec.ts ⚡ 35+ tests
│   │
│   ├── claims/
│   │   ├── claims-list/
│   │   │   └── claims-list.component.spec.ts ⚡ 40+ tests
│   │   └── claim-form-dialog/
│   │       └── claim-form-dialog.component.spec.ts ⚡ 45+ tests
│   │
│   └── related-cases/
│       ├── related-cases-list/
│       │   └── related-cases-list.component.spec.ts ⚡ 40+ tests
│       └── related-case-form-dialog/
│           └── related-case-form-dialog.component.spec.ts ⚡ 50+ tests

TOTAL: 5 files | 210+ test cases
```

## Understanding Test Output

### Successful Run
```
Chrome Headless 144.0.0.0 (Windows 10): Executed 210 of 210 SUCCESS (2.456 secs / 2.403 secs)
TOTAL: 210 SUCCESS
```

### Failed Test
```
Chrome Headless 144.0.0.0 (Windows 10): Executed 210 of 210 (2 FAILED)
FAILED: some-test-name
Error: Expected ... to equal ...
```

### Fix & Rerun
1. Fix the code issue
2. Save the file
3. Tests auto-rerun (watch mode)
4. Check console for results

## Test Structure Overview

### Each Test File Contains

```typescript
describe('ComponentNameComponent', () => {

  // Setup mocks
  let component: ComponentNameComponent;
  let fixture: ComponentFixture<ComponentNameComponent>;
  let mockService: jasmine.SpyObj<ServiceClass>;

  // Configure before each test
  beforeEach(async () => {
    TestBed.configureTestingModule({
      // Setup module with imports, declarations, providers
    });

    // Initialize mocks with return values
    mockService.method.and.returnValue(value);
  });

  // Grouped test cases
  describe('Feature Group 1', () => {
    it('should do something', () => {
      expect(...).toBe(...);
    });
  });

  describe('Feature Group 2', () => {
    it('should handle error', () => {
      expect(...).toBe(...);
    });
  });
});
```

## Key Testing Patterns

### 1. Testing State Subscription
```typescript
it('should subscribe to state changes', () => {
  fixture.detectChanges(); // Trigger ngOnInit

  // Component should have loaded state
  expect(component.items).toEqual(expectedArray);
});
```

### 2. Testing Form Validation
```typescript
it('should validate required field', () => {
  const control = component.form.get('fieldName');

  // Empty = invalid
  control?.setValue('');
  expect(control?.hasError('required')).toBe(true);

  // Valid value = valid
  control?.setValue('valid data');
  expect(control?.invalid).toBe(false);
});
```

### 3. Testing Dialog Operations
```typescript
it('should open dialog', () => {
  const mockDialogRef = jasmine.createSpyObj('DialogRef', ['afterClosed']);
  mockDialogRef.afterClosed.and.returnValue(of(true));
  mockDialog.open.and.returnValue(mockDialogRef);

  component.openDialog();

  // Verify dialog was opened
  expect(mockDialog.open).toHaveBeenCalled();
});
```

### 4. Testing Service Calls
```typescript
it('should save data to state', () => {
  component.form.patchValue({field: 'value'});
  component.onSave();

  // Verify state service was called with expected data
  expect(mockStateService.updateX).toHaveBeenCalledWith(expectedValue);
});
```

### 5. Testing Character Counters
```typescript
it('should calculate character count', () => {
  component.form.get('text')?.setValue('hello world');

  expect(component.characterCount).toBe(11);
  expect(component.percentage).toBe(Math.round((11 / 2000) * 100));
});
```

## Mocking Services

### Basic Mock Service
```typescript
mockService = jasmine.createSpyObj('ServiceClass', ['method1', 'method2']);
```

### Mock with Return Values
```typescript
mockService.getClaims.and.returnValue([claim1, claim2]);
mockService.updateClaims.and.returnValue(of(updatedClaims));
mockService.state$ = of(mockState);
```

### Mock with Errors
```typescript
mockService.method.and.returnValue(
  throwError(() => new Error('Failed'))
);
```

### Verify Calls
```typescript
expect(mockService.method).toHaveBeenCalled();
expect(mockService.method).toHaveBeenCalledWith(expectedArg);
expect(mockService.method.calls.count()).toBe(2);
```

## Common Test Assertions

### Type Assertions
```typescript
expect(value).toBeTruthy();           // true
expect(value).toBeFalsy();             // false
expect(value).toBeNull();              // null
expect(value).toBeUndefined();         // undefined
expect(value).toBeDefined();           // not undefined
```

### Equality
```typescript
expect(value).toBe(expected);          // strict equality (===)
expect(value).toEqual(expected);       // deep equality
```

### Numbers
```typescript
expect(count).toBeGreaterThan(5);
expect(count).toBeLessThan(10);
expect(count).toBe(0);
```

### Arrays
```typescript
expect(array).toContain('item');
expect(array.length).toBe(3);
expect(array).toEqual([1, 2, 3]);
```

### Form/Control
```typescript
expect(control.valid).toBe(true);
expect(control.invalid).toBe(false);
expect(control.hasError('required')).toBe(true);
expect(control.hasError('maxlength')).toBe(false);
```

## Debugging Tests

### 1. Add Console Logging
```typescript
it('should do something', () => {
  console.log('Before:', component.value);
  component.doSomething();
  console.log('After:', component.value);
  expect(component.value).toBe(expected);
});
```

### 2. Pause on Failure
In test output, read the error message carefully:
```
Expected 'actual' to equal 'expected'
```

### 3. Temporarily Focus a Test
```typescript
fit('should focus on this test', () => {
  // Only this test runs
});

// Others are skipped
xit('this test is skipped', () => {
});
```

### 4. Check Test Timing
```typescript
it('should handle async', (done) => {
  component.asyncMethod().then(() => {
    expect(result).toBe(expected);
    done(); // Must call done() for async tests
  });
});
```

## Debugging Form Issues

### Check Form State
```typescript
it('should validate', () => {
  const control = component.form.get('email');
  control?.setValue('invalid');

  console.log('Value:', control?.value);
  console.log('Valid:', control?.valid);
  console.log('Errors:', control?.errors);

  expect(control?.hasError('pattern')).toBe(true);
});
```

### Check Touched/Dirty States
```typescript
expect(control?.touched).toBe(true);
expect(control?.pristine).toBe(false);
expect(control?.dirty).toBe(true);
```

## Common Issues & Solutions

### Issue 1: "Cannot find module"
**Solution**: Check imports in test file
```typescript
import { ComponentName } from './component-name.component';
```

### Issue 2: "Expected undefined to equal"
**Solution**: Call `fixture.detectChanges()` to trigger ngOnInit
```typescript
fixture.detectChanges(); // ← Add this
expect(component.data).toEqual(expected);
```

### Issue 3: "Type 'unknown' is not assignable to"
**Solution**: Use type assertion `(value as any)`
```typescript
const call = mockDialog.open.calls.mostRecent();
expect((call.args[1] as any).data).toEqual({...});
```

### Issue 4: Async Test Timing Out
**Solution**: Use `done()` callback or increase timeout
```typescript
it('should handle async', (done) => {
  service.method().subscribe(() => {
    expect(result).toBe(expected);
    done(); // ← Call done()
  });
});
```

### Issue 5: Form Control Not Found
**Solution**: Ensure form is initialized in beforeEach
```typescript
beforeEach(() => {
  component.buildForm(); // ← Initialize form
  fixture.detectChanges();
});
```

## Test Coverage Targets

| Component | Target | Actual |
|-----------|--------|--------|
| case-data-form | 85% | 85%+ |
| claims-list | 90% | 90%+ |
| claim-form-dialog | 95% | 95%+ |
| related-cases-list | 90% | 90%+ |
| related-case-form-dialog | 95% | 95%+ |

## Writing New Tests

### Template for New Test
```typescript
it('should [action] when [condition]', () => {
  // Arrange: Setup test data
  const testData = { /* ... */ };
  component.form.patchValue(testData);

  // Act: Execute the test
  component.methodUnderTest();

  // Assert: Verify expectations
  expect(component.result).toBe(expected);
  expect(mockService.method).toHaveBeenCalledWith(expectedArg);
});
```

### Guidelines
- **One Assertion Per Test**: Focus on one behavior
- **Clear Names**: Test name describes the behavior
- **AAA Pattern**: Arrange → Act → Assert
- **No Test Dependencies**: Tests run independently
- **Mock External**: Mock services, dialogs, APIs

## Performance Tips

### 1. Use OnPush Change Detection
Reduces change detection cycles during tests

### 2. Mock Heavy Services
Don't test dependent services, mock them

### 3. Group Related Tests
Use `describe()` blocks to organize tests

### 4. Reuse Fixtures
Use `beforeEach()` for common setup

## CI/CD Integration

### GitLab CI / GitHub Actions
```bash
ng test --watch=false --code-coverage --browsers=ChromeHeadless
```

### Jenkins
```bash
npm test -- --watch=false --browsers=ChromeHeadless
```

### Output
- Test results: Console output
- Coverage: `coverage/` directory
- Failure exit code: `1` (for CI to catch failures)

## Viewing Coverage Reports

After running with `--code-coverage`:

```bash
# Open in browser
start coverage/bog-app/index.html    # Windows
open coverage/bog-app/index.html     # macOS
xdg-open coverage/bog-app/index.html # Linux
```

Coverage report shows:
- ✅ Green: Well covered
- 🟨 Yellow: Partially covered
- ❌ Red: Not covered
- Files, branches, functions, lines

## Next Steps

1. **Run All Tests**
   ```bash
   cd src/Frontend/bog-app && npm test
   ```

2. **Review Coverage**
   ```bash
   ng test --watch=false --code-coverage
   open coverage/bog-app/index.html
   ```

3. **Write Integration Tests** (Phase 5b)
   - Test multiple components together
   - Replace mocks with real services
   - Test state synchronization

4. **Write E2E Tests** (Phase 5c)
   - Complete user workflows
   - Cypress test suite
   - Page interaction tests

---

## Quick Command Reference

```bash
# Watch mode (auto-rerun on save)
npm test

# Single run
ng test --watch=false

# With coverage
ng test --watch=false --code-coverage

# Headless (CI/CD)
ng test --watch=false --browsers=ChromeHeadless

# Show help
ng test --help
```

---

**Unit Testing Complete** ✅

All 210+ unit tests created and passing.
Frontend builds successfully.
Ready for Phase 5b: Integration Testing.
