# Phase 2 Quick Reference Card

## Status: ✅ COMPLETE

---

## Build & Run Commands

### Start Backend
```bash
cd src/Backend
dotnet run --project BOG.API
# Runs on: https://localhost:5001
```

### Start Frontend
```bash
cd src/Frontend/bog-app
ng serve
# Runs on: http://localhost:4200
```

---

## API Endpoints

### Courts Lookup
```
GET /api/lookups/courts
Response: Array of {id, name, nameAr, regionId, cityId}
```

### Claims (to be implemented in Phase 3)
```
GET /api/case-requests/{requestId}/claims
PUT /api/case-requests/{requestId}/claims
```

### Related Cases (to be implemented in Phase 3)
```
GET /api/case-requests/{requestId}/related-cases
PUT /api/case-requests/{requestId}/related-cases
```

---

## Component Files

### Claims Section
```
claims-list/
├── .component.ts    (List logic)
├── .component.html  (Table/card display)
└── .component.scss  (Styling)

claim-form-dialog/
├── .component.ts    (Form logic)
├── .component.html  (Form template)
└── .component.scss  (Dialog styling)
```

### Related Cases Section
```
related-cases-list/
├── .component.ts    (List logic)
├── .component.html  (Table display)
└── .component.scss  (Styling)

related-case-form-dialog/
├── .component.ts    (Form logic)
├── .component.html  (Form template)
└── .component.scss  (Dialog styling)
```

---

## Key Services

### CaseDataStateService
```typescript
// Get data
getClaims(): ClaimVM[]
getRelatedCases(): RelatedCaseVM[]

// Update data
updateClaims(claims: ClaimVM[]): void
updateRelatedCases(cases: RelatedCaseVM[]): void

// Subscribe to changes
state$: Observable<CaseDataState>
```

### ClaimApiService
```typescript
getClaims(requestId: number): Observable<ClaimVM[]>
updateClaims(requestId: number, dto: ClaimsBatchUpdateDTO): Observable<ClaimVM[]>
```

### RelatedCaseApiService
```typescript
getRelatedCases(requestId: number): Observable<RelatedCaseVM[]>
updateRelatedCases(requestId: number, dto: RelatedCasesBatchUpdateDTO): Observable<RelatedCaseVM[]>
```

### LookupsApiService
```typescript
getCourts(): Observable<CourtLookup[]>
```

---

## Models

### Claim
```typescript
interface ClaimVM {
  id: number;
  caseRegistrationRequestId: number;
  claimText: string;
  createdDate: Date;
  modifiedDate: Date;
}
```

### RelatedCase
```typescript
interface RelatedCaseVM {
  id: number;
  caseRegistrationRequestId: number;
  courtId?: number;
  courtName?: string;
  caseNumber: number;
  caseYear: number;
  createdDate: Date;
  modifiedDate: Date;
}
```

---

## UI Features

### Claims
- ✅ Add claim (textarea, max 2000 chars)
- ✅ Edit claim
- ✅ Delete claim
- ✅ Character counter
- ✅ Empty state
- ✅ Card display

### Related Cases
- ✅ Add related case (court, case #, year)
- ✅ Edit related case
- ✅ Delete related case
- ✅ Court dropdown
- ✅ Empty state
- ✅ Table display

---

## Validation Rules

### Claims
- Text required
- Max 2000 characters

### Related Cases
- Case number required (integer)
- Case year required (4 digits, Hijri)
- Court optional

---

## Important Files to Modify (Phase 3)

### For API Integration
1. `claim-form-dialog.component.ts` - Replace state with API call
2. `claims-list.component.ts` - Load from API instead of state
3. `related-case-form-dialog.component.ts` - Replace state with API call
4. `related-cases-list.component.ts` - Load from API instead of state

### For Rich Text Editor
1. Upgrade ngx-editor when dependencies fixed
2. Update claim-form-dialog template to use ngx-editor component
3. Update claim-form-dialog styles for editor

---

## Troubleshooting

### Build Fails
```bash
# Clear node_modules and reinstall
cd src/Frontend/bog-app
rm -r node_modules package-lock.json
npm install
npm run build
```

### API Not Responding
```bash
# Check backend is running
# Verify https://localhost:5001/swagger is accessible
# Check connection string in appsettings.json
```

### Courts Dropdown Empty
```bash
# Verify courts exist in database
# Check GET /api/lookups/courts returns data
# Check browser Network tab for API calls
```

### Data Not Persisting
```bash
# Check localStorage is enabled in browser
# Open DevTools console and run:
localStorage.getItem('case-data-state')
# Should return JSON data
```

---

## Testing Checklist

- [ ] Add claim
- [ ] Edit claim
- [ ] Delete claim
- [ ] Add related case
- [ ] Edit related case
- [ ] Delete related case
- [ ] Refresh page - data persists
- [ ] Character counter works
- [ ] Form validation works
- [ ] Empty states display
- [ ] Responsive on mobile
- [ ] RTL layout correct

---

## Code Examples

### Add a Claim
```typescript
// In claim-form-dialog.component.ts onSave()
const newClaim: ClaimVM = {
  id: Math.max(...currentClaims.map(c => c.id), 0) + 1,
  caseRegistrationRequestId: this.requestId,
  claimText: this.claimForm.get('claimText')!.value,
  createdDate: new Date(),
  modifiedDate: new Date()
};
this.caseDataState.updateClaims([...currentClaims, newClaim]);
```

### Subscribe to Claims Changes
```typescript
// In claims-list.component.ts
this.caseDataState.state$
  .pipe(
    map(state => state.claims),
    distinctUntilChanged(),
    takeUntil(this.destroy$)
  )
  .subscribe(claims => {
    this.claims = claims;
    this.countChanged.emit(claims.length);
  });
```

### Load Courts
```typescript
// In related-case-form-dialog.component.ts
this.lookupsApi.getCourts().subscribe({
  next: (courts) => {
    this.courts = courts;
  },
  error: (error) => {
    this.snackBar.open('Error loading courts', 'Close');
  }
});
```

---

## CSS Classes

### Claims
```scss
.claims-list       // Container
.claim-card        // Individual card
.claim-header      // Card header
.claim-info        // Title/date
.claim-text        // Claim content
.claim-stats       // Character counter
.character-counter // Counter bar
.empty-state       // No claims message
```

### Related Cases
```scss
.table-container      // Table wrapper
.related-cases-table  // Material table
.empty-state          // No cases message
.loading-state        // Loading spinner
```

---

## Storage

### localStorage Key
```
Key: 'case-data-state'
Value: JSON object with:
{
  subject: string
  evidence: string
  claims: ClaimVM[]
  relatedCases: RelatedCaseVM[]
  classificationIds: number[]
  primaryMobile: string
  secondaryMobile: string
  email: string
}
```

---

## Performance Notes

- localhost storage limit: 5-10MB (sufficient)
- Character limit per claim: 2000
- Recommended max items: 100+ per section
- Consider pagination if > 1000 items

---

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+
- Mobile (iOS/Android)

---

## Documentation Files

1. **PHASE2_IMPLEMENTATION_COMPLETE.md** - Detailed feature breakdown
2. **PHASE2_TESTING_GUIDE.md** - Test cases and procedures
3. **PHASE2_SUMMARY.md** - Architecture and decisions
4. **PHASE2_QUICK_REFERENCE.md** - This file

---

## Next Steps (Phase 3)

1. [ ] Implement API endpoints for persistence
2. [ ] Replace localStorage with API calls
3. [ ] Upgrade ngx-editor when ready
4. [ ] Add search/filter functionality
5. [ ] Add batch operations
6. [ ] Add export functionality

---

**Phase 2 Status**: ✅ COMPLETE AND READY FOR QA
