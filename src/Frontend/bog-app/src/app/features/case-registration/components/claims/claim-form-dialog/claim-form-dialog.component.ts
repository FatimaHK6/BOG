import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Editor, Toolbar } from 'ngx-editor';
import { ClaimVM, ClaimDTO } from '../../../models/claim.model';
import { CaseDataStateService } from '../../../services/case-data-state.service';

@Component({
  selector: 'app-claim-form-dialog',
  templateUrl: './claim-form-dialog.component.html',
  styleUrls: ['./claim-form-dialog.component.scss']
})
export class ClaimFormDialogComponent implements OnInit, OnDestroy {
  claimForm!: UntypedFormGroup;
  mode: 'create' | 'edit' | 'view';
  requestId: number;
  claim?: ClaimVM;
  saving = false;
  serverError: string | null = null;

  // ngx-editor
  editor!: Editor;
  toolbar: Toolbar = [
    ['bold', 'italic', 'underline', 'strike'],
    ['bullet_list', 'ordered_list'],
    [{ heading: ['h1', 'h2', 'h3', 'h4', 'h5', 'h6'] }],
    ['link'],
    ['text_color', 'background_color'],
    ['align_left', 'align_center', 'align_right', 'align_justify']
  ];

  readonly maxCharacters = 2000;

  get claimTextLength() {
    return this.claimForm.get('claimText')?.value?.length || 0;
  }

  get characterPercentage(): number {
    return Math.round((this.claimTextLength / this.maxCharacters) * 100);
  }

  constructor(
    private fb: UntypedFormBuilder,
    private caseDataState: CaseDataStateService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ClaimFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.mode = data.mode;
    this.requestId = data.requestId;
    this.claim = data.claim;
  }

  ngOnInit() {
    this.editor = new Editor();
    this.buildForm();
  }

  ngOnDestroy() {
    this.editor.destroy();
  }

  buildForm() {
    const claimText = this.claim?.claimText || '';
    this.claimForm = this.fb.group({
      claimText: [claimText, [
        Validators.required,
        Validators.maxLength(this.maxCharacters)
      ]]
    });

    if (this.mode === 'view') {
      this.claimForm.disable();
    }
  }

  onTextChange() {
    // Force form validation update for character counter
    const control = this.claimForm.get('claimText');
    if (control) {
      control.updateValueAndValidity();
    }
  }

  onSave() {
    if (!this.claimForm.valid) {
      this.snackBar.open('يرجى ملء جميع الحقول المطلوبة', 'إغلاق', { duration: 3000 });
      return;
    }

    this.saving = true;
    this.serverError = null;

    const claimText = this.claimForm.get('claimText')!.value;

    // Get current claims from state
    const currentClaims = this.caseDataState.getClaims();

    if (this.mode === 'create') {
      // Create new claim with default values
      const newClaim: ClaimVM = {
        id: Math.max(...currentClaims.map(c => c.id), 0) + 1,
        caseRegistrationRequestId: this.requestId,
        claimText,
        createdDate: new Date(),
        modifiedDate: new Date()
      };

      // Update state
      this.caseDataState.updateClaims([...currentClaims, newClaim]);
      this.snackBar.open('تم إضافة طلب الدعوى بنجاح', 'إغلاق', { duration: 3000 });
      this.dialogRef.close(true);
    } else if (this.mode === 'edit' && this.claim) {
      // Update existing claim
      const updatedClaims = currentClaims.map(c =>
        c.id === this.claim!.id
          ? { ...c, claimText, modifiedDate: new Date() }
          : c
      );

      // Update state
      this.caseDataState.updateClaims(updatedClaims);
      this.snackBar.open('تم تحديث طلب الدعوى بنجاح', 'إغلاق', { duration: 3000 });
      this.dialogRef.close(true);
    }

    this.saving = false;
  }

  onCancel() {
    this.dialogRef.close();
  }
}
