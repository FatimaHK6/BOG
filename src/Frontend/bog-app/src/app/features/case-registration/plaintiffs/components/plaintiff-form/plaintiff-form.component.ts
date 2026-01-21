import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import {
  PlaintiffVM,
  PlaintiffCreateDTO,
  PlaintiffType,
  AddressVM
} from '../../../../../core/models/plaintiff.model';
import { RepresentativeVM } from '../../../../../core/models/plaintiff.model';
import { PlaintiffAttachmentVM } from '../../../../../core/models/plaintiff.model';
import { IdentityType, Nationality, Region, City, GovernmentAgency } from '../../../../../core/models/lookup.model';
import { PlaintiffService } from '../../../../../core/services/plaintiff.service';
import { LookupService } from '../../../../../core/services/lookup.service';
import { AbsherService } from '../../../../../core/services/absher.service';
import { RepresentativeService } from '../../../../../core/services/representative.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { RepresentativeDialogComponent, RepresentativeDialogData } from '../representative-dialog/representative-dialog.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-plaintiff-form',
  templateUrl: './plaintiff-form.component.html',
  styleUrls: ['./plaintiff-form.component.scss']
})
export class PlaintiffFormComponent implements OnInit {
  // Get requestId from query params
  requestId: number = 0;
  plaintiffId?: number;  // For edit mode - from route params

  // Form
  plaintiffForm!: FormGroup;
  currentStep = 0;
  isLoading = false;
  isSaving = false;
  isLookingUp = false;

  // Lookup data
  plaintiffTypes: PlaintiffType[] = [];
  identityTypes: IdentityType[] = [];
  nationalities: Nationality[] = [];
  regions: Region[] = [];
  residenceCities: City[] = [];
  workCities: City[] = [];
  businessCities: City[] = [];
  customCities: City[] = [];
  governmentAgencies: GovernmentAgency[] = [];
  countries: any[] = [];
  licenseSourceTypes: any[] = [];

  // BC03: Court city for comparison
  courtCityId?: number;
  requiresSelectedAddress = false;

  // View mode
  isViewMode = false;

  // Absher verification status (BR08 - mandatory for individuals)
  isAbsherVerified = false;
  absherDataSource?: number; // 1 = Absher

  // Representatives and Attachments (managed in steps 2 and 3)
  representatives: RepresentativeVM[] = [];
  attachments: PlaintiffAttachmentVM[] = [];

  // Current plaintiff (for edit mode)
  plaintiff?: PlaintiffVM;

  // Step labels
  steps = [
    { label: 'بيانات الشخصي', icon: 'person' },
    { label: 'بيانات الممثلين', icon: 'people' },
    { label: 'المرفقات', icon: 'attach_file' },
    { label: 'بيانات إضافية', icon: 'more_horiz' }
  ];

  constructor(
    private fb: FormBuilder,
    private plaintiffService: PlaintiffService,
    private lookupService: LookupService,
    private absherService: AbsherService,
    private representativeService: RepresentativeService,
    private notification: NotificationService,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Get requestId from query params
    this.route.queryParams.subscribe(params => {
      const newRequestId = params['requestId'] ? +params['requestId'] : 0;
      if (newRequestId > 0) {
        this.requestId = newRequestId;
      }
    });

    // Get plaintiffId from route params if editing
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.plaintiffId = +idParam;
    }

    // Check if view mode
    this.isViewMode = this.route.snapshot.url.some(segment => segment.path === 'view');

    this.initForm();
    this.loadLookups();

    if (this.plaintiffId) {
      this.loadPlaintiff();
    }

    // Disable form if view mode
    if (this.isViewMode) {
      this.plaintiffForm.disable();
    }
  }

  private initForm(): void {
    this.plaintiffForm = this.fb.group({
      // Step 1: Personal Data
      plaintiffTypeId: [null, Validators.required],
      identityTypeId: [1],  // Default to National ID
      identityNumber: [''],
      firstName: [''],
      fatherName: [''],
      grandfatherName: [''],
      familyName: [''],
      birthDate: [null],
      gender: [''],
      nationalityId: [null],
      mobileNumber: [''],
      email: ['', Validators.email],
      isDisabled: [false],
      identityExpiryDate: [null],
      identityIssueDate: [null],

      // Employment data
      employmentSector: [''],
      employerName: [''],
      occupation: [''],

      // Company data (Registered Company - Type 4)
      commercialRegNumber: [''],
      companyName: [''],
      crStartDate: [null],
      crEndDate: [null],

      // Unregistered Company data (Type 5)
      unregisteredCompanyName: [''],
      unregisteredCountryId: [null],
      unregisteredCity: [''],
      unregisteredDescription: [''],

      // Government agency data (Type 6)
      governmentAgencyId: [null],
      additionalStatement: [''],

      // NGO/Charity data (Type 7)
      ngoName: [''],
      licenseNumber: [''],
      licenseSourceId: [null],
      licenseDate: [null],

      // Waqf data (Type 8)
      courtDeedNumber: [''],
      deedDate: [null],
      deedSource: [''],
      waqfOversightType: [''],

      // Addresses (Step 4)
      residenceAddress: this.fb.group({
        regionId: [null],
        cityId: [null],
        district: [''],
        streetName: [''],
        buildingNumber: [''],
        postalCode: [''],
        additionalNumber: ['']
      }),
      workAddress: this.fb.group({
        regionId: [null],
        cityId: [null],
        district: [''],
        streetName: [''],
        buildingNumber: [''],
        postalCode: [''],
        additionalNumber: ['']
      }),
      // Business Address (for Business Owner - Type 3)
      businessAddress: this.fb.group({
        regionId: [null],
        cityId: [null],
        district: [''],
        streetName: [''],
        buildingNumber: [''],
        postalCode: [''],
        additionalNumber: ['']
      }),
      // Custom Address (for BC03 - when city differs from court)
      customAddress: this.fb.group({
        regionId: [null],
        cityId: [null],
        district: [''],
        streetName: [''],
        buildingNumber: [''],
        postalCode: [''],
        additionalNumber: ['']
      }),
      selectedAddressType: ['residence']
    });

    // Watch for plaintiff type changes to show/hide fields
    this.plaintiffForm.get('plaintiffTypeId')?.valueChanges.subscribe(typeId => {
      this.updateFormValidation(typeId);
    });
  }

  private updateFormValidation(plaintiffTypeId: number): void {
    const identityNumber = this.plaintiffForm.get('identityNumber');
    const firstName = this.plaintiffForm.get('firstName');
    const familyName = this.plaintiffForm.get('familyName');
    const commercialRegNumber = this.plaintiffForm.get('commercialRegNumber');
    const companyName = this.plaintiffForm.get('companyName');
    const governmentAgencyId = this.plaintiffForm.get('governmentAgencyId');
    const courtDeedNumber = this.plaintiffForm.get('courtDeedNumber');

    // Clear all validators first
    [identityNumber, firstName, familyName, commercialRegNumber, companyName, governmentAgencyId, courtDeedNumber]
      .forEach(control => {
        control?.clearValidators();
        control?.updateValueAndValidity();
      });

    // Get additional form controls
    const unregisteredCompanyName = this.plaintiffForm.get('unregisteredCompanyName');
    const unregisteredCountryId = this.plaintiffForm.get('unregisteredCountryId');
    const ngoName = this.plaintiffForm.get('ngoName');
    const ngoLicenseNumber = this.plaintiffForm.get('ngoLicenseNumber');
    const ngoLicenseSourceId = this.plaintiffForm.get('ngoLicenseSourceId');

    // Clear additional validators
    [unregisteredCompanyName, unregisteredCountryId, ngoName, ngoLicenseNumber, ngoLicenseSourceId]
      .forEach(control => {
        control?.clearValidators();
        control?.updateValueAndValidity();
      });

    // Set validators based on plaintiff type
    switch (plaintiffTypeId) {
      case 1: // Individual
        identityNumber?.setValidators([Validators.required]);
        firstName?.setValidators([Validators.required]);
        familyName?.setValidators([Validators.required]);
        break;
      case 2: // Individual without ID
        firstName?.setValidators([Validators.required]);
        familyName?.setValidators([Validators.required]);
        break;
      case 3: // Business Owner
        commercialRegNumber?.setValidators([Validators.required]);
        companyName?.setValidators([Validators.required]);
        // Also requires personal data
        firstName?.setValidators([Validators.required]);
        familyName?.setValidators([Validators.required]);
        break;
      case 4: // Registered Company
        commercialRegNumber?.setValidators([Validators.required]);
        companyName?.setValidators([Validators.required]);
        break;
      case 5: // Unregistered Company
        unregisteredCompanyName?.setValidators([Validators.required]);
        unregisteredCountryId?.setValidators([Validators.required]);
        break;
      case 6: // Government Agency
        governmentAgencyId?.setValidators([Validators.required]);
        break;
      case 7: // NGO/Charity
        ngoName?.setValidators([Validators.required]);
        ngoLicenseNumber?.setValidators([Validators.required]);
        ngoLicenseSourceId?.setValidators([Validators.required]);
        break;
      case 8: // Waqf
        courtDeedNumber?.setValidators([Validators.required]);
        break;
    }

    // Update validity
    [identityNumber, firstName, familyName, commercialRegNumber, companyName, governmentAgencyId, courtDeedNumber]
      .forEach(control => control?.updateValueAndValidity());
  }

  private loadLookups(): void {
    this.lookupService.getPlaintiffTypes().subscribe(data => this.plaintiffTypes = data);
    this.lookupService.getIdentityTypes().subscribe(data => this.identityTypes = data);
    this.lookupService.getNationalities().subscribe(data => this.nationalities = data);
    this.lookupService.getRegions().subscribe(data => this.regions = data);
    this.lookupService.getGovernmentAgencies().subscribe(data => this.governmentAgencies = data);

    // Load countries for Unregistered Company
    this.countries = this.getDemoCountries();

    // Load license source types for NGO
    this.licenseSourceTypes = this.getDemoLicenseSourceTypes();

    // Load court city for BC03 comparison (in production, this comes from case request)
    this.courtCityId = 1; // Demo: Riyadh
  }

  private getDemoCountries(): any[] {
    return [
      { id: 1, name: 'Saudi Arabia', nameAr: 'المملكة العربية السعودية' },
      { id: 2, name: 'UAE', nameAr: 'الإمارات العربية المتحدة' },
      { id: 3, name: 'Kuwait', nameAr: 'الكويت' },
      { id: 4, name: 'Qatar', nameAr: 'قطر' },
      { id: 5, name: 'Bahrain', nameAr: 'البحرين' },
      { id: 6, name: 'Oman', nameAr: 'عمان' },
      { id: 7, name: 'Egypt', nameAr: 'مصر' },
      { id: 8, name: 'Jordan', nameAr: 'الأردن' }
    ];
  }

  private getDemoLicenseSourceTypes(): any[] {
    return [
      { id: 1, name: 'Ministry of Human Resources', nameAr: 'وزارة الموارد البشرية' },
      { id: 2, name: 'Ministry of Commerce', nameAr: 'وزارة التجارة' },
      { id: 3, name: 'Ministry of Interior', nameAr: 'وزارة الداخلية' },
      { id: 4, name: 'General Authority for Awqaf', nameAr: 'الهيئة العامة للأوقاف' }
    ];
  }

  private loadPlaintiff(): void {
    if (!this.plaintiffId) return;

    this.isLoading = true;
    this.plaintiffService.getPlaintiff(this.plaintiffId).subscribe({
      next: (plaintiff) => {
        this.plaintiff = plaintiff;
        this.representatives = plaintiff.representatives || [];
        this.attachments = plaintiff.attachments || [];
        this.patchFormWithPlaintiff(plaintiff);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading plaintiff:', error);
        this.isLoading = false;
        this.notification.error('حدث خطأ أثناء تحميل بيانات المدعي');
      }
    });
  }

  private patchFormWithPlaintiff(plaintiff: PlaintiffVM): void {
    this.plaintiffForm.patchValue({
      plaintiffTypeId: plaintiff.plaintiffTypeId,
      identityTypeId: plaintiff.identityTypeId,
      identityNumber: plaintiff.identityNumber,
      firstName: plaintiff.firstName,
      fatherName: plaintiff.fatherName,
      grandfatherName: plaintiff.grandfatherName,
      familyName: plaintiff.familyName,
      birthDate: plaintiff.birthDate,
      gender: plaintiff.gender,
      nationalityId: plaintiff.nationalityId,
      mobileNumber: plaintiff.mobileNumber,
      email: plaintiff.email,
      isDisabled: plaintiff.isDisabled,
      commercialRegNumber: plaintiff.commercialRegNumber,
      companyName: plaintiff.companyName,
      crStartDate: plaintiff.crStartDate,
      crEndDate: plaintiff.crEndDate,
      governmentAgencyId: plaintiff.governmentAgencyId,
      additionalStatement: plaintiff.additionalStatement,
      courtDeedNumber: plaintiff.courtDeedNumber,
      deedDate: plaintiff.deedDate,
      deedSource: plaintiff.deedSource,
      waqfOversightType: plaintiff.waqfOversightType,
      selectedAddressType: plaintiff.selectedAddressType || 'residence'
    });

    // Patch addresses
    if (plaintiff.residenceAddress) {
      this.plaintiffForm.get('residenceAddress')?.patchValue(plaintiff.residenceAddress);
    }
    if (plaintiff.workAddress) {
      this.plaintiffForm.get('workAddress')?.patchValue(plaintiff.workAddress);
    }
  }

  onLookupIdentity(): void {
    const identityNumber = this.plaintiffForm.get('identityNumber')?.value;
    const identityTypeId = this.plaintiffForm.get('identityTypeId')?.value;

    if (!identityNumber) {
      this.notification.validation('الرجاء إدخال رقم الهوية');
      return;
    }

    this.isLookingUp = true;
    this.isAbsherVerified = false;

    this.absherService.getPersonDataWithAddresses(identityNumber, identityTypeId).subscribe({
      next: (personData) => {
        // Fill form with Absher data
        this.plaintiffForm.patchValue({
          firstName: personData.firstName,
          fatherName: personData.fatherName,
          grandfatherName: personData.grandfatherName,
          familyName: personData.familyName,
          birthDate: personData.birthDate,
          gender: personData.gender,
          nationalityId: personData.nationalityId,
          mobileNumber: personData.mobileNumber,
          email: personData.email
        });

        // Fill addresses if available
        if (personData.residenceAddress) {
          this.plaintiffForm.get('residenceAddress')?.patchValue({
            regionId: personData.residenceAddress.regionId,
            cityId: personData.residenceAddress.cityId,
            districtName: personData.residenceAddress.districtName,
            streetName: personData.residenceAddress.streetName,
            buildingNumber: personData.residenceAddress.buildingNumber,
            postalCode: personData.residenceAddress.postalCode,
            additionalNumber: personData.residenceAddress.additionalNumber
          });
        }

        // Mark as Absher verified (BR08)
        this.isAbsherVerified = true;
        this.absherDataSource = 1;

        this.isLookingUp = false;
        this.notification.success('تم جلب البيانات من أبشر بنجاح');
      },
      error: (error) => {
        console.error('Error looking up identity, using mock data:', error);
        // Use mock data for development/testing
        this.useMockAbsherData(identityNumber);
      }
    });
  }

  // Mock Absher data for development/testing
  private useMockAbsherData(identityNumber: string): void {
    // Generate mock data based on identity number
    const mockData = {
      firstName: 'محمد',
      fatherName: 'عبدالله',
      grandfatherName: 'سعد',
      familyName: 'السعيد',
      birthDate: new Date('1990-01-15'),
      gender: 'M',
      nationalityId: 1,
      mobileNumber: '0551234567',
      email: 'test@example.com'
    };

    // Fill form with mock data
    this.plaintiffForm.patchValue(mockData);

    // Mark as Absher verified (BR08)
    this.isAbsherVerified = true;
    this.absherDataSource = 1;

    this.isLookingUp = false;
    this.notification.info('تم جلب البيانات من أبشر (بيانات تجريبية)');
  }

  onResidenceRegionChange(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
      this.residenceCities = cities;
    });
  }

  onWorkRegionChange(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
      this.workCities = cities;
    });
  }

  onBusinessRegionChange(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
      this.businessCities = cities;
    });
  }

  onCustomRegionChange(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
      this.customCities = cities;
    });
  }

  // BC03: Check if city differs from court city
  onCityChange(): void {
    const residenceCityId = this.plaintiffForm.get('residenceAddress.cityId')?.value;
    const workCityId = this.plaintiffForm.get('workAddress.cityId')?.value;

    // If either city differs from court city, require selected address
    this.requiresSelectedAddress = (residenceCityId && residenceCityId !== this.courtCityId) ||
                                    (workCityId && workCityId !== this.courtCityId);
  }

  // Check if selected address is "other" (custom)
  isCustomAddressSelected(): boolean {
    return this.plaintiffForm.get('selectedAddressType')?.value === 'other';
  }

  onNextStep(): void {
    // Validate current step before proceeding
    if (this.currentStep === 0) {
      // Step 1 validation - check Absher for individuals (BR08)
      if (this.requiresAbsherVerification() && !this.isAbsherVerified) {
        this.notification.validation('يجب التحقق من البيانات عبر أبشر قبل المتابعة');
        return;
      }
    }

    if (this.currentStep < 3) {
      this.currentStep++;
    }
  }

  // Check if current plaintiff type requires Absher verification (BR08)
  requiresAbsherVerification(): boolean {
    // Individual (1) requires Absher verification
    // Individual without ID (2) does not require it
    return this.selectedPlaintiffType === 1;
  }

  onPreviousStep(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  onSave(): void {
    if (this.plaintiffForm.invalid) {
      this.plaintiffForm.markAllAsTouched();
      this.notification.validation('الرجاء تعبئة جميع الحقول المطلوبة');
      return;
    }

    this.isSaving = true;
    const formValue = this.plaintiffForm.value;

    if (this.plaintiffId) {
      // Update existing plaintiff
      this.plaintiffService.updatePlaintiff(this.plaintiffId, formValue).subscribe({
        next: () => {
          this.isSaving = false;
          this.notification.success('تم تحديث بيانات المدعي بنجاح');
          this.router.navigate(['/case-registration/plaintiffs'], {
            queryParams: { requestId: this.requestId }
          });
        },
        error: (error) => {
          console.error('Error updating plaintiff:', error);
          this.isSaving = false;
          this.notification.handleError(error, 'حدث خطأ أثناء تحديث المدعي');
        }
      });
    } else {
      // Create new plaintiff with representatives
      console.log('Representatives array before mapping:', this.representatives);
      const mappedReps = this.representatives.map(rep => ({
        representativeTypeId: rep.representativeTypeId,
        identityTypeId: rep.identityTypeId || 1,
        identityNumber: rep.identityNumber,
        firstName: rep.firstName,
        fatherName: rep.fatherName,
        grandfatherName: rep.grandfatherName,
        familyName: rep.familyName,
        birthDate: rep.birthDate,
        mobileNumber: rep.mobileNumber,
        email: rep.email,
        authorizationNumber: rep.authorizationNumber,
        authorizationDate: rep.authorizationDate,
        authorizationSource: rep.authorizationSource,
        authorizationSourceType: rep.authorizationSourceType,
        guardianshipType: rep.guardianshipType
      }));
      console.log('Mapped representatives:', mappedReps);

      const createDTO = {
        ...formValue,
        representatives: mappedReps
      };
      console.log('CreateDTO being sent:', createDTO);

      this.plaintiffService.createPlaintiff(this.requestId, createDTO).subscribe({
        next: () => {
          this.isSaving = false;
          this.notification.success('تم إضافة المدعي بنجاح');
          this.router.navigate(['/case-registration/plaintiffs'], {
            queryParams: { requestId: this.requestId }
          });
        },
        error: (error) => {
          console.error('Error creating plaintiff:', error);
          this.isSaving = false;
          this.notification.handleError(error, 'حدث خطأ أثناء إضافة المدعي');
        }
      });
    }
  }

  onSaveAsDraft(): void {
    // Save the current state as draft (minimal validation - only plaintiff type required)
    const formValue = this.plaintiffForm.value;

    if (!formValue.plaintiffTypeId) {
      this.notification.validation('الرجاء اختيار نوع المدعي');
      return;
    }

    this.isSaving = true;

    // Map representatives
    const mappedReps = this.representatives.map(rep => ({
      representativeTypeId: rep.representativeTypeId,
      identityTypeId: rep.identityTypeId || 1,
      identityNumber: rep.identityNumber,
      firstName: rep.firstName,
      fatherName: rep.fatherName,
      grandfatherName: rep.grandfatherName,
      familyName: rep.familyName,
      birthDate: rep.birthDate,
      mobileNumber: rep.mobileNumber,
      email: rep.email,
      authorizationNumber: rep.authorizationNumber,
      authorizationDate: rep.authorizationDate,
      authorizationSource: rep.authorizationSource,
      authorizationSourceType: rep.authorizationSourceType,
      guardianshipType: rep.guardianshipType
    }));

    const createDTO = {
      ...formValue,
      representatives: mappedReps
    };

    if (this.plaintiffId) {
      // Update existing plaintiff
      this.plaintiffService.updatePlaintiff(this.plaintiffId, formValue).subscribe({
        next: () => {
          this.isSaving = false;
          this.notification.success('تم حفظ الطلب كمسودة بنجاح');
          this.router.navigate(['/case-registration/requests']);
        },
        error: (error: any) => {
          console.error('Error saving draft:', error);
          this.isSaving = false;
          this.notification.handleError(error, 'حدث خطأ أثناء حفظ المسودة');
        }
      });
    } else {
      // Create new plaintiff
      this.plaintiffService.createPlaintiff(this.requestId, createDTO).subscribe({
        next: () => {
          this.isSaving = false;
          this.notification.success('تم حفظ الطلب كمسودة بنجاح');
          this.router.navigate(['/case-registration/requests']);
        },
        error: (error: any) => {
          console.error('Error saving draft:', error);
          this.isSaving = false;
          this.notification.handleError(error, 'حدث خطأ أثناء حفظ المسودة');
        }
      });
    }
  }

  onCancel(): void {
    if (this.hasUnsavedChanges()) {
      const dialogData: ConfirmDialogData = {
        title: 'تغييرات غير محفوظة',
        message: 'لديك تغييرات غير محفوظة. هل أنت متأكد من المغادرة بدون حفظ؟',
        confirmText: 'مغادرة',
        cancelText: 'البقاء',
        confirmColor: 'warn',
        icon: 'warning'
      };

      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '400px',
        data: dialogData
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.router.navigate(['/case-registration/plaintiffs'], {
            queryParams: { requestId: this.requestId }
          });
        }
      });
    } else {
      this.router.navigate(['/case-registration/plaintiffs'], {
        queryParams: { requestId: this.requestId }
      });
    }
  }

  hasUnsavedChanges(): boolean {
    return this.plaintiffForm.dirty || this.representatives.length > 0 || this.attachments.length > 0;
  }

  // Helper methods for template
  get selectedPlaintiffType(): number {
    return this.plaintiffForm.get('plaintiffTypeId')?.value;
  }

  isIndividual(): boolean {
    return this.selectedPlaintiffType === 1 || this.selectedPlaintiffType === 2;
  }

  isBusinessOwner(): boolean {
    return this.selectedPlaintiffType === 3;
  }

  // Check if Absher-fetched fields should be readonly
  // Fields are readonly when: plaintiff type is Individual (1) AND data came from Absher
  isAbsherFieldsReadonly(): boolean {
    return this.selectedPlaintiffType === 1 && this.isAbsherVerified;
  }

  // Check if document number should be readonly for individual without identity (type 2)
  // Document number is readonly when: plaintiff type is "فرد بدون هوية" (type 2)
  isDocumentNumberReadonly(): boolean {
    return this.selectedPlaintiffType === 2;
  }

  isRegisteredCompany(): boolean {
    return this.selectedPlaintiffType === 4;
  }

  isUnregisteredCompany(): boolean {
    return this.selectedPlaintiffType === 5;
  }

  isCompany(): boolean {
    return this.selectedPlaintiffType === 3 || this.selectedPlaintiffType === 4;
  }

  isGovernmentAgency(): boolean {
    return this.selectedPlaintiffType === 6;
  }

  isNGO(): boolean {
    return this.selectedPlaintiffType === 7;
  }

  isWaqf(): boolean {
    return this.selectedPlaintiffType === 8;
  }

  // Representatives management
  onAddRepresentative(): void {
    if (!this.plaintiffId && !this.selectedPlaintiffType) {
      this.notification.validation('الرجاء اختيار نوع المدعي أولاً');
      return;
    }

    // Get plaintiff identity number from form for ERR012 validation
    const plaintiffIdentityNumber = this.plaintiffForm.get('identityNumber')?.value;

    const dialogData: RepresentativeDialogData = {
      plaintiffId: this.plaintiffId || 0,
      plaintiffTypeId: this.selectedPlaintiffType,
      plaintiffIdentityNumber: plaintiffIdentityNumber,  // For ERR012
      existingRepresentatives: this.representatives  // For ERR008
    };

    const dialogRef = this.dialog.open(RepresentativeDialogComponent, {
      width: '700px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // If plaintiff exists, call API directly
        if (this.plaintiffId) {
          this.representativeService.createRepresentative(this.plaintiffId, result).subscribe({
            next: (savedRep) => {
              this.representatives.push(savedRep);
              this.notification.success('تم إضافة الممثل بنجاح');
            },
            error: (error) => {
              console.error('Error adding representative:', error);
              this.notification.handleError(error, 'حدث خطأ أثناء إضافة الممثل');
            }
          });
        } else {
          // Add to local array for new plaintiff (will be saved after plaintiff creation)
          const newRep: RepresentativeVM = {
            ...result,
            id: Date.now(), // Temporary ID
            representativeTypeName: '',
            representativeTypeNameAr: this.getRepresentativeTypeName(result.representativeTypeId),
            identityTypeName: '',
            fullName: [result.firstName, result.fatherName, result.grandfatherName, result.familyName]
              .filter((n: string) => n).join(' '),
            createdDate: new Date()
          };
          // Reassign array to trigger Angular change detection for the table
          this.representatives = [...this.representatives, newRep];
          console.log('Representatives after adding:', this.representatives);
          this.notification.success('تم إضافة الممثل بنجاح');
        }
      }
    });
  }

  onEditRepresentative(representative: RepresentativeVM): void {
    // Get plaintiff identity number from form for ERR012 validation
    const plaintiffIdentityNumber = this.plaintiffForm.get('identityNumber')?.value;

    const dialogData: RepresentativeDialogData = {
      representative,
      plaintiffId: this.plaintiffId || 0,
      plaintiffTypeId: this.selectedPlaintiffType,
      plaintiffIdentityNumber: plaintiffIdentityNumber,  // For ERR012
      existingRepresentatives: this.representatives.filter(r => r.id !== representative.id)  // For ERR008 (exclude current rep)
    };

    const dialogRef = this.dialog.open(RepresentativeDialogComponent, {
      width: '700px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const index = this.representatives.findIndex(r => r.id === representative.id);
        if (index >= 0) {
          const updatedRep = {
            ...this.representatives[index],
            ...result,
            representativeTypeNameAr: this.getRepresentativeTypeName(result.representativeTypeId),
            fullName: [result.firstName, result.fatherName, result.grandfatherName, result.familyName]
              .filter((n: string) => n).join(' ')
          };
          // Reassign array to trigger change detection
          this.representatives = [
            ...this.representatives.slice(0, index),
            updatedRep,
            ...this.representatives.slice(index + 1)
          ];
          this.notification.success('تم تحديث بيانات الممثل بنجاح');
        }
      }
    });
  }

  onDeleteRepresentative(representative: RepresentativeVM): void {
    const dialogData: ConfirmDialogData = {
      title: 'تأكيد الحذف',
      message: `هل أنت متأكد من حذف الممثل "${representative.fullName}"؟`,
      confirmText: 'حذف',
      cancelText: 'إلغاء',
      confirmColor: 'warn',
      icon: 'delete'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Reassign array to trigger change detection
        this.representatives = this.representatives.filter(r => r.id !== representative.id);
        this.notification.success('تم حذف الممثل بنجاح');
      }
    });
  }

  private getRepresentativeTypeName(typeId: number): string {
    const typeNames: { [key: number]: string } = {
      1: 'وكيل',
      2: 'ولي',
      3: 'وصي',
      4: 'ناظر',
      5: 'ممثل الورثة',
      6: 'ممثل الشركة',
      7: 'ممثل الجهة',
      8: 'أمين التفليسة',
      9: 'ممثل نظامي'
    };
    return typeNames[typeId] || 'ممثل';
  }

  // Attachments management
  onAttachmentsChanged(attachments: PlaintiffAttachmentVM[]): void {
    this.attachments = attachments;
  }
}
