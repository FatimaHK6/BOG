import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DefendantService } from '../../../../../core/services/defendant.service';
import { LookupService } from '../../../../../core/services/lookup.service';
import { Region, City, LicenseSource } from '../../../../../core/models/lookup.model';

@Component({
  selector: 'app-defendant-ngo-form',
  templateUrl: './defendant-ngo-form.component.html',
  styleUrls: ['./defendant-ngo-form.component.scss']
})
export class DefendantNGOFormComponent implements OnInit {
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

  // Active tab (0 = بيانات الجمعية/المؤسسة, 1 = العنوان الوطني)
  activeTab = 0;

  // Lookups
  licenseSources: LicenseSource[] = [];
  regions: Region[] = [];
  cities: City[] = [];

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
      // Tab 1: بيانات الجمعية/المؤسسة
      licenseNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      licenseSourceId: [null, Validators.required],
      ngoName: ['', [Validators.required, Validators.maxLength(200)]],
      licenseDate: [null, Validators.required],
      // Tab 2: العنوان الوطني (all required)
      ngoRegionId: [null, Validators.required],
      ngoCityId: [null, Validators.required],
      ngoDistrict: ['', [Validators.required, Validators.maxLength(100)]],
      ngoStreet: ['', [Validators.required, Validators.maxLength(200)]],
      ngoBuildingNumber: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]],
      ngoUnitNumber: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      ngoPostalCode: ['', [Validators.required, Validators.pattern('^[0-9]{5}$')]],
      ngoAdditionalCode: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]]
    });
  }

  private loadLookups(): void {
    this.isLoading = true;

    // Load license sources
    this.lookupService.getLicenseSources().subscribe({
      next: (sources) => {
        this.licenseSources = sources;
      }
    });

    // Load regions
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
        if (defendant.ngoRegionId) {
          this.loadCities(defendant.ngoRegionId);
        }

        this.form.patchValue({
          licenseNumber: defendant.licenseNumber,
          licenseSourceId: defendant.licenseSourceId,
          ngoName: defendant.ngoName,
          licenseDate: defendant.licenseDate ? new Date(defendant.licenseDate + '') : null,
          ngoRegionId: defendant.ngoRegionId,
          ngoCityId: defendant.ngoCityId,
          ngoDistrict: defendant.ngoDistrict,
          ngoStreet: defendant.ngoStreet,
          ngoBuildingNumber: defendant.ngoBuildingNumber,
          ngoUnitNumber: defendant.ngoUnitNumber,
          ngoPostalCode: defendant.ngoPostalCode,
          ngoAdditionalCode: defendant.ngoAdditionalCode
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
    this.form.patchValue({ ngoCityId: null });
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

    // Convert date to DateOnly format (YYYY-MM-DD)
    const licenseDate = this.form.value.licenseDate;

    const formatDate = (date: any): string | undefined => {
      if (!date) return undefined;
      const d = new Date(date);
      return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    };

    const dto = {
      defendantTypeId: 6, // NGO/Society (جمعية/مؤسسة أهلية)
      licenseNumber: this.form.value.licenseNumber,
      licenseSourceId: this.form.value.licenseSourceId,
      ngoName: this.form.value.ngoName,
      licenseDate: formatDate(licenseDate),
      ngoRegionId: this.form.value.ngoRegionId,
      ngoCityId: this.form.value.ngoCityId,
      ngoDistrict: this.form.value.ngoDistrict,
      ngoStreet: this.form.value.ngoStreet,
      ngoBuildingNumber: this.form.value.ngoBuildingNumber,
      ngoUnitNumber: this.form.value.ngoUnitNumber,
      ngoPostalCode: this.form.value.ngoPostalCode,
      ngoAdditionalCode: this.form.value.ngoAdditionalCode
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
    const tab1Fields = ['licenseNumber', 'licenseSourceId', 'ngoName', 'licenseDate'];
    return tab1Fields.some(field => {
      const control = this.form.get(field);
      return control?.invalid && (control?.touched || this.submitted);
    });
  }

  // Check if Tab 2 has validation errors
  hasTab2Errors(): boolean {
    const tab2Fields = ['ngoRegionId', 'ngoCityId', 'ngoDistrict', 'ngoStreet',
                        'ngoBuildingNumber', 'ngoUnitNumber', 'ngoPostalCode', 'ngoAdditionalCode'];
    return tab2Fields.some(field => {
      const control = this.form.get(field);
      return control?.invalid && (control?.touched || this.submitted);
    });
  }

  // Helper for form field access
  get f() { return this.form.controls; }
}






