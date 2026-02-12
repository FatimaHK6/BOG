# Angular Frontend Implementation Plan - Case Registration Module (UC 6.5.1)

## Overview

Implementation of Angular frontend for **UC 6.5.1.1.11 - 6.5.1.1.24** (14 use cases) based on UX specification (UC-6.5.1-UX-Specification.html) with RTL Arabic support, Material Design principles, and integration with the completed .NET backend APIs.

**Scope**: Complete UI/UX for Case Registration Request workflow including Plaintiffs (Developer-A), Defendants (Developer-B), Case Data, Attachments, Additional Info, Request Actions, Search functionality, and responsive design.

**Backend Status**: ✅ 100% Complete (All APIs, repositories, BL services, migrations applied, 95 tests passing)

## Design System (From UX Specification)

### Color Scheme
- **Primary**: `#1B5E20` (Dark Green)
- **Secondary**: `#0D3D14`
- **Success**: `#4CAF50`
- **Warning**: `#FF9800`
- **Error**: `#F44336`
- **Info**: `#2196F3`

### Responsive Breakpoints
- **Desktop**: ≥1200px (full sidebar + content)
- **Tablet**: 768px - 1199px (collapsible sidebar)
- **Mobile**: <768px (bottom navigation)

### Key UI Patterns
- **RTL Layout** (Arabic right-to-left)
- **Sidebar Navigation** with section indicators and counts
- **Wizard/Stepper** for plaintiff registration (4 steps)
- **Modal Dialogs** for confirmations and forms
- **Table Pagination** (10, 25, 50 items per page)
- **Form Validation** with inline error messages (ERR codes)
- **Status Badges** color-coded by request state
- **Auto-save** for draft requests (debounced 2 seconds)

---

## Implementation Strategy

**Approach**: Feature Module Architecture (Lazy-loaded Case Registration Module)
**State Management**: Lightweight service-based state (RequestStateService) - avoid NgRx complexity
**Forms**: Reactive Forms with custom validators matching backend validation rules
**Testing**: Component testing (Jasmine/Karma) + E2E (Cypress for critical flows)
**Timeline**: 6 weeks (30 working days)

---

## Phase 0: Exploration & Setup - Week 1 (5 days)

### Objective
Understand existing Angular codebase, identify reusable patterns, and set up Case Registration feature module.

### 0.1 Codebase Exploration (Days 1-2)

**Exploration Tasks:**
1. **Module Structure**
   - Explore `src/Frontend/bog-app/src/app` structure
   - Document existing feature modules (if any)
   - Check for shared module patterns

2. **Shared Components**
   - Search for existing UI components (buttons, inputs, tables, modals)
   - Check Material Design usage (Angular Material version)
   - Identify reusable form controls

3. **API Integration Patterns**
   - Review existing HttpClient setup and interceptors
   - Check error handling patterns
   - Verify authentication/authorization patterns

4. **Routing & Navigation**
   - Examine app routing configuration
   - Check for lazy loading patterns
   - Identify navigation guard patterns

5. **Styling & Theming**
   - Check for existing RTL support
   - Review Material Design theme configuration
   - Identify global styles and variables

**Deliverables**:
- Exploration report documenting:
  - Reusable components to leverage
  - Gaps requiring new components
  - Existing patterns to follow
  - Angular Material version and theme setup

### 0.2 Feature Module Setup (Day 3)

**Location**: `src/Frontend/bog-app/src/app/features/case-registration/`

Create module structure:
```
case-registration/
├── case-registration.module.ts
├── case-registration-routing.module.ts
├── components/
│   ├── shared/                    # Feature-specific shared components
│   │   ├── section-container/
│   │   ├── validation-message/
│   │   └── confirmation-dialog/
│   ├── defendants/
│   │   ├── defendants-list/
│   │   └── defendant-form-dialog/
│   ├── case-data/
│   │   └── case-data-form/
│   ├── attachments/
│   │   └── attachments-list/
│   ├── additional-info/
│   │   └── additional-info-form/
│   ├── deficiencies/
│   │   └── deficiencies-list/
│   └── request-actions/
│       ├── request-actions/
│       └── take-action-dialog/
├── pages/
│   ├── request-list/              # Search & list
│   ├── request-create/            # Create new (Draft)
│   └── request-details/           # View/edit request
├── services/
│   ├── case-registration-api.service.ts
│   ├── defendant-api.service.ts
│   ├── attachment-api.service.ts
│   ├── request-action-api.service.ts
│   └── request-state.service.ts   # Lightweight state management
├── models/
│   ├── defendant.model.ts
│   ├── case-request.model.ts
│   ├── attachment.model.ts
│   └── enums.ts
├── validators/
│   └── custom-validators.ts       # Match backend validation rules
└── interceptors/ (if needed)
    └── error-handler.interceptor.ts
```

**Module Registration**:
```typescript
@NgModule({
  declarations: [
    // Components declared incrementally
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CaseRegistrationRoutingModule,
    // Material modules
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatPaginatorModule,
    MatDialogModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDatepickerModule,
    MatMenuModule
  ]
})
export class CaseRegistrationModule { }
```

### 0.3 Routing Setup (Day 4)

**File**: `case-registration-routing.module.ts`

```typescript
const routes: Routes = [
  {
    path: '',
    children: [
      { path: '', redirectTo: 'list', pathMatch: 'full' },
      {
        path: 'list',
        component: RequestListComponent,
        data: { title: 'قائمة طلبات التسجيل' }
      },
      {
        path: 'create',
        component: RequestDetailsComponent,
        data: { mode: 'create', title: 'طلب تسجيل دعوى جديد' }
      },
      {
        path: ':id/edit',
        component: RequestDetailsComponent,
        data: { mode: 'edit', title: 'تعديل طلب تسجيل الدعوى' }
      },
      {
        path: ':id/view',
        component: RequestDetailsComponent,
        data: { mode: 'view', title: 'عرض طلب تسجيل الدعوى' }
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CaseRegistrationRoutingModule { }
```

**App-level Lazy Loading** (update `app-routing.module.ts`):
```typescript
{
  path: 'case-registration',
  loadChildren: () => import('./features/case-registration/case-registration.module')
    .then(m => m.CaseRegistrationModule),
  canActivate: [AuthGuard], // If auth is required
  data: { breadcrumb: 'إدارة الدعاوى' }
}
```

### 0.4 API Services Skeleton (Day 5)

**File**: `services/case-registration-api.service.ts`

```typescript
@Injectable({ providedIn: 'root' })
export class CaseRegistrationApiService {
  private baseUrl = `${environment.apiUrl}/api/case-requests`;

  constructor(private http: HttpClient) {}

  getRequests(params: any): Observable<PagedResult<CaseRequestVM>> {
    return this.http.get<PagedResult<CaseRequestVM>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<CaseRequestVM> {
    return this.http.get<CaseRequestVM>(`${this.baseUrl}/${id}`);
  }

  create(dto: CaseRequestCreateDTO): Observable<CaseRequestVM> {
    return this.http.post<CaseRequestVM>(this.baseUrl, dto);
  }

  update(id: number, dto: CaseRequestUpdateDTO): Observable<CaseRequestVM> {
    return this.http.put<CaseRequestVM>(`${this.baseUrl}/${id}`, dto);
  }

  submit(id: number): Observable<CaseRequestVM> {
    return this.http.post<CaseRequestVM>(`${this.baseUrl}/${id}/submit`, {});
  }
}
```

**File**: `services/defendant-api.service.ts`

```typescript
@Injectable({ providedIn: 'root' })
export class DefendantApiService {
  private baseUrl = `${environment.apiUrl}/api/case-requests`;

  constructor(private http: HttpClient) {}

  getDefendants(requestId: number): Observable<DefendantVM[]> {
    return this.http.get<DefendantVM[]>(`${this.baseUrl}/${requestId}/defendants`);
  }

  getById(id: number): Observable<DefendantVM> {
    return this.http.get<DefendantVM>(`${this.baseUrl}/defendants/${id}`);
  }

  create(requestId: number, dto: DefendantCreateDTO): Observable<DefendantVM> {
    return this.http.post<DefendantVM>(`${this.baseUrl}/${requestId}/defendants`, dto);
  }

  update(id: number, dto: DefendantUpdateDTO): Observable<DefendantVM> {
    return this.http.put<DefendantVM>(`${this.baseUrl}/defendants/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/defendants/${id}`);
  }
}
```

**Verification**: Build successfully
```bash
cd src/Frontend/bog-app
ng build --configuration development
```

---

## Phase 1: Core Layout & Shared Components - Week 2 (5 days)

### Objective
Create the main request details layout with sidebar navigation and reusable shared components.

### 1.1 Request Details Layout Component (Days 1-2)

**Component**: `pages/request-details/request-details.component.ts`

See full implementation in the plan file for:
- Complete HTML template with sidebar navigation
- TypeScript component logic
- SCSS styles with RTL support
- Responsive behavior (Desktop/Tablet/Mobile)
- State management integration

### 1.2 Shared Components (Days 3-4)

**1. Section Container Component**
**2. Validation Message Component**
**3. Confirmation Dialog Component**

(Full code provided in plan)

### 1.3 Request State Service (Day 5)

Lightweight state management service for request data and validation errors.

---

## Phase 2: Defendants Management UI - Week 3 (5 days)

### Objective
Implement complete UI for Defendant CRUD operations (UC 6.5.1.1.11-14).

### 2.1 Models & Enums (Day 1)
### 2.2 Defendants List Component (Days 2-3)
### 2.3 Defendant Form Dialog (Days 4-5)

Complete implementation provided in plan file with:
- Table display with type badges
- Add/Edit/Delete functionality
- ERR013 duplicate validation
- Empty and loading states
- Mobile responsive design

---

## Phase 3: Case Data, Attachments & Additional Info - Week 4 (5 days)

### Components to Implement:
1. **CaseDataFormComponent** - Subject, Evidence, Classifications (ERR005-007)
2. **AttachmentsListComponent** - File upload with BR04 validation (PDF only, max 4MB)
3. **AdditionalInfoFormComponent** - Notes, priority, related cases

---

## Phase 4: Request Actions & State Management - Week 5 (5 days)

### Components to Implement:
1. **RequestActionsComponent** - Submit, Register, Reject, RequestCompletion
2. **TakeActionDialogComponent** - Multi-action dialog with deficiencies
3. **DeficienciesListComponent** - Display completion requirements

---

## Phase 5: Search & List Functionality - Week 6 (5 days)

### Components to Implement:
1. **RequestListComponent** - Search filters and pagination
2. Integration with all API services
3. Navigation to view/edit requests

---

## Phase 6: Testing, Responsive Design & Polish - Remaining Days

### Testing:
- Component unit tests (Jasmine/Karma)
- E2E tests (Cypress)
- Accessibility testing

### Responsive Design:
- Desktop (≥1200px)
- Tablet (768-1199px)
- Mobile (<768px)

### Polish:
- RTL layout verification
- Color contrast (WCAG 2.1 AA)
- Performance optimization

---

## Critical Files to Implement

### Components (14+ Components)
1. RequestDetailsComponent - Main layout
2. SectionContainerComponent - Reusable wrapper
3. ValidationMessageComponent - Error display
4. ConfirmationDialogComponent - Confirmations
5. DefendantsListComponent - Defendants table
6. DefendantFormDialogComponent - Add/Edit form
7. CaseDataFormComponent - Case data
8. AttachmentsListComponent - File uploads
9. AdditionalInfoFormComponent - Additional info
10. DeficienciesListComponent - Deficiencies
11. RequestActionsComponent - Actions
12. TakeActionDialogComponent - Action dialog
13. RequestListComponent - Search/List
14. [Plus additional components]

### Services (6 Services)
1. CaseRegistrationApiService
2. DefendantApiService
3. AttachmentApiService
4. RequestActionApiService
5. AdditionalInfoApiService
6. RequestStateService

### Models (10+ Files)
- Defendant models (VM, CreateDTO, UpdateDTO)
- CaseRequest models (VM, CreateDTO, UpdateDTO)
- Attachment models
- AdditionalInfo models
- Enums (DefendantType, IdentityType, RequestStatus)

---

## Validation Rules Mapping

| Error Code | Backend Validation | Frontend Validation Location |
|------------|-------------------|----------------------------|
| ERR001 | At least one plaintiff required | RequestActionsComponent.validateBeforeSubmit() |
| ERR002 | At least one defendant required | DefendantsListComponent (visual indicator) |
| ERR003 | Mandatory attachments incomplete | AttachmentsListComponent.checkMandatory() |
| ERR004 | Applicant not specified | PlaintiffsListComponent (Developer-A) |
| ERR005 | Classifications not specified | CaseDataFormComponent.validateClassifications() |
| ERR006 | Subject is empty | CaseDataFormComponent (Validators.required) |
| ERR007 | Evidence is empty | CaseDataFormComponent (Validators.required) |
| ERR013 | Defendant already exists | DefendantFormDialogComponent (server error) |
| BR04 | PDF only, max 4MB | AttachmentsListComponent.validateFile() |
| BR05 | Auto-reject after 30 days | DeficienciesListComponent (deadline display) |

---

## Verification Checklist

### Phase 0 (Setup)
- [ ] Module created and lazy-loaded
- [ ] Routing configured
- [ ] API services created
- [ ] Build succeeds

### Phase 1 (Layout & Shared)
- [ ] Layout renders correctly
- [ ] Sidebar navigation functional
- [ ] Mobile toggle works
- [ ] Shared components functional
- [ ] RTL layout working

### Phase 2 (Defendants)
- [ ] List displays correctly
- [ ] Add/Edit/Delete work
- [ ] ERR013 validation displays
- [ ] Empty/loading states work
- [ ] Mobile responsive

### Phase 3-6 (Remaining Components)
- [ ] All forms functional
- [ ] All validations working
- [ ] File upload with BR04
- [ ] Actions and state transitions
- [ ] Search and pagination
- [ ] Tests passing
- [ ] Responsive on all breakpoints
- [ ] Accessibility compliant

---

## Commands Reference

```bash
# Development
cd src/Frontend/bog-app
ng serve

# Build
ng build --configuration development
ng build --configuration production

# Test
ng test
ng e2e

# Generate
ng generate component features/case-registration/components/name
ng generate service features/case-registration/services/name

# Lint
ng lint
```

---

## Success Criteria

The Angular frontend is complete when:

1. ✅ All 14+ components created and functional
2. ✅ All 6 API services integrated
3. ✅ All 14 use cases have UI
4. ✅ All 10 validation rules enforced with proper messaging
5. ✅ State transitions work with visual feedback
6. ✅ Search with pagination works
7. ✅ File upload with BR04 validation
8. ✅ RTL layout on all screens
9. ✅ Material Design with green color scheme
10. ✅ Responsive (Desktop/Tablet/Mobile)
11. ✅ All tests passing
12. ✅ No build errors/warnings
13. ✅ Accessibility (WCAG 2.1 AA)
14. ✅ Production build optimized

**Estimated Effort**: 6 weeks (240 hours)

---

*Angular Frontend Implementation for UC 6.5.1.1.11-24 based on UC-6.5.1-UX-Specification.html with RTL Arabic, Material Design, and .NET backend integration.*
