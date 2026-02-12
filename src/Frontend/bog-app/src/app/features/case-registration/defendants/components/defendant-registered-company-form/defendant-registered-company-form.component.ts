import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DefendantService } from '../../../../../core/services/defendant.service';
import { LookupService } from '../../../../../core/services/lookup.service';
import { Region, City } from '../../../../../core/models/lookup.model';

@Component({
  selector: 'app-defendant-registered-company-form',
  templateUrl: './defendant-registered-company-form.component.html',
  styleUrls: ['./defendant-registered-company-form.component.scss']
})
export class DefendantRegisteredCompanyFormComponent implements OnInit {
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

  // Active tab (0 = بيانات السجل التجاري, 1 = العنوان الوطني)
  activeTab = 0;

  // Lookups
  regions: Region[] = [];
  cities: City[] = [];

  // Max date for registration start (today)
  maxDate = new Date();

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
      // Tab 1: بيانات السجل التجاري
      commercialRegNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      companyName: ['', [Validators.required, Validators.maxLength(200)]],
      registrationStartDate: [null, Validators.required],
      registrationEndDate: [null, Validators.required],
      // Tab 2: العنوان الوطني (all required)
      regCompanyRegionId: [null, Validators.required],
      regCompanyCityId: [null, Validators.required],
      regCompanyDistrict: ['', [Validators.required, Validators.maxLength(100)]],
      regCompanyStreet: ['', [Validators.required, Validators.maxLength(200)]],
      regCompanyBuildingNumber: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]],
      regCompanyUnitNumber: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      regCompanyPostalCode: ['', [Validators.required, Validators.pattern('^[0-9]{5}$')]],
      regCompanyAdditionalCode: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]]
    });
  }

  private loadLookups(): void {
    this.isLoading = true;
    this.lookupService.getRegions().subscribe({
      next: (regions) => {
        this.regions = regions;
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
        // Load cities first if region is set
        if (defendant.regCompanyRegionId) {
          this.loadCities(defendant.regCompanyRegionId);
        }

        this.form.patchValue({
          commercialRegNumber: defendant.commercialRegNumber,
          companyName: defendant.companyName,
          registrationStartDate: defendant.registrationStartDate ? new Date(defendant.registrationStartDate + '') : null,
          registrationEndDate: defendant.registrationEndDate ? new Date(defendant.registrationEndDate + '') : null,
          regCompanyRegionId: defendant.regCompanyRegionId,
          regCompanyCityId: defendant.regCompanyCityId,
          regCompanyDistrict: defendant.regCompanyDistrict,
          regCompanyStreet: defendant.regCompanyStreet,
          regCompanyBuildingNumber: defendant.regCompanyBuildingNumber,
          regCompanyUnitNumber: defendant.regCompanyUnitNumber,
          regCompanyPostalCode: defendant.regCompanyPostalCode,
          regCompanyAdditionalCode: defendant.regCompanyAdditionalCode
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

  onRegionChange(regionId: number): void {
    this.form.patchValue({ regCompanyCityId: null });
    this.cities = [];
    if (regionId) {
      this.loadCities(regionId);
    }
  }

  private loadCities(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe({
      next: (cities) => {
        this.cities = cities;
      }
    });
  }

  setActiveTab(tabIndex: number): void {
    this.activeTab = tabIndex;
  }

  onSubmit(): void {
    this.submitted = true;
    this.form.markAllAsTouched();

    if (this.form.invalid || this.requestId <= 0) {
      // Switch to tab with errors
      if (this.hasTab1Errors()) {
        this.activeTab = 0;
      } else if (this.hasTab2Errors()) {
        this.activeTab = 1;
      }
      return;
    }

    this.isSaving = true;

    // Convert dates to DateOnly format (YYYY-MM-DD)
    const startDate = this.form.value.registrationStartDate;
    const endDate = this.form.value.registrationEndDate;

    const formatDate = (date: any): string | undefined => {
      if (!date) return undefined;
      const d = new Date(date);
      return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    };

    const dto = {
      defendantTypeId: 2, // Registered Company (شركة مسجلة)
      commercialRegNumber: this.form.value.commercialRegNumber,
      companyName: this.form.value.companyName,
      registrationStartDate: formatDate(startDate),
      registrationEndDate: formatDate(endDate),
      regCompanyRegionId: this.form.value.regCompanyRegionId,
      regCompanyCityId: this.form.value.regCompanyCityId,
      regCompanyDistrict: this.form.value.regCompanyDistrict,
      regCompanyStreet: this.form.value.regCompanyStreet,
      regCompanyBuildingNumber: this.form.value.regCompanyBuildingNumber,
      regCompanyUnitNumber: this.form.value.regCompanyUnitNumber,
      regCompanyPostalCode: this.form.value.regCompanyPostalCode,
      regCompanyAdditionalCode: this.form.value.regCompanyAdditionalCode
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

  // Check if Tab 1 has validation errors
  hasTab1Errors(): boolean {
    const tab1Fields = ['commercialRegNumber', 'companyName', 'registrationStartDate', 'registrationEndDate'];
    return tab1Fields.some(field => {
      const control = this.form.get(field);
      return control?.invalid && (control?.touched || this.submitted);
    });
  }

  // Check if Tab 2 has validation errors
  hasTab2Errors(): boolean {
    const tab2Fields = ['regCompanyRegionId', 'regCompanyCityId', 'regCompanyDistrict', 'regCompanyStreet',
                        'regCompanyBuildingNumber', 'regCompanyUnitNumber', 'regCompanyPostalCode', 'regCompanyAdditionalCode'];
    return tab2Fields.some(field => {
      const control = this.form.get(field);
      return control?.invalid && (control?.touched || this.submitted);
    });
  }

  // Helper for form field access
  get f() { return this.form.controls; }
}





