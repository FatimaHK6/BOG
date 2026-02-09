import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { RepresentativeVM, RepresentativeAttachmentVM } from '../../../../../core/models/plaintiff.model';
import { RepresentativeType, ALLOWED_REPRESENTATIVE_TYPES } from '../../../../../core/models/representative.model';
import { IdentityType, Region, City, Nationality } from '../../../../../core/models/lookup.model';
import { LookupService } from '../../../../../core/services/lookup.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

export interface RepresentativeDialogData {
  representative?: RepresentativeVM;
  plaintiffId: number;
  plaintiffTypeId: number;
  representativeTypeId?: number;  // Pre-selected type from menu
  plaintiffIdentityNumber?: string;  // For ERR012 validation
  existingRepresentatives?: RepresentativeVM[];  // For ERR008 validation
}

@Component({
  selector: 'app-representative-dialog',
  templateUrl: './representative-dialog.component.html',
  styleUrls: ['./representative-dialog.component.scss']
})
export class RepresentativeDialogComponent implements OnInit {
  representativeForm!: FormGroup;
  representativeTypes: RepresentativeType[] = [];
  identityTypes: IdentityType[] = [];
  regions: Region[] = [];
  residenceCities: City[] = [];
  workCities: City[] = [];
  nationalities: Nationality[] = [];
  filteredNationalities: Nationality[] = [];
  isLoading = false;
  isLookingUp = false;
  isEditMode = false;
  isFromAbsher = false;  // Track if data came from Absher
  selectedRepTypeName = '';  // Display name of pre-selected type

  // Employment status options
  employmentStatusOptions = [
    { value: 'government', label: 'حكومي' },
    { value: 'private', label: 'خاص' },
    { value: 'unemployed', label: 'بدون عمل' }
  ];

  // Attachment properties
  attachments: RepresentativeAttachmentVM[] = [];
  isDragOver = false;
  isUploading = false;
  readonly MAX_FILE_SIZE = 4 * 1024 * 1024; // 4MB
  readonly ALLOWED_TYPES = ['application/pdf'];

  // Saudi nationality ID constant
  readonly SAUDI_NATIONALITY_ID = 1;

  constructor(
    private fb: FormBuilder,
    private lookupService: LookupService,
    private dialog: MatDialog,
    private notification: NotificationService,
    private dialogRef: MatDialogRef<RepresentativeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RepresentativeDialogData
  ) {
    this.isEditMode = !!data.representative;
  }

  ngOnInit(): void {
    this.initForm();
    this.loadLookups();

    // Set pre-selected representative type from menu
    if (this.data.representativeTypeId && !this.isEditMode) {
      this.representativeForm.patchValue({ representativeTypeId: this.data.representativeTypeId });
    }

    if (this.isEditMode && this.data.representative) {
      this.populateForm(this.data.representative);
    }
  }

  private initForm(): void {
    this.representativeForm = this.fb.group({
      representativeTypeId: [null, Validators.required],
      identityTypeId: [1, Validators.required],
      identityNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      firstName: ['', Validators.required],
      fatherName: ['', Validators.required],  // SRS: Required
      grandfatherName: [''],
      familyName: ['', Validators.required],
      clanName: [''],           // اسم الفخذ - SRS 6.3.1
      birthDate: [null, Validators.required],        // SRS: Required
      gender: ['', Validators.required],             // SRS: Required
      nationalityId: [null, Validators.required],    // SRS: Required
      identityIssueDate: [null, Validators.required], // SRS: Required
      identityExpiryDate: [null, Validators.required], // SRS: Required

      // Residence Address (عنوان السكن - 6.3.2) - ALL fields required
      residenceRegionId: [null, Validators.required],
      residenceCityId: [null, Validators.required],
      residenceDistrict: ['', [Validators.required, Validators.maxLength(100)]],
      residenceStreet: ['', [Validators.required, Validators.maxLength(200)]],
      residenceBuildingNumber: ['', [Validators.required, Validators.pattern(/^\d{4}$/)]],
      residenceUnitNumber: ['', [Validators.required, Validators.pattern(/^\d+$/)]],
      residencePostalCode: ['', [Validators.required, Validators.pattern(/^\d{5}$/)]],
      residenceAdditionalCode: ['', [Validators.required, Validators.pattern(/^\d{4}$/)]],

      // Employment Data (بيانات العمل)
      employmentStatus: ['', Validators.required],
      employer: [''],           // BC01: conditional
      profession: ['', Validators.required],

      // Work Address (عنوان العمل - 6.3.2) - BC02: conditional
      workRegionId: [null],
      workCityId: [null],
      workDistrict: [''],
      workStreet: [''],
      workBuildingNumber: ['', Validators.pattern(/^\d{0,4}$/)],
      workUnitNumber: [''],
      workPostalCode: ['', Validators.pattern(/^\d{0,5}$/)],
      workAdditionalCode: ['', Validators.pattern(/^\d{0,4}$/)],

      // Contact Info - SRS: Mobile required with pattern
      mobileNumber: ['', [Validators.required, Validators.pattern(/^05\d{8}$/)]],
      email: ['', Validators.email],

      // Lawyer authorization fields
      authorizationNumber: [''],
      authorizationDate: [null],
      authorizationSource: [''],
      authorizationSourceType: [''],
      lawyerLicenseNumber: [''],
      lawyerLicenseDate: [null],
      lawyerLicenseExpiryDate: [null],
      // Liquidator fields (6.3.12)
      decisionNumber: [''],
      decisionDate: [null],
      decisionSource: [''],
      // Guardian fields (6.3.16)
      deedNumber: [''],
      deedDate: [null],
      deedSource: [''],
      guardianshipType: [''],
      // CompanyRepresentative fields (ممثل الشركة - Type 6)
      representationDocSource: [''],
      representativeCapacity: [''],
      representationDocType: [''],
      representationDocNumber: [''],
      // AgencyRepresentative fields (ممثل الجهة - Type 7)
      representationLetterNumber: [''],
      representationLetterDate: [null],
      representationLetterSource: ['']
    });

    // Subscribe to employment status changes for BC01 and BC02
    this.representativeForm.get('employmentStatus')?.valueChanges.subscribe(status => {
      this.onEmploymentStatusChange(status);
    });

    // Subscribe to identity type changes for nationality conditional logic
    this.representativeForm.get('identityTypeId')?.valueChanges.subscribe(typeId => {
      this.onIdentityTypeChange(typeId);
    });

    // Subscribe to representative type changes for type-specific field validation
    this.representativeForm.get('representativeTypeId')?.valueChanges.subscribe(typeId => {
      this.onRepresentativeTypeChange(typeId);
    });

    // Initialize nationality based on default identity type (National ID = 1)
    this.onIdentityTypeChange(1);
  }

  // Handle identity type change for nationality conditional logic
  private onIdentityTypeChange(identityTypeId: number): void {
    const nationalityControl = this.representativeForm.get('nationalityId');

    if (identityTypeId === 1) {
      // National ID (هوية وطنية): Set Saudi nationality and disable
      nationalityControl?.setValue(this.SAUDI_NATIONALITY_ID);
      nationalityControl?.disable();
      this.filteredNationalities = this.nationalities.filter(n => n.id === this.SAUDI_NATIONALITY_ID);
    } else if (identityTypeId === 2) {
      // Resident ID (هوية مقيم): Enable and filter out Saudi
      nationalityControl?.enable();
      nationalityControl?.setValue(null);
      this.filteredNationalities = this.nationalities.filter(n => n.id !== this.SAUDI_NATIONALITY_ID);
    } else {
      // Other types: Enable all nationalities
      nationalityControl?.enable();
      this.filteredNationalities = [...this.nationalities];
    }
  }

  // Handle representative type change for dynamic field validation
  private onRepresentativeTypeChange(typeId: number): void {
    // Clear all type-specific validators first
    this.clearTypeSpecificValidators();

    // Apply validators based on representative type (IDs from database)
    // 1: Agent (وكيل), 2: Guardian (ولي), 3: Custodian (وصي), 4: Executor (ناظر)
    // 5: HeirRepresentative (ممثل الورثة), 6: CompanyRepresentative (ممثل الشركة)
    // 7: AgencyRepresentative (ممثل الجهة), 8: Trustee (أمين التفليسة), 9: LegalRepresentative (ممثل نظامي)
    if (typeId === 1) {
      // Agent (وكيل): Authorization fields required
      this.setLawyerFieldsRequired(true);
    } else if (typeId === 2) {
      // Guardian (ولي): Deed + guardianship type fields required
      this.setGuardianFieldsRequired(true);
    } else if (typeId === 3) {
      // Custodian (وصي): Deed fields required (no guardianship type)
      this.setCustodianFieldsRequired(true);
    } else if (typeId === 4) {
      // Executor (ناظر): Decision fields required
      this.setLiquidatorFieldsRequired(true);
    } else if (typeId === 6) {
      // CompanyRepresentative (ممثل الشركة): Document fields required
      this.setCompanyRepresentativeFieldsRequired(true);
    } else if (typeId === 7) {
      // AgencyRepresentative (ممثل الجهة): Letter fields required
      this.setAgencyRepresentativeFieldsRequired(true);
    } else if (typeId === 9) {
      // LegalRepresentative (ممثل نظامي): Document fields required (same as type 6)
      this.setCompanyRepresentativeFieldsRequired(true);
    } else if (typeId === 10) {
      // Liquidator (مصفي): Decision fields required
      this.setMussaffiFieldsRequired(true);
    } else if (typeId === 11) {
      // JudicialCustodian (حارس قضائي): Decision fields required
      this.setJudicialCustodianFieldsRequired(true);
    }
  }

  private clearTypeSpecificValidators(): void {
    // Clear lawyer fields
    ['authorizationNumber', 'authorizationDate', 'authorizationSource', 'authorizationSourceType'].forEach(field => {
      this.representativeForm.get(field)?.clearValidators();
      this.representativeForm.get(field)?.updateValueAndValidity();
    });
    // Clear liquidator fields
    ['decisionNumber', 'decisionDate', 'decisionSource'].forEach(field => {
      this.representativeForm.get(field)?.clearValidators();
      this.representativeForm.get(field)?.updateValueAndValidity();
    });
    // Clear company representative fields
    ['representationDocSource', 'representativeCapacity', 'representationDocType', 'representationDocNumber'].forEach(field => {
      this.representativeForm.get(field)?.clearValidators();
      this.representativeForm.get(field)?.updateValueAndValidity();
    });
    // Clear custodian fields (deed only, no guardianshipType)
    ['deedNumber', 'deedDate', 'deedSource'].forEach(field => {
      this.representativeForm.get(field)?.clearValidators();
      this.representativeForm.get(field)?.updateValueAndValidity();
    });
    // Clear guardian fields (includes guardianshipType)
    ['guardianshipType'].forEach(field => {
      this.representativeForm.get(field)?.clearValidators();
      this.representativeForm.get(field)?.updateValueAndValidity();
    });
    // Clear agency representative fields
    ['representationLetterNumber', 'representationLetterDate', 'representationLetterSource'].forEach(field => {
      this.representativeForm.get(field)?.clearValidators();
      this.representativeForm.get(field)?.updateValueAndValidity();
    });
  }

  private setLawyerFieldsRequired(required: boolean): void {
    const fields = ['authorizationNumber', 'authorizationDate', 'authorizationSource', 'authorizationSourceType'];
    fields.forEach(field => {
      const control = this.representativeForm.get(field);
      if (required) {
        control?.setValidators(Validators.required);
      } else {
        control?.clearValidators();
      }
      control?.updateValueAndValidity();
    });
  }

  private setLiquidatorFieldsRequired(required: boolean): void {
    const fields = ['decisionNumber', 'decisionDate', 'decisionSource'];
    fields.forEach(field => {
      const control = this.representativeForm.get(field);
      if (required) {
        if (field === 'decisionNumber') {
          control?.setValidators([Validators.required, Validators.maxLength(20)]);
        } else if (field === 'decisionSource') {
          control?.setValidators([Validators.required, Validators.maxLength(200)]);
        } else {
          control?.setValidators(Validators.required);
        }
      } else {
        control?.clearValidators();
      }
      control?.updateValueAndValidity();
    });
  }

  private setCompanyRepresentativeFieldsRequired(required: boolean): void {
    const fields = ['representationDocSource', 'representativeCapacity', 'representationDocType', 'representationDocNumber'];
    fields.forEach(field => {
      const control = this.representativeForm.get(field);
      if (required) {
        if (field === 'representationDocSource') {
          control?.setValidators([Validators.required, Validators.maxLength(200)]);
        } else if (field === 'representationDocNumber') {
          control?.setValidators([Validators.required, Validators.maxLength(20)]);
        } else {
          control?.setValidators(Validators.required);
        }
      } else {
        control?.clearValidators();
      }
      control?.updateValueAndValidity();
    });
  }

  private setCustodianFieldsRequired(required: boolean): void {
    const fields = ['deedNumber', 'deedDate', 'deedSource'];
    fields.forEach(field => {
      const control = this.representativeForm.get(field);
      if (required) {
        if (field === 'deedNumber') {
          control?.setValidators([Validators.required, Validators.maxLength(20)]);
        } else if (field === 'deedSource') {
          control?.setValidators([Validators.required, Validators.maxLength(200)]);
        } else {
          control?.setValidators(Validators.required);
        }
      } else {
        control?.clearValidators();
      }
      control?.updateValueAndValidity();
    });
  }

  private setGuardianFieldsRequired(required: boolean): void {
    const fields = ['deedNumber', 'deedDate', 'deedSource', 'guardianshipType'];
    fields.forEach(field => {
      const control = this.representativeForm.get(field);
      if (required) {
        control?.setValidators(Validators.required);
      } else {
        control?.clearValidators();
      }
      control?.updateValueAndValidity();
    });
  }

  private setMussaffiFieldsRequired(required: boolean): void {
    const fields = ['decisionNumber', 'decisionDate', 'decisionSource'];
    fields.forEach(field => {
      const control = this.representativeForm.get(field);
      if (required) {
        if (field === 'decisionNumber' || field === 'decisionSource') {
          control?.setValidators([Validators.required, Validators.maxLength(20)]);
        } else {
          control?.setValidators(Validators.required);
        }
      } else {
        control?.clearValidators();
      }
      control?.updateValueAndValidity();
    });
  }

  private setJudicialCustodianFieldsRequired(required: boolean): void {
    const fields = ['decisionNumber', 'decisionDate', 'decisionSource'];
    fields.forEach(field => {
      const control = this.representativeForm.get(field);
      if (required) {
        if (field === 'decisionNumber' || field === 'decisionSource') {
          control?.setValidators([Validators.required, Validators.maxLength(20)]);
        } else {
          control?.setValidators(Validators.required);
        }
      } else {
        control?.clearValidators();
      }
      control?.updateValueAndValidity();
    });
  }

  private setAgencyRepresentativeFieldsRequired(required: boolean): void {
    const fields = ['representationLetterNumber', 'representationLetterDate', 'representationLetterSource'];
    fields.forEach(field => {
      const control = this.representativeForm.get(field);
      if (required) {
        if (field === 'representationLetterNumber') {
          control?.setValidators([Validators.required, Validators.maxLength(20)]);
        } else if (field === 'representationLetterSource') {
          control?.setValidators([Validators.required, Validators.maxLength(200)]);
        } else {
          control?.setValidators(Validators.required);
        }
      } else {
        control?.clearValidators();
      }
      control?.updateValueAndValidity();
    });
  }

  private onEmploymentStatusChange(status: string): void {
    const employerControl = this.representativeForm.get('employer');
    const workRegionControl = this.representativeForm.get('workRegionId');
    const workCityControl = this.representativeForm.get('workCityId');

    const professionControl = this.representativeForm.get('profession');

    // Employer and profession required if government or private, hidden if unemployed
    if (status === 'government' || status === 'private') {
      employerControl?.setValidators(Validators.required);
      professionControl?.setValidators(Validators.required);
    } else {
      employerControl?.clearValidators();
      employerControl?.setValue('');
      professionControl?.clearValidators();
      professionControl?.setValue('');
    }
    employerControl?.updateValueAndValidity();
    professionControl?.updateValueAndValidity();

    // Work address - ALL fields required only when private
    if (status === 'private') {
      workRegionControl?.setValidators(Validators.required);
      workCityControl?.setValidators(Validators.required);
      this.representativeForm.get('workDistrict')?.setValidators([Validators.required, Validators.maxLength(100)]);
      this.representativeForm.get('workStreet')?.setValidators([Validators.required, Validators.maxLength(200)]);
      this.representativeForm.get('workBuildingNumber')?.setValidators([Validators.required, Validators.pattern(/^\d{4}$/)]);
      this.representativeForm.get('workUnitNumber')?.setValidators([Validators.required, Validators.pattern(/^\d+$/)]);
      this.representativeForm.get('workPostalCode')?.setValidators([Validators.required, Validators.pattern(/^\d{5}$/)]);
      this.representativeForm.get('workAdditionalCode')?.setValidators([Validators.required, Validators.pattern(/^\d{4}$/)]);
    } else {
      workRegionControl?.clearValidators();
      workCityControl?.clearValidators();
      this.representativeForm.get('workDistrict')?.clearValidators();
      this.representativeForm.get('workStreet')?.clearValidators();
      this.representativeForm.get('workBuildingNumber')?.clearValidators();
      this.representativeForm.get('workUnitNumber')?.clearValidators();
      this.representativeForm.get('workPostalCode')?.clearValidators();
      this.representativeForm.get('workAdditionalCode')?.clearValidators();
      // Clear work address fields
      this.representativeForm.patchValue({
        workRegionId: null,
        workCityId: null,
        workDistrict: '',
        workStreet: '',
        workBuildingNumber: '',
        workUnitNumber: '',
        workPostalCode: '',
        workAdditionalCode: ''
      });
    }
    // Update validity for all work address fields
    ['workRegionId', 'workCityId', 'workDistrict', 'workStreet', 'workBuildingNumber', 'workUnitNumber', 'workPostalCode', 'workAdditionalCode'].forEach(field => {
      this.representativeForm.get(field)?.updateValueAndValidity();
    });
  }

  private loadLookups(): void {
    this.lookupService.getRepresentativeTypes().subscribe(types => {
      // Filter representative types based on plaintiff type
      const allowedIds = ALLOWED_REPRESENTATIVE_TYPES[this.data.plaintiffTypeId] || [];
      this.representativeTypes = types.filter(t => allowedIds.includes(t.id));

      // Set type name for readonly display
      const typeId = this.data.representativeTypeId || this.data.representative?.representativeTypeId;
      if (typeId) {
        const selectedType = this.representativeTypes.find(t => t.id === typeId);
        this.selectedRepTypeName = selectedType?.nameAr || '';
      }
    });

    this.lookupService.getIdentityTypes().subscribe(types => {
      // Filter out passport (id=3) for representatives - SRS requirement
      this.identityTypes = types.filter(t => t.id !== 3);
    });

    // Load regions for address dropdowns
    this.lookupService.getRegions().subscribe(regions => {
      this.regions = regions;
    });

    // Load nationalities
    this.lookupService.getNationalities().subscribe(nationalities => {
      this.nationalities = nationalities;
      // Initialize filtered nationalities based on current identity type
      const currentIdentityType = this.representativeForm.get('identityTypeId')?.value;
      this.onIdentityTypeChange(currentIdentityType || 1);
    });
  }

  onResidenceRegionChange(regionId: number, preserveCity = false): void {
    if (!preserveCity) {
      this.representativeForm.patchValue({ residenceCityId: null });
    }
    if (regionId) {
      this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
        this.residenceCities = cities;
      });
    } else {
      this.residenceCities = [];
    }
  }

  onWorkRegionChange(regionId: number, preserveCity = false): void {
    if (!preserveCity) {
      this.representativeForm.patchValue({ workCityId: null });
    }
    if (regionId) {
      this.lookupService.getCitiesByRegion(regionId).subscribe(cities => {
        this.workCities = cities;
      });
    } else {
      this.workCities = [];
    }
  }

  // BC01: Show employer field if employment status is government or private
  showEmployerField(): boolean {
    const status = this.representativeForm.get('employmentStatus')?.value;
    return status === 'government' || status === 'private';
  }

  // Show work address section only if employment status is private
  showWorkAddress(): boolean {
    const status = this.representativeForm.get('employmentStatus')?.value;
    return status === 'private';
  }

  private populateForm(rep: RepresentativeVM): void {
    this.representativeForm.patchValue({
      representativeTypeId: rep.representativeTypeId,
      identityTypeId: rep.identityTypeId,
      identityNumber: rep.identityNumber,
      firstName: rep.firstName,
      fatherName: rep.fatherName,
      grandfatherName: rep.grandfatherName,
      familyName: rep.familyName,
      clanName: rep.clanName,
      birthDate: rep.birthDate,
      gender: rep.gender,
      nationalityId: rep.nationalityId,
      identityIssueDate: rep.identityIssueDate,
      identityExpiryDate: rep.identityExpiryDate,
      // Residence Address
      residenceRegionId: rep.residenceRegionId,
      residenceCityId: rep.residenceCityId,
      residenceDistrict: rep.residenceDistrict,
      residenceStreet: rep.residenceStreet,
      residenceBuildingNumber: rep.residenceBuildingNumber,
      residenceUnitNumber: rep.residenceUnitNumber,
      residencePostalCode: rep.residencePostalCode,
      residenceAdditionalCode: rep.residenceAdditionalCode,
      // Employment Data
      employmentStatus: rep.employmentStatus,
      employer: rep.employer,
      profession: rep.profession,
      // Work Address
      workRegionId: rep.workRegionId,
      workCityId: rep.workCityId,
      workDistrict: rep.workDistrict,
      workStreet: rep.workStreet,
      workBuildingNumber: rep.workBuildingNumber,
      workUnitNumber: rep.workUnitNumber,
      workPostalCode: rep.workPostalCode,
      workAdditionalCode: rep.workAdditionalCode,
      // Contact Info
      mobileNumber: rep.mobileNumber,
      email: rep.email,
      authorizationNumber: rep.authorizationNumber,
      authorizationDate: rep.authorizationDate,
      authorizationSource: rep.authorizationSource,
      authorizationSourceType: rep.authorizationSourceType,
      lawyerLicenseNumber: rep.lawyerLicenseNumber,
      lawyerLicenseDate: rep.lawyerLicenseDate,
      lawyerLicenseExpiryDate: rep.lawyerLicenseExpiryDate,
      decisionNumber: rep.decisionNumber,
      decisionDate: rep.decisionDate,
      decisionSource: rep.decisionSource,
      deedNumber: rep.deedNumber,
      deedDate: rep.deedDate,
      deedSource: rep.deedSource,
      guardianshipType: rep.guardianshipType,
      representationDocSource: rep.representationDocSource,
      representativeCapacity: rep.representativeCapacity,
      representationDocType: rep.representationDocType,
      representationDocNumber: rep.representationDocNumber,
      representationLetterNumber: rep.representationLetterNumber,
      representationLetterDate: rep.representationLetterDate,
      representationLetterSource: rep.representationLetterSource
    });

    // Load cities for address dropdowns if region is set (preserve city selection)
    if (rep.residenceRegionId) {
      this.onResidenceRegionChange(rep.residenceRegionId, true);
    }
    if (rep.workRegionId) {
      this.onWorkRegionChange(rep.workRegionId, true);
    }

    // Load existing attachments
    if (rep.attachments) {
      this.attachments = [...rep.attachments];
    }
  }

  onLookupIdentity(): void {
    const identityNumber = this.representativeForm.get('identityNumber')?.value;
    if (!identityNumber) return;

    this.isLookingUp = true;
    // Simulate Absher lookup
    setTimeout(() => {
      // Demo data for testing
      this.representativeForm.patchValue({
        firstName: 'أحمد',
        fatherName: 'محمد',
        grandfatherName: 'عبدالله',
        familyName: 'الشمري',
        clanName: 'شمر',
        birthDate: new Date(1990, 0, 15),
        identityIssueDate: new Date(2020, 5, 1),
        identityExpiryDate: new Date(2030, 5, 1)
      });

      // Lock Absher fields - SRS requirement
      this.isFromAbsher = true;
      this.lockAbsherFields();

      this.isLookingUp = false;
      this.notification.success('تم جلب البيانات من أبشر');
    }, 1000);
  }

  // Lock fields after Absher lookup - SRS requirement
  private lockAbsherFields(): void {
    const fieldsToLock = [
      'identityTypeId', 'identityNumber', 'birthDate',
      'firstName', 'fatherName', 'grandfatherName', 'clanName', 'familyName',
      'identityIssueDate', 'identityExpiryDate'
    ];
    fieldsToLock.forEach(field => {
      this.representativeForm.get(field)?.disable();
    });
  }

  // Unlock Absher fields (for reset or edit)
  private unlockAbsherFields(): void {
    const fieldsToUnlock = [
      'identityTypeId', 'identityNumber', 'birthDate',
      'firstName', 'fatherName', 'grandfatherName', 'clanName', 'familyName',
      'identityIssueDate', 'identityExpiryDate'
    ];
    fieldsToUnlock.forEach(field => {
      this.representativeForm.get(field)?.enable();
    });
    this.isFromAbsher = false;
  }

  // Reset Absher data and unlock fields
  onResetAbsherData(): void {
    this.unlockAbsherFields();
    this.representativeForm.patchValue({
      firstName: '',
      fatherName: '',
      grandfatherName: '',
      familyName: '',
      clanName: '',
      birthDate: null,
      identityIssueDate: null,
      identityExpiryDate: null
    });
    this.notification.info('تم إلغاء بيانات أبشر');
  }

  onSave(): void {
    if (this.representativeForm.invalid) {
      this.representativeForm.markAllAsTouched();
      this.notification.validation('يرجى تعبئة جميع الحقول المطلوبة');
      return;
    }

    // Get form value including disabled fields (for Absher locked fields)
    const formValue = this.representativeForm.getRawValue();
    const identityNumber = formValue.identityNumber;

    // Attachment validation - SRS: Required
    if (this.attachments.length === 0) {
      this.notification.validation('يجب إرفاق صورة التمثيل (PDF)');
      return;
    }

    // ERR012: Representative identity cannot be same as plaintiff identity (for Individual type: 1, 2)
    const isIndividualPlaintiff = this.data.plaintiffTypeId === 1 || this.data.plaintiffTypeId === 2;
    if (isIndividualPlaintiff && this.data.plaintiffIdentityNumber && identityNumber === this.data.plaintiffIdentityNumber) {
      this.notification.error('ERR012: لا يمكن إضافة ممثّل بنفس رقم هوية المدّعي');
      return;
    }

    // ERR008: Cannot add representative that already exists for this plaintiff
    if (!this.isEditMode && this.data.existingRepresentatives) {
      const existingRep = this.data.existingRepresentatives.find(r => r.identityNumber === identityNumber);
      if (existingRep) {
        this.notification.error('ERR008: الممثّل موجود مسبقاً للمدّعي على الطلب');
        return;
      }
    }

    // Validate identity expiry date is after issue date
    if (formValue.identityIssueDate && formValue.identityExpiryDate) {
      const issueDate = new Date(formValue.identityIssueDate);
      const expiryDate = new Date(formValue.identityExpiryDate);
      if (expiryDate <= issueDate) {
        this.notification.validation('تاريخ انتهاء الهوية يجب أن يكون بعد تاريخ الإصدار');
        return;
      }
    }

    // Validate birth date is not in the future
    if (formValue.birthDate) {
      const birthDate = new Date(formValue.birthDate);
      if (birthDate > new Date()) {
        this.notification.validation('تاريخ الميلاد لا يمكن أن يكون في المستقبل');
        return;
      }
    }

    const fullName = [formValue.firstName, formValue.fatherName, formValue.grandfatherName, formValue.familyName]
      .filter(n => n)
      .join(' ');

    // Convert empty strings to null for optional fields to avoid backend validation issues
    const representative: any = {
      ...formValue,
      email: formValue.email?.trim() || null,
      mobileNumber: formValue.mobileNumber?.trim() || null,
      fullName,
      plaintiffId: this.data.plaintiffId,
      attachments: this.attachments,
      isFromAbsher: this.isFromAbsher
    };

    if (this.isEditMode && this.data.representative) {
      representative.id = this.data.representative.id;
    }

    this.dialogRef.close(representative);
  }

  onCancel(): void {
    if (this.representativeForm.dirty) {
      const dialogData: ConfirmDialogData = {
        title: 'تغييرات غير محفوظة',
        message: 'لديك تغييرات غير محفوظة. هل أنت متأكد من المغادرة بدون حفظ؟',
        confirmText: 'مغادرة',
        cancelText: 'البقاء',
        confirmColor: 'warn',
        icon: 'warning'
      };

      const confirmDialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '400px',
        data: dialogData
      });

      confirmDialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.dialogRef.close();
        }
      });
    } else {
      this.dialogRef.close();
    }
  }

  // Check if authorization fields should be shown
  showAuthorizationFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    // Agent/Lawyer (1) requires authorization/power of attorney
    return typeId === 1;
  }

  // Check if guardianship type should be shown
  showGuardianshipType(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    // Guardian (2) requires guardianship type (ولي - نوع الولاية)
    return typeId === 2;
  }

  // Check if company representative fields should be shown (ممثل الشركة - type 6)
  showCompanyRepresentativeFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    return typeId === 6;
  }

  // Check if custodian fields should be shown (وصي - type 3)
  showCustodianFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    return typeId === 3;
  }

  // Check if liquidator fields should be shown (أمين التفليسة - type 8)
  showLiquidatorFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    return typeId === 8;
  }

  // Check if executor fields should be shown (ناظر - type 4)
  showExecutorFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    return typeId === 4;
  }

  // Check if legal representative fields should be shown (ممثل نظامي - type 9)
  showLegalRepresentativeFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    return typeId === 9;
  }

  // Check if liquidator fields should be shown (مصفي - type 10)
  showMussaffiFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    return typeId === 10;
  }

  // Check if judicial custodian fields should be shown (حارس قضائي - type 11)
  showJudicialCustodianFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    return typeId === 11;
  }

  // Check if agency representative fields should be shown (ممثل الجهة - type 7)
  showAgencyRepresentativeFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    return typeId === 7;
  }

  // Check if guardian deed fields should be shown (ولي - 6.3.16)
  showGuardianFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    // Guardian (2) requires deed number, date, source, guardianship type
    return typeId === 2;
  }

  // ========== Attachment Methods ==========

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.processFile(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.processFile(input.files[0]);
      input.value = ''; // Reset for re-selecting same file
    }
  }

  private processFile(file: File): void {
    // Validate file type
    if (!this.ALLOWED_TYPES.includes(file.type)) {
      this.notification.validation('يجب أن يكون الملف بصيغة PDF فقط');
      return;
    }

    // Validate file size
    if (file.size > this.MAX_FILE_SIZE) {
      this.notification.validation('حجم الملف يتجاوز الحد الأقصى (4 ميجابايت)');
      return;
    }

    // Directly add attachment without notes dialog
    this.addAttachment(file);
  }

  private addAttachment(file: File): void {
    const newAttachment: RepresentativeAttachmentVM = {
      id: Date.now(), // Temporary ID
      representativeId: 0,
      attachmentTypeId: 10, // Representation document type
      attachmentTypeName: 'Representation Document',
      attachmentTypeNameAr: 'صورة التمثيل',
      fileName: file.name,
      fileSizeBytes: file.size,
      contentType: file.type,
      downloadUrl: URL.createObjectURL(file),
      uploadDate: new Date(),
      file: file
    };

    this.attachments = [...this.attachments, newAttachment];

    this.notification.success('تم إضافة المرفق');
  }

  onViewAttachment(attachment: RepresentativeAttachmentVM): void {
    if (attachment.downloadUrl) {
      window.open(attachment.downloadUrl, '_blank');
    }
  }

  onDeleteAttachment(index: number): void {
    const dialogData: ConfirmDialogData = {
      title: 'تأكيد الحذف',
      message: `هل أنت متأكد من حذف المرفق "${this.attachments[index].fileName}"؟`,
      confirmText: 'حذف',
      cancelText: 'إلغاء',
      confirmColor: 'warn',
      icon: 'delete'
    };

    const confirmDialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    confirmDialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.attachments = this.attachments.filter((_, i) => i !== index);
        this.notification.success('تم حذف المرفق');
      }
    });
  }

  formatFileSize(bytes: number): string {
    if (!bytes || isNaN(bytes)) return '0 B';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}
