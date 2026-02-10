# CaseDataStateService - Architecture & Integration

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                     CASE REGISTRATION REQUEST                        │
│                  (Displayed in Request Details Page)                 │
└─────────────────────────────────────────────────────────────────────┘
                                 ↓
┌─────────────────────────────────────────────────────────────────────┐
│              CaseDataContainerComponent (Parent)                      │
│            (Vertical Tabs Layout - Phase 4)                          │
└─────────────────────────────────────────────────────────────────────┘
                                 ↓
        ┌────────────────────┬────────────────────┬────────────────────┐
        ↓                    ↓                    ↓                    ↓
    ┌────────┐          ┌────────┐          ┌────────┐          ┌────────┐
    │  Tab 1 │          │  Tab 2 │          │  Tab 3 │          │  Tab 4 │
    │Subject │          │ Claims │          │Related │          │Classify│
    │Evidence│          │  List  │          │ Cases  │          │ -ication│
    └────────┘          └────────┘          └────────┘          └────────┘
        ↓                    ↓                    ↓                    ↓
    ┌────────────────────────────────────────────────────────────────────┐
    │                    CaseDataStateService                             │
    │                  (Centralized State Store)                          │
    │                                                                     │
    │  ┌─────────────────────────────────────────────────────────────┐   │
    │  │ State:                                                      │   │
    │  │  • subject: string                                          │   │
    │  │  • evidence: string                                         │   │
    │  │  • claims: ClaimVM[]                                        │   │
    │  │  • relatedCases: RelatedCaseVM[]                            │   │
    │  │  • classificationIds: number[]                              │   │
    │  │  • primaryMobile: string                                    │   │
    │  │  • secondaryMobile: string                                  │   │
    │  │  • email: string                                            │   │
    │  └─────────────────────────────────────────────────────────────┘   │
    │                                                                     │
    │  ┌─────────────────────────────────────────────────────────────┐   │
    │  │ Observable: state$ (BehaviorSubject)                        │   │
    │  │  - All components subscribe for reactive updates            │   │
    │  │  - Emits on every state change                              │   │
    │  └─────────────────────────────────────────────────────────────┘   │
    │                                                                     │
    │  ┌─────────────────────────────────────────────────────────────┐   │
    │  │ Persistence: LocalStorage                                   │   │
    │  │  - Key: 'case-data-state'                                   │   │
    │  │  - Auto-save on every update                                │   │
    │  │  - Load on service initialization                           │   │
    │  └─────────────────────────────────────────────────────────────┘   │
    └────────────────────────────────────────────────────────────────────┘
                                 ↑
        ┌────────────────────┬────────────────────┬────────────────────┐
        │                    │                    │                    │
        ↓                    ↓                    ↓                    ↓
    ┌──────────┐        ┌──────────┐        ┌──────────┐        ┌──────────┐
    │Component │        │Component │        │Component │        │Component │
    │ 1        │        │ 2        │        │ 3        │        │ 4        │
    │Subscribe │        │Subscribe │        │Subscribe │        │Subscribe │
    │ & Update │        │ & Update │        │ & Update │        │ & Update │
    └──────────┘        └──────────┘        └──────────┘        └──────────┘
```

## Data Flow Diagram

### Update Flow
```
Component UI Event (e.g., input change)
    ↓
updateSubject() / updateClaims() / etc.
    ↓
updateState(newState)
    ↓
    ├─→ stateSubject.next(newState) ──→ All subscribers notified (state$)
    │
    └─→ saveToLocalStorage(newState) ──→ Browser storage updated
```

### Component Subscription Flow
```
Component OnInit
    ↓
this.caseDataState.state$.subscribe(state => { ... })
    ↓
Listen for state changes
    ↓
When service updates state → subscriber callback fires
    ↓
Component updates UI with new data
```

## State Management Lifecycle

### 1. Creating New Request
```
User clicks "New Case Request"
    ↓
CaseDataContainerComponent created
    ↓
CaseDataStateService initialized
    ↓
Initial empty state created
    ↓
resetState() called (optional)
    ↓
Ready for user input
```

### 2. Opening Existing Request
```
User opens existing case request
    ↓
Backend API returns CaseRegistrationRequest
    ↓
loadFromRequest(request) called
    ↓
State populated from request data
    ↓
All components receive state update via state$
    ↓
UI displays loaded data
```

### 3. User Editing
```
User enters subject text
    ↓
Subject input change event
    ↓
updateSubject(newValue) called
    ↓
State updated (immutably)
    ↓
Saved to localStorage
    ↓
All subscribers notified via state$
    ↓
Other components update their UI if needed
```

### 4. Page Refresh/Recovery
```
Browser page refresh or crash
    ↓
CaseDataStateService constructor
    ↓
loadFromLocalStorage() called
    ↓
Previous state restored from localStorage
    ↓
Users see their unsaved data restored
```

### 5. Saving to Backend
```
User clicks "Save Draft" button
    ↓
getAllData() returns complete CaseDataState
    ↓
Map to backend DTO format
    ↓
Send HTTP PUT/POST to backend API
    ↓
Backend saves to database
    ↓
User sees success message
```

## Component Integration Examples

### Example 1: SubjectEvidenceFormComponent
```typescript
export class SubjectEvidenceFormComponent implements OnInit {
  subject$ = this.caseDataState.state$.pipe(
    map(state => state.subject),
    distinctUntilChanged()
  );

  evidence$ = this.caseDataState.state$.pipe(
    map(state => state.evidence),
    distinctUntilChanged()
  );

  constructor(private caseDataState: CaseDataStateService) {}

  onSubjectChange(value: string) {
    this.caseDataState.updateSubject(value);
  }

  onEvidenceChange(value: string) {
    this.caseDataState.updateEvidence(value);
  }
}
```

Template:
```html
<mat-form-field>
  <mat-label>موضوع الدعوى</mat-label>
  <textarea
    matInput
    [ngModel]="subject$ | async"
    (change)="onSubjectChange($event.target.value)">
  </textarea>
</mat-form-field>

<mat-form-field>
  <mat-label>أسانيد الدعوى</mat-label>
  <textarea
    matInput
    [ngModel]="evidence$ | async"
    (change)="onEvidenceChange($event.target.value)">
  </textarea>
</mat-form-field>
```

### Example 2: ClaimsListComponent
```typescript
export class ClaimsListComponent implements OnInit {
  claims$ = this.caseDataState.state$.pipe(
    map(state => state.claims),
    distinctUntilChanged((prev, curr) =>
      JSON.stringify(prev) === JSON.stringify(curr)
    )
  );

  constructor(private caseDataState: CaseDataStateService) {}

  onAddClaim(newClaim: ClaimVM) {
    const current = this.caseDataState.getClaims();
    this.caseDataState.updateClaims([...current, newClaim]);
  }

  onDeleteClaim(claimId: number) {
    const current = this.caseDataState.getClaims();
    const filtered = current.filter(c => c.id !== claimId);
    this.caseDataState.updateClaims(filtered);
  }
}
```

### Example 3: ClassificationsTabComponent
```typescript
export class ClassificationsTabComponent {
  selectedClassifications$ = this.caseDataState.state$.pipe(
    map(state => state.classificationIds),
    distinctUntilChanged()
  );

  constructor(private caseDataState: CaseDataStateService) {}

  onSelectClassifications(ids: number[]) {
    this.caseDataState.updateClassifications(ids);
  }

  getSelectedClassifications(): number[] {
    return this.caseDataState.getClassificationIds();
  }
}
```

## Auto-Save Implementation (Phase 4)

```typescript
export class CaseDataContainerComponent {
  private saveSubject = new Subject<CaseDataState>();

  ngOnInit() {
    // Setup auto-save with 2-second debounce
    this.caseDataState.state$.pipe(
      debounceTime(2000),
      distinctUntilChanged((prev, curr) =>
        JSON.stringify(prev) === JSON.stringify(curr)
      ),
      switchMap(state => this.caseApi.saveRequest(this.requestId, state))
    ).subscribe({
      next: (response) => {
        this.showSaveSuccess();
      },
      error: (error) => {
        this.showSaveError(error);
      }
    });
  }
}
```

## Error Handling Strategy

### LocalStorage Errors
```typescript
// Handled internally by service
try {
  localStorage.setItem(key, value);
} catch (error) {
  console.warn('localStorage unavailable', error);
  // Continue functioning without persistence
}
```

### JSON Parse Errors
```typescript
try {
  const state = JSON.parse(saved);
  this.stateSubject.next(state);
} catch (error) {
  console.warn('Corrupted saved state', error);
  // Fall back to initial state
  this.stateSubject.next(this.initialState);
}
```

### Missing Data Fields
```typescript
// loadFromRequest handles missing fields
const newState: CaseDataState = {
  subject: request.subject || '',
  evidence: request.evidence || '',
  claims: request.claims || [],
  // ... etc with defaults
};
```

## Performance Considerations

### 1. Memory Usage
- Stores complete case data in memory (typically < 1MB)
- No memory leaks with proper unsubscribe patterns

### 2. Change Detection
- Use `OnPush` change detection in components
- Observable subscriptions trigger change detection only
- Avoid unnecessary re-renders with `distinctUntilChanged()`

### 3. RxJS Optimization
```typescript
// Good: Only react to subject changes
this.caseDataState.state$.pipe(
  map(state => state.subject),
  distinctUntilChanged()
).subscribe(...)

// Avoid: Reacting to entire state
this.caseDataState.state$.subscribe(...)
```

### 4. LocalStorage Performance
- Serialization/deserialization is fast for < 1MB data
- No blocking operations
- Could migrate to IndexedDB for larger data

## Security Considerations

### 1. LocalStorage
- User's case data stored locally (not transmitted)
- Only accessible by same-origin scripts
- Users can manually clear via browser tools

### 2. State Immutability
- Prevents accidental mutations
- Safer multi-component updates

### 3. Data Validation
- Backend validates all received data
- Frontend state is for UI only
- No sensitive data stored in localStorage

## Testing Strategy

### Unit Tests
- ✅ State updates work correctly
- ✅ LocalStorage persistence/recovery
- ✅ Observable emissions
- ✅ Error handling
- ✅ Data loading from requests

### Integration Tests (Phase 4)
- Component + service integration
- Auto-save functionality
- Multi-component state synchronization
- Browser refresh recovery

### E2E Tests (Phase 4)
- Complete user workflow
- Save and load case request
- Data persistence across sessions
- Error scenarios

## Comparison with Alternative Approaches

| Approach | Pros | Cons |
|----------|------|------|
| **Current (BehaviorSubject)** | Simple, observable-based, works well with Angular, built-in RxJS | Limited feature set vs NgRx |
| **NgRx Store** | Powerful, time-travel debugging, excellent for large apps | Boilerplate, overkill for this use case |
| **Akita** | Good balance, less boilerplate than NgRx | Additional dependency |
| **Component State Only** | Simplest | Props drilling, hard to sync across components |

## Why BehaviorSubject Pattern?

1. ✅ Lightweight and built-in to Angular/RxJS
2. ✅ Perfect for small-to-medium state (not enterprise app)
3. ✅ Easy to understand and maintain
4. ✅ Integrates seamlessly with reactive forms
5. ✅ Sufficient for this case registration use case
6. ✅ Can upgrade to NgRx later if needed

## Migration Path (Future)

If application grows to need NgRx:
```
CaseDataStateService (simple BehaviorSubject)
    ↓ (if needed in future)
NgRx Store (enterprise state management)
```

Components would use same interface, minimal changes needed.
