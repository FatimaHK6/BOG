// Plaintiff Types
export interface PlaintiffType {
  id: number;
  name: string;
  nameAr: string;
}

// Plaintiff List View Model (for table display)
export interface PlaintiffListVM {
  id: number;
  plaintiffTypeId: number;
  plaintiffTypeName: string;
  plaintiffTypeNameAr?: string;
  identityNumber?: string;
  fullName: string;
  displayName?: string;
  representativesCount?: number;
  attachmentsCount?: number;
  attachmentStatus?: 'complete' | 'incomplete';
  isApplicant: boolean;
}

// Full Plaintiff View Model
export interface PlaintiffVM {
  id: number;
  plaintiffTypeId: number;
  plaintiffTypeName: string;
  plaintiffTypeNameAr: string;

  // Personal Data
  identityTypeId?: number;
  identityTypeName?: string;
  identityNumber?: string;
  documentNumber?: string; // رقم الوثيقة - for Type 2
  firstName?: string;
  fatherName?: string;
  grandfatherName?: string;
  clanName?: string; // اسم الفخذ
  familyName?: string;
  fullName: string;
  birthDate?: Date;
  gender?: string;
  nationalityId?: number;
  nationalityName?: string;
  identityIssueDate?: Date;
  identityExpiryDate?: Date;
  mobileNumber?: string;
  email?: string;
  dataSourceId?: number;
  dataSourceName?: string;
  isApplicant: boolean;

  // Employment Data
  employmentStatusId?: number; // حالة العمل
  employer?: string; // جهة العمل
  profession?: string; // المهنة

  // Commercial Registration
  commercialRegNumber?: string;
  companyName?: string;
  crStartDate?: Date;
  crEndDate?: Date;

  // Government Agency
  governmentAgencyId?: number;
  governmentAgencyName?: string;
  headquarters?: string; // المقر
  additionalStatement?: string;

  // NGO/Charity
  licenseNumber?: string;
  licenseSourceId?: number;
  licenseSourceName?: string;
  ngoName?: string;
  licenseDate?: Date;

  // Waqf
  courtDeedNumber?: string;
  waqfName?: string; // اسم الوقف
  deedDate?: Date;
  deedSource?: string;
  waqfOversightType?: string;
  waqfAgencyName?: string; // اسم الجهة
  waqfDescription?: string; // وصف الوقف

  // Unregistered Company
  unregisteredCompanyAddress?: string;
  countryId?: number;
  countryName?: string;
  unregisteredCompanyCity?: string;
  description?: string;

  // Addresses
  residenceAddress?: AddressVM;
  workAddress?: AddressVM;
  businessAddress?: AddressVM;
  companyAddress?: AddressVM;
  ngoAddress?: AddressVM;
  waqfAddress?: AddressVM;
  selectedAddress?: AddressVM;
  selectedAddressType?: string;

  // Related entities
  representatives: RepresentativeVM[];
  attachments: PlaintiffAttachmentVM[];
  createdDate: Date;
  modifiedDate: Date;
}

// Address View Model
export interface AddressVM {
  id: number;
  regionId?: number;
  regionName?: string;
  cityId?: number;
  cityName?: string;
  districtId?: number;
  districtName?: string;
  district?: string;           // Backend property name
  street?: string;
  streetName?: string;         // Backend property name
  buildingNumber?: string;
  unitNumber?: string;
  postalCode?: string;
  additionalCode?: string;
  additionalNumber?: string;   // Backend property name
  fullAddress?: string;
}

// Representative View Model
export interface RepresentativeVM {
  id: number;
  plaintiffId: number;
  representativeTypeId: number;
  representativeTypeName: string;
  representativeTypeNameAr: string;
  identityTypeId: number;
  identityTypeName: string;
  identityNumber: string;
  firstName?: string;
  fatherName?: string;
  grandfatherName?: string;
  familyName?: string;
  clanName?: string;          // اسم الفخذ - SRS 6.3.1
  fullName: string;
  birthDate?: Date;           // تاريخ الميلاد - SRS 6.3.1
  gender?: string;            // الجنس - SRS 6.3.1
  nationalityId?: number;     // الجنسية - SRS 6.3.1
  nationalityName?: string;
  identityIssueDate?: Date;   // تاريخ إصدار الهوية - SRS 6.3.1
  identityExpiryDate?: Date;  // تاريخ انتهاء الهوية - SRS 6.3.1

  // Residence Address (عنوان السكن - 6.3.2)
  residenceRegionId?: number;
  residenceRegionName?: string;
  residenceCityId?: number;
  residenceCityName?: string;
  residenceDistrict?: string;
  residenceStreet?: string;
  residenceBuildingNumber?: string;
  residenceUnitNumber?: string;
  residencePostalCode?: string;
  residenceAdditionalCode?: string;

  // Employment Data (بيانات العمل)
  employmentStatus?: string;  // government/private/unemployed
  employer?: string;          // جهة العمل
  profession?: string;        // المهنة

  // Work Address (عنوان العمل - 6.3.2)
  workRegionId?: number;
  workRegionName?: string;
  workCityId?: number;
  workCityName?: string;
  workDistrict?: string;
  workStreet?: string;
  workBuildingNumber?: string;
  workUnitNumber?: string;
  workPostalCode?: string;
  workAdditionalCode?: string;

  // Contact Info
  mobileNumber?: string;
  email?: string;
  dataSourceId?: number;
  dataSourceName?: string;

  // Lawyer/Agent (محامي/وكيل) authorization fields
  authorizationNumber?: string;
  authorizationDate?: Date;
  authorizationSource?: string;
  authorizationSourceType?: string;
  lawyerLicenseNumber?: string;
  lawyerLicenseDate?: Date;
  lawyerLicenseExpiryDate?: Date;
  // Liquidator (مصفي - 6.3.12) fields
  decisionNumber?: string;    // رقم القرار
  decisionDate?: Date;        // تاريخ القرار
  decisionSource?: string;    // مصدر القرار
  // Guardian (ولي - 6.3.16) fields
  deedNumber?: string;        // رقم الصك
  deedDate?: Date;            // تاريخ الصك
  deedSource?: string;        // مصدر الصك
  guardianshipType?: string;  // نوع الولاية
  // CompanyRepresentative fields (ممثل الشركة - Type 6)
  representationDocSource?: string;   // مصدر مستند التمثيل
  representativeCapacity?: string;    // صفة الممثل
  representationDocType?: string;     // نوع مستند التمثيل
  representationDocNumber?: string;   // رقم مستند التمثيل
  // AgencyRepresentative fields (ممثل الجهة - Type 7)
  representationLetterNumber?: string;  // رقم خطاب التمثيل
  representationLetterDate?: Date;      // تاريخ خطاب التمثيل
  representationLetterSource?: string;  // مصدر خطاب التمثيل
  isApplicant: boolean;
  createdDate: Date;
  attachments?: RepresentativeAttachmentVM[];
}

// Representative Attachment View Model (صورة التمثيل)
export interface RepresentativeAttachmentVM {
  id: number;
  representativeId: number;
  attachmentTypeId: number;
  attachmentTypeName: string;
  attachmentTypeNameAr: string;
  fileName: string;
  fileSizeBytes: number;
  contentType: string;
  downloadUrl?: string;
  uploadDate: Date;
  description?: string;
  file?: File; // Temporary file storage for upload
}

// Plaintiff Attachment View Model
export interface PlaintiffAttachmentVM {
  id: number;
  plaintiffId: number;
  attachmentTypeId: number;
  attachmentTypeName: string;
  attachmentTypeNameAr: string;
  fileName: string;
  fileSizeBytes: number;
  contentType: string;
  downloadUrl?: string;
  uploadDate: Date;
  description?: string; // User note/comment for the attachment (required)
  file?: File; // Temporary file storage for upload
}

// Create DTOs
export interface PlaintiffCreateDTO {
  plaintiffTypeId: number;

  // Personal Data (6.3.1) - for Individual types: 1, 2, 3
  identityTypeId?: number;
  identityNumber?: string;
  documentNumber?: string; // رقم الوثيقة - for Type 2 (Individual without ID)
  firstName?: string;
  fatherName?: string;
  grandfatherName?: string;
  clanName?: string; // اسم الفخذ - SRS 6.3.1
  familyName?: string;
  birthDate?: Date;
  gender?: string;
  nationalityId?: number;
  identityIssueDate?: Date;
  identityExpiryDate?: Date;
  mobileNumber?: string;
  email?: string;

  // Employment Data (6.3.9)
  employer?: string; // جهة العمل
  profession?: string; // المهنة

  // Commercial Registration (6.3.3) - for types: 3, 4, 5
  commercialRegNumber?: string;
  companyName?: string;
  crStartDate?: Date;
  crEndDate?: Date;

  // Government Agency (6.3.5) - for type 6
  governmentAgencyId?: number;
  headquarters?: string; // المقر - auto-filled based on agency
  additionalStatement?: string;

  // NGO/Charity (6.3.4) - Type 7
  licenseNumber?: string;
  licenseSourceId?: number;
  ngoName?: string; // اسم الجمعية
  licenseDate?: Date;

  // Waqf (6.3.11) - Type 8
  courtDeedNumber?: string;
  waqfName?: string; // اسم الوقف
  deedDate?: Date;
  deedSource?: string;
  waqfOversightType?: string; // خاصة/حكومية
  waqfAgencyName?: string; // اسم الجهة - required if WaqfOversightType is حكومية
  waqfDescription?: string; // وصف الوقف

  // Unregistered Company (6.3.7) - Type 5
  unregisteredCompanyAddress?: string; // عنوان الشركة
  countryId?: number; // الدولة
  unregisteredCompanyCity?: string; // المدينة
  description?: string; // وصف تقريبي

  // Addresses (6.3.2)
  residenceAddress?: AddressDTO;
  workAddress?: AddressDTO;
  businessAddress?: AddressDTO;
  companyAddress?: AddressDTO; // for Type 4 (Registered Company)
  ngoAddress?: AddressDTO; // for Type 7 (NGO)
  waqfAddress?: AddressDTO; // for Type 8 (Waqf)
  selectedAddressType?: string;
}

export interface PlaintiffUpdateDTO extends PlaintiffCreateDTO {
  id: number;
}

// Selected Address DTO
export interface SelectedAddressDTO {
  addressType: 'residence' | 'work' | 'other';
  customAddress?: AddressDTO;
}

export interface AddressDTO {
  regionId: number;
  cityId: number;
  districtId: number; // Changed from district: string to match backend FK
  street: string; // Renamed from streetName
  buildingNumber: string; // 4 digits
  unitNumber: string; // Added - digits only
  postalCode: string; // 5 digits
  additionalCode: string; // Renamed from additionalNumber - 4 digits
}
