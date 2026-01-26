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
  clanName?: string;          // اسم الفخذ
  birthDate?: Date;           // تاريخ الميلاد
  gender?: string;            // الجنس
  nationalityId?: number;     // الجنسية
  identityIssueDate?: Date;   // تاريخ إصدار الهوية
  identityExpiryDate?: Date;  // تاريخ انتهاء الهوية

  // Residence Address (عنوان السكن - 6.3.2)
  residenceRegionId?: number;
  residenceCityId?: number;
  residenceDistrict?: string;
  residenceStreet?: string;
  residenceBuildingNumber?: string;
  residenceUnitNumber?: string;
  residencePostalCode?: string;
  residenceAdditionalCode?: string;

  // Employment Data (بيانات العمل)
  employmentStatus?: string;  // government/private/unemployed
  employer?: string;          // جهة العمل - conditional BC01
  profession?: string;        // المهنة

  // Work Address (عنوان العمل - 6.3.2) - Conditional BC02
  workRegionId?: number;
  workCityId?: number;
  workDistrict?: string;
  workStreet?: string;
  workBuildingNumber?: string;
  workUnitNumber?: string;
  workPostalCode?: string;
  workAdditionalCode?: string;

  // Contact Info
  mobileNumber?: string;
  email?: string;

  // Lawyer authorization fields
  authorizationNumber?: string;
  authorizationDate?: Date;
  authorizationSource?: string;
  authorizationSourceType?: string;
  lawyerLicenseNumber?: string;
  lawyerLicenseDate?: Date;
  lawyerLicenseExpiryDate?: Date;
  // Liquidator fields (6.3.12)
  decisionNumber?: string;
  decisionDate?: Date;
  decisionSource?: string;
  // Guardian fields (6.3.16)
  deedNumber?: string;
  deedDate?: Date;
  deedSource?: string;
  guardianshipType?: string;
}

export interface RepresentativeUpdateDTO extends RepresentativeCreateDTO {
  id: number;
}

// Allowed Representative Types by Plaintiff Type per SRS table 2.16
// Must match backend: BOG.BL/Services/RepresentativeBL.cs
// Representative Types:
// 1: Lawyer (محامي/وكيل), 2: Liquidator (مصفي), 3: BankruptcyTrustee (أمين تفليسة), 4: JudicialCustodian (حارس قضائي)
// 5: CompanyRep (ممثل نظامي), 6: Guardian (ولي), 7: GovRep (ممثل جهة حكومية)
// 8: Conservator (وصي), 9: WaqfInspector (ناظر)
export const ALLOWED_REPRESENTATIVE_TYPES: { [key: number]: number[] } = {
  1: [1, 3, 6, 8],            // فرد Individual: وكيل(1)، أمين تفليسة(3)، ولي(6)، وصي(8)
  2: [1, 2, 3, 4, 5, 6, 7, 8, 9], // فرد بدون هوية: جميع الأنواع
  3: [1, 3, 6],               // صاحب مؤسسة Business Owner: وكيل(1)، أمين تفليسة(3)، ولي(6)
  4: [1, 2, 3, 4, 5],         // شركة مسجلة Registered Company: وكيل، مصفي، أمين تفليسة، حارس قضائي، ممثل نظامي
  5: [1],                     // شركة غير مسجلة Unregistered Company: وكيل(1) فقط
  6: [1, 7],                  // جهة حكومية Government Agency: وكيل(1)، ممثل جهة حكومية(7)
  7: [1],                     // جمعية/مؤسسة أهلية NGO: وكيل(1) فقط
  8: [1, 9]                   // وقف Waqf: وكيل(1)، ناظر(9)
};
