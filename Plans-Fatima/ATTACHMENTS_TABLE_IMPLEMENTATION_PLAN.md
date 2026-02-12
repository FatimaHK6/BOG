# Implementation Plan: Attachments Table with Three Columns

## Overview

Replace the current Material list display for attachments with a Material table showing three columns:
1. **نوع المرفق** (Attachment Type)
2. **ملاحظات** (Notes/Description)
3. **المرفق** (Attachment - download link)

## Current State

The attachments feature is fully implemented with:
- ✅ Backend CRUD API endpoints
- ✅ File upload/download functionality
- ✅ Frontend component using `mat-list` to display attachments
- ⚠️ **Missing**: Description field not included in backend VM or frontend interface
- ⚠️ **Current Display**: Shows filename, file size, and date instead of the requested columns

## Problem

1. **Backend**: The `RequestAttachmentVM` doesn't include the `Description` field (even though the entity has it)
2. **Frontend**: The component uses `mat-list` instead of `mat-table`, and doesn't display the description column

## Solution

### Backend Changes

Add the `Description` field to the view model and mapping logic.

### Frontend Changes

Replace `mat-list` with `mat-table` to display three columns with proper Arabic RTL support.

---

## Implementation Steps

### Step 1: Update Backend View Model

**File**: `src/Backend/BOG.VM/RequestAttachment/RequestAttachmentVM.cs`

**Add property after line 62** (after DownloadUrl):

```csharp
/// <summary>
/// Attachment description/notes (optional)
/// </summary>
public string? Description { get; set; }
```

**Location**: Add as the last property in the class before the closing brace.

---

### Step 2: Update Backend Mapping Logic

**File**: `src/Backend/BOG.BL/Services/CaseRegistration/RequestAttachmentBL.cs`

**Update `MapToViewModel` method** (line 313-328):

Add the Description mapping after line 326 (after ModifiedDate):

```csharp
private static RequestAttachmentVM MapToViewModel(RequestAttachment attachment)
{
    return new RequestAttachmentVM
    {
        Id = attachment.Id,
        RequestId = attachment.CaseRegistrationRequestId,
        AttachmentTypeId = attachment.AttachmentTypeId,
        AttachmentTypeName = attachment.AttachmentType?.NameAr ?? "",
        FileName = attachment.FileName ?? "",
        FileSize = (int)attachment.FileSizeBytes,
        FileSizeKb = Math.Round(attachment.FileSizeBytes / 1024.0, 2),
        IsMandatory = attachment.AttachmentType?.IsMandatory ?? false,
        CreatedDate = attachment.CreatedDate,
        ModifiedDate = attachment.ModifiedDate,
        Description = attachment.Description  // ADD THIS LINE
    };
}
```

---

### Step 3: Update Frontend TypeScript Interface

**File**: `src/Frontend/bog-app/src/app/features/case-registration/services/attachment-api.service.ts`

**Update `RequestAttachmentVM` interface** (lines 5-17):

Add `description` property:

```typescript
export interface RequestAttachmentVM {
  id: number;
  requestId: number;
  attachmentTypeId: number;
  attachmentTypeName: string;
  fileName: string;
  fileSize: number;
  fileSizeKb: number;
  isMandatory: boolean;
  createdDate: Date;
  modifiedDate: Date;
  downloadUrl?: string;
  description?: string;  // ADD THIS LINE
}
```

---

### Step 4: Update Frontend Component Template

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/attachments/attachments-list.component.ts`

**Replace the attachments list section** (lines 56-74) with a Material table:

```html
<!-- Attachments Table -->
<div class="attachments-table" *ngIf="attachments.length">
  <table mat-table [dataSource]="attachments" class="mat-elevation-z2">

    <!-- Attachment Type Column -->
    <ng-container matColumnDef="type">
      <th mat-header-cell *matHeaderCellDef>نوع المرفق</th>
      <td mat-cell *matCellDef="let attachment">
        {{ attachment.attachmentTypeName }}
        <span *ngIf="attachment.isMandatory" class="mandatory-badge"> *</span>
      </td>
    </ng-container>

    <!-- Notes/Description Column -->
    <ng-container matColumnDef="notes">
      <th mat-header-cell *matHeaderCellDef>ملاحظات</th>
      <td mat-cell *matCellDef="let attachment">
        {{ attachment.description || '-' }}
      </td>
    </ng-container>

    <!-- Attachment Download Column -->
    <ng-container matColumnDef="attachment">
      <th mat-header-cell *matHeaderCellDef>المرفق</th>
      <td mat-cell *matCellDef="let attachment">
        <button mat-button color="primary" (click)="downloadAttachment(attachment)">
          <mat-icon>download</mat-icon>
          {{ attachment.fileName }}
        </button>
        <button mat-icon-button color="warn"
                (click)="deleteAttachment(attachment)"
                *ngIf="canEdit"
                matTooltip="حذف">
          <mat-icon>delete</mat-icon>
        </button>
      </td>
    </ng-container>

    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
    <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
  </table>
</div>
```

---

### Step 5: Update Component TypeScript

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/attachments/attachments-list.component.ts`

**Add displayedColumns property** (after line 115):

```typescript
displayedColumns: string[] = ['type', 'notes', 'attachment'];
```

---

### Step 6: Update Component Styles

**File**: `src/Frontend/bog-app/src/app/features/case-registration/components/attachments/attachments-list.component.ts`

**Replace styles section** (lines 84-102) with:

```css
.attachments-section { padding: 16px 0; }
.upload-section { margin: 16px 0; }
.full-width { width: 100%; margin-bottom: 16px; }
.help-text { display: block; margin-top: 8px; color: #666; }
.mandatory-badge { color: red; font-weight: bold; margin-right: 4px; }
.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: #999;
}
.empty-state mat-icon {
  font-size: 48px;
  width: 48px;
  height: 48px;
  margin-bottom: 16px;
}
.attachments-table {
  margin-top: 20px;
  overflow-x: auto;
}
.attachments-table table {
  width: 100%;
}
.attachments-table th {
  background-color: #f5f5f5;
  font-weight: 600;
  text-align: right;
  padding: 12px 16px;
}
.attachments-table td {
  text-align: right;
  padding: 12px 16px;
  border-bottom: 1px solid #e0e0e0;
}
```

---

### Step 7: Ensure Material Table Module Import

**File**: `src/Frontend/bog-app/src/app/features/case-registration/case-registration.module.ts`

Verify that `MatTableModule` is imported. If not, add it:

```typescript
import { MatTableModule } from '@angular/material/table';

@NgModule({
  imports: [
    // ... other imports
    MatTableModule,
    // ...
  ]
})
```

---

## Files to Modify

### Backend (2 files)

1. **`src/Backend/BOG.VM/RequestAttachment/RequestAttachmentVM.cs`**
   - Add `Description` property

2. **`src/Backend/BOG.BL/Services/CaseRegistration/RequestAttachmentBL.cs`**
   - Update `MapToViewModel` to include Description mapping (line 327)

### Frontend (2-3 files)

3. **`src/Frontend/bog-app/src/app/features/case-registration/services/attachment-api.service.ts`**
   - Add `description?: string` to `RequestAttachmentVM` interface

4. **`src/Frontend/bog-app/src/app/features/case-registration/components/attachments/attachments-list.component.ts`**
   - Replace `mat-list` with `mat-table`
   - Add `displayedColumns` property
   - Update styles for table layout

5. **`src/Frontend/bog-app/src/app/features/case-registration/case-registration.module.ts`** (verify only)
   - Ensure `MatTableModule` is imported

---

## Testing Plan

### Backend Testing

1. **Test API returns description field**:
   ```bash
   curl http://localhost:5002/api/case-requests/{requestId}/attachments
   ```
   Verify response includes `"description": "..."` for each attachment

2. **Test description persists after upload**:
   - Upload attachment with description via API
   - Retrieve attachments
   - Verify description is returned

### Frontend Testing

1. **Upload with Description**:
   - Open a draft case request
   - Go to Attachments tab
   - Select attachment type
   - Enter description: "هذا مستند تجريبي"
   - Upload PDF file
   - Verify success message

2. **Verify Table Display**:
   - Check table has 3 columns with Arabic headers:
     - نوع المرفق
     - ملاحظات
     - المرفق
   - Verify uploaded attachment appears in table
   - Verify attachment type shows correctly
   - Verify description shows in Notes column
   - Verify download button works
   - Verify filename is clickable/downloadable

3. **Test Edge Cases**:
   - Upload attachment WITHOUT description → Notes column should show "-"
   - Upload multiple attachments → All show in table rows
   - Delete attachment (if canEdit=true) → Row removed from table
   - Empty state → Shows "لا توجد مرفقات" message

4. **RTL Layout Verification**:
   - Table text alignment is right-to-left
   - Column headers aligned right
   - Delete button positioned correctly

### Browser Testing

Test in:
- Chrome (latest)
- Edge (latest)
- Firefox (latest)

---

## Success Criteria

### Backend
- ✅ `RequestAttachmentVM` includes `Description` property
- ✅ API returns description field in GET responses
- ✅ Description persists when uploaded
- ✅ Solution builds without errors

### Frontend
- ✅ Table displays with 3 columns
- ✅ Column headers in Arabic: نوع المرفق, ملاحظات, المرفق
- ✅ Attachment type shows correctly (from lookup)
- ✅ Notes/description displays (or "-" if empty)
- ✅ Download link works with filename
- ✅ Delete button works (when canEdit=true)
- ✅ Empty state displays when no attachments
- ✅ RTL layout works correctly
- ✅ No console errors
- ✅ Upload form still works as before

---

## Rollback Plan

If issues occur:
1. Revert frontend changes first (restore `mat-list`)
2. Keep backend changes (Description field is backward compatible)
3. Frontend will ignore the description field if not used

---

## Notes

- The backend entity `RequestAttachment` already has the `Description` field - we're just exposing it in the VM
- Material Table is already part of Angular Material, just need to ensure module import
- The upload form doesn't change - only the display of existing attachments
- RTL support is automatic with Material's directionality system
- The download functionality remains unchanged
