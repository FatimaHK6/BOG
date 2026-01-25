import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomValidators } from '../../../../../shared/validators/custom-validators';
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
  companyCities: City[] = [];
  ngoCities: City[] = [];
  waqfCities: City[] = [];
  customCities: City[] = [];
  selectedAddressCities: City[] = [];
  governmentAgencies: GovernmentAgency[] = [];
  countries: any[] = [];
  licenseSourceTypes: any[] = [];

  // BC03: Court city for comparison
  courtCityId?: number;
  requiresSelectedAddress = false;

  // Business Rules flags
  // BC01: Show employer when employment status is Government or Private
  showEmployer = true;
  // BC02: Show work address only when employment status is Private
  showWorkAddress = false;
  // BC03/Waqf: Show agency name when oversight type is Government
  showWaqfAgencyName = false;
  // SRS 6.3.10: Residence Address required for Type 2
  residenceAddressRequired = false;
  // Nationality Rule: Filter nationalities based on identity type
  filteredNationalities: Nationality[] = [];
  // Employment status options
  employmentStatusOptions = [
    { id: 1, nameAr: 'حكومي', nameEn: 'Government' },
    { id: 2, nameAr: 'خاص', nameEn: 'Private' },
    { id: 3, nameAr: 'بدون عمل', nameEn: 'Unemployed' }
  ];
  // Saudi nationality ID (for nationality rule)
  private readonly SAUDI_NATIONALITY_ID = 1;

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
      // Step 1: Personal Data - NO VALIDATORS INITIALLY (set based on plaintiff type)
      plaintiffTypeId: [null, Validators.required],
      identityTypeId: [1],  // Default to National ID
      identityNumber: [''],
      firstName: [''],
      fatherName: [''],
      grandfatherName: [''],
      clanName: [''],
      familyName: [''],
      birthDate: [null],
      gender: [''],
      nationalityId: [null],
      mobileNumber: [''],
      email: [''],
      isDisabled: [false],
      identityIssueDate: [null],
      identityExpiryDate: [null],
      documentNumber: [''],

      // Employment data (BC01/BC02)
      employmentStatusId: [null],
      employer: [''],
      profession: [''],

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
      unregisteredCompanyAddress: [''],
      countryId: [null],
      description: [''],

      // Government agency data (Type 6)
      governmentAgencyId: [null],
      headquarters: [''],
      additionalStatement: [''],

      // NGO/Charity data (Type 7)
      ngoName: [''],
      licenseNumber: [''],
      licenseSourceId: [null],
      licenseDate: [null],

      // Waqf data (Type 8)
      courtDeedNumber: [''],
      waqfName: [''],
      deedDate: [null],
      deedSource: [''],
      waqfOversightType: [''],
      waqfDescription: [''],
      waqfAgencyName: [''],

      // Addresses (Step 1 - moved from Step 4)
      residenceAddress: this.createAddressFormGroup(),
      workAddress: this.createAddressFormGroup(),
      // Business Address (for Business Owner - Type 3)
      businessAddress: this.createAddressFormGroup(),
      // Company Address (for Type 4 - Registered Company) - SRS 6.3.6
      companyAddress: this.createAddressFormGroup(),
      // NGO Address (for Type 7 - NGO/Charity) - SRS 6.3.4
      ngoAddress: this.createAddressFormGroup(),
      // Waqf Address (for Type 8 - Waqf) - SRS 6.3.11
      waqfAddress: this.createAddressFormGroup(),

      // Step 4: Additional Data - Checkboxes
      isApplicant: [false],
      addSelectedAddress: [false],
      // Selected Address (shows when addSelectedAddress checkbox is checked)
      selectedAddress: this.createAddressFormGroup(),
      // Legacy fields (kept for compatibility)
      customAddress: this.createAddressFormGroup(),
      selectedAddressType: ['residence']
    });

    // Watch for plaintiff type changes to show/hide fields
    this.plaintiffForm.get('plaintiffTypeId')?.valueChanges.subscribe(typeId => {
      console.log('=== TYPE CHANGED ===');
      console.log('New typeId:', typeId, 'typeof:', typeof typeId);

      // NUCLEAR OPTION: Reset all non-common form values when type changes
      this.resetTypeSpecificFields(typeId);
      this.updateFormValidation(typeId);
    });

    // Watch for identity type changes to re-validate identity number
    this.plaintiffForm.get('identityTypeId')?.valueChanges.subscribe(() => {
      this.plaintiffForm.get('identityNumber')?.updateValueAndValidity();
    });

    // Watch for CR start date changes to validate CR end date
    this.plaintiffForm.get('crStartDate')?.valueChanges.subscribe(() => {
      this.plaintiffForm.get('crEndDate')?.updateValueAndValidity();
    });

    // Watch for identity issue date changes to validate expiry date
    this.plaintiffForm.get('identityIssueDate')?.valueChanges.subscribe(() => {
      this.plaintiffForm.get('identityExpiryDate')?.updateValueAndValidity();
    });

    // Watch for identity type changes for Nationality Rule
    this.plaintiffForm.get('identityTypeId')?.valueChanges.subscribe(typeId => {
      this.onIdentityTypeChange(typeId);
    });

    // Watch for employment status changes for BC01/BC02
    this.plaintiffForm.get('employmentStatusId')?.valueChanges.subscribe(statusId => {
      this.onEmploymentStatusChange(statusId);
    });

    // Watch for waqf oversight type changes for BC03
    this.plaintiffForm.get('waqfOversightType')?.valueChanges.subscribe(type => {
      this.onWaqfOversightChange(type);
    });
  }

  /**
   * Nationality Rule: Handle identity type changes.
   * - National ID: Set Saudi nationality (readonly)
   * - Resident ID/Passport: Filter out Saudi from nationalities
   * - Type 2 (Individual without ID): Skip this rule - nationality handled in updateFormValidation
   */
  private onIdentityTypeChange(typeId: number): void {
    // Skip nationality rules for Type 2 (Individual without ID) - they don't have identity type
    // Nationality for Type 2 is handled in updateFormValidation
    if (this.selectedPlaintiffType === 2) {
      return;
    }

    const nationalityControl = this.plaintiffForm.get('nationalityId');

    if (typeId === 1) { // National ID - Saudi only
      nationalityControl?.setValue(this.SAUDI_NATIONALITY_ID);
      nationalityControl?.disable();
      this.filteredNationalities = this.nationalities.filter(n => n.id === this.SAUDI_NATIONALITY_ID);
    } else if (typeId === 2 || typeId === 3) { // Resident ID or Passport - exclude Saudi
      nationalityControl?.enable();
      nationalityControl?.setValue(null);
      this.filteredNationalities = this.nationalities.filter(n => n.id !== this.SAUDI_NATIONALITY_ID);
    } else {
      nationalityControl?.enable();
      this.filteredNationalities = this.nationalities;
    }
  }

  /**
   * BC01/BC02: Handle employment status changes.
   * BC01: Show employer when Government or Private
   * BC02: Show work address only when Private
   */
  private onEmploymentStatusChange(statusId: number): void {
    const employerControl = this.plaintiffForm.get('employer');
    const professionControl = this.plaintiffForm.get('profession');
    const workAddressGroup = this.plaintiffForm.get('workAddress');

    // BC01: Employer and Profession visibility
    if (statusId === 1 || statusId === 2) { // Government or Private
      this.showEmployer = true;
      employerControl?.setValidators([Validators.required, Validators.maxLength(200)]);
      professionControl?.setValidators([Validators.maxLength(200)]);
    } else { // Unemployed (3)
      this.showEmployer = false;
      employerControl?.clearValidators();
      employerControl?.setValue(null);
      professionControl?.clearValidators();
      professionControl?.setValue(null);
    }
    employerControl?.updateValueAndValidity();
    professionControl?.updateValueAndValidity();

    // BC02: Work Address visibility
    if (statusId === 2) { // Private only
      this.showWorkAddress = true;
    } else { // Government or Unemployed
      this.showWorkAddress = false;
      workAddressGroup?.reset();
    }
  }

  /**
   * BC03: Handle waqf oversight type changes.
   * Show agency name when oversight is Government (حكومية)
   */
  private onWaqfOversightChange(type: string): void {
    const agencyControl = this.plaintiffForm.get('waqfAgencyName');

    if (type === 'حكومية') {
      this.showWaqfAgencyName = true;
      agencyControl?.setValidators([Validators.required, Validators.maxLength(200)]);
    } else { // خاصة (أهلي)
      this.showWaqfAgencyName = false;
      agencyControl?.clearValidators();
      agencyControl?.setValue(null);
    }
    agencyControl?.updateValueAndValidity();
  }

  /**
   * Creates a FormGroup for address WITHOUT validators.
   * Address fields are optional - validators only applied when needed.
   */
  private createAddressFormGroup(): FormGroup {
    return this.fb.group({
      regionId: [null],
      cityId: [null],
      districtId: [null],
      street: [''],
      buildingNumber: [''],
      unitNumber: [''],
      postalCode: [''],
      additionalCode: ['']
    });
  }

  /**
   * Patches an address form group with data from backend.
   * Maps backend property names to form field names.
   * Backend: streetName, additionalNumber, district
   * Form: street, additionalCode, districtId
   */
  private patchAddressFormGroup(formGroupName: string, address: any): void {
    const formGroup = this.plaintiffForm.get(formGroupName);
    if (!formGroup || !address) {
      console.log(`[patchAddressFormGroup] Skipping ${formGroupName} - formGroup:`, !!formGroup, 'address:', address);
      return;
    }

    const mappedData = {
      regionId: address.regionId,
      cityId: address.cityId,
      districtId: address.district || address.districtId,  // Backend uses 'district'
      street: address.streetName || address.street,         // Backend uses 'streetName'
      buildingNumber: address.buildingNumber,
      unitNumber: address.unitNumber,
      postalCode: address.postalCode,
      additionalCode: address.additionalNumber || address.additionalCode  // Backend uses 'additionalNumber'
    };

    console.log(`[patchAddressFormGroup] Patching ${formGroupName} with:`, mappedData);
    console.log(`[patchAddressFormGroup] Original backend data:`, address);

    formGroup.patchValue(mappedData);

    console.log(`[patchAddressFormGroup] Form group value after patch:`, formGroup.value);
  }

  private updateFormValidation(plaintiffTypeId: number): void {
    console.log('=== UPDATE FORM VALIDATION START ===');
    console.log('New plaintiffTypeId:', plaintiffTypeId);

    // Reset business rule flags
    this.residenceAddressRequired = false;
    this.showEmployer = false;
    this.showWorkAddress = false;

    // Clear employmentStatusId validators and errors first (will be set if needed)
    const employmentControl = this.plaintiffForm.get('employmentStatusId');
    if (employmentControl) {
      employmentControl.clearValidators();
      employmentControl.setErrors(null);
      employmentControl.markAsPristine();
      employmentControl.markAsUntouched();
      employmentControl.updateValueAndValidity({ emitEvent: false });
    }

    // Get all form controls that need dynamic validation
    const controls = {
      identityTypeId: this.plaintiffForm.get('identityTypeId'),
      identityNumber: this.plaintiffForm.get('identityNumber'),
      firstName: this.plaintiffForm.get('firstName'),
      fatherName: this.plaintiffForm.get('fatherName'),
      grandfatherName: this.plaintiffForm.get('grandfatherName'),
      clanName: this.plaintiffForm.get('clanName'),
      familyName: this.plaintiffForm.get('familyName'),
      birthDate: this.plaintiffForm.get('birthDate'),
      gender: this.plaintiffForm.get('gender'),
      nationalityId: this.plaintiffForm.get('nationalityId'),
      mobileNumber: this.plaintiffForm.get('mobileNumber'),
      email: this.plaintiffForm.get('email'),
      identityIssueDate: this.plaintiffForm.get('identityIssueDate'),
      identityExpiryDate: this.plaintiffForm.get('identityExpiryDate'),
      profession: this.plaintiffForm.get('profession'),
      employer: this.plaintiffForm.get('employer'),
      documentNumber: this.plaintiffForm.get('documentNumber'),
      commercialRegNumber: this.plaintiffForm.get('commercialRegNumber'),
      companyName: this.plaintiffForm.get('companyName'),
      crStartDate: this.plaintiffForm.get('crStartDate'),
      crEndDate: this.plaintiffForm.get('crEndDate'),
      unregisteredCompanyName: this.plaintiffForm.get('unregisteredCompanyName'),
      unregisteredCompanyAddress: this.plaintiffForm.get('unregisteredCompanyAddress'),
      countryId: this.plaintiffForm.get('countryId'),
      unregisteredCity: this.plaintiffForm.get('unregisteredCity'),
      description: this.plaintiffForm.get('description'),
      governmentAgencyId: this.plaintiffForm.get('governmentAgencyId'),
      headquarters: this.plaintiffForm.get('headquarters'),
      additionalStatement: this.plaintiffForm.get('additionalStatement'),
      ngoName: this.plaintiffForm.get('ngoName'),
      licenseNumber: this.plaintiffForm.get('licenseNumber'),
      licenseSourceId: this.plaintiffForm.get('licenseSourceId'),
      licenseDate: this.plaintiffForm.get('licenseDate'),
      courtDeedNumber: this.plaintiffForm.get('courtDeedNumber'),
      waqfName: this.plaintiffForm.get('waqfName'),
      deedDate: this.plaintiffForm.get('deedDate'),
      deedSource: this.plaintiffForm.get('deedSource'),
      waqfOversightType: this.plaintiffForm.get('waqfOversightType'),
      waqfDescription: this.plaintiffForm.get('waqfDescription'),
      waqfAgencyName: this.plaintiffForm.get('waqfAgencyName')
    };

    // Clear required validators on dynamic fields (keep format validators)
    this.clearRequiredValidators(controls);

    // DEBUG: Check companyName immediately after clearing
    const companyNameCtrl = controls.companyName;
    console.log(`[AFTER CLEAR] companyName: invalid=${companyNameCtrl?.invalid}, errors=${JSON.stringify(companyNameCtrl?.errors)}, value="${companyNameCtrl?.value}"`);

    // Clear all address form group validators (for type switching)
    this.clearAddressValidators();

    // Identity number validator that checks format based on identity type
    const identityNumberValidator = CustomValidators.identityNumber(
      () => this.plaintiffForm.get('identityTypeId')?.value
    );

    // CR End Date validator that checks it's after start date
    const crEndDateValidator = CustomValidators.dateAfter(
      () => this.plaintiffForm.get('crStartDate')?.value,
      'تاريخ نهاية السجل'
    );

    // Identity Expiry Date validator that checks it's after issue date
    const identityExpiryValidator = CustomValidators.dateAfter(
      () => this.plaintiffForm.get('identityIssueDate')?.value,
      'تاريخ انتهاء الهوية'
    );

    // Set validators based on plaintiff type (SRS Section 1.3 - 8 types)
    console.log(`[SWITCH] plaintiffTypeId = ${plaintiffTypeId}, typeof = ${typeof plaintiffTypeId}`);

    // Force to number to handle string "2" vs number 2
    const typeIdNum = Number(plaintiffTypeId);
    console.log(`[SWITCH] typeIdNum = ${typeIdNum}`);

    switch (typeIdNum) {
      case 1: // فرد (Individual) - SRS Section 1.3
        controls.identityTypeId?.setValidators([Validators.required]);
        controls.identityNumber?.setValidators([Validators.required, Validators.maxLength(20), identityNumberValidator]);
        controls.firstName?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.fatherName?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.familyName?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.birthDate?.setValidators([Validators.required, CustomValidators.notFutureDate('تاريخ الميلاد')]);
        controls.gender?.setValidators([Validators.required, CustomValidators.gender()]);
        controls.nationalityId?.setValidators([Validators.required]);
        controls.mobileNumber?.setValidators([Validators.required, CustomValidators.mobileNumber()]);
        controls.identityIssueDate?.setValidators([Validators.required, CustomValidators.notFutureDate('تاريخ إصدار الهوية')]);
        controls.identityExpiryDate?.setValidators([Validators.required, identityExpiryValidator]);
        // Profession is only required when employment status is Government(1) or Private(2)
        controls.profession?.clearValidators();
        // Employment status is required for Individual
        this.plaintiffForm.get('employmentStatusId')?.setValidators([Validators.required]);
        this.plaintiffForm.get('employmentStatusId')?.updateValueAndValidity();
        // Trigger employment status change to set profession validators correctly
        const currentStatus = this.plaintiffForm.get('employmentStatusId')?.value;
        if (currentStatus) {
          this.onEmploymentStatusChange(currentStatus);
        }
        break;

      case 2: // فرد بدون هوية (Individual w/o ID) - SRS Section 1.3
        console.log('[CASE 2] Individual without ID - setting validators');
        // Name fields required but no identity validation
        controls.firstName?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.fatherName?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.familyName?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.mobileNumber?.setValidators([Validators.required, CustomValidators.mobileNumber()]);
        console.log(`[CASE 2] companyName after case: invalid=${controls.companyName?.invalid}, errors=${JSON.stringify(controls.companyName?.errors)}`);
        break;

      case 3: // صاحب مؤسسة (Business Owner) - SRS Section 1.3
        console.log('[CASE 3] Business Owner - setting validators INCLUDING companyName');
        // Personal data + Commercial registration
        controls.identityTypeId?.setValidators([Validators.required]);
        controls.identityNumber?.setValidators([Validators.required, Validators.maxLength(20), identityNumberValidator]);
        controls.firstName?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.fatherName?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.familyName?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.nationalityId?.setValidators([Validators.required]);
        controls.mobileNumber?.setValidators([Validators.required, CustomValidators.mobileNumber()]);
        controls.identityIssueDate?.setValidators([Validators.required, CustomValidators.notFutureDate('تاريخ إصدار الهوية')]);
        controls.identityExpiryDate?.setValidators([Validators.required, identityExpiryValidator]);
        // Business Owner specific fields
        controls.commercialRegNumber?.setValidators([Validators.required, CustomValidators.commercialRegNumber()]);
        controls.companyName?.setValidators([Validators.required, Validators.maxLength(200), CustomValidators.arabicOnly('اسم المؤسسة')]);
        controls.crStartDate?.setValidators([Validators.required, CustomValidators.notFutureDate('تاريخ بداية السجل التجاري')]);
        controls.crEndDate?.setValidators([Validators.required, crEndDateValidator]);
        break;

      case 4: // شركة مسجلة (Registered Company) - SRS Section 1.3
        controls.commercialRegNumber?.setValidators([Validators.required, CustomValidators.commercialRegNumber()]);
        controls.companyName?.setValidators([Validators.required, Validators.maxLength(200), CustomValidators.arabicOnly('اسم الشركة')]);
        controls.crStartDate?.setValidators([Validators.required, CustomValidators.notFutureDate('تاريخ بداية السجل التجاري')]);
        controls.crEndDate?.setValidators([Validators.required, crEndDateValidator]);
        break;

      case 5: // شركة غير مسجلة (Unregistered Company) - SRS Section 1.3
        console.log('[CASE 5] Unregistered Company - setting validators on unregisteredCompanyName');
        // FIXED: Use unregisteredCompanyName, not companyName (field name mismatch bug)
        controls.unregisteredCompanyName?.setValidators([Validators.required, Validators.maxLength(200), CustomValidators.arabicOnly('اسم الشركة')]);
        controls.mobileNumber?.setValidators([Validators.required, CustomValidators.mobileNumber()]);
        // No commercial registration required
        break;

      case 6: // جهة حكومية (Government Agency) - SRS Section 1.3
        controls.governmentAgencyId?.setValidators([Validators.required]);
        controls.headquarters?.setValidators([Validators.required, Validators.maxLength(200)]);
        controls.additionalStatement?.setValidators([Validators.maxLength(4000)]);
        break;

      case 7: // جمعية/مؤسسة أهلية (Society/NGO) - SRS Section 1.3
        controls.licenseNumber?.setValidators([Validators.required, CustomValidators.licenseNumber()]);
        controls.licenseSourceId?.setValidators([Validators.required]);
        controls.ngoName?.setValidators([Validators.required, Validators.maxLength(200), CustomValidators.arabicOnly('اسم الجمعية')]);
        controls.licenseDate?.setValidators([Validators.required, CustomValidators.notFutureDate('تاريخ الترخيص')]);
        // NGO Address is required
        const ngoAddressGroup = this.plaintiffForm.get('ngoAddress');
        if (ngoAddressGroup) {
          ngoAddressGroup.get('regionId')?.setValidators([Validators.required]);
          ngoAddressGroup.get('cityId')?.setValidators([Validators.required]);
          ngoAddressGroup.get('regionId')?.updateValueAndValidity();
          ngoAddressGroup.get('cityId')?.updateValueAndValidity();
        }
        break;

      case 8: // وقف (Waqf) - SRS Section 1.3
        controls.courtDeedNumber?.setValidators([Validators.required, CustomValidators.courtDeedNumber()]);
        controls.waqfName?.setValidators([Validators.required, Validators.maxLength(200), CustomValidators.arabicOnly('اسم الوقف')]);
        controls.deedDate?.setValidators([Validators.required, CustomValidators.notFutureDate('تاريخ صك المحكمة')]);
        controls.deedSource?.setValidators([Validators.required, Validators.maxLength(100)]);
        controls.waqfOversightType?.setValidators([Validators.required, CustomValidators.waqfOversightType()]);
        controls.waqfDescription?.setValidators([Validators.required, Validators.maxLength(200)]);
        // Waqf Address is required - set validators on address form group
        const waqfAddressGroup = this.plaintiffForm.get('waqfAddress');
        if (waqfAddressGroup) {
          waqfAddressGroup.get('regionId')?.setValidators([Validators.required]);
          waqfAddressGroup.get('cityId')?.setValidators([Validators.required]);
          waqfAddressGroup.get('regionId')?.updateValueAndValidity();
          waqfAddressGroup.get('cityId')?.updateValueAndValidity();
        }
        break;

      default:
        console.log(`[DEFAULT CASE] No validators set - typeIdNum=${typeIdNum} did not match any case!`);
        break;
    }

    // Update validity for all controls
    Object.values(controls).forEach(control => control?.updateValueAndValidity());

    // DEBUG: Log final validator state for key fields
    console.log('=== VALIDATORS AFTER TYPE CHANGE ===');
    console.log('plaintiffTypeId:', plaintiffTypeId);

    const debugFields = ['governmentAgencyId', 'headquarters', 'companyName', 'commercialRegNumber', 'firstName', 'identityNumber'];
    debugFields.forEach(fieldName => {
      const ctrl = controls[fieldName as keyof typeof controls];
      if (ctrl) {
        console.log(`${fieldName}: invalid=${ctrl.invalid}, errors=${JSON.stringify(ctrl.errors)}, value=${ctrl.value}`);
      }
    });

    // Check form overall validity
    console.log('Form valid:', this.plaintiffForm.valid);
    console.log('Form errors:', this.plaintiffForm.errors);

    // Find ALL invalid controls
    const invalidControls: string[] = [];
    Object.keys(this.plaintiffForm.controls).forEach(key => {
      const control = this.plaintiffForm.get(key);
      if (control && control.invalid) {
        invalidControls.push(`${key}: ${JSON.stringify(control.errors)}`);
      }
    });
    console.log('ALL invalid controls:', invalidControls);
    console.log('=== UPDATE FORM VALIDATION END ===');
  }

  /**
   * Clears ALL validators and errors from controls to start fresh for each plaintiff type.
   * This ensures no remnants from previous type selection remain.
   */
  private clearRequiredValidators(controls: { [key: string]: AbstractControl | null }): void {
    console.log('=== CLEARING ALL VALIDATORS ===');

    // Clear all validators, errors, and reset state for all controls
    Object.entries(controls).forEach(([key, control]) => {
      if (control) {
        const hadErrors = control.errors;
        const wasInvalid = control.invalid;

        // 1. Clear all validators
        control.clearValidators();
        control.clearAsyncValidators();
        // 2. Clear any existing errors
        control.setErrors(null);
        // 3. Mark as pristine and untouched to reset error display state
        control.markAsPristine();
        control.markAsUntouched();
        // 4. Update validity without emitting events to avoid cascading changes
        control.updateValueAndValidity({ emitEvent: false });

        // Log only fields that had errors
        if (hadErrors || wasInvalid) {
          console.log(`[CLEARED] ${key}: had errors=${JSON.stringify(hadErrors)}, wasInvalid=${wasInvalid}, nowInvalid=${control.invalid}, nowErrors=${JSON.stringify(control.errors)}`);
        }
      }
    });

    console.log('=== VALIDATORS CLEARED ===');
  }

  /**
   * Clears validators and errors from all address form groups when switching plaintiff types.
   * This ensures address validation is completely reset for the new type.
   */
  private clearAddressValidators(): void {
    const addressGroups = ['residenceAddress', 'workAddress', 'businessAddress', 'companyAddress', 'ngoAddress', 'waqfAddress'];
    const addressFields = ['regionId', 'cityId', 'districtId', 'street', 'buildingNumber', 'unitNumber', 'postalCode', 'additionalCode'];

    addressGroups.forEach(groupName => {
      const group = this.plaintiffForm.get(groupName);
      if (group) {
        addressFields.forEach(fieldName => {
          const control = group.get(fieldName);
          if (control) {
            // 1. Clear all validators
            control.clearValidators();
            // 2. Clear any existing errors
            control.setErrors(null);
            // 3. Mark as pristine and untouched to reset error display state
            control.markAsPristine();
            control.markAsUntouched();
            // 4. Update validity
            control.updateValueAndValidity({ emitEvent: false });
          }
        });
        // Also clear errors on the group itself
        group.setErrors(null);
        group.markAsPristine();
        group.markAsUntouched();
      }
    });
  }

  /**
   * NUCLEAR RESET: Resets all type-specific form fields when plaintiff type changes.
   * This ensures no stale values or validators remain from the previous type.
   */
  private resetTypeSpecificFields(newTypeId: number): void {
    console.log('=== RESETTING TYPE-SPECIFIC FIELDS ===');
    console.log('New type:', newTypeId);

    const typeId = Number(newTypeId);

    // Define fields for each type
    const individualFields = ['identityTypeId', 'identityNumber', 'firstName', 'fatherName', 'grandfatherName',
      'clanName', 'familyName', 'birthDate', 'gender', 'nationalityId', 'mobileNumber', 'email', 'isDisabled',
      'identityIssueDate', 'identityExpiryDate', 'documentNumber', 'employmentStatusId', 'employer', 'profession'];

    const businessOwnerFields = ['commercialRegNumber', 'companyName', 'crStartDate', 'crEndDate'];

    const registeredCompanyFields = ['commercialRegNumber', 'companyName', 'crStartDate', 'crEndDate'];

    const unregisteredCompanyFields = ['unregisteredCompanyName', 'unregisteredCountryId', 'unregisteredCity',
      'unregisteredDescription', 'unregisteredCompanyAddress', 'countryId', 'description'];

    const governmentFields = ['governmentAgencyId', 'headquarters', 'additionalStatement'];

    const ngoFields = ['ngoName', 'licenseNumber', 'licenseSourceId', 'licenseDate'];

    const waqfFields = ['waqfName', 'courtDeedNumber', 'deedDate', 'deedSource', 'waqfOversightType',
      'waqfDescription', 'waqfAgencyName'];

    // Reset fields NOT belonging to the new type
    const fieldsToReset: string[] = [];

    // Types 1, 2, 3 use individual fields
    if (![1, 2, 3].includes(typeId)) {
      fieldsToReset.push(...individualFields);
    }

    // Types 3, 4 use business/company fields
    if (![3, 4].includes(typeId)) {
      fieldsToReset.push(...businessOwnerFields);
    }

    // Type 5 uses unregistered company fields
    if (typeId !== 5) {
      fieldsToReset.push(...unregisteredCompanyFields);
    }

    // Type 6 uses government fields
    if (typeId !== 6) {
      fieldsToReset.push(...governmentFields);
    }

    // Type 7 uses NGO fields
    if (typeId !== 7) {
      fieldsToReset.push(...ngoFields);
    }

    // Type 8 uses waqf fields
    if (typeId !== 8) {
      fieldsToReset.push(...waqfFields);
    }

    // Reset each field
    fieldsToReset.forEach(fieldName => {
      const control = this.plaintiffForm.get(fieldName);
      if (control) {
        control.reset(null, { emitEvent: false });
        control.clearValidators();
        control.clearAsyncValidators();
        control.setErrors(null);
        control.markAsPristine();
        control.markAsUntouched();
        control.updateValueAndValidity({ emitEvent: false });
      }
    });

    console.log('Fields reset:', fieldsToReset.length);
  }

  private loadLookups(): void {
    this.lookupService.getPlaintiffTypes().subscribe(data => this.plaintiffTypes = data);
    this.lookupService.getIdentityTypes().subscribe(data => this.identityTypes = data);
    this.lookupService.getNationalities().subscribe(data => {
      this.nationalities = data;
      // Initialize filteredNationalities with all nationalities
      this.filteredNationalities = data;
      // If Type 2 is already selected, ensure nationality is enabled with all nationalities
      if (this.selectedPlaintiffType === 2) {
        this.plaintiffForm.get('nationalityId')?.enable();
      }
    });
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
    console.log('[loadPlaintiff] Loading plaintiff ID:', this.plaintiffId);

    this.plaintiffService.getPlaintiff(this.plaintiffId).subscribe({
      next: (plaintiff) => {
        console.log('[loadPlaintiff] Received plaintiff data:', plaintiff);
        console.log('[loadPlaintiff] residenceAddress:', plaintiff.residenceAddress);
        console.log('[loadPlaintiff] workAddress:', plaintiff.workAddress);
        console.log('[loadPlaintiff] selectedAddress:', plaintiff.selectedAddress);

        this.plaintiff = plaintiff;
        this.representatives = plaintiff.representatives || [];
        this.attachments = plaintiff.attachments || [];

        // Set Absher verification status based on data source
        if (plaintiff.dataSourceId === 1) {
          this.isAbsherVerified = true;
          this.absherDataSource = 1;
        } else {
          // Reset Absher status for non-Absher data (important for edit mode)
          this.isAbsherVerified = false;
          this.absherDataSource = undefined;
        }

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
    console.log('=== PATCHING FORM WITH PLAINTIFF ===');
    console.log('Plaintiff data:', plaintiff);
    console.log('plaintiffTypeId from data:', plaintiff.plaintiffTypeId);

    // IMPORTANT: Temporarily disable valueChanges to prevent updateFormValidation from running mid-patch
    // We will manually call updateFormValidation after patching
    const typeControl = this.plaintiffForm.get('plaintiffTypeId');

    // Patch all form fields from plaintiff data
    this.plaintiffForm.patchValue({
      // Basic Info
      plaintiffTypeId: plaintiff.plaintiffTypeId,

      // Personal Data (Individual types: 1, 2, 3)
      identityTypeId: plaintiff.identityTypeId,
      identityNumber: plaintiff.identityNumber,
      documentNumber: plaintiff.documentNumber,
      firstName: plaintiff.firstName,
      fatherName: plaintiff.fatherName,
      grandfatherName: plaintiff.grandfatherName,
      clanName: plaintiff.clanName,
      familyName: plaintiff.familyName,
      birthDate: plaintiff.birthDate,
      gender: plaintiff.gender,
      nationalityId: plaintiff.nationalityId,
      identityIssueDate: plaintiff.identityIssueDate,
      identityExpiryDate: plaintiff.identityExpiryDate,
      mobileNumber: plaintiff.mobileNumber,
      email: plaintiff.email,
      isDisabled: plaintiff.isDisabled,

      // Employment Data (Types 1, 3)
      employmentStatusId: plaintiff.employmentStatusId,
      employer: plaintiff.employer,
      profession: plaintiff.profession,

      // Commercial Registration (Types 3, 4)
      commercialRegNumber: plaintiff.commercialRegNumber,
      companyName: plaintiff.companyName,
      crStartDate: plaintiff.crStartDate,
      crEndDate: plaintiff.crEndDate,

      // Government Agency (Type 6)
      governmentAgencyId: plaintiff.governmentAgencyId,
      headquarters: plaintiff.headquarters,
      additionalStatement: plaintiff.additionalStatement,

      // NGO/Charity (Type 7)
      licenseNumber: plaintiff.licenseNumber,
      licenseSourceId: plaintiff.licenseSourceId,
      ngoName: plaintiff.ngoName,
      licenseDate: plaintiff.licenseDate,

      // Waqf (Type 8)
      courtDeedNumber: plaintiff.courtDeedNumber,
      waqfName: plaintiff.waqfName,
      deedDate: plaintiff.deedDate,
      deedSource: plaintiff.deedSource,
      waqfOversightType: plaintiff.waqfOversightType,
      waqfAgencyName: plaintiff.waqfAgencyName,
      waqfDescription: plaintiff.waqfDescription,

      // Unregistered Company (Type 5)
      unregisteredCompanyAddress: plaintiff.unregisteredCompanyAddress,
      countryId: plaintiff.countryId,
      unregisteredCity: plaintiff.unregisteredCompanyCity,
      description: plaintiff.description,

      // Step 4: Additional Data
      isApplicant: plaintiff.isApplicant,
      addSelectedAddress: !!plaintiff.selectedAddress,

      // Selected Address
      selectedAddressType: plaintiff.selectedAddressType || 'residence'
    });

    // Patch addresses - map backend property names to form field names
    if (plaintiff.residenceAddress) {
      this.patchAddressFormGroup('residenceAddress', plaintiff.residenceAddress);
      // Load cities for region
      if (plaintiff.residenceAddress.regionId) {
        this.onResidenceRegionChange(plaintiff.residenceAddress.regionId);
      }
    }
    if (plaintiff.workAddress) {
      this.patchAddressFormGroup('workAddress', plaintiff.workAddress);
      // Load cities for region
      if (plaintiff.workAddress.regionId) {
        this.onWorkRegionChange(plaintiff.workAddress.regionId);
      }
    }
    if (plaintiff.businessAddress) {
      this.patchAddressFormGroup('businessAddress', plaintiff.businessAddress);
      // Load cities for region
      if (plaintiff.businessAddress.regionId) {
        this.onBusinessRegionChange(plaintiff.businessAddress.regionId);
      }
    }
    if (plaintiff.companyAddress) {
      this.patchAddressFormGroup('companyAddress', plaintiff.companyAddress);
      // Load cities for region
      if (plaintiff.companyAddress.regionId) {
        this.onCompanyRegionChange(plaintiff.companyAddress.regionId);
      }
    }
    if (plaintiff.ngoAddress) {
      this.patchAddressFormGroup('ngoAddress', plaintiff.ngoAddress);
      // Load cities for region
      if (plaintiff.ngoAddress.regionId) {
        this.onNgoRegionChange(plaintiff.ngoAddress.regionId);
      }
    }
    if (plaintiff.waqfAddress) {
      this.patchAddressFormGroup('waqfAddress', plaintiff.waqfAddress);
      // Load cities for region
      if (plaintiff.waqfAddress.regionId) {
        this.onWaqfRegionChange(plaintiff.waqfAddress.regionId);
      }
    }
    // Handle selectedAddress with explicit checkbox state management
    console.log('[patchFormWithPlaintiff] selectedAddress exists:', !!plaintiff.selectedAddress);
    console.log('[patchFormWithPlaintiff] addSelectedAddress checkbox will be:', !!plaintiff.selectedAddress);

    if (plaintiff.selectedAddress) {
      console.log('[patchFormWithPlaintiff] selectedAddress data:', plaintiff.selectedAddress);

      // Force the addSelectedAddress checkbox to true BEFORE patching the address
      // This ensures the address section is visible when the form fields are patched
      this.plaintiffForm.get('addSelectedAddress')?.setValue(true, { emitEvent: false });
      console.log('[patchFormWithPlaintiff] Forced addSelectedAddress checkbox to true');

      this.patchAddressFormGroup('selectedAddress', plaintiff.selectedAddress);

      // Load cities for region
      if (plaintiff.selectedAddress.regionId) {
        console.log('[patchFormWithPlaintiff] Loading cities for selected address region:', plaintiff.selectedAddress.regionId);
        this.onSelectedRegionChange(plaintiff.selectedAddress.regionId);
      }

      // Verify the form values after patching
      console.log('[patchFormWithPlaintiff] selectedAddress form values after patch:', this.plaintiffForm.get('selectedAddress')?.value);
      console.log('[patchFormWithPlaintiff] addSelectedAddress checkbox value:', this.plaintiffForm.get('addSelectedAddress')?.value);
    }

    // Trigger employment status change to show/hide employer and work address fields
    if (plaintiff.employmentStatusId) {
      this.onEmploymentStatusChange(plaintiff.employmentStatusId);
    }

    // Trigger identity type change to handle nationality (disable for National ID)
    if (plaintiff.identityTypeId) {
      this.onIdentityTypeChange(plaintiff.identityTypeId);
      // Re-set nationality after identity type change (it may have been cleared)
      if (plaintiff.nationalityId) {
        this.plaintiffForm.get('nationalityId')?.setValue(plaintiff.nationalityId);
      }
    } else if (plaintiff.plaintiffTypeId === 2) {
      // Type 2 (Individual without ID): Enable nationality and show all nationalities
      const nationalityControl = this.plaintiffForm.get('nationalityId');
      nationalityControl?.enable();
      this.filteredNationalities = this.nationalities;
      if (plaintiff.nationalityId) {
        nationalityControl?.setValue(plaintiff.nationalityId);
      }
    }

    // Trigger waqf oversight type change to show/hide agency name field
    if (plaintiff.waqfOversightType) {
      this.onWaqfOversightChange(plaintiff.waqfOversightType);
      // Re-set waqf agency name after oversight type change (it may have been cleared)
      if (plaintiff.waqfAgencyName) {
        this.plaintiffForm.get('waqfAgencyName')?.setValue(plaintiff.waqfAgencyName);
      }
    }
  }

  onLookupIdentity(): void {
    // Prevent Absher lookup in edit mode - existing plaintiffs are already verified
    if (this.plaintiffId) {
      this.notification.info('التحقق من أبشر غير متاح في وضع التعديل');
      return;
    }

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
          email: personData.email,
          identityIssueDate: personData.identityIssueDate,
          identityExpiryDate: personData.identityExpiryDate
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
    // Gender must be Arabic values: ذكر (male) or أنثى (female)
    const mockData = {
      firstName: 'محمد',
      fatherName: 'عبدالله',
      grandfatherName: 'سعد',
      familyName: 'السعيد',
      birthDate: new Date('1990-01-15'),
      gender: 'ذكر',  // Arabic: male
      nationalityId: 1,
      mobileNumber: '0551234567',
      email: 'test@example.com',
      identityIssueDate: new Date('2020-01-01'),
      identityExpiryDate: new Date('2030-01-01')
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

  onCompanyRegionChange(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
      this.companyCities = cities;
    });
  }

  onNgoRegionChange(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
      this.ngoCities = cities;
    });
  }

  onWaqfRegionChange(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
      this.waqfCities = cities;
    });
  }

  onCustomRegionChange(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
      this.customCities = cities;
    });
  }

  onSelectedRegionChange(regionId: number): void {
    this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
      this.selectedAddressCities = cities;
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
    console.log('=== ON SAVE CALLED ===');
    console.log('plaintiffTypeId:', this.selectedPlaintiffType);

    // FORCE re-validation with current type before save (validators only, not values)
    const currentType = this.selectedPlaintiffType;
    if (currentType) {
      console.log('Forcing validator update for type:', currentType);
      this.updateFormValidation(currentType);
    }

    console.log('Form valid after forced re-validation:', this.plaintiffForm.valid);

    // DEBUG: Show ALL controls and their status
    console.log('=== ALL FORM CONTROLS STATUS ===');
    Object.keys(this.plaintiffForm.controls).forEach(key => {
      const control = this.plaintiffForm.get(key);
      if (control && control.invalid) {
        console.log(`INVALID: ${key} - errors: ${JSON.stringify(control.errors)}, value: ${JSON.stringify(control.value)}`);
      }
    });

    if (this.plaintiffForm.invalid) {
      this.plaintiffForm.markAllAsTouched();
      // Log invalid fields for debugging
      const invalidFields = this.getInvalidFields();
      console.log('Invalid fields:', invalidFields);
      this.notification.validation('الرجاء تعبئة جميع الحقول المطلوبة:\n' + invalidFields.join('\n'));
      return;
    }

    this.isSaving = true;
    const formValue = this.prepareFormDataForSubmit();
    console.log('=== SAVE PLAINTIFF ===');
    console.log('plaintiffId:', this.plaintiffId);
    console.log('formValue:', JSON.stringify(formValue, null, 2));

    if (this.plaintiffId) {
      // Update existing plaintiff
      this.plaintiffService.updatePlaintiff(this.plaintiffId, formValue).subscribe({
        next: (response) => {
          console.log('UPDATE SUCCESS - Response:', response);
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
        mobileNumber: rep.mobileNumber?.trim() || null,
        email: rep.email?.trim() || null,
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

  /**
   * Prepares form data for submission.
   * Converts empty address objects to null to avoid backend validation errors.
   */
  private prepareFormDataForSubmit(): any {
    // Use getRawValue() to include disabled fields (e.g., nationality for National ID)
    const formValue = { ...this.plaintiffForm.getRawValue() };

    // === DEBUG: Log Waqf fields before cleaning ===
    console.log('=== WAQF FIELDS BEFORE CLEAN ===');
    console.log('plaintiffTypeId:', formValue.plaintiffTypeId);
    console.log('waqfName:', formValue.waqfName);
    console.log('courtDeedNumber:', formValue.courtDeedNumber);
    console.log('deedDate:', formValue.deedDate);
    console.log('deedSource:', formValue.deedSource);
    console.log('waqfOversightType:', formValue.waqfOversightType);
    console.log('waqfDescription:', formValue.waqfDescription);
    console.log('waqfAddress (raw):', JSON.stringify(formValue.waqfAddress));

    // Convert empty strings to null for optional fields to avoid backend validation issues
    formValue.email = formValue.email?.trim() || null;
    formValue.mobileNumber = formValue.mobileNumber?.trim() || null;

    // Clean up address objects - set to null if regionId/cityId are not filled
    formValue.residenceAddress = this.cleanAddressData(formValue.residenceAddress);
    formValue.workAddress = this.cleanAddressData(formValue.workAddress);
    formValue.businessAddress = this.cleanAddressData(formValue.businessAddress);
    formValue.companyAddress = this.cleanAddressData(formValue.companyAddress);
    formValue.ngoAddress = this.cleanAddressData(formValue.ngoAddress);
    formValue.waqfAddress = this.cleanAddressData(formValue.waqfAddress);
    formValue.selectedAddress = this.cleanAddressData(formValue.selectedAddress);
    formValue.customAddress = this.cleanAddressData(formValue.customAddress);

    // === DEBUG: Log Waqf address after cleaning ===
    console.log('=== WAQF ADDRESS AFTER CLEAN ===');
    console.log('waqfAddress (cleaned):', JSON.stringify(formValue.waqfAddress));

    // Clean up type-specific fields based on plaintiff type to avoid FK constraint violations
    const typeId = formValue.plaintiffTypeId;

    // Type 1 (Individual) fields - only keep for types 1, 2, 3
    if (![1, 2, 3].includes(typeId)) {
      formValue.identityTypeId = null;
      formValue.identityNumber = null;
      formValue.firstName = null;
      formValue.fatherName = null;
      formValue.grandfatherName = null;
      formValue.familyName = null;
      formValue.birthDate = null;
      formValue.gender = null;
      formValue.nationalityId = null;
      formValue.identityIssueDate = null;
      formValue.identityExpiryDate = null;
    }

    // Type 3 (Business Owner) & Type 4 (Registered Company) fields
    if (![3, 4].includes(typeId)) {
      formValue.commercialRegNumber = null;
      formValue.crStartDate = null;
      formValue.crEndDate = null;
    }

    // Type 5 (Unregistered Company) specific - map unregisteredCompanyName to companyName for backend
    if (typeId === 5) {
      // Backend expects companyName, but form uses unregisteredCompanyName for Type 5
      formValue.companyName = formValue.unregisteredCompanyName;
      // Also map other Type 5 fields to backend expected names
      formValue.countryId = formValue.unregisteredCountryId;
      formValue.description = formValue.unregisteredDescription;
    }

    // Type 5 (Unregistered Company) specific - companyName shared with types 3, 4, 5
    if (![3, 4, 5].includes(typeId)) {
      formValue.companyName = null;
    }

    // Clean up frontend-only Type 5 fields (not sent to backend)
    if (typeId !== 5) {
      formValue.unregisteredCompanyName = null;
      formValue.unregisteredCountryId = null;
      formValue.unregisteredCity = null;
      formValue.unregisteredDescription = null;
      formValue.unregisteredCompanyAddress = null;
    }

    // Type 6 (Government Agency) specific
    if (typeId !== 6) {
      formValue.governmentAgencyId = null;
      formValue.headquarters = null;
      formValue.additionalStatement = null;
    }

    // Type 7 (NGO/Society) specific
    if (typeId !== 7) {
      formValue.ngoName = null;
      formValue.licenseNumber = null;
      formValue.licenseSourceId = null;
      formValue.licenseDate = null;
    }

    // Type 8 (Waqf) specific
    if (typeId !== 8) {
      formValue.waqfName = null;
      formValue.courtDeedNumber = null;
      formValue.deedDate = null;
      formValue.deedSource = null;
      formValue.waqfOversightType = null;
      formValue.waqfDescription = null;
      formValue.waqfAgencyName = null;
    }

    return formValue;
  }

  /**
   * Cleans address data - returns null if address is incomplete (missing region OR city).
   * Both regionId AND cityId are required by the backend.
   */
  private cleanAddressData(address: any): any {
    if (!address || !address.regionId || !address.cityId) {
      return null;
    }
    return address;
  }

  /**
   * Gets list of invalid field names for user feedback
   */
  private getInvalidFields(): string[] {
    const invalidFields: string[] = [];
    const fieldLabels: { [key: string]: string } = {
      plaintiffTypeId: 'نوع المدعي',
      identityTypeId: 'نوع الهوية',
      identityNumber: 'رقم الهوية',
      firstName: 'الاسم الأول',
      fatherName: 'اسم الأب',
      grandfatherName: 'اسم الجد',
      clanName: 'اسم الفخذ',
      familyName: 'اسم العائلة',
      birthDate: 'تاريخ الميلاد',
      gender: 'الجنس',
      nationalityId: 'الجنسية',
      mobileNumber: 'رقم الجوال',
      email: 'البريد الإلكتروني',
      identityIssueDate: 'تاريخ إصدار الهوية',
      identityExpiryDate: 'تاريخ انتهاء الهوية',
      documentNumber: 'رقم الوثيقة',
      employmentStatusId: 'حالة العمل',
      employer: 'جهة العمل',
      profession: 'المهنة',
      commercialRegNumber: 'رقم السجل التجاري',
      companyName: 'اسم الشركة/المؤسسة',
      crStartDate: 'تاريخ بداية السجل',
      crEndDate: 'تاريخ نهاية السجل',
      unregisteredCompanyName: 'اسم الشركة',
      unregisteredCompanyAddress: 'عنوان الشركة',
      countryId: 'الدولة',
      unregisteredCity: 'المدينة',
      description: 'الوصف',
      governmentAgencyId: 'الجهة الحكومية',
      headquarters: 'المقر',
      additionalStatement: 'البيان الإضافي',
      ngoName: 'اسم الجمعية',
      licenseNumber: 'رقم الترخيص',
      licenseSourceId: 'مصدر الترخيص',
      licenseDate: 'تاريخ الترخيص',
      courtDeedNumber: 'رقم صك المحكمة',
      waqfName: 'اسم الوقف',
      deedDate: 'تاريخ الصك',
      deedSource: 'مصدر الصك',
      waqfOversightType: 'نظارة الوقف',
      waqfDescription: 'وصف الوقف',
      waqfAgencyName: 'اسم الجهة'
    };

    // Only check top-level controls (not nested address groups)
    Object.keys(this.plaintiffForm.controls).forEach(key => {
      const control = this.plaintiffForm.get(key);
      // Skip FormGroups (addresses) - only check simple controls
      if (control && control.invalid && !(control instanceof FormGroup)) {
        const label = fieldLabels[key] || key;
        invalidFields.push(`- ${label}`);
      }
    });

    return invalidFields;
  }

  onSaveAsDraft(): void {
    // Save the current state as draft (minimal validation - only plaintiff type required)
    const formValue = this.plaintiffForm.value;

    if (!formValue.plaintiffTypeId) {
      this.notification.validation('الرجاء اختيار نوع المدعي');
      return;
    }

    this.isSaving = true;

    // Map representatives - convert empty strings to null
    const mappedReps = this.representatives.map(rep => ({
      representativeTypeId: rep.representativeTypeId,
      identityTypeId: rep.identityTypeId || 1,
      identityNumber: rep.identityNumber,
      firstName: rep.firstName,
      fatherName: rep.fatherName,
      grandfatherName: rep.grandfatherName,
      familyName: rep.familyName,
      birthDate: rep.birthDate,
      mobileNumber: rep.mobileNumber?.trim() || null,
      email: rep.email?.trim() || null,
      authorizationNumber: rep.authorizationNumber,
      authorizationDate: rep.authorizationDate,
      authorizationSource: rep.authorizationSource,
      authorizationSourceType: rep.authorizationSourceType,
      guardianshipType: rep.guardianshipType
    }));

    // Convert empty strings to null for optional fields
    formValue.email = formValue.email?.trim() || null;
    formValue.mobileNumber = formValue.mobileNumber?.trim() || null;

    const createDTO = {
      ...formValue,
      representatives: mappedReps,
      isDraft: true
    };

    if (this.plaintiffId) {
      // Update existing plaintiff
      const updateDTO = { ...formValue, isDraft: true };
      this.plaintiffService.updatePlaintiff(this.plaintiffId, updateDTO).subscribe({
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

  // ===== Plaintiff Type Helper Methods (SRS Section 1.3 - 8 types) =====

  // Type 1 or 2 - فرد or فرد بدون هوية (Individual types)
  // Both types share name fields, address fields, contact fields
  isIndividual(): boolean {
    return this.selectedPlaintiffType === 1 || this.selectedPlaintiffType === 2;
  }

  // Type 1 - Individual with ID (same as isIndividual)
  isIndividualWithId(): boolean {
    return this.selectedPlaintiffType === 1;
  }

  // Type 2 - فرد بدون هوية (Individual without ID)
  isIndividualWithoutId(): boolean {
    return this.selectedPlaintiffType === 2;
  }

  // Type 3 - صاحب مؤسسة (Business Owner)
  isBusinessOwner(): boolean {
    return this.selectedPlaintiffType === 3;
  }

  // Type 4 - شركة مسجلة (Registered Company)
  isRegisteredCompany(): boolean {
    return this.selectedPlaintiffType === 4;
  }

  // Type 5 - شركة غير مسجلة (Unregistered Company)
  isUnregisteredCompany(): boolean {
    return this.selectedPlaintiffType === 5;
  }

  // Type 6 - جهة حكومية (Government Agency)
  isGovernmentAgency(): boolean {
    return this.selectedPlaintiffType === 6;
  }

  // Type 7 - جمعية/مؤسسة أهلية (Society/NGO)
  isNGO(): boolean {
    return this.selectedPlaintiffType === 7;
  }

  // Type 8 - وقف (Waqf)
  isWaqf(): boolean {
    return this.selectedPlaintiffType === 8;
  }

  // Alias for backwards compatibility
  isCompany(): boolean {
    // Returns true for any company type (Registered=4 or Unregistered=5)
    return this.selectedPlaintiffType === 4 || this.selectedPlaintiffType === 5;
  }

  // Check if Absher-fetched fields should be readonly
  // Fields are readonly when: plaintiff type is Individual (1) or Business Owner (3) AND data came from Absher
  isAbsherFieldsReadonly(): boolean {
    return (this.selectedPlaintiffType === 1 || this.selectedPlaintiffType === 3) && this.isAbsherVerified;
  }

  // Deprecated - kept for backwards compatibility
  isMinorOrIncapacitated(): boolean {
    return false; // Type doesn't exist in SRS
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
