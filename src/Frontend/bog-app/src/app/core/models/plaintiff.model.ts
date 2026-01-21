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
  identityTypeId?: number;
  identityTypeName?: string;
  identityNumber?: string;
  firstName?: string;
  fatherName?: string;
  grandfatherName?: string;
  familyName?: string;
  fullName: string;
  birthDate?: Date;
  gender?: string;
  nationalityId?: number;
  nationalityName?: string;
  mobileNumber?: string;
  email?: string;
  dataSourceId?: number;
  dataSourceName?: string;
  isApplicant: boolean;
  isDisabled: boolean;
  identityExpiryDate?: Date;
  identityIssueDate?: Date;
  commercialRegNumber?: string;
  companyName?: string;
  crStartDate?: Date;
  crEndDate?: Date;
  governmentAgencyId?: number;
  governmentAgencyName?: string;
  additionalStatement?: string;
  courtDeedNumber?: string;
  deedDate?: Date;
  deedSource?: string;
  waqfOversightType?: string;
  residenceAddress?: AddressVM;
  workAddress?: AddressVM;
  selectedAddress?: AddressVM;
  selectedAddressType?: string;
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
  districtName?: string;
  streetName?: string;
  buildingNumber?: string;
  postalCode?: string;
  additionalNumber?: string;
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
  fullName: string;
  mobileNumber?: string;
  email?: string;
  dataSourceId?: number;
  dataSourceName?: string;
  authorizationNumber?: string;
  authorizationDate?: Date;
  authorizationSource?: string;
  authorizationSourceType?: string;
  lawyerLicenseNumber?: string;
  lawyerLicenseDate?: Date;
  lawyerLicenseExpiryDate?: Date;
  guardianshipType?: string;
  isApplicant: boolean;
  createdDate: Date;
  birthDate?: Date;
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
  identityTypeId?: number;
  identityNumber?: string;
  firstName?: string;
  fatherName?: string;
  grandfatherName?: string;
  familyName?: string;
  birthDate?: Date;
  gender?: string;
  nationalityId?: number;
  mobileNumber?: string;
  email?: string;
  isDisabled?: boolean;
  identityExpiryDate?: Date;
  identityIssueDate?: Date;
  commercialRegNumber?: string;
  companyName?: string;
  crStartDate?: Date;
  crEndDate?: Date;
  governmentAgencyId?: number;
  additionalStatement?: string;
  // NGO/Charity (Type 7)
  ngoName?: string;
  licenseNumber?: string;
  licenseSourceId?: number;
  licenseSource?: string;
  licenseDate?: Date;
  // Waqf (Type 8)
  courtDeedNumber?: string;
  deedDate?: Date;
  deedSource?: string;
  waqfOversightType?: string;
  // Unregistered Company (Type 5)
  unregisteredCompanyName?: string;
  unregisteredCountryId?: number;
  unregisteredCity?: string;
  unregisteredDescription?: string;
  // Addresses
  residenceAddress?: AddressDTO;
  workAddress?: AddressDTO;
  businessAddress?: AddressDTO;
  customAddress?: AddressDTO;
  selectedAddressType?: string;
  employmentSector?: string;
  employerName?: string;
  occupation?: string;
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
  regionId?: number;
  cityId?: number;
  district?: string;
  streetName?: string;
  buildingNumber?: string;
  postalCode?: string;
  additionalNumber?: string;
}
