# CaseDataStateService - Quick Reference Guide

## Import & Inject
```typescript
import { CaseDataStateService } from './services/case-data-state.service';

constructor(private caseDataState: CaseDataStateService) {}
```

## Update State

### Text Fields
```typescript
// Subject
this.caseDataState.updateSubject('Case subject text');

// Evidence
this.caseDataState.updateEvidence('Case evidence details');
```

### Arrays
```typescript
// Claims
const newClaim: ClaimVM = { id: 1, claimText: '...' };
const currentClaims = this.caseDataState.getClaims();
this.caseDataState.updateClaims([...currentClaims, newClaim]);

// Related Cases
const newCase: RelatedCaseVM = { caseNumber: 123, caseYear: 1445 };
const currentCases = this.caseDataState.getRelatedCases();
this.caseDataState.updateRelatedCases([...currentCases, newCase]);

// Classifications
this.caseDataState.updateClassifications([1, 2, 3]);
```

### Contact Information
```typescript
// All together
this.caseDataState.updateContactInfo('0501234567', '0505678901', 'user@example.com');

// Individual fields
this.caseDataState.updatePrimaryMobile('0501234567');
this.caseDataState.updateSecondaryMobile('0505678901');
this.caseDataState.updateEmail('user@example.com');
```

## Read State

### Sync (Immediate)
```typescript
const subject = this.caseDataState.getSubject();
const evidence = this.caseDataState.getEvidence();
const claims = this.caseDataState.getClaims();
const cases = this.caseDataState.getRelatedCases();
const classifications = this.caseDataState.getClassificationIds();
const contact = this.caseDataState.getContactInfo();
const allData = this.caseDataState.getAllData();
```

### Async (Observable)
```typescript
// Full state
this.caseDataState.state$.subscribe(state => {
  console.log('State changed:', state);
});

// Specific property
this.caseDataState.state$.pipe(
  map(state => state.subject),
  distinctUntilChanged()
).subscribe(subject => {
  console.log('Subject changed:', subject);
});
```

### In Template
```html
<!-- With async pipe -->
<div>{{ (caseDataState.state$ | async)?.subject }}</div>

<!-- Or component property -->
<!-- this.subject$ = this.caseDataState.state$.pipe(map(s => s.subject)) -->
<div>{{ subject$ | async }}</div>
```

## Loading Data

### From Backend Request
```typescript
// When opening existing case
this.caseApi.getRequest(id).subscribe(request => {
  this.caseDataState.loadFromRequest(request);
});
```

### Manual State Load
```typescript
const mockData = {
  subject: 'Test',
  evidence: 'Evidence',
  claims: [],
  relatedCases: [],
  classificationIds: [1, 2],
  primaryMobile: '0501234567',
  secondaryMobile: '',
  email: 'test@example.com'
};

this.caseDataState.loadFromRequest(mockData);
```

## Saving Data

### Get All Data for Save
```typescript
const allData = this.caseDataState.getAllData();

// Map to backend DTO
const dto = {
  subject: allData.subject,
  evidence: allData.evidence,
  claims: allData.claims.map(c => ({ claimText: c.claimText })),
  relatedCases: allData.relatedCases,
  classificationIds: allData.classificationIds,
  primaryMobile: allData.primaryMobile,
  secondaryMobile: allData.secondaryMobile,
  email: allData.email
};

// Send to backend
this.caseApi.updateRequest(requestId, dto).subscribe(...);
```

## Reset & Clear

### Reset Everything
```typescript
// Clears in-memory state + localStorage
this.caseDataState.resetState();
```

### Clear Storage Only
```typescript
// Clears localStorage but keeps in-memory state
this.caseDataState.clearFromLocalStorage();
```

## Common Patterns

### Form Integration
```typescript
export class SubjectFormComponent {
  subjectControl = new FormControl('');

  constructor(private caseDataState: CaseDataStateService) {}

  ngOnInit() {
    // Load from state
    this.subjectControl.setValue(
      this.caseDataState.getSubject(),
      { emitEvent: false }
    );

    // Save on change with debounce
    this.subjectControl.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(value => {
      this.caseDataState.updateSubject(value);
    });
  }
}
```

### List Management
```typescript
export class ClaimsComponent {
  claims$ = this.caseDataState.state$.pipe(
    map(s => s.claims)
  );

  onAdd(claim: ClaimVM) {
    const current = this.caseDataState.getClaims();
    this.caseDataState.updateClaims([...current, claim]);
  }

  onDelete(id: number) {
    const claims = this.caseDataState.getClaims().filter(c => c.id !== id);
    this.caseDataState.updateClaims(claims);
  }

  onUpdate(id: number, updated: ClaimVM) {
    const claims = this.caseDataState.getClaims().map(c =>
      c.id === id ? updated : c
    );
    this.caseDataState.updateClaims(claims);
  }
}
```

### Auto-Save
```typescript
export class CaseContainerComponent implements OnInit {
  constructor(
    private caseDataState: CaseDataStateService,
    private caseApi: CaseRegistrationApiService
  ) {}

  ngOnInit() {
    this.caseDataState.state$.pipe(
      debounceTime(2000),
      distinctUntilChanged((prev, curr) =>
        JSON.stringify(prev) === JSON.stringify(curr)
      ),
      switchMap(state => this.saveState(state)),
      catchError(err => {
        console.error('Auto-save failed:', err);
        return of(null);
      })
    ).subscribe();
  }

  private saveState(state: CaseDataState) {
    const dto = this.mapStateToDto(state);
    return this.caseApi.updateRequest(this.requestId, dto);
  }
}
```

### Query Specific State
```typescript
// Get only classifications
const ids = this.caseDataState.getClassificationIds();

// Get contact info
const { email, primaryMobile } = this.caseDataState.getContactInfo();

// Get claims count
const claimCount = this.caseDataState.getClaims().length;
```

## In Form Validation

```typescript
export class CaseFormValidator {
  static validateCaseData(state: CaseDataState): ValidationError[] {
    const errors: ValidationError[] = [];

    // Subject required
    if (!state.subject?.trim()) {
      errors.push({ field: 'subject', message: 'موضوع الدعوى مطلوب' });
    }

    // At least one claim
    if (state.claims.length === 0) {
      errors.push({ field: 'claims', message: 'يجب إضافة طلب دعوى واحد على الأقل' });
    }

    // Email format if provided
    if (state.email && !this.isValidEmail(state.email)) {
      errors.push({ field: 'email', message: 'البريد الإلكتروني غير صحيح' });
    }

    // Mobile format if provided
    if (state.primaryMobile && !this.isValidMobile(state.primaryMobile)) {
      errors.push({ field: 'primaryMobile', message: 'رقم الجوال غير صحيح' });
    }

    return errors;
  }

  private static isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  private static isValidMobile(mobile: string): boolean {
    return /^05\d{8}$/.test(mobile);
  }
}

// Usage in component
const errors = CaseFormValidator.validateCaseData(
  this.caseDataState.getAllData()
);

if (errors.length > 0) {
  this.showErrors(errors);
}
```

## Debugging

### Log All Changes
```typescript
this.caseDataState.state$.subscribe(state => {
  console.log('State updated:', {
    subject: state.subject.substring(0, 50),
    claimsCount: state.claims.length,
    casesCount: state.relatedCases.length,
    classificationsCount: state.classificationIds.length,
    contact: state.email ? 'set' : 'empty'
  });
});
```

### Check LocalStorage
```typescript
// In browser console
const stored = JSON.parse(localStorage.getItem('case-data-state'));
console.log('Stored state:', stored);

// Clear if corrupted
localStorage.removeItem('case-data-state');
```

### Monitor Observable
```typescript
// With tap for debugging
this.caseDataState.state$.pipe(
  tap(state => console.log('State:', state)),
  map(state => state.subject),
  tap(subject => console.log('Subject:', subject))
).subscribe(subject => {
  // Use subject
});
```

## Performance Tips

1. **Use `distinctUntilChanged()` for specific properties**
   ```typescript
   // ✅ Good - only updates when subject changes
   this.caseDataState.state$.pipe(
     map(s => s.subject),
     distinctUntilChanged()
   )

   // ❌ Bad - updates on any state change
   this.caseDataState.state$
   ```

2. **Use `async` pipe for template subscriptions**
   ```typescript
   // ✅ Good - auto unsubscribe
   <div>{{ (caseDataState.state$ | async)?.subject }}</div>

   // ❌ Bad - manual management
   this.subject$ = this.caseDataState.state$.map(s => s.subject);
   ```

3. **Debounce rapid updates**
   ```typescript
   // ✅ Good - waits for user to finish typing
   input.valueChanges.pipe(debounceTime(500))

   // ❌ Bad - updates on every keystroke
   input.valueChanges
   ```

4. **Use OnPush change detection**
   ```typescript
   @Component({
     changeDetection: ChangeDetectionStrategy.OnPush
   })
   ```

## Troubleshooting

### State Not Updating?
```typescript
// Make sure you're updating, not mutating
// ❌ Wrong - mutates array
const claims = this.caseDataState.getClaims();
claims.push(newClaim);
// ❌ No update event

// ✅ Right - creates new array
const claims = this.caseDataState.getClaims();
this.caseDataState.updateClaims([...claims, newClaim]);
```

### Components Not Receiving Updates?
```typescript
// Make sure to subscribe to state$
// ❌ Wrong - no subscription
const state = this.caseDataState.getAllData();

// ✅ Right - subscribes to changes
this.caseDataState.state$.subscribe(state => {
  // Use state
});
```

### LocalStorage Not Persisting?
```typescript
// Check browser localStorage
// 1. Browser console: localStorage.getItem('case-data-state')
// 2. DevTools Storage tab
// 3. Check for full quota errors
// 4. Check if localStorage disabled (incognito mode)

// Test without localStorage
this.caseDataState.clearFromLocalStorage();
this.caseDataState.updateSubject('Test');
// Should still work, just won't persist
```

## API Reference

| Method | Parameters | Returns | Use Case |
|--------|-----------|---------|----------|
| `updateSubject(value)` | string | void | Set case subject |
| `updateEvidence(value)` | string | void | Set case evidence |
| `updateClaims(claims)` | ClaimVM[] | void | Update all claims |
| `updateRelatedCases(cases)` | RelatedCaseVM[] | void | Update all related cases |
| `updateClassifications(ids)` | number[] | void | Set classification IDs |
| `updateContactInfo(p, s, e)` | strings | void | Set all contact fields |
| `updatePrimaryMobile(value)` | string | void | Set primary mobile |
| `updateSecondaryMobile(value)` | string | void | Set secondary mobile |
| `updateEmail(value)` | string | void | Set email |
| `getSubject()` | none | string | Get subject |
| `getEvidence()` | none | string | Get evidence |
| `getClaims()` | none | ClaimVM[] | Get claims |
| `getRelatedCases()` | none | RelatedCaseVM[] | Get related cases |
| `getClassificationIds()` | none | number[] | Get classification IDs |
| `getContactInfo()` | none | ContactInfo | Get contact data |
| `getAllData()` | none | CaseDataState | Get all state (for saving) |
| `loadFromRequest(req)` | any | void | Load existing request data |
| `resetState()` | none | void | Reset to initial state |
| `clearFromLocalStorage()` | none | void | Clear localStorage only |

## Next Steps

After using this service, proceed to:
1. **Phase 4**: Build UI components that inject and use this service
2. Add auto-save with debounce
3. Implement form validation
4. Add error handling
5. Test complete workflow

---

**File Locations:**
- Service: `src/Frontend/bog-app/src/app/features/case-registration/services/case-data-state.service.ts`
- Tests: `src/Frontend/bog-app/src/app/features/case-registration/services/case-data-state.service.spec.ts`
- Documentation: See PHASE3_IMPLEMENTATION_SUMMARY.md and PHASE3_SERVICE_ARCHITECTURE.md
