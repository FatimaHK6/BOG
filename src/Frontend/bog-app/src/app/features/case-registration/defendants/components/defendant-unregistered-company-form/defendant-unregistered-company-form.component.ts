import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DefendantService } from '../../../../../core/services/defendant.service';
import { LookupService } from '../../../../../core/services/lookup.service';

interface Country {
  id: number;
  name: string;
  nameAr: string;
  isoCode: string;
}

@Component({
  selector: 'app-defendant-unregistered-company-form',
  templateUrl: './defendant-unregistered-company-form.component.html',
  styleUrls: ['./defendant-unregistered-company-form.component.scss']
})
export class DefendantUnregisteredCompanyFormComponent implements OnInit {
  form!: FormGroup;
  @Input() requestId: number = 0;
  @Input() defendantId: number | null = null;
  @Input() mode: 'add' | 'edit' | 'view' = 'add';

  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();
  isEditMode = false;
  isViewMode = false;
  isLoading = false;
  isSaving = false;
  submitted = false;

  countries: Country[] = [];

  constructor(
    private fb: FormBuilder,
    private defendantService: DefendantService,
    private lookupService: LookupService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.isViewMode = this.mode === 'view';
    this.isEditMode = this.mode === 'edit';
    this.initForm();
    this.loadLookups();
  }

  private initForm(): void {
    this.form = this.fb.group({
      commercialRegNumber: ['', [Validators.required, Validators.maxLength(20)]],
      companyName: ['', [Validators.required, Validators.maxLength(200)]],
      countryId: [null, Validators.required],
      city: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.maxLength(1000)]]
    });
  }

  private loadLookups(): void {
    this.isLoading = true;
    this.lookupService.getCountries().subscribe({
      next: (countries) => {
        this.countries = countries;
        // Load defendant data if edit or view mode
        if ((this.isEditMode || this.isViewMode) && this.defendantId && this.defendantId > 0) {
          this.loadDefendant();
        } else {
          this.isLoading = false;
        }
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  private loadDefendant(): void {
    this.defendantService.getDefendant(this.defendantId!).subscribe({
      next: (defendant) => {
        this.form.patchValue({
          commercialRegNumber: defendant.commercialRegNumber,
          companyName: defendant.companyName,
          countryId: defendant.countryId,
          city: defendant.city,
          description: defendant.description
        });
        // Disable form if view mode
        if (this.isViewMode) {
          this.form.disable();
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading defendant:', err);
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.form.markAllAsTouched();

    if (this.form.invalid || this.requestId <= 0) {
      return;
    }

    this.isSaving = true;
    const dto = {
      defendantTypeId: 4, // Unregistered Company (شركة غير مسجلة في المملكة)
      commercialRegNumber: this.form.value.commercialRegNumber,
      companyName: this.form.value.companyName,
      countryId: this.form.value.countryId,
      city: this.form.value.city,
      description: this.form.value.description
    };

    const request$ = this.isEditMode
      ? this.defendantService.updateDefendant(this.defendantId!, dto)
      : this.defendantService.createDefendant(this.requestId, dto);

    request$.subscribe({
      next: () => {
        this.isSaving = false;
        const message = this.isEditMode ? 'تم تعديل المدعى عليه بنجاح' : 'تم إنشاء المدعى عليه بنجاح';
        this.snackBar.open(message, 'إغلاق', { duration: 3000 });
        // Navigate back to defendants list
        this.saved.emit();
      },
      error: (err) => {
        console.error('Error saving defendant:', err);
        this.isSaving = false;
        const errorMessage = err.error?.message || err.error || 'حدث خطأ أثناء حفظ البيانات';
        this.snackBar.open(errorMessage, 'إغلاق', { duration: 5000 });
      }
    });
  }

  onCancel(): void {
    this.cancelled.emit();
  }

  // Helper for form field access
  get f() { return this.form.controls; }
}






