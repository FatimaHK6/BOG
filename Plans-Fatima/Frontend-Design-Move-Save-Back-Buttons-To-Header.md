# Frontend Design Plan: Move Save/Back Buttons to Green Header Bar

## Quick Summary

**What:** Move Save and Back buttons from case-data-container to the green header bar in request-details page

**Why:** Better UX - buttons should be globally accessible from the header, not buried inside a specific section

**Changes:** 3 files modified (2 templates, 1 TypeScript file)

**Complexity:** Low-Medium (template + component communication)

---

## Current State vs Desired State

### Current State ✅ (Partially Done)
- Sidebar navigation WITHOUT Claims, Related Cases, Attachments ✅
- Case Data section shows 6 vertical tabs ✅
- Save/Back buttons exist in TWO places:
  1. Sidebar bottom (lines 57-69 of request-details.component.html)
  2. Inside case-data-container (lines 8-45 of case-data-container.component.html)

### Desired State 🎯
```
┌──────────────────────────────────────────────────────────┐
│ [رجوع] [حفظ]    طلب تسجيل دعوى    رقم الطلب: 123  مسودة │ ← Green Header Bar
├──────────────────────────────────────────────────────────┤
│ SIDEBAR          │  MAIN CONTENT                         │
│ ─────────        │  ─────────────                        │
│ المدعى عليهم     │                                       │
│ ✓ بيانات الدعوى │  [6 Vertical Tabs on Right]           │
│ معلومات إضافية   │  [Tab Content on Left]                │
│ إجراءات الطلب    │                                       │
└──────────────────┴───────────────────────────────────────┘
```

**Key Changes:**
1. **Green header bar**: Add Save/Back buttons on the LEFT side (RTL layout)
2. **Sidebar**: Remove Save/Back buttons from sidebar bottom
3. **case-data-container**: Remove fixed-action-bar with Save/Back buttons
4. **Save logic**: Move from case-data-container to request-details (use @ViewChild)

---

## Implementation Plan

### Step 1: Add Save/Back Buttons to Green Header Bar

**File:** `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.html`

**Location:** Lines 2-13 (the header section)

**Current Header:**
```html
<header class="layout-header">
  <div class="header-content">
    <h1>{{ pageTitle }}</h1>
    <div class="header-info">
      <span class="request-number" *ngIf="requestId">رقم الطلب: {{ requestId }}</span>
      <span class="request-status" [class]="'status-' + currentStatus">
        {{ currentStatusName }}
      </span>
    </div>
  </div>
</header>
```

**New Header (with buttons on LEFT side for RTL):**
```html
<header class="layout-header">
  <div class="header-content">
    <!-- Action Buttons (LEFT side for RTL) -->
    <div class="header-actions">
      <button mat-raised-button
              class="back-button"
              (click)="navigateBack()">
        <mat-icon>arrow_forward</mat-icon>
        <span>رجوع</span>
      </button>

      <button mat-raised-button
              color="primary"
              class="save-button"
              (click)="saveRequest()"
              [disabled]="!canSave || isSaving"
              *ngIf="canEdit">
        <mat-icon>save</mat-icon>
        <span>حفظ</span>
      </button>

      <!-- Save Status Indicators -->
      <div class="save-status" *ngIf="isSaving">
        <mat-spinner diameter="20"></mat-spinner>
        <span>جاري الحفظ...</span>
      </div>

      <div class="save-success" *ngIf="saveSuccess && !isSaving">
        <mat-icon>check_circle</mat-icon>
        <span>تم الحفظ</span>
      </div>
    </div>

    <!-- Title and Info (CENTER/RIGHT) -->
    <div class="header-title-info">
      <h1>{{ pageTitle }}</h1>
      <div class="header-info">
        <span class="request-number" *ngIf="requestId">رقم الطلب: {{ requestId }}</span>
        <span class="request-status" [class]="'status-' + currentStatus">
          {{ currentStatusName }}
        </span>
      </div>
    </div>
  </div>
</header>
```

---

### Step 2: Remove Save/Back Buttons from Sidebar

**File:** Same as Step 1

**Location:** Lines 57-69

**REMOVE:**
```html
<!-- Sticky Action Buttons -->
<div class="sidebar-actions">
  <button mat-raised-button color="primary"
          (click)="saveRequest()"
          [disabled]="!canSave"
          *ngIf="canEdit">
    <mat-icon>save</mat-icon>
    حفظ
  </button>
  <button mat-stroked-button (click)="navigateBack()">
    <mat-icon>arrow_back</mat-icon>
    رجوع
  </button>
</div>
```

**Reason:** Buttons now in header bar, no need for duplicate buttons in sidebar

---

### Step 3: Remove Fixed Action Bar from case-data-container

**File:** `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.html`

**Location:** Lines 7-45

**REMOVE:**
```html
<!-- Fixed Action Bar -->
<div class="fixed-action-bar">
  <button
    mat-raised-button
    class="back-button"
    (click)="goBack()"
    [disabled]="isSaving">
    <mat-icon>arrow_forward</mat-icon>
    <span>عودة</span>
  </button>

  <div class="save-section">
    <button
      mat-raised-button
      color="primary"
      class="save-button"
      (click)="saveAllData()"
      [disabled]="!canEdit || isSaving">
      <mat-icon>save</mat-icon>
      <span>حفظ كمسودة</span>
    </button>

    <!-- Save Status Indicator -->
    <div class="save-status" *ngIf="isSaving">
      <mat-spinner diameter="24"></mat-spinner>
      <span>جاري الحفظ...</span>
    </div>

    <div class="save-success" *ngIf="saveSuccess && !isSaving">
      <mat-icon>check_circle</mat-icon>
      <span>تم الحفظ بنجاح</span>
    </div>

    <div class="save-error" *ngIf="saveError">
      <mat-icon>error</mat-icon>
      <span>{{ saveError }}</span>
    </div>
  </div>
</div>
```

**Keep only:** The tab content section (starts at line 48)

---

### Step 4: Update request-details Component TypeScript

**File:** `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.ts`

**Changes Needed:**

1. **Add ViewChild to access case-data-container:**
```typescript
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CaseDataContainerComponent } from '../../components/case-data/case-data-container/case-data-container.component';

export class RequestDetailsComponent implements OnInit, OnDestroy {
  @ViewChild(CaseDataContainerComponent) caseDataContainer?: CaseDataContainerComponent;

  // ... existing properties ...

  // Add saving state properties
  isSaving = false;
  saveSuccess = false;
```

2. **Implement saveRequest() method:**
```typescript
saveRequest() {
  console.log('Save requested from header bar');

  // If we're on case-data section, delegate to case-data-container
  if (this.activeSection === 'case-data' && this.caseDataContainer) {
    this.isSaving = true;
    this.saveSuccess = false;

    // Call child component's save method
    this.caseDataContainer.saveAllData();

    // Subscribe to save completion (we'll need to add an @Output in child)
    // Or use the state service to detect when save completes
    this.caseDataState.state$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.isSaving = false;
        this.saveSuccess = true;

        // Reset success indicator after 2 seconds
        setTimeout(() => {
          this.saveSuccess = false;
        }, 2000);
      });
  } else {
    // Handle saving for other sections
    console.log('Save for section:', this.activeSection);
  }
}
```

---

### Step 5: Update case-data-container Component (Optional Output)

**File:** `src/Frontend/bog-app/src/app/features/case-registration/components/case-data/case-data-container/case-data-container.component.ts`

**Add Output event for save completion:**
```typescript
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

export class CaseDataContainerComponent implements OnInit, OnDestroy {
  @Input() requestId!: number;
  @Input() canEdit = false;
  @Output() saveComplete = new EventEmitter<{ success: boolean; error?: string }>();

  // Remove goBack() method since parent handles navigation now
  // Keep saveAllData() but emit event on completion

  saveAllData() {
    if (!this.canEdit) {
      this.snackBar.open('لا يمكنك حفظ البيانات في وضع العرض', 'إغلاق', { duration: 3000 });
      this.saveComplete.emit({ success: false, error: 'لا يمكنك حفظ البيانات في وضع العرض' });
      return;
    }

    this.isSaving = true;
    this.saveSuccess = false;
    this.saveError = null;

    const state = this.caseDataState.getAllData();
    const payload = {
      subject: state.subject,
      evidence: state.evidence,
      claims: state.claims.map(c => ({ claimText: c.claimText })),
      relatedCases: state.relatedCases.map(rc => ({
        courtId: rc.courtId,
        caseNumber: rc.caseNumber,
        caseYear: rc.caseYear
      })),
      classificationIds: state.classificationIds,
      primaryMobile: state.primaryMobile,
      secondaryMobile: state.secondaryMobile,
      email: state.email
    };

    this.caseRegistrationApi.update(this.requestId, payload).subscribe({
      next: (response: any) => {
        this.isSaving = false;
        this.saveSuccess = true;
        this.snackBar.open('تم حفظ البيانات بنجاح', 'إغلاق', { duration: 3000 });
        this.saveComplete.emit({ success: true });

        setTimeout(() => {
          this.saveSuccess = false;
        }, 2000);
      },
      error: (error: any) => {
        this.isSaving = false;
        this.saveError = error?.error?.message || 'حدث خطأ أثناء حفظ البيانات';
        console.error('Error saving case data:', error);
        this.snackBar.open(this.saveError || 'حدث خطأ', 'إغلاق', { duration: 5000 });
        this.saveComplete.emit({ success: false, error: this.saveError || undefined });
      }
    });
  }
}
```

---

### Step 6: Update request-details Template for Output Event

**File:** `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.html`

**Location:** Lines 93-98 (case-data section)

**Change FROM:**
```html
<section id="case-data" *ngSwitchCase="'case-data'" class="tab-content">
  <app-case-data-container
    [requestId]="requestId"
    [canEdit]="canEdit">
  </app-case-data-container>
</section>
```

**Change TO:**
```html
<section id="case-data" *ngSwitchCase="'case-data'" class="tab-content">
  <app-case-data-container
    [requestId]="requestId"
    [canEdit]="canEdit"
    (saveComplete)="onSaveComplete($event)">
  </app-case-data-container>
</section>
```

**Add handler in TypeScript:**
```typescript
onSaveComplete(event: { success: boolean; error?: string }) {
  this.isSaving = false;

  if (event.success) {
    this.saveSuccess = true;
    setTimeout(() => {
      this.saveSuccess = false;
    }, 2000);
  } else {
    console.error('Save failed:', event.error);
  }
}
```

---

### Step 7: Update Styles for Header Bar

**File:** `src/Frontend/bog-app/src/app/features/case-registration/pages/request-details/request-details.component.scss`

**Add styles for header actions:**
```scss
.layout-header {
  background: #2c5f2d; // Green background
  color: white;
  padding: 1rem 1.5rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);

  .header-content {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 1rem;

    // RTL: Actions on left, title/info on right
    .header-actions {
      display: flex;
      align-items: center;
      gap: 0.75rem;

      button {
        height: 36px;

        &.back-button {
          background: white;
          color: #2c5f2d;
        }

        &.save-button {
          // Primary color from Angular Material
        }
      }

      .save-status,
      .save-success {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        font-size: 0.9rem;
        color: white;

        mat-spinner {
          display: inline-block;
        }
      }

      .save-success {
        mat-icon {
          color: #4caf50;
        }
      }
    }

    .header-title-info {
      display: flex;
      flex-direction: column;
      align-items: flex-end; // RTL: align right
      gap: 0.25rem;

      h1 {
        margin: 0;
        font-size: 1.5rem;
        font-weight: 600;
      }

      .header-info {
        display: flex;
        gap: 1rem;
        font-size: 0.9rem;

        .request-number {
          opacity: 0.9;
        }

        .request-status {
          padding: 0.25rem 0.75rem;
          border-radius: 12px;
          background: rgba(255,255,255,0.2);
          font-weight: 500;
        }
      }
    }
  }
}

// Mobile responsive
@media (max-width: 768px) {
  .layout-header {
    .header-content {
      flex-direction: column;
      align-items: stretch;

      .header-actions {
        order: 2;
        justify-content: flex-start;
      }

      .header-title-info {
        order: 1;
        align-items: flex-start;
      }
    }
  }
}
```

---

## Files to Modify

### 1. request-details.component.html (3 changes)
- Add Save/Back buttons to header (lines 2-13)
- Remove Save/Back buttons from sidebar (lines 57-69)
- Add saveComplete output binding to case-data-container (lines 93-98)

### 2. request-details.component.ts (2 additions)
- Add @ViewChild for CaseDataContainerComponent
- Implement saveRequest() method
- Add onSaveComplete() handler
- Add isSaving and saveSuccess properties

### 3. case-data-container.component.html (1 removal)
- Remove fixed-action-bar (lines 7-45)

### 4. case-data-container.component.ts (1 modification)
- Add @Output() saveComplete EventEmitter
- Emit event in saveAllData() on success/error
- Remove goBack() method (parent handles navigation)

### 5. request-details.component.scss (1 addition)
- Add styles for .header-actions
- Add styles for .header-title-info
- Add mobile responsive styles

---

## Testing Plan

### Visual Verification

1. **Run the application:**
   ```bash
   cd src/Frontend/bog-app
   ng serve
   ```

2. **Check Header Bar:**
   - [ ] Green header bar visible at top
   - [ ] Save button on LEFT side (RTL layout)
   - [ ] Back button on LEFT side (RTL layout)
   - [ ] Title and status info on RIGHT side
   - [ ] Buttons are properly styled

3. **Check Sidebar:**
   - [ ] No Save/Back buttons at bottom
   - [ ] Navigation items still work
   - [ ] Sidebar structure clean

4. **Check Case Data Section:**
   - [ ] 6 vertical tabs visible
   - [ ] No action bar inside container
   - [ ] Content area clean

5. **Test Save Functionality:**
   - [ ] Click Save button in header
   - [ ] Spinner shows "جاري الحفظ..."
   - [ ] Success message shows "تم الحفظ"
   - [ ] Data persists
   - [ ] Network request successful

6. **Test Back Functionality:**
   - [ ] Click Back button in header
   - [ ] Navigates to request list
   - [ ] No errors in console

### Mobile Testing

7. **Mobile View (375x667):**
   - [ ] Header buttons stack properly
   - [ ] All buttons accessible
   - [ ] No overflow issues

---

## Success Criteria

✅ Implementation complete when:

1. Save/Back buttons visible in green header bar
2. Buttons positioned on LEFT side (RTL layout)
3. No duplicate buttons in sidebar or case-data-container
4. Save functionality works from header
5. Spinner and success messages display in header
6. Navigation works from header
7. No console errors
8. Mobile responsive
9. All 6 tabs still functional
10. Data persists after save

---

## Risk Assessment

**Risk Level:** 🟢 LOW

**Why Low:**
- Template changes only (no database/API changes)
- Component communication using standard Angular patterns (@ViewChild, @Output)
- Easy to rollback by reverting template changes
- No data loss risk

**Rollback Plan:**
1. Revert the 3 modified files
2. Application returns to previous state
3. All functionality restored

---

## Expected Outcome

```
┌────────────────────────────────────────────────────────────────┐
│ [رجوع] [حفظ] [✓ تم الحفظ]    طلب تسجيل دعوى    رقم: 123  مسودة │ ← GREEN HEADER
├────────────────────────────────────────────────────────────────┤
│ SIDEBAR         │  MAIN CONTENT AREA                           │
│ ───────         │  ─────────────────                           │
│ المدعى عليهم    │  ┌──────────────┬────────────────────────┐  │
│ ✓ بيانات الدعوى│  │              │ موضوع وأسانيد الدعوى ✓│  │
│ معلومات إضافية  │  │  Tab Content │ طلبات الدعوى (2)      │  │
│ إجراءات الطلب   │  │              │ الدعاوى المرتبطة (1)  │  │
│                 │  │              │ تصنيف الدعوى         │  │
│                 │  │              │ بيانات التواصل       │  │
│                 │  │              │ المرفقات (3)         │  │
│                 │  └──────────────┴────────────────────────┘  │
└─────────────────┴──────────────────────────────────────────────┘
```

**Key Features:**
- ✅ Save/Back in green header (globally accessible)
- ✅ Clean sidebar (no duplicate buttons)
- ✅ Clean case-data-container (no action bar)
- ✅ 6 vertical tabs functional
- ✅ RTL layout preserved
