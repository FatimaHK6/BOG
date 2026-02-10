# Angular Frontend Implementation Plan - Developer-B Components

## Overview

Implementation of Angular frontend for **Developer-B scope (UC 6.5.1.1.11 - 6.5.1.1.24)** based on UX specification with RTL Arabic support, Material Design principles, and integration with completed .NET backend APIs.

**Developer-B Scope**:
- ✅ Defendants Management (CRUD operations)
- ✅ Case Data Form (Subject, Evidence, Classifications)
- ✅ Attachments Upload (with BR04 validation)
- ✅ Additional Info Form
- ✅ Request Actions (Submit, Register, Reject, RequestCompletion)
- ✅ Deficiencies Display
- ✅ Search & List Functionality

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
- **Mobile**: <768px (mobile-optimized layout)

### Key UI Patterns
- **RTL Layout** (Arabic right-to-left)
- **Sidebar Navigation** with section indicators and counts
- **Modal Dialogs** for confirmations and forms
- **Table Pagination** (10, 25, 50 items per page)
- **Form Validation** with inline error messages (ERR codes)
- **Status Badges** color-coded by request state
- **Auto-save** for draft requests (debounced 2 seconds)

---

## Implementation Strategy

**Approach**: Feature Module Architecture (Lazy-loaded Case Registration Module)
**State Management**: Lightweight service-based state (RequestStateService)
**Forms**: Reactive Forms with custom validators matching backend validation rules
**Testing**: Component testing (Jasmine/Karma) + E2E (Cypress for critical flows)
**Timeline**: 5 weeks (25 working days)

---

## Phase 0: Exploration & Setup - Week 1 (5 days)

### Objective
Understand existing Angular codebase, identify reusable patterns, and set up feature module for Developer-B components.

### 0.1 Codebase Exploration (Days 1-2)

**Exploration Tasks:**
1. **Module Structure**
   - Explore `src/Frontend/bog-app/src/app` structure
   - Document existing feature modules
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
- Exploration report documenting reusable components and patterns

### 0.2 Feature Module Setup (Day 3)

**Location**: `src/Frontend/bog-app/src/app/features/case-registration/`

Create module structure for Developer-B components:
```
case-registration/
├── case-registration.module.ts
├── case-registration-routing.module.ts
├── components/
│   ├── shared/
│   │   ├── section-container/
│   │   ├── validation-message/
│   │   └── confirmation-dialog/
│   ├── defendants/                 # Developer-B
│   │   ├── defendants-list/
│   │   └── defendant-form-dialog/
│   ├── case-data/                  # Developer-B
│   │   └── case-data-form/
│   ├── attachments/                # Developer-B
│   │   └── attachments-list/
│   ├── additional-info/            # Developer-B
│   │   └── additional-info-form/
│   ├── deficiencies/               # Developer-B
│   │   └── deficiencies-list/
│   └── request-actions/            # Developer-B
│       ├── request-actions/
│       └── take-action-dialog/
├── pages/
│   ├── request-list/               # Developer-B
│   └── request-details/
├── services/
│   ├── case-registration-api.service.ts
│   ├── defendant-api.service.ts
│   ├── attachment-api.service.ts
│   ├── request-action-api.service.ts
│   ├── additional-info-api.service.ts
│   └── request-state.service.ts
├── models/
│   ├── defendant.model.ts
│   ├── case-request.model.ts
│   ├── attachment.model.ts
│   ├── additional-info.model.ts
│   └── enums.ts
└── validators/
    └── custom-validators.ts
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
```

### 0.4 API Services Skeleton (Day 5)

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

**Additional Services**:
- `attachment-api.service.ts` - File upload/download
- `request-action-api.service.ts` - Submit, Register, Reject actions
- `additional-info-api.service.ts` - Additional info CRUD
- `case-registration-api.service.ts` - Main request operations

**Verification**: Build successfully
```bash
cd src/Frontend/bog-app
ng build --configuration development
```

---

## Phase 1: Shared Components & Layout - Week 2 (5 days)

### Objective
Create reusable shared components and basic layout structure.

### 1.1 Shared Components (Days 1-3)

**1. Section Container Component**

```typescript
@Component({
  selector: 'app-section-container',
  template: `
    <div class="section-container" [id]="sectionId">
      <div class="section-header">
        <h2>
          <mat-icon *ngIf="icon">{{ icon }}</mat-icon>
          {{ title }}
        </h2>
        <span class="section-count" *ngIf="count !== null">{{ count }}</span>
      </div>
      <div class="section-content">
        <ng-content></ng-content>
      </div>
    </div>
  `
})
export class SectionContainerComponent {
  @Input() title!: string;
  @Input() sectionId!: string;
  @Input() icon?: string;
  @Input() count: number | null = null;
}
```

**2. Validation Message Component**

```typescript
@Component({
  selector: 'app-validation-message',
  template: `
    <div class="validation-message" [class]="'validation-' + type">
      <mat-icon>{{ getIcon() }}</mat-icon>
      <span>{{ message }}</span>
    </div>
  `
})
export class ValidationMessageComponent {
  @Input() type: 'error' | 'warning' | 'info' | 'success' = 'error';
  @Input() message!: string;

  getIcon(): string {
    const icons = {
      error: 'error',
      warning: 'warning',
      info: 'info',
      success: 'check_circle'
    };
    return icons[this.type];
  }
}
```

**3. Confirmation Dialog Component**

```typescript
@Component({
  selector: 'app-confirmation-dialog',
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">إلغاء</button>
      <button mat-raised-button [color]="data.confirmColor || 'primary'" (click)="onConfirm()">
        {{ data.confirmText || 'تأكيد' }}
      </button>
    </mat-dialog-actions>
  `
})
export class ConfirmationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmationDialogData
  ) {}

  onConfirm() { this.dialogRef.close(true); }
  onCancel() { this.dialogRef.close(false); }
}
```

### 1.2 Request State Service (Day 4)

```typescript
@Injectable({ providedIn: 'root' })
export class RequestStateService {
  private currentRequestSubject = new BehaviorSubject<CaseRequestVM | null>(null);
  public currentRequest$ = this.currentRequestSubject.asObservable();

  private validationErrorsSubject = new BehaviorSubject<string[]>([]);
  public validationErrors$ = this.validationErrorsSubject.asObservable();

  constructor(private api: CaseRegistrationApiService) {}

  loadRequest(id: number): Observable<CaseRequestVM> {
    return this.api.getById(id).pipe(
      tap(request => this.currentRequestSubject.next(request))
    );
  }

  updateRequest(request: CaseRequestVM) {
    this.currentRequestSubject.next(request);
  }

  refreshRequest() {
    const current = this.currentRequestSubject.value;
    if (current?.id) {
      this.loadRequest(current.id).subscribe();
    }
  }

  // Permission checks
  canEdit(): boolean {
    const request = this.currentRequestSubject.value;
    return request?.requestStatusId === 1 || request?.requestStatusId === 8;
  }

  canSubmit(): boolean {
    const request = this.currentRequestSubject.value;
    return request?.requestStatusId === 1;
  }
}
```

### 1.3 Models & Enums (Day 5)

**File**: `models/defendant.model.ts`

```typescript
export interface DefendantVM {
  id: number;
  defendantTypeId: number;
  defendantTypeName: string;
  fullName: string;
  identityNumber?: string;
  identityTypeName?: string;
  addressText?: string;
  createdDate: Date;
}

export interface DefendantCreateDTO {
  defendantTypeId: number;
  fullName: string;
  identityNumber?: string;
  identityTypeId?: number;
  addressText?: string;
  commercialRegNumber?: string;
  governmentAgencyId?: number;
  additionalStatement?: string;
}

export interface DefendantUpdateDTO extends DefendantCreateDTO {}
```

**File**: `models/enums.ts`

```typescript
export enum DefendantType {
  Natural = 1,
  Company = 2,
  GovernmentEntity = 3,
  NonProfit = 4,
  Unknown = 5,
  Other = 6
}

export enum RequestStatus {
  Draft = 1,
  Submitted = 2,
  New = 3,
  UnderReview = 4,
  OnJudgeDesk = 5,
  Registered = 6,
  Returned = 7,
  PendingCompletion = 8,
  ReviewComplete = 9,
  Rejected = 10
}
```

---

## Phase 2: Defendants Management UI - Week 3 (5 days)

### Objective
Implement complete Defendants CRUD UI (UC 6.5.1.1.11-14).

### 2.1 Defendants List Component (Days 1-2)

**Component**: `defendants-list.component.ts`

**Features**:
- Table display with defendant type, name, identity, address
- Add/Edit/Delete actions
- ERR002 validation message (at least one defendant required)
- Empty state and loading state
- Count badge integration with state service

**Template Structure**:
```html
<app-section-container title="المدعى عليهم" sectionId="defendants" icon="group" [count]="defendants.length">

  <!-- Add Button -->
  <div class="section-actions" *ngIf="canEdit">
    <button mat-raised-button color="primary" (click)="openAddDefendant()">
      <mat-icon>add</mat-icon>
      إضافة مدعى عليه
    </button>
  </div>

  <!-- ERR002 Validation -->
  <app-validation-message
    *ngIf="showValidation && defendants.length === 0"
    type="error"
    message="ERR002: يجب إضافة مدعى عليه واحد على الأقل">
  </app-validation-message>

  <!-- Table with defendants data -->
  <!-- Empty state -->
  <!-- Loading state -->

</app-section-container>
```

### 2.2 Defendant Form Dialog (Days 3-4)

**Component**: `defendant-form-dialog.component.ts`

**Features**:
- Reactive form with validation
- Defendant type selection (Natural, Company, Government, etc.)
- Conditional fields based on type
- ERR013 duplicate validation display
- Character counters for text fields
- Save/Cancel actions

**Form Fields**:
- Defendant Type (required)
- Full Name (required, max 200 chars)
- Identity Type & Number (conditional)
- Commercial Registration Number (for companies)
- Government Agency (for government entities)
- Address (optional, max 2000 chars)
- Additional Statement (optional, max 4000 chars)

### 2.3 Integration & Testing (Day 5)

- Integrate with DefendantApiService
- Test CRUD operations
- Verify ERR013 duplicate validation
- Test responsive behavior
- Verify count updates in state service

**Verification**:
```bash
ng test --include='**/defendants*.spec.ts'
```

---

## Phase 3: Case Data Form - Week 4 (5 days)

### Objective
Implement Case Data form with Subject, Evidence, and Classifications.

### 3.1 Case Data Form Component (Days 1-3)

**Component**: `case-data-form.component.ts`

**Features**:
- Subject field (ERR006 validation)
- Evidence field (ERR007 validation)
- Classifications multi-select (ERR005 validation)
- Court selection
- Auto-save with debounce (2 seconds)
- Character counters

**Template Structure**:
```html
<app-section-container title="بيانات الدعوى" sectionId="case-data" icon="description">

  <form [formGroup]="caseDataForm">
    <!-- Court Selection -->
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>المحكمة</mat-label>
      <mat-select formControlName="courtId" required>
        <mat-option *ngFor="let court of courts" [value]="court.id">
          {{ court.nameAr }}
        </mat-option>
      </mat-select>
    </mat-form-field>

    <!-- Subject (ERR006) -->
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>موضوع الدعوى</mat-label>
      <textarea matInput formControlName="subject" required maxlength="2000" rows="3"></textarea>
      <mat-hint align="end">{{ subjectLength }}/2000</mat-hint>
      <mat-error *ngIf="caseDataForm.get('subject')?.hasError('required')">
        ERR006: موضوع الدعوى مطلوب
      </mat-error>
    </mat-form-field>

    <!-- Evidence (ERR007) -->
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>الأدلة والبينات</mat-label>
      <textarea matInput formControlName="evidence" required maxlength="4000" rows="5"></textarea>
      <mat-hint align="end">{{ evidenceLength }}/4000</mat-hint>
      <mat-error *ngIf="caseDataForm.get('evidence')?.hasError('required')">
        ERR007: الأدلة والبينات مطلوبة
      </mat-error>
    </mat-form-field>

    <!-- Classifications (ERR005) -->
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>تصنيفات الطلب</mat-label>
      <mat-select formControlName="classificationIds" multiple required>
        <mat-option *ngFor="let classification of classifications" [value]="classification.id">
          {{ classification.nameAr }}
        </mat-option>
      </mat-select>
      <mat-error *ngIf="caseDataForm.get('classificationIds')?.hasError('required')">
        ERR005: يجب تحديد تصنيف واحد على الأقل
      </mat-error>
    </mat-form-field>

    <!-- Auto-save indicator -->
    <div class="auto-save-status" *ngIf="autoSaving">
      <mat-spinner diameter="16"></mat-spinner>
      <span>جاري الحفظ التلقائي...</span>
    </div>
  </form>

</app-section-container>
```

### 3.2 Auto-Save Implementation (Day 4)

```typescript
setupAutoSave() {
  this.autoSaveSubscription = this.caseDataForm.valueChanges.pipe(
    debounceTime(2000),
    distinctUntilChanged()
  ).subscribe(() => {
    if (this.caseDataForm.valid && this.canEdit) {
      this.saveData();
    }
  });
}
```

### 3.3 Testing & Validation (Day 5)

- Test ERR005, ERR006, ERR007 validations
- Test auto-save functionality
- Verify lookups loading (courts, classifications)

---

## Phase 4: Attachments & Additional Info - Week 5 (5 days)

### 4.1 Attachments List Component (Days 1-3)

**Component**: `attachments-list.component.ts`

**Features**:
- File upload with BR04 validation (PDF only, max 4MB)
- Attachment type selection
- Mandatory attachments check (ERR003)
- File download functionality
- Delete attachment with confirmation

**BR04 Validation**:
```typescript
onFileSelected(event: Event) {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length > 0) {
    const file = input.files[0];

    // BR04: PDF only
    this.fileTypeError = file.type !== 'application/pdf';

    // BR04: Max 4MB
    const MAX_FILE_SIZE = 4 * 1024 * 1024;
    this.fileSizeError = file.size > MAX_FILE_SIZE;

    if (!this.fileTypeError && !this.fileSizeError) {
      this.selectedFile = file;
    } else {
      this.selectedFile = null;
    }
  }
}
```

**ERR003 Validation**:
```typescript
checkMandatoryAttachments() {
  const mandatoryTypes = this.attachmentTypes.filter(t => t.isMandatory);
  const uploadedTypeIds = new Set(this.attachments.map(a => a.attachmentTypeId));

  this.missingMandatoryTypes = mandatoryTypes
    .filter(t => !uploadedTypeIds.has(t.id))
    .map(t => t.nameAr);
}
```

### 4.2 Additional Info Form Component (Days 4-5)

**Component**: `additional-info-form.component.ts`

**Features**:
- Notes field
- Related case number
- Legal basis
- Priority level selection
- External reference

**Template**:
```html
<app-section-container title="معلومات إضافية" sectionId="additional-info" icon="info">

  <form [formGroup]="additionalInfoForm">
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>ملاحظات</mat-label>
      <textarea matInput formControlName="notes" rows="3" maxlength="2000"></textarea>
    </mat-form-field>

    <mat-form-field appearance="outline" class="full-width">
      <mat-label>رقم قضية مرتبطة</mat-label>
      <input matInput formControlName="relatedCaseNumber" maxlength="50">
    </mat-form-field>

    <mat-form-field appearance="outline" class="full-width">
      <mat-label>مستوى الأولوية</mat-label>
      <mat-select formControlName="priorityLevel">
        <mat-option [value]="1">منخفضة</mat-option>
        <mat-option [value]="2">متوسطة</mat-option>
        <mat-option [value]="3">عالية</mat-option>
        <mat-option [value]="4">عاجلة</mat-option>
      </mat-select>
    </mat-form-field>
  </form>

</app-section-container>
```

---

## Phase 5: Request Actions & Search - Remaining Days

### 5.1 Request Actions Component

**Features**:
- Submit request (validates ERR002, ERR003, ERR005-007)
- Register case action
- Reject request action
- Request completion action (with deficiencies)
- Send to judge action
- Display current status and available actions

### 5.2 Deficiencies List Component

**Features**:
- Display deficiencies when status = PendingCompletion
- Show completion deadline
- Days remaining calculation
- BR05 warning (30-day deadline)

### 5.3 Request List & Search Component

**Features**:
- Search filters (identity number, party name, court, status)
- Pagination (10, 25, 50, 100 per page)
- Status badges
- Navigation to view/edit requests
- Responsive table

---

## Developer-B Validation Rules

| Error Code | Rule | Implementation Location |
|------------|------|------------------------|
| **ERR002** | At least one defendant required | DefendantsListComponent (visual indicator) |
| **ERR003** | Mandatory attachments incomplete | AttachmentsListComponent.checkMandatory() |
| **ERR005** | Classifications not specified | CaseDataFormComponent (Validators.required) |
| **ERR006** | Subject is empty | CaseDataFormComponent (Validators.required) |
| **ERR007** | Evidence is empty | CaseDataFormComponent (Validators.required) |
| **ERR013** | Defendant already exists | DefendantFormDialogComponent (server error) |
| **BR04** | PDF only, max 4MB | AttachmentsListComponent.validateFile() |
| **BR05** | Auto-reject after 30 days | DeficienciesListComponent (deadline display) |

**Note**: ERR001 (plaintiff required) and ERR004 (applicant specified) are Developer-A's responsibility.

---

## Developer-B Components Summary

### Components (11 Components)
1. SectionContainerComponent - Shared wrapper
2. ValidationMessageComponent - Error display
3. ConfirmationDialogComponent - Confirmations
4. DefendantsListComponent - Defendants table
5. DefendantFormDialogComponent - Add/Edit defendant
6. CaseDataFormComponent - Case data
7. AttachmentsListComponent - File uploads
8. AdditionalInfoFormComponent - Additional info
9. DeficienciesListComponent - Deficiencies
10. RequestActionsComponent - Actions
11. RequestListComponent - Search/List

### Services (6 Services)
1. DefendantApiService
2. AttachmentApiService
3. RequestActionApiService
4. AdditionalInfoApiService
5. CaseRegistrationApiService
6. RequestStateService

### Models
- Defendant models (VM, CreateDTO, UpdateDTO)
- CaseRequest models
- Attachment models
- AdditionalInfo models
- Enums (DefendantType, RequestStatus)

---

## Verification Checklist

### Phase 0 (Setup)
- [ ] Module created and lazy-loaded
- [ ] Routing configured
- [ ] API services created
- [ ] Build succeeds

### Phase 1 (Shared Components)
- [ ] Section container functional
- [ ] Validation message displays correctly
- [ ] Confirmation dialog works
- [ ] State service functional
- [ ] Models defined

### Phase 2 (Defendants)
- [ ] List displays correctly
- [ ] Add defendant works
- [ ] Edit defendant works
- [ ] Delete defendant works
- [ ] ERR002 validation displays
- [ ] ERR013 duplicate validation displays
- [ ] Empty/loading states work
- [ ] Mobile responsive

### Phase 3 (Case Data)
- [ ] Subject validation (ERR006)
- [ ] Evidence validation (ERR007)
- [ ] Classifications validation (ERR005)
- [ ] Auto-save works
- [ ] Character counters work

### Phase 4 (Attachments & Additional Info)
- [ ] File upload accepts PDF only (BR04)
- [ ] File size validation max 4MB (BR04)
- [ ] Mandatory attachments check (ERR003)
- [ ] File download works
- [ ] Additional info form saves

### Phase 5 (Actions & Search)
- [ ] Submit validation works
- [ ] Actions functional
- [ ] Deficiencies display
- [ ] BR05 deadline shown
- [ ] Search filters work
- [ ] Pagination works

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

# Generate Developer-B components
ng generate component features/case-registration/components/defendants/defendants-list
ng generate component features/case-registration/components/defendants/defendant-form-dialog
ng generate component features/case-registration/components/case-data/case-data-form
ng generate component features/case-registration/components/attachments/attachments-list
ng generate component features/case-registration/components/additional-info/additional-info-form
```

---

## Success Criteria

Developer-B Angular components are complete when:

1. ✅ All 11 components created and functional
2. ✅ All 6 API services integrated
3. ✅ All Developer-B validations enforced (ERR002, ERR003, ERR005-007, ERR013, BR04-05)
4. ✅ Defendants CRUD fully functional
5. ✅ Case data form with auto-save
6. ✅ File upload with BR04 validation
7. ✅ Request actions functional
8. ✅ Search with pagination works
9. ✅ RTL layout on all screens
10. ✅ Material Design with green color scheme
11. ✅ Responsive (Desktop/Tablet/Mobile)
12. ✅ All component tests passing
13. ✅ No build errors/warnings
14. ✅ Production build optimized

**Estimated Effort**: 5 weeks (200 hours)

---

*Angular Frontend Implementation - Developer-B Components (UC 6.5.1.1.11-24) with RTL Arabic, Material Design, and .NET backend integration.*
