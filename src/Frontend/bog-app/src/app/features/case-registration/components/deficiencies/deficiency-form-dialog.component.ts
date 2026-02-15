import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeficiencyTypeVM, DeficiencyDescriptionVM } from '../../models/deficiency.model';
import { LookupsApiService } from '../../services/lookups-api.service';
import { MatSnackBar } from '@angular/material/snack-bar';

export interface DeficiencyFormDialogData {
  mode: 'create' | 'edit' | 'view';
  deficiency?: any;
  preSelectedTypeId?: number;
}

@Component({
  selector: 'app-deficiency-form-dialog',
  templateUrl: './deficiency-form-dialog.component.html',
  styleUrls: ['./deficiency-form-dialog.component.scss']
})
export class DeficiencyFormDialogComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  deficiencyTypes: DeficiencyTypeVM[] = [];
  deficiencyDescriptions: DeficiencyDescriptionVM[] = [];
  filteredDescriptions: DeficiencyDescriptionVM[] = [];
  loading = false;
  isViewMode = false;

  private destroy$ = new Subject<void>();

  constructor(
    public dialogRef: MatDialogRef<DeficiencyFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DeficiencyFormDialogData,
    private fb: FormBuilder,
    private lookupsApi: LookupsApiService,
    private snackBar: MatSnackBar
  ) {
    this.isViewMode = data?.mode === 'view';
  }

  ngOnInit() {
    this.initializeForm();
    this.loadDeficiencyTypes();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForm() {
    this.form = this.fb.group({
      deficiencyTypeId: ['', Validators.required],
      deficiencyDescriptionId: ['', Validators.required]
    });

    // Pre-select type if provided (for contextual buttons)
    if (this.data?.preSelectedTypeId) {
      this.form.get('deficiencyTypeId')?.setValue(this.data.preSelectedTypeId);
    }

    // Listen to type changes to filter descriptions
    this.form.get('deficiencyTypeId')?.valueChanges.subscribe(typeId => {
      this.onTypeChanged(typeId);
    });

    if (this.isViewMode) {
      this.form.disable();
    }
  }

  private loadDeficiencyTypes() {
    this.loading = true;
    this.lookupsApi.getDeficiencyTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (types) => {
          this.deficiencyTypes = types;
          this.loadDeficiencyDescriptions();
        },
        error: (error) => {
          console.error('Error loading deficiency types:', error);
          this.loading = false;
          this.snackBar.open('فشل في تحميل أنواع النواقص', 'إغلاق', { duration: 3000 });
        }
      });
  }

  private loadDeficiencyDescriptions() {
    this.lookupsApi.getDeficiencyDescriptions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (descriptions) => {
          this.deficiencyDescriptions = descriptions;
          this.loading = false;
          // If type was pre-selected, filter descriptions immediately
          if (this.data?.preSelectedTypeId) {
            this.onTypeChanged(this.data.preSelectedTypeId);
          }
        },
        error: (error) => {
          console.error('Error loading deficiency descriptions:', error);
          this.loading = false;
          this.snackBar.open('فشل في تحميل أوصاف النواقص', 'إغلاق', { duration: 3000 });
        }
      });
  }

  private onTypeChanged(typeId: number) {
    if (!typeId) {
      this.filteredDescriptions = [];
      this.form.get('deficiencyDescriptionId')?.setValue('');
      return;
    }

    // Filter descriptions by selected type
    this.filteredDescriptions = this.deficiencyDescriptions.filter(
      d => d.deficiencyTypeId === typeId
    );

    // Reset description selection
    this.form.get('deficiencyDescriptionId')?.setValue('');
  }

  cancel() {
    this.dialogRef.close(null);
  }

  save() {
    if (!this.form.valid) {
      this.snackBar.open('يرجى تعبئة جميع الحقول المطلوبة', 'إغلاق', { duration: 3000 });
      return;
    }

    const result = {
      deficiencyDescriptionId: this.form.value.deficiencyDescriptionId
    };

    this.dialogRef.close(result);
  }

  get dialogTitle(): string {
    if (this.data?.mode === 'view') return 'عرض النقص';
    if (this.data?.mode === 'edit') return 'تعديل النقص';
    return 'إضافة نقص';
  }

  get saveButtonText(): string {
    return this.data?.mode === 'edit' ? 'تحديث' : 'إضافة';
  }
}
