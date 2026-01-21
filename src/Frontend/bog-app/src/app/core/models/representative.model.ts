// Representative Types
export interface RepresentativeType {
  id: number;
  name: string;
  nameAr: string;
}

// Representative Create DTO
export interface RepresentativeCreateDTO {
  representativeTypeId: number;
  identityTypeId: number;
  identityNumber: string;
  firstName?: string;
  fatherName?: string;
  grandfatherName?: string;
  familyName?: string;
  mobileNumber?: string;
  email?: string;
  authorizationNumber?: string;
  authorizationDate?: Date;
  authorizationSource?: string;
  authorizationSourceType?: string;
  lawyerLicenseNumber?: string;
  lawyerLicenseDate?: Date;
  lawyerLicenseExpiryDate?: Date;
  guardianshipType?: string;
}

export interface RepresentativeUpdateDTO extends RepresentativeCreateDTO {
  id: number;
}

// Allowed Representative Types by Plaintiff Type
// Must match backend: BOG.BL/Services/RepresentativeBL.cs
// Representative Types:
// 1: Lawyer (وكيل), 2: Liquidator (المصفي), 3: Trustee (أمين التفليسة), 4: Custodian (الوصي)
// 5: CompanyRep (ممثل الشركة), 6: Guardian (الولي), 7: GovRep (ممثل الجهة)
// 8: Conservator (الناظر/القيم), 9: WaqfInspector (ناظر الوقف)
export const ALLOWED_REPRESENTATIVE_TYPES: { [key: number]: number[] } = {
  1: [1, 2, 3, 4, 6, 8],      // Individual: Lawyer, Liquidator, Trustee, Custodian, Guardian, Conservator
  2: [1, 2, 3, 4, 6, 8],      // Individual without ID: Same as Individual
  3: [1, 2, 3, 4],            // Business Owner: Lawyer, Liquidator, Trustee, Custodian
  4: [1, 2, 3, 4, 5],         // Registered Company: + CompanyRep
  5: [1, 2, 3, 4],            // Unregistered Company: Lawyer, Liquidator, Trustee, Custodian
  6: [1, 7],                  // Government Agency: Lawyer, GovRep
  7: [1, 5],                  // Society/NGO: Lawyer, CompanyRep
  8: [1, 9]                   // Waqf: Lawyer, WaqfInspector
};
