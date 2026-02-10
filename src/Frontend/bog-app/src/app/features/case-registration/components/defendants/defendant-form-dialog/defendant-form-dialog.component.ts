import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DefendantVM, DefendantCreateDTO, DefendantUpdateDTO } from '../../../models/defendant.model';
import { DefendantApiService } from '../../../services/defendant-api.service';

@Component({
  selector: 'app-defendant-form-dialog',
  templateUrl: './defendant-form-dialog.component.html',
  styleUrls: ['./defendant-form-dialog.component.scss']
})
export class DefendantFormDialogComponent implements OnInit {
  defendantForm!: FormGroup;
  mode: 'create' | 'edit' | 'view';
  requestId: number;
  defendant?: DefendantVM;
  saving = false;
  serverError: string | null = null;

  showIdentityFields = true;
  isCompanyType = false;
  isGovernmentType = false;

  get fullNameLength() {
    return this.defendantForm.get('fullName')?.value?.length || 0;
  }

  get addressLength() {
    return this.defendantForm.get('addressText')?.value?.length || 0;
  }

  get statementLength() {
    return this.defendantForm.get('additionalStatement')?.value?.length || 0;
  }

  constructor(
    private fb: FormBuilder,
    private defendantApi: DefendantApiService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<DefendantFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.mode = data.mode;
    this.requestId = data.requestId;
    this.defendant = data.defendant;
  }

  ngOnInit() {
    this.buildForm();

    this.defendantForm.get('defendantTypeId')?.valueChanges.subscribe(() => {
      this.onDefendantTypeChange();
    });

    this.onDefendantTypeChange();

    if (this.mode === 'view') {
      this.defendantForm.disable();
    }
  }

  buildForm() {
    this.defendantForm = this.fb.group({
      defendantTypeId: [this.defendant?.defendantTypeId || 1, Validators.required],
      fullName: [
        this.defendant?.fullName || '',
        [Validators.required, Validators.maxLength(200)]
      ],
      identityTypeId: [this.defendant?.identityTypeId || null],
      identityNumber: [this.defendant?.identityNumber || '', Validators.maxLength(50)],
      commercialRegNumber: ['', Validators.maxLength(50)],
      governmentAgencyId: [null],
      addressText: [this.defendant?.addressText || '', Validators.maxLength(2000)],
      additionalStatement: ['', Validators.maxLength(4000)]
    });
  }

  onDefendantTypeChange() {
    const typeId = this.defendantForm.get('defendantTypeId')?.value;

    this.isCompanyType = typeId === 2;
    this.isGovernmentType = typeId === 3;
    this.showIdentityFields = [1, 5].includes(typeId);
  }

  onSave() {
    if (this.defendantForm.invalid) {
      this.defendantForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.serverError = null;

    const dto: DefendantCreateDTO = this.defendantForm.value;

    const request$ = this.mode === 'create'
      ? this.defendantApi.create(this.requestId, dto)
      : this.defendantApi.update(this.defendant!.id, dto);

    request$.subscribe({
      next: (result) => {
        this.snackBar.open(
          this.mode === 'create' ? 'تم إضافة المدعى عليه بنجاح' : 'تم تعديل المدعى عليه بنجاح',
          'إغلاق',
          { duration: 3000 }
        );
        this.dialogRef.close(result);
      },
      error: (error) => {
        this.saving = false;

        if (error.error?.errorCode === 'ERR013' || error.error?.message?.includes('ERR013')) {
          this.serverError = 'ERR013: المدعى عليه موجود مسبقاً بنفس رقم الهوية';
        } else {
          this.serverError = error.error?.message || 'حدث خطأ أثناء الحفظ';
        }
      }
    });
  }

  onCancel() {
    this.dialogRef.close();
  }
}
