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
  // CompanyRepresentative fields (ممثل الشركة - Type 6)
  representationDocSource?: string;
  representativeCapacity?: string;
  representationDocType?: string;
  representationDocNumber?: string;
  // AgencyRepresentative fields (ممثل الجهة - Type 7)
  representationLetterNumber?: string;
  representationLetterDate?: Date;
  representationLetterSource?: string;

  // Attachments (صورة التمثيل)
  attachments?: {
    attachmentTypeId: number;
    fileName: string;
    contentType: string;
    fileSizeBytes: number;
    description?: string;
  }[];
}

export interface RepresentativeUpdateDTO extends RepresentativeCreateDTO {
  id: number;
}

// Allowed Representative Types by Plaintiff Type
// Source: plaintiff-user-stories-plan.html table 5.2 "الممثلين المسموحين حسب نوع المدعي"
// DB Representative Types:
// 1: وكيل (Agent), 2: ولي (Guardian), 3: وصي (Custodian), 4: ناظر (Executor)
// 5: ممثل الورثة (HeirRepresentative), 6: ممثل الشركة (CompanyRepresentative)
// 7: ممثل الجهة (AgencyRepresentative), 8: أمين التفليسة (Trustee)
// 9: ممثل نظامي (LegalRepresentative), 10: مصفي (Liquidator), 11: حارس قضائي (JudicialCustodian)
export const ALLOWED_REPRESENTATIVE_TYPES: { [key: number]: number[] } = {
  1: [1, 2, 3, 8],            // فرد: وكيل(1)، ولي(2)، وصي(3)، أمين تفليسة(8)
  2: [1, 2, 3, 4, 7, 8, 9, 10, 11], // فرد بدون هوية: جميع الأنواع
  3: [1, 2, 8],               // صاحب مؤسسة: وكيل(1)، ولي(2)، أمين تفليسة(8)
  4: [1, 8, 9, 10, 11],       // شركة مسجلة: وكيل(1)، أمين تفليسة(8)، ممثل نظامي(9)، مصفي(10)، حارس قضائي(11)
  5: [1],                     // شركة غير مسجلة: وكيل(1) فقط
  6: [1, 7],                  // جهة حكومية: وكيل(1)، ممثل الجهة(7)
  7: [1],                     // جمعية/مؤسسة أهلية: وكيل(1) فقط
  8: [1, 4]                   // وقف: وكيل(1)، ناظر(4)
};
