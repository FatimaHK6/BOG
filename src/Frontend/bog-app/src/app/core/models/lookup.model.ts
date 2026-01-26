// Generic Lookup Item
export interface LookupItem {
  id: number;
  name: string;
  nameAr: string;
}

// Identity Types
export interface IdentityType extends LookupItem { }

// Nationalities
export interface Nationality extends LookupItem { }

// Regions
export interface Region extends LookupItem { }

// Cities
export interface City extends LookupItem {
  regionId: number;
}

// Government Agencies
export interface GovernmentAgency extends LookupItem { }

// Courts
export interface Court extends LookupItem {
  cityId: number;
}

// Absher Verification Result
export interface AbsherVerificationResult {
  isVerified: boolean;
  errorCode?: string;
  errorMessage?: string;
  errorMessageAr?: string;
}

// Person Data from Absher
export interface AbsherPersonData {
  firstName: string;
  fatherName: string;
  grandfatherName: string;
  familyName: string;
  fullName: string;
  birthDate?: Date;
  gender: string;
  nationalityId: number;
  nationalityName: string;
  mobileNumber?: string;
  email?: string;
  identityIssueDate?: Date;
  identityExpiryDate?: Date;
  residenceAddress?: AbsherAddress;
  workAddress?: AbsherAddress;
}

export interface AbsherAddress {
  regionId: number;
  regionName: string;
  cityId: number;
  cityName: string;
  districtName: string;
  streetName: string;
  buildingNumber: string;
  postalCode: string;
  additionalNumber: string;
}
