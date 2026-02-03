# BC01 & BC02 Implementation Summary

## Business Rules for Individual Form (نموذج بيانات فرد)

### BC01 - Employment Data Fields (بيانات جهة العمل)
**Two fields together:**
1. جهة العمل (Employer)
2. المهنة (Profession)

- **يتم اظهار** نموذج بيانات جهة العمل فقط اذا كانت حالة العمل للفرد **حكومي أو خاص**
- **يتم اخفاء** نموذج بيانات جهة العمل فقط اذا كانت حالة العمل للفرد **بدون عمل**

### BC02 - Work Address (عنوان العمل)
- **يتم اظهار** نموذج العنوان الوطني للعمل فقط اذا كانت حالة العمل للفرد **خاص**
- **يتم اخفاء** نموذج العنوان الوطني للعمل فقط اذا كانت حالة العمل للفرد **حكومي**

---

## Employment Status Values (حالة العمل)

| ID | Arabic | English |
|----|--------|---------|
| 1 | حكومي | Government |
| 2 | خاص | Private |
| 3 | بدون عمل | Unemployed |

---

## Implementation Summary Table

| حالة العمل | بيانات جهة العمل (BC01) | عنوان العمل (BC02) |
|------------|------------------------|-------------------|
| | جهة العمل + المهنة | |
| حكومي (1) | يظهر | يخفى |
| خاص (2) | يظهر | يظهر |
| بدون عمل (3) | يخفى | يخفى |

---

## Code Implementation

### TypeScript (plaintiff-form.component.ts)

```typescript
/**
 * BC01/BC02: Handle employment status changes.
 * BC01: Show employer when Government or Private
 * BC02: Show work address only when Private
 */
private onEmploymentStatusChange(statusId: number): void {
  const employerControl = this.plaintiffForm.get('employer');
  const workAddressGroup = this.plaintiffForm.get('workAddress');

  // BC01: Employer visibility
  if (statusId === 1 || statusId === 2) { // Government or Private
    this.showEmployer = true;
    employerControl?.setValidators([Validators.required, Validators.maxLength(200)]);
  } else { // Unemployed
    this.showEmployer = false;
    employerControl?.clearValidators();
    employerControl?.setValue(null);
  }
  employerControl?.updateValueAndValidity();

  // BC02: Work Address visibility
  if (statusId === 2) { // Private only
    this.showWorkAddress = true;
  } else { // Government or Unemployed
    this.showWorkAddress = false;
    workAddressGroup?.reset();
  }
}
```

### HTML (plaintiff-form.component.html)

```html
<!-- BC01: بيانات جهة العمل (both fields together) -->
<ng-container *ngIf="showEmployer">
  <mat-form-field appearance="outline" class="form-field">
    <mat-label>جهة العمل</mat-label>
    <input matInput formControlName="employer">
  </mat-form-field>

  <mat-form-field appearance="outline" class="form-field">
    <mat-label>المهنة</mat-label>
    <input matInput formControlName="profession">
  </mat-form-field>
</ng-container>

<!-- BC02: Work Address section -->
<div class="address-section" formGroupName="workAddress"
     *ngIf="(selectedPlaintiffType === 1 || isBusinessOwner()) && showWorkAddress">
  <h4>عنوان العمل</h4>
  <!-- Address fields -->
</div>
```

---

## Files Modified

| File | Changes |
|------|---------|
| `plaintiff-form.component.ts` | Added `showEmployer`, `showWorkAddress` flags and `onEmploymentStatusChange()` method |
| `plaintiff-form.component.html` | Added `*ngIf="showEmployer"` and `*ngIf="... && showWorkAddress"` conditions |

---

## Applies To

- Type 1: فرد (Individual)
- Type 3: صاحب مؤسسة فردية (Business Owner)
