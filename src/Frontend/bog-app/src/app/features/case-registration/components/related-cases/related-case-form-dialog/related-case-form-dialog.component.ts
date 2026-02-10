import { Component, Inject, OnInit } from '@angular/core';
import { UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RelatedCaseVM, RelatedCaseDTO, CourtLookup } from '../../../models/related-case.model';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { LookupsApiService } from '../../../services/lookups-api.service';

@Component({
  selector: 'app-related-case-form-dialog',
  templateUrl: './related-case-form-dialog.component.html',
  styleUrls: ['./related-case-form-dialog.component.scss']
})
export class RelatedCaseFormDialogComponent implements OnInit {
  relatedCaseForm!: UntypedFormGroup;
  mode: 'create' | 'edit' | 'view';
  requestId: number;
  relatedCase?: RelatedCaseVM;
  saving = false;
  serverError: string | null = null;
  courts: CourtLookup[] = [];
  loadingCourts = false;

  constructor(
    private fb: UntypedFormBuilder,
    private caseDataState: CaseDataStateService,
    private lookupsApi: LookupsApiService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<RelatedCaseFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.mode = data.mode;
    this.requestId = data.requestId;
    this.relatedCase = data.relatedCase;
  }

  ngOnInit() {
    this.buildForm();
    this.loadCourts();
  }

  loadCourts() {
    this.loadingCourts = true;
    this.lookupsApi.getCourts().subscribe({
      next: (courts) => {
        this.courts = courts;
        this.loadingCourts = false;
      },
      error: (error) => {
        this.loadingCourts = false;
        this.snackBar.open('خطأ في تحميل المحاكم', 'إغلاق', { duration: 3000 });
        console.error('Error loading courts:', error);
      }
    });
  }

  buildForm() {
    const courtId = this.relatedCase?.courtId || null;
    const caseNumber = this.relatedCase?.caseNumber || '';
    const caseYear = this.relatedCase?.caseYear || '';

    this.relatedCaseForm = this.fb.group({
      courtId: [courtId],
      caseNumber: [caseNumber, [Validators.required, Validators.pattern(/^\d+$/)]],
      caseYear: [caseYear, [Validators.required, Validators.min(1900), Validators.max(2100)]]
    });

    if (this.mode === 'view') {
      this.relatedCaseForm.disable();
    }
  }

  getCourtName(courtId: number): string {
    const court = this.courts.find(c => c.id === courtId);
    return court ? court.nameAr : '';
  }

  onSave() {
    if (!this.relatedCaseForm.valid) {
      this.snackBar.open('يرجى ملء جميع الحقول المطلوبة', 'إغلاق', { duration: 3000 });
      return;
    }

    this.saving = true;
    this.serverError = null;

    const courtId = this.relatedCaseForm.get('courtId')!.value;
    const caseNumber = parseInt(this.relatedCaseForm.get('caseNumber')!.value);
    const caseYear = parseInt(this.relatedCaseForm.get('caseYear')!.value);

    // Get current related cases from state
    const currentCases = this.caseDataState.getRelatedCases();

    if (this.mode === 'create') {
      // Create new related case with default values
      const newCase: RelatedCaseVM = {
        id: Math.max(...currentCases.map(c => c.id), 0) + 1,
        caseRegistrationRequestId: this.requestId,
        courtId,
        courtName: this.getCourtName(courtId),
        caseNumber,
        caseYear,
        createdDate: new Date(),
        modifiedDate: new Date()
      };

      // Update state
      this.caseDataState.updateRelatedCases([...currentCases, newCase]);
      this.snackBar.open('تم إضافة الدعوى المرتبطة بنجاح', 'إغلاق', { duration: 3000 });
      this.dialogRef.close(true);
    } else if (this.mode === 'edit' && this.relatedCase) {
      // Update existing related case
      const updatedCases = currentCases.map(c =>
        c.id === this.relatedCase!.id
          ? {
              ...c,
              courtId,
              courtName: this.getCourtName(courtId),
              caseNumber,
              caseYear,
              modifiedDate: new Date()
            }
          : c
      );

      // Update state
      this.caseDataState.updateRelatedCases(updatedCases);
      this.snackBar.open('تم تحديث الدعوى المرتبطة بنجاح', 'إغلاق', { duration: 3000 });
      this.dialogRef.close(true);
    }

    this.saving = false;
  }

  onCancel() {
    this.dialogRef.close();
  }
}
