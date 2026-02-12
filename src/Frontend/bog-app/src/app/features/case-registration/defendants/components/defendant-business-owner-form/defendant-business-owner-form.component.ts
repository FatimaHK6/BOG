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
  selector: 'app-defendant-business-owner-form',
  templateUrl: './defendant-business-owner-form.component.html',
  styleUrls: ['./defendant-business-owner-form.component.scss']
})
export class DefendantBusinessOwnerFormComponent implements OnInit {
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

  // Active tab
  activeTab = 0;

  // Lookups
  identityTypes: IdentityType[] = [];
  nationalities: Nationality[] = [];
  regions: Region[] = [];
  indCities: City[] = [];
  workCities: City[] = [];
  businessCities: City[] = [];
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
  readonly SAUDI_NATIONALITY_ID = 1;

  // Max dates
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
    this.isViewMode = this.mode === 'view';
    this.isEditMode = this.mode === 'edit';
    this.initForm();
    this.loadLookups();
  }

  private initForm(): void {
    this.form = this.fb.group({
      // Tab 1: بيانات شخصية
      identityTypeId: [null, Validators.required],
      identityNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      fatherName: ['', [Validators.required, Validators.maxLength(100)]],
      grandfatherName: ['', [Validators.maxLength(100)]],
      tribeName: ['', [Validators.maxLength(100)]],
      familyName: ['', [Validators.required, Validators.maxLength(100)]],
      birthDate: [null, Validators.required],
      identityIssueDate: [null, Validators.required],
      identityExpiryDate: [null, Validators.required],
      genderId: [null, Validators.required],
      nationalityId: [null, Validators.required],
      mobileNumber: ['', [Validators.required, Validators.pattern('^05[0-9]{8}$')]],
      email: ['', [Validators.email]],
      // Tab 2: عنوان السكن (all required)
      indRegionId: [null, Validators.required],
      indCityId: [null, Validators.required],
      indDistrict: ['', [Validators.required, Validators.maxLength(100)]],
      indStreet: ['', [Validators.required, Validators.maxLength(200)]],
      indBuildingNumber: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]],
      indUnitNumber: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]],
      indPostalCode: ['', [Validators.required, Validators.pattern('^[0-9]{5}$')]],
      indAdditionalCode: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]],
      // Tab 3: بيانات جهة العمل
      employmentStatusId: [null],
      employer: ['', [Validators.maxLength(200)]],
      occupation: ['', [Validators.maxLength(200)]],
      // عنوان العمل (conditional)
      workRegionId: [null],
      workCityId: [null],
      workDistrict: ['', [Validators.maxLength(100)]],
      workStreet: ['', [Validators.maxLength(200)]],
      workBuildingNumber: ['', [Validators.pattern('^[0-9]{4}$')]],
      workUnitNumber: ['', [Validators.pattern('^[0-9]{4}$')]],
      workPostalCode: ['', [Validators.pattern('^[0-9]{5}$')]],
      workAdditionalCode: ['', [Validators.pattern('^[0-9]{4}$')]],
      // Tab 4: بيانات السجل التجاري
      commercialRegNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      companyName: ['', [Validators.required, Validators.maxLength(200)]],
      registrationStartDate: [null, Validators.required],
      registrationEndDate: [null, Validators.required],
      // Tab 5: عنوان المؤسسة
      regCompanyRegionId: [null, Validators.required],
      regCompanyCityId: [null, Validators.required],
      regCompanyDistrict: ['', [Validators.required, Validators.maxLength(100)]],
      regCompanyStreet: ['', [Validators.required, Validators.maxLength(200)]],
      regCompanyBuildingNumber: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]],
      regCompanyUnitNumber: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      regCompanyPostalCode: ['', [Validators.required, Validators.pattern('^[0-9]{5}$')]],
      regCompanyAdditionalCode: ['', [Validators.required, Validators.pattern('^[0-9]{4}$')]]
    });

    // Watch identity type changes
    this.form.get('identityTypeId')?.valueChanges.subscribe(value => {
      this.onIdentityTypeChange(value);
    });

    // Watch employment status changes
    this.form.get('employmentStatusId')?.valueChanges.subscribe(value => {
      this.onEmploymentStatusChange(value);
    });
  }

  private loadLookups(): void {
    this.isLoading = true;

    this.lookupService.getIdentityTypes().subscribe({
      next: (types) => {
        this.identityTypes = types.filter(t => t.id === 1 || t.id === 2);
      }
    });

    this.lookupService.getNationalities().subscribe({
      next: (nationalities) => {
        this.nationalities = nationalities;
      }
    });

    this.lookupService.getRegions().subscribe({
      next: (regions) => {
        this.regions = regions;
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
        if (defendant.indRegionId) {
          this.loadIndCities(defendant.indRegionId);
        }
        if (defendant.workRegionId) {
          this.loadWorkCities(defendant.workRegionId);
        }
        if (defendant.regCompanyRegionId) {
          this.loadBusinessCities(defendant.regCompanyRegionId);
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
          identityIssueDate: defendant.identityIssueDate ? new Date(defendant.identityIssueDate + '') : null,
          identityExpiryDate: defendant.identityExpiryDate ? new Date(defendant.identityExpiryDate + '') : null,
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
          workRegionId: defendant.workRegionId,
          workCityId: defendant.workCityId,
          workDistrict: defendant.workDistrict,
          workStreet: defendant.workStreet,
          workBuildingNumber: defendant.workBuildingNumber,
          workUnitNumber: defendant.workUnitNumber,
          workPostalCode: defendant.workPostalCode,
          workAdditionalCode: defendant.workAdditionalCode,
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
      nationalityControl?.setValue(this.SAUDI_NATIONALITY_ID);
      nationalityControl?.disable();
      fatherNameControl?.setValidators([Validators.required, Validators.maxLength(100)]);
    } else if (identityTypeId === this.IDENTITY_RESIDENT_ID) {
      nationalityControl?.enable();
      if (nationalityControl?.value === this.SAUDI_NATIONALITY_ID) {
        nationalityControl?.setValue(null);
      }
      fatherNameControl?.setValidators([Validators.maxLength(100)]);
    } else {
      nationalityControl?.enable();
      fatherNameControl?.setValidators([Validators.maxLength(100)]);
    }
    fatherNameControl?.updateValueAndValidity();
  }

  get filteredNationalities(): Nationality[] {
    const identityTypeId = this.form.get('identityTypeId')?.value;
    if (identityTypeId === this.IDENTITY_RESIDENT_ID) {
      return this.nationalities.filter(n => n.id !== this.SAUDI_NATIONALITY_ID);
    }
    return this.nationalities;
  }

  get showEmploymentFields(): boolean {
    const status = this.form.get('employmentStatusId')?.value;
    return status === 1 || status === 2;
  }

  get showWorkAddressFields(): boolean {
    const status = this.form.get('employmentStatusId')?.value;
    return status === 2;
  }

  onEmploymentStatusChange(statusId: number | null): void {
    const employerControl = this.form.get('employer');
    const occupationControl = this.form.get('occupation');

    // Show employer and occupation only if Government (1) or Private (2)
    if (statusId === 1 || statusId === 2) {
      employerControl?.setValidators([Validators.required, Validators.maxLength(200)]);
      occupationControl?.setValidators([Validators.required, Validators.maxLength(200)]);
    } else {
      // Unemployed (3) or not selected - clear fields and remove validators
      employerControl?.setValidators([Validators.maxLength(200)]);
      occupationControl?.setValidators([Validators.maxLength(200)]);
      employerControl?.setValue('');
      occupationControl?.setValue('');
    }
    employerControl?.updateValueAndValidity();
    occupationControl?.updateValueAndValidity();

    // Show work address only if Private (2)
    if (statusId === 2) {
      this.setWorkAddressValidators(true);
    } else {
      // Government (1), Unemployed (3), or not selected - hide work address
      this.clearWorkAddressFields();
    }
  }

  private setWorkAddressValidators(required: boolean): void {
    const controls = ['workRegionId', 'workCityId', 'workDistrict', 'workStreet',
                      'workBuildingNumber', 'workUnitNumber', 'workPostalCode', 'workAdditionalCode'];

    controls.forEach(name => {
      const control = this.form.get(name);
      if (required) {
        if (name === 'workRegionId' || name === 'workCityId') {
          control?.setValidators([Validators.required]);
        } else if (name === 'workBuildingNumber' || name === 'workAdditionalCode') {
          control?.setValidators([Validators.required, Validators.pattern('^[0-9]{4}$')]);
        } else if (name === 'workUnitNumber') {
          control?.setValidators([Validators.required, Validators.pattern('^[0-9]{4}$')]);
        } else if (name === 'workPostalCode') {
          control?.setValidators([Validators.required, Validators.pattern('^[0-9]{5}$')]);
        } else if (name === 'workDistrict') {
          control?.setValidators([Validators.required, Validators.maxLength(100)]);
        } else if (name === 'workStreet') {
          control?.setValidators([Validators.required, Validators.maxLength(200)]);
        }
      } else {
        if (name === 'workBuildingNumber' || name === 'workAdditionalCode' || name === 'workUnitNumber') {
          control?.setValidators([Validators.pattern('^[0-9]{4}$')]);
        } else if (name === 'workPostalCode') {
          control?.setValidators([Validators.pattern('^[0-9]{5}$')]);
        } else if (name === 'workDistrict') {
          control?.setValidators([Validators.maxLength(100)]);
        } else if (name === 'workStreet') {
          control?.setValidators([Validators.maxLength(200)]);
        } else {
          control?.clearValidators();
        }
      }
      control?.updateValueAndValidity();
    });
  }

  private clearWorkAddressFields(): void {
    this.setWorkAddressValidators(false);
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

  onIndRegionChange(regionId: number): void {
    this.form.patchValue({ indCityId: null });
    this.indCities = [];
    if (regionId) {
      this.loadIndCities(regionId);
    }
  }

  onWorkRegionChange(regionId: number): void {
    this.form.patchValue({ workCityId: null });
    this.workCities = [];
    if (regionId) {
      this.loadWorkCities(regionId);
    }
  }

  onBusinessRegionChange(regionId: number): void {
    this.form.patchValue({ regCompanyCityId: null });
    this.businessCities = [];
    if (regionId) {
      this.loadBusinessCities(regionId);
    }
  }

  private loadIndCities(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe({
      next: (cities) => this.indCities = cities
    });
  }

  private loadWorkCities(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe({
      next: (cities) => this.workCities = cities
    });
  }

  private loadBusinessCities(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe({
      next: (cities) => this.businessCities = cities
    });
  }

  setActiveTab(tabIndex: number): void {
    this.activeTab = tabIndex;
  }

  onSubmit(): void {
    this.submitted = true;
    this.form.markAllAsTouched();

    if (this.form.invalid || this.requestId <= 0) {
      if (this.hasTab1Errors()) this.activeTab = 0;
      else if (this.hasTab2Errors()) this.activeTab = 1;
      else if (this.hasTab3Errors()) this.activeTab = 2;
      else if (this.hasTab4Errors()) this.activeTab = 3;
      else if (this.hasTab5Errors()) this.activeTab = 4;
      return;
    }

    this.isSaving = true;

    const formatDate = (date: any): string | undefined => {
      if (!date) return undefined;
      const d = new Date(date);
      if (isNaN(d.getTime())) return undefined;
      const year = d.getFullYear();
      // Ensure valid 4-digit year
      if (year < 1900 || year > 2100) return undefined;
      return `${year}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    };

    const dto = {
      defendantTypeId: 5, // Business Owner (صاحب مؤسسة)
      // Personal data
      identityTypeId: this.form.value.identityTypeId,
      identityNumber: this.form.value.identityNumber || undefined,
      firstName: this.form.value.firstName,
      fatherName: this.form.value.fatherName || undefined,
      grandfatherName: this.form.value.grandfatherName || undefined,
      tribeName: this.form.value.tribeName || undefined,
      familyName: this.form.value.familyName,
      birthDate: formatDate(this.form.value.birthDate),
      identityIssueDate: formatDate(this.form.value.identityIssueDate),
      identityExpiryDate: formatDate(this.form.value.identityExpiryDate),
      genderId: this.form.value.genderId,
      nationalityId: this.form.getRawValue().nationalityId,
      mobileNumber: this.form.value.mobileNumber,
      email: this.form.value.email || undefined,
      // Residence address
      indRegionId: this.form.value.indRegionId,
      indCityId: this.form.value.indCityId,
      indDistrict: this.form.value.indDistrict || undefined,
      indStreet: this.form.value.indStreet || undefined,
      indBuildingNumber: this.form.value.indBuildingNumber || undefined,
      indUnitNumber: this.form.value.indUnitNumber || undefined,
      indPostalCode: this.form.value.indPostalCode || undefined,
      indAdditionalCode: this.form.value.indAdditionalCode || undefined,
      // Employment
      employmentStatusId: this.form.value.employmentStatusId,
      employer: this.form.value.employer || undefined,
      occupation: this.form.value.occupation,
      // Work address
      workRegionId: this.form.value.workRegionId,
      workCityId: this.form.value.workCityId,
      workDistrict: this.form.value.workDistrict || undefined,
      workStreet: this.form.value.workStreet || undefined,
      workBuildingNumber: this.form.value.workBuildingNumber || undefined,
      workUnitNumber: this.form.value.workUnitNumber || undefined,
      workPostalCode: this.form.value.workPostalCode || undefined,
      workAdditionalCode: this.form.value.workAdditionalCode || undefined,
      // Commercial registration
      commercialRegNumber: this.form.value.commercialRegNumber,
      companyName: this.form.value.companyName,
      registrationStartDate: formatDate(this.form.value.registrationStartDate),
      registrationEndDate: formatDate(this.form.value.registrationEndDate),
      // Business address
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

  hasTab1Errors(): boolean {
    const fields = ['identityTypeId', 'identityNumber', 'firstName', 'fatherName',
                    'grandfatherName', 'tribeName', 'familyName', 'birthDate',
                    'identityIssueDate', 'identityExpiryDate',
                    'genderId', 'nationalityId', 'mobileNumber', 'email'];
    return fields.some(f => this.form.get(f)?.invalid && (this.form.get(f)?.touched || this.submitted));
  }

  hasTab2Errors(): boolean {
    const fields = ['indRegionId', 'indCityId', 'indDistrict', 'indStreet',
                    'indBuildingNumber', 'indUnitNumber', 'indPostalCode', 'indAdditionalCode'];
    return fields.some(f => this.form.get(f)?.invalid && (this.form.get(f)?.touched || this.submitted));
  }

  hasTab3Errors(): boolean {
    const fields = ['employmentStatusId', 'employer', 'occupation',
                    'workRegionId', 'workCityId', 'workDistrict', 'workStreet',
                    'workBuildingNumber', 'workUnitNumber', 'workPostalCode', 'workAdditionalCode'];
    return fields.some(f => this.form.get(f)?.invalid && (this.form.get(f)?.touched || this.submitted));
  }

  hasTab4Errors(): boolean {
    const fields = ['commercialRegNumber', 'companyName', 'registrationStartDate', 'registrationEndDate'];
    return fields.some(f => this.form.get(f)?.invalid && (this.form.get(f)?.touched || this.submitted));
  }

  hasTab5Errors(): boolean {
    const fields = ['regCompanyRegionId', 'regCompanyCityId', 'regCompanyDistrict', 'regCompanyStreet',
                    'regCompanyBuildingNumber', 'regCompanyUnitNumber', 'regCompanyPostalCode', 'regCompanyAdditionalCode'];
    return fields.some(f => this.form.get(f)?.invalid && (this.form.get(f)?.touched || this.submitted));
  }

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






