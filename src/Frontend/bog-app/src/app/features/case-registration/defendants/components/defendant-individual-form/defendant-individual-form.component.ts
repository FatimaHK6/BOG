import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DefendantService } from '../../../../../core/services/defendant.service';
import { LookupService } from '../../../../../core/services/lookup.service';
import { Region, City, IdentityType, Nationality } from '../../../../../core/models/lookup.model';

interface Gender {
  id: number;
  nameAr: string;
}

interface EmploymentStatus {
  id: number;
  nameAr: string;
}

@Component({
  selector: 'app-defendant-individual-form',
  templateUrl: './defendant-individual-form.component.html',
  styleUrls: ['./defendant-individual-form.component.scss']
})
export class DefendantIndividualFormComponent implements OnInit {
  // Input properties
  @Input() requestId: number = 0;
  @Input() defendantId: number | null = null;
  @Input() mode: 'add' | 'edit' | 'view' = 'add';

  // Output events
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  form!: FormGroup;
  isEditMode = false;
  isViewMode = false;
  isLoading = false;
  isSaving = false;
  submitted = false;

  // Active tab (0 = بيانات شخصية, 1 = بيانات مكان الإقامة, 2 = بيانات جهة العمل)
  activeTab = 0;

  // Lookups
  identityTypes: IdentityType[] = [];
  nationalities: Nationality[] = [];
  regions: Region[] = [];
  cities: City[] = [];
  workCities: City[] = [];
  genders: Gender[] = [
    { id: 1, nameAr: 'ذكر' },
    { id: 2, nameAr: 'أنثى' }
  ];
  employmentStatuses: EmploymentStatus[] = [
    { id: 1, nameAr: 'حكومي' },
    { id: 2, nameAr: 'خاص' },
    { id: 3, nameAr: 'بدون عمل' }
  ];

  // Identity type constants
  readonly IDENTITY_NATIONAL_ID = 1;
  readonly IDENTITY_RESIDENT_ID = 2;

  // Saudi nationality ID
  readonly SAUDI_NATIONALITY_ID = 1;

  // Today's date for max date validation
  today = new Date();

  // Absher integration
  isAbsherLocked = false;
  isFetchingAbsher = false;

  constructor(
    private fb: FormBuilder,
    private defendantService: DefendantService,
    private lookupService: LookupService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    // Set modes based on @Input property
    this.isViewMode = this.mode === 'view';
    this.isEditMode = this.mode === 'edit';

    this.initForm();
    this.loadLookups();
  }

  private initForm(): void {
    this.form = this.fb.group({
      // Tab 1: بيانات شخصية
      identityTypeId: [null],
      identityNumber: ['', [Validators.pattern('^[0-9]{10}$')]],
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      fatherName: ['', [Validators.maxLength(100)]], // Required only for National ID
      grandfatherName: ['', [Validators.maxLength(100)]],
      tribeName: ['', [Validators.maxLength(100)]],
      familyName: ['', [Validators.required, Validators.maxLength(100)]],
      birthDate: [null],
      genderId: [null, Validators.required],
      nationalityId: [null, Validators.required],
      mobileNumber: ['', [Validators.required, Validators.pattern('^05[0-9]{8}$')]],
      email: ['', [Validators.email]],
      // Tab 2: بيانات مكان الإقامة
      indRegionId: [null, Validators.required],
      indCityId: [null, Validators.required],
      indDistrict: ['', [Validators.maxLength(100)]],
      indStreet: ['', [Validators.maxLength(200)]],
      indBuildingNumber: ['', [Validators.pattern('^[0-9]{4}$')]],
      indUnitNumber: ['', [Validators.pattern('^[0-9]{4}$')]],
      indPostalCode: ['', [Validators.pattern('^[0-9]{5}$')]],
      indAdditionalCode: ['', [Validators.pattern('^[0-9]{4}$')]],
      // Tab 3: بيانات جهة العمل
      employmentStatusId: [null],
      employer: ['', [Validators.maxLength(200)]],
      occupation: ['', [Validators.maxLength(200)]],
      // عنوان العمل (يظهر فقط إذا حالة العمل = خاص) - validators added dynamically
      workRegionId: [null],
      workCityId: [null],
      workDistrict: ['', [Validators.maxLength(100)]],
      workStreet: ['', [Validators.maxLength(200)]],
      workBuildingNumber: ['', [Validators.pattern('^[0-9]{4}$')]],
      workUnitNumber: ['', [Validators.pattern('^[0-9]{4}$')]],
      workPostalCode: ['', [Validators.pattern('^[0-9]{5}$')]],
      workAdditionalCode: ['', [Validators.pattern('^[0-9]{4}$')]]
    });

    // Watch identity type changes for conditional validation
    this.form.get('identityTypeId')?.valueChanges.subscribe(value => {
      this.onIdentityTypeChange(value);
    });

    // Watch employment status changes for conditional validation
    this.form.get('employmentStatusId')?.valueChanges.subscribe(value => {
      this.onEmploymentStatusChange(value);
    });
  }

  private loadLookups(): void {
    this.isLoading = true;

    // Load identity types
    this.lookupService.getIdentityTypes().subscribe({
      next: (types) => {
        // Filter to only National ID and Resident ID
        this.identityTypes = types.filter(t => t.id === 1 || t.id === 2);
      }
    });

    // Load nationalities
    this.lookupService.getNationalities().subscribe({
      next: (nationalities) => {
        this.nationalities = nationalities;
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
      next: (defendant: any) => {
        // Load cities first if region is set
        if (defendant.indRegionId) {
          this.loadCities(defendant.indRegionId);
        }
        // Load work cities if work region is set
        if (defendant.workRegionId) {
          this.loadWorkCities(defendant.workRegionId);
        }

        this.form.patchValue({
          identityTypeId: defendant.identityTypeId,
          identityNumber: defendant.identityNumber,
          firstName: defendant.firstName,
          fatherName: defendant.fatherName,
          grandfatherName: defendant.grandfatherName,
          tribeName: defendant.tribeName,
          familyName: defendant.familyName,
          birthDate: defendant.birthDate ? new Date(defendant.birthDate + '') : null,
          genderId: defendant.genderId,
          nationalityId: defendant.nationalityId,
          mobileNumber: defendant.mobileNumber,
          email: defendant.email,
          indRegionId: defendant.indRegionId,
          indCityId: defendant.indCityId,
          indDistrict: defendant.indDistrict,
          indStreet: defendant.indStreet,
          indBuildingNumber: defendant.indBuildingNumber,
          indUnitNumber: defendant.indUnitNumber,
          indPostalCode: defendant.indPostalCode,
          indAdditionalCode: defendant.indAdditionalCode,
          employmentStatusId: defendant.employmentStatusId,
          employer: defendant.employer,
          occupation: defendant.occupation,
          // عنوان العمل
          workRegionId: defendant.workRegionId,
          workCityId: defendant.workCityId,
          workDistrict: defendant.workDistrict,
          workStreet: defendant.workStreet,
          workBuildingNumber: defendant.workBuildingNumber,
          workUnitNumber: defendant.workUnitNumber,
          workPostalCode: defendant.workPostalCode,
          workAdditionalCode: defendant.workAdditionalCode
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

  onIdentityTypeChange(identityTypeId: number | null): void {
    const nationalityControl = this.form.get('nationalityId');
    const fatherNameControl = this.form.get('fatherName');

    if (identityTypeId === this.IDENTITY_NATIONAL_ID) {
      // National ID: Set nationality to Saudi (read-only)
      nationalityControl?.setValue(this.SAUDI_NATIONALITY_ID);
      nationalityControl?.disable();
      // Father name is required for National ID
      fatherNameControl?.setValidators([Validators.required, Validators.maxLength(100)]);
    } else if (identityTypeId === this.IDENTITY_RESIDENT_ID) {
      // Resident ID: Exclude Saudi from nationalities
      nationalityControl?.enable();
      if (nationalityControl?.value === this.SAUDI_NATIONALITY_ID) {
        nationalityControl?.setValue(null);
      }
      // Father name is optional for Resident ID
      fatherNameControl?.setValidators([Validators.maxLength(100)]);
    } else {
      // No identity type selected
      nationalityControl?.enable();
      // Father name is optional
      fatherNameControl?.setValidators([Validators.maxLength(100)]);
    }
    fatherNameControl?.updateValueAndValidity();
  }

  get filteredNationalities(): Nationality[] {
    const identityTypeId = this.form.get('identityTypeId')?.value;
    if (identityTypeId === this.IDENTITY_RESIDENT_ID) {
      // Exclude Saudi for resident ID
      return this.nationalities.filter(n => n.id !== this.SAUDI_NATIONALITY_ID);
    }
    return this.nationalities;
  }

  // بيانات جهة العمل تظهر فقط إذا حالة العمل = حكومي (1) أو خاص (2)
  get showEmploymentFields(): boolean {
    const status = this.form.get('employmentStatusId')?.value;
    return status === 1 || status === 2; // Government or Private
  }

  // عنوان العمل يظهر فقط إذا حالة العمل = خاص (2)
  get showWorkAddressFields(): boolean {
    const status = this.form.get('employmentStatusId')?.value;
    return status === 2; // Private only
  }

  onEmploymentStatusChange(statusId: number | null): void {
    const employerControl = this.form.get('employer');
    const occupationControl = this.form.get('occupation');

    if (statusId === 1 || statusId === 2) {
      // Government or Private: employer and occupation are required
      employerControl?.setValidators([Validators.required, Validators.maxLength(200)]);
      occupationControl?.setValidators([Validators.required, Validators.maxLength(200)]);
    } else {
      // Unemployed or not selected: clear validators and values
      employerControl?.setValidators([Validators.maxLength(200)]);
      occupationControl?.setValidators([Validators.maxLength(200)]);
      employerControl?.setValue('');
      occupationControl?.setValue('');
    }
    employerControl?.updateValueAndValidity();
    occupationControl?.updateValueAndValidity();

    // Work address fields - only for Private (خاص)
    if (statusId === 2) {
      this.setWorkAddressValidators(true);
    } else {
      this.clearWorkAddressFields();
    }
  }

  private setWorkAddressValidators(required: boolean): void {
    const workRegionControl = this.form.get('workRegionId');
    const workCityControl = this.form.get('workCityId');
    const workDistrictControl = this.form.get('workDistrict');
    const workStreetControl = this.form.get('workStreet');
    const workBuildingNumberControl = this.form.get('workBuildingNumber');
    const workUnitNumberControl = this.form.get('workUnitNumber');
    const workPostalCodeControl = this.form.get('workPostalCode');
    const workAdditionalCodeControl = this.form.get('workAdditionalCode');

    if (required) {
      workRegionControl?.setValidators([Validators.required]);
      workCityControl?.setValidators([Validators.required]);
      workDistrictControl?.setValidators([Validators.required, Validators.maxLength(100)]);
      workStreetControl?.setValidators([Validators.required, Validators.maxLength(200)]);
      workBuildingNumberControl?.setValidators([Validators.required, Validators.pattern('^[0-9]{4}$')]);
      workUnitNumberControl?.setValidators([Validators.required, Validators.pattern('^[0-9]{4}$')]);
      workPostalCodeControl?.setValidators([Validators.required, Validators.pattern('^[0-9]{5}$')]);
      workAdditionalCodeControl?.setValidators([Validators.required, Validators.pattern('^[0-9]{4}$')]);
    } else {
      workRegionControl?.clearValidators();
      workCityControl?.clearValidators();
      workDistrictControl?.setValidators([Validators.maxLength(100)]);
      workStreetControl?.setValidators([Validators.maxLength(200)]);
      workBuildingNumberControl?.setValidators([Validators.pattern('^[0-9]{4}$')]);
      workUnitNumberControl?.setValidators([Validators.pattern('^[0-9]{4}$')]);
      workPostalCodeControl?.setValidators([Validators.pattern('^[0-9]{5}$')]);
      workAdditionalCodeControl?.setValidators([Validators.pattern('^[0-9]{4}$')]);
    }

    workRegionControl?.updateValueAndValidity();
    workCityControl?.updateValueAndValidity();
    workDistrictControl?.updateValueAndValidity();
    workStreetControl?.updateValueAndValidity();
    workBuildingNumberControl?.updateValueAndValidity();
    workUnitNumberControl?.updateValueAndValidity();
    workPostalCodeControl?.updateValueAndValidity();
    workAdditionalCodeControl?.updateValueAndValidity();
  }

  private clearWorkAddressFields(): void {
    // Clear validators first
    this.setWorkAddressValidators(false);
    // Then clear values
    this.form.patchValue({
      workRegionId: null,
      workCityId: null,
      workDistrict: '',
      workStreet: '',
      workBuildingNumber: '',
      workUnitNumber: '',
      workPostalCode: '',
      workAdditionalCode: ''
    });
    this.workCities = [];
  }

  onRegionChange(regionId: number): void {
    this.form.patchValue({ indCityId: null });
    this.cities = [];
    if (regionId) {
      this.loadCities(regionId);
    }
  }

  onWorkRegionChange(regionId: number): void {
    this.form.patchValue({ workCityId: null });
    this.workCities = [];
    if (regionId) {
      this.loadWorkCities(regionId);
    }
  }

  private loadCities(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe({
      next: (cities) => {
        this.cities = cities;
      }
    });
  }

  private loadWorkCities(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe({
      next: (cities) => {
        this.workCities = cities;
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
      } else if (this.hasTab3Errors()) {
        this.activeTab = 2;
      }
      return;
    }

    this.isSaving = true;

    // Convert date to DateOnly format (YYYY-MM-DD)
    const birthDate = this.form.value.birthDate;

    const formatDate = (date: any): string | undefined => {
      if (!date) return undefined;
      const d = new Date(date);
      return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    };

    const dto = {
      defendantTypeId: 1, // Individual (فرد)
      identityTypeId: this.form.value.identityTypeId,
      identityNumber: this.form.value.identityNumber,
      firstName: this.form.value.firstName,
      fatherName: this.form.value.fatherName,
      grandfatherName: this.form.value.grandfatherName || undefined,
      tribeName: this.form.value.tribeName || undefined,
      familyName: this.form.value.familyName,
      birthDate: formatDate(birthDate),
      genderId: this.form.value.genderId,
      nationalityId: this.form.getRawValue().nationalityId, // Use getRawValue to include disabled control
      mobileNumber: this.form.value.mobileNumber,
      email: this.form.value.email || undefined,
      indRegionId: this.form.value.indRegionId,
      indCityId: this.form.value.indCityId,
      indDistrict: this.form.value.indDistrict || undefined,
      indStreet: this.form.value.indStreet || undefined,
      indBuildingNumber: this.form.value.indBuildingNumber || undefined,
      indUnitNumber: this.form.value.indUnitNumber || undefined,
      indPostalCode: this.form.value.indPostalCode || undefined,
      indAdditionalCode: this.form.value.indAdditionalCode || undefined,
      employmentStatusId: this.form.value.employmentStatusId,
      employer: this.form.value.employer || undefined,
      occupation: this.form.value.occupation || undefined,
      // عنوان العمل (فقط إذا حالة العمل = خاص)
      workRegionId: this.form.value.workRegionId,
      workCityId: this.form.value.workCityId,
      workDistrict: this.form.value.workDistrict || undefined,
      workStreet: this.form.value.workStreet || undefined,
      workBuildingNumber: this.form.value.workBuildingNumber || undefined,
      workUnitNumber: this.form.value.workUnitNumber || undefined,
      workPostalCode: this.form.value.workPostalCode || undefined,
      workAdditionalCode: this.form.value.workAdditionalCode || undefined
    };

    const request$ = this.isEditMode
      ? this.defendantService.updateDefendant(this.defendantId!, dto)
      : this.defendantService.createDefendant(this.requestId, dto);

    request$.subscribe({
      next: () => {
        this.isSaving = false;
        const message = this.isEditMode ? 'تم تعديل المدعى عليه بنجاح' : 'تم إنشاء المدعى عليه بنجاح';
        this.snackBar.open(message, 'إغلاق', { duration: 3000 });
        // Emit saved event to parent
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
    const tab1Fields = ['identityTypeId', 'identityNumber', 'firstName', 'fatherName',
                        'grandfatherName', 'tribeName', 'familyName', 'birthDate',
                        'genderId', 'nationalityId', 'mobileNumber', 'email'];
    return tab1Fields.some(field => {
      const control = this.form.get(field);
      return control?.invalid && (control?.touched || this.submitted);
    });
  }

  // Check if Tab 2 has validation errors
  hasTab2Errors(): boolean {
    const tab2Fields = ['indRegionId', 'indCityId', 'indDistrict', 'indStreet',
                        'indBuildingNumber', 'indUnitNumber', 'indPostalCode', 'indAdditionalCode'];
    return tab2Fields.some(field => {
      const control = this.form.get(field);
      return control?.invalid && (control?.touched || this.submitted);
    });
  }

  // Check if Tab 3 has validation errors
  hasTab3Errors(): boolean {
    const tab3Fields = ['employmentStatusId', 'employer', 'occupation',
                        'workRegionId', 'workCityId', 'workDistrict', 'workStreet',
                        'workBuildingNumber', 'workUnitNumber', 'workPostalCode', 'workAdditionalCode'];
    return tab3Fields.some(field => {
      const control = this.form.get(field);
      return control?.invalid && (control?.touched || this.submitted);
    });
  }

  // Helper for form field access
  get f() { return this.form.controls; }

  // Mock Absher Integration
  getMockAbsherData(identityNumber: string, identityTypeId: number) {
    // Simulated Absher response - replace with real API later
    return {
      firstName: 'محمد',
      fatherName: 'أحمد',
      grandfatherName: 'علي',
      tribeName: 'العتيبي',
      familyName: 'السعود'
    };
  }

  onFetchAbsherData(): void {
    const identityNumber = this.form.get('identityNumber')?.value;
    const identityTypeId = this.form.get('identityTypeId')?.value;

    if (!identityNumber || !identityTypeId) {
      this.snackBar.open('يرجى إدخال رقم الهوية ونوع الهوية', 'إغلاق', { duration: 3000 });
      return;
    }

    this.isFetchingAbsher = true;

    // Simulate API delay
    setTimeout(() => {
      const absherData = this.getMockAbsherData(identityNumber, identityTypeId);

      // Populate fields
      this.form.patchValue({
        firstName: absherData.firstName,
        fatherName: absherData.fatherName,
        grandfatherName: absherData.grandfatherName,
        tribeName: absherData.tribeName,
        familyName: absherData.familyName
      });

      // Lock fields
      this.isAbsherLocked = true;
      this.isFetchingAbsher = false;
      this.snackBar.open('تم جلب البيانات بنجاح', 'إغلاق', { duration: 3000 });
    }, 1000); // 1 second delay to simulate API call
  }
}
