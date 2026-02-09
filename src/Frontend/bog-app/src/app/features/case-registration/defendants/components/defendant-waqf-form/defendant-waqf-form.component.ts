import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DefendantService } from '../../../../../core/services/defendant.service';
import { LookupService } from '../../../../../core/services/lookup.service';
import { Region, City } from '../../../../../core/models/lookup.model';

interface WaqfSupervisoryType {
  id: number;
  nameAr: string;
}

@Component({
  selector: 'app-defendant-waqf-form',
  templateUrl: './defendant-waqf-form.component.html',
  styleUrls: ['./defendant-waqf-form.component.scss']
})
export class DefendantWaqfFormComponent implements OnInit {
  form!: FormGroup;
  requestId: number = 0;
  defendantId: number = 0;
  isEditMode = false;
  isViewMode = false;
  isLoading = false;
  isSaving = false;
  submitted = false;

  // Active tab (0 = بيانات الوقف, 1 = العنوان الوطني)
  activeTab = 0;

  // Lookups
  regions: Region[] = [];
  cities: City[] = [];
  waqfSupervisoryTypes: WaqfSupervisoryType[] = [
    { id: 1, nameAr: 'أهلية' },
    { id: 2, nameAr: 'حكومية' }
  ];

  // Max date for court deed (today)
  maxDate = new Date();

  constructor(
    private fb: FormBuilder,
    private defendantService: DefendantService,
    private lookupService: LookupService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    // Get requestId from query params
    this.route.queryParams.subscribe(params => {
      this.requestId = params['requestId'] ? +params['requestId'] : 0;
    });

    // Check if edit or view mode (id in route params)
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.defendantId = +params['id'];
        // Check if view mode by URL path
        const url = this.router.url;
        this.isViewMode = url.includes('/view/');
        this.isEditMode = !this.isViewMode;
      }
    });

    this.initForm();
    this.loadLookups();
  }

  private initForm(): void {
    this.form = this.fb.group({
      // Tab 1: بيانات الوقف
      waqfName: ['', [Validators.required, Validators.maxLength(200)]],
      courtDeedNumber: ['', [Validators.required, Validators.maxLength(10)]],
      courtDeedDate: [null, Validators.required],
      deedSource: ['', [Validators.required, Validators.maxLength(100)]],
      waqfSupervisoryTypeId: [null, Validators.required],
      waqfAgencyName: ['', Validators.maxLength(200)],
      // Tab 2: العنوان الوطني
      waqfRegionId: [null, Validators.required],
      waqfCityId: [null, Validators.required],
      waqfDistrict: ['', Validators.maxLength(100)],
      waqfStreet: ['', Validators.maxLength(200)],
      waqfBuildingNumber: ['', Validators.pattern('^[0-9]{4}$')],
      waqfUnitNumber: ['', Validators.pattern('^[0-9]*$')],
      waqfPostalCode: ['', Validators.pattern('^[0-9]{5}$')],
      waqfAdditionalCode: ['', Validators.pattern('^[0-9]{4}$')],
      waqfAddressDescription: ['', Validators.required]
    });

    // Watch for supervisory type changes
    this.form.get('waqfSupervisoryTypeId')?.valueChanges.subscribe(value => {
      const agencyNameControl = this.form.get('waqfAgencyName');
      if (value === 2) { // حكومية
        agencyNameControl?.setValidators([Validators.required, Validators.maxLength(200)]);
      } else {
        agencyNameControl?.clearValidators();
        agencyNameControl?.setValue('');
      }
      agencyNameControl?.updateValueAndValidity();
    });
  }

  private loadLookups(): void {
    this.isLoading = true;
    this.lookupService.getRegions().subscribe({
      next: (regions) => {
        this.regions = regions;
        // Load defendant data if edit or view mode
        if ((this.isEditMode || this.isViewMode) && this.defendantId > 0) {
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
    this.defendantService.getDefendant(this.defendantId).subscribe({
      next: (defendant) => {
        // Load cities first if region is set
        if (defendant.waqfRegionId) {
          this.loadCities(defendant.waqfRegionId);
        }

        this.form.patchValue({
          waqfName: defendant.waqfName,
          courtDeedNumber: defendant.courtDeedNumber,
          courtDeedDate: defendant.courtDeedDate ? new Date(defendant.courtDeedDate + '') : null,
          deedSource: defendant.deedSource,
          waqfSupervisoryTypeId: defendant.waqfSupervisoryTypeId,
          waqfAgencyName: defendant.waqfAgencyName,
          waqfRegionId: defendant.waqfRegionId,
          waqfCityId: defendant.waqfCityId,
          waqfDistrict: defendant.waqfDistrict,
          waqfStreet: defendant.waqfStreet,
          waqfBuildingNumber: defendant.waqfBuildingNumber,
          waqfUnitNumber: defendant.waqfUnitNumber,
          waqfPostalCode: defendant.waqfPostalCode,
          waqfAdditionalCode: defendant.waqfAdditionalCode,
          waqfAddressDescription: defendant.waqfAddressDescription
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
    this.form.patchValue({ waqfCityId: null });
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
    const courtDeedDate = this.form.value.courtDeedDate;
    let formattedDate: string | undefined = undefined;
    if (courtDeedDate) {
      const d = new Date(courtDeedDate);
      formattedDate = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    }

    const dto = {
      defendantTypeId: 7, // Waqf (وقف)
      waqfName: this.form.value.waqfName,
      courtDeedNumber: this.form.value.courtDeedNumber,
      courtDeedDate: formattedDate,
      deedSource: this.form.value.deedSource,
      waqfSupervisoryTypeId: this.form.value.waqfSupervisoryTypeId,
      waqfAgencyName: this.form.value.waqfAgencyName || undefined,
      waqfRegionId: this.form.value.waqfRegionId,
      waqfCityId: this.form.value.waqfCityId,
      waqfDistrict: this.form.value.waqfDistrict || undefined,
      waqfStreet: this.form.value.waqfStreet || undefined,
      waqfBuildingNumber: this.form.value.waqfBuildingNumber || undefined,
      waqfUnitNumber: this.form.value.waqfUnitNumber || undefined,
      waqfPostalCode: this.form.value.waqfPostalCode || undefined,
      waqfAdditionalCode: this.form.value.waqfAdditionalCode || undefined,
      waqfAddressDescription: this.form.value.waqfAddressDescription
    };

    const request$ = this.isEditMode
      ? this.defendantService.updateDefendant(this.defendantId, dto)
      : this.defendantService.createDefendant(this.requestId, dto);

    request$.subscribe({
      next: () => {
        this.isSaving = false;
        const message = this.isEditMode ? 'تم تعديل المدعى عليه بنجاح' : 'تم إنشاء المدعى عليه بنجاح';
        this.snackBar.open(message, 'إغلاق', { duration: 3000 });
        // Navigate back to defendants list
        this.router.navigate(['/case-registration/defendants'], {
          queryParams: { requestId: this.requestId }
        });
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
    this.router.navigate(['/case-registration/defendants'], {
      queryParams: { requestId: this.requestId }
    });
  }

  // Check if Tab 1 has validation errors
  hasTab1Errors(): boolean {
    const tab1Fields = ['waqfName', 'courtDeedNumber', 'courtDeedDate', 'deedSource', 'waqfSupervisoryTypeId', 'waqfAgencyName'];
    return tab1Fields.some(field => {
      const control = this.form.get(field);
      return control?.invalid && (control?.touched || this.submitted);
    });
  }

  // Check if Tab 2 has validation errors
  hasTab2Errors(): boolean {
    const tab2Fields = ['waqfRegionId', 'waqfCityId', 'waqfDistrict', 'waqfStreet', 'waqfBuildingNumber', 'waqfUnitNumber', 'waqfPostalCode', 'waqfAdditionalCode', 'waqfAddressDescription'];
    return tab2Fields.some(field => {
      const control = this.form.get(field);
      return control?.invalid && (control?.touched || this.submitted);
    });
  }

  // Show agency name field only when supervisory = حكومية (id=2)
  get showAgencyName(): boolean {
    return this.form.get('waqfSupervisoryTypeId')?.value === 2;
  }

  // Helper for form field access
  get f() { return this.form.controls; }
}
