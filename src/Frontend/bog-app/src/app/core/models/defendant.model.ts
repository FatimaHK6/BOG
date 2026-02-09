/**
 * Defendant List ViewModel - for list display
 */
export interface DefendantListVM {
  id: number;
  defendantTypeId: number;
  defendantTypeNameAr: string;
  displayName: string;
}

/**
 * Defendant ViewModel - full details
 */
export interface DefendantVM {
  id: number;
  defendantTypeId: number;
  defendantTypeNameAr: string;
  displayName: string;
  // Individual (Type 1)
  identityTypeId?: number;
  identityTypeName?: string;
  identityNumber?: string;
  firstName?: string;
  fatherName?: string;
  grandfatherName?: string;
  tribeName?: string;
  familyName?: string;
  birthDate?: string;
  genderId?: number;
  genderName?: string;
  nationalityId?: number;
  nationalityName?: string;
  mobileNumber?: string;
  email?: string;
  indRegionId?: number;
  indRegionName?: string;
  indCityId?: number;
  indCityName?: string;
  indDistrict?: string;
  indStreet?: string;
  indBuildingNumber?: string;
  indUnitNumber?: string;
  indPostalCode?: string;
  indAdditionalCode?: string;
  employmentStatusId?: number;
  employmentStatusName?: string;
  employer?: string;
  occupation?: string;
  // عنوان العمل (Individual Type 1 - خاص فقط)
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
  // Registered Company (Type 2)
  registrationStartDate?: string;
  registrationEndDate?: string;
  regCompanyRegionId?: number;
  regCompanyRegionName?: string;
  regCompanyCityId?: number;
  regCompanyCityName?: string;
  regCompanyDistrict?: string;
  regCompanyStreet?: string;
  regCompanyBuildingNumber?: string;
  regCompanyUnitNumber?: string;
  regCompanyPostalCode?: string;
  regCompanyAdditionalCode?: string;
  // Government Agency (Type 3)
  governmentAgencyId?: number;
  governmentAgencyName?: string;
  headquarters?: string;
  additionalStatement?: string;
  // Unregistered Company (Type 4)
  commercialRegNumber?: string;
  companyName?: string;
  countryId?: number;
  countryName?: string;
  city?: string;
  description?: string;
  // Waqf (Type 7)
  waqfName?: string;
  courtDeedNumber?: string;
  courtDeedDate?: string;
  deedSource?: string;
  waqfSupervisoryTypeId?: number;
  waqfSupervisoryTypeName?: string;
  waqfAgencyName?: string;
  waqfRegionId?: number;
  waqfRegionName?: string;
  waqfCityId?: number;
  waqfCityName?: string;
  waqfDistrict?: string;
  waqfStreet?: string;
  waqfBuildingNumber?: string;
  waqfUnitNumber?: string;
  waqfPostalCode?: string;
  waqfAdditionalCode?: string;
  waqfAddressDescription?: string;
  // NGO/Society (Type 6)
  licenseNumber?: string;
  licenseSourceId?: number;
  licenseSourceName?: string;
  ngoName?: string;
  licenseDate?: string;
  ngoRegionId?: number;
  ngoRegionName?: string;
  ngoCityId?: number;
  ngoCityName?: string;
  ngoDistrict?: string;
  ngoStreet?: string;
  ngoBuildingNumber?: string;
  ngoUnitNumber?: string;
  ngoPostalCode?: string;
  ngoAdditionalCode?: string;
  createdDate: Date;
}

/**
 * DTO for creating a defendant
 */
export interface DefendantCreateDTO {
  defendantTypeId: number;
  // Individual (Type 1)
  identityTypeId?: number;
  identityNumber?: string;
  firstName?: string;
  fatherName?: string;
  grandfatherName?: string;
  tribeName?: string;
  familyName?: string;
  birthDate?: string;
  genderId?: number;
  nationalityId?: number;
  mobileNumber?: string;
  email?: string;
  indRegionId?: number;
  indCityId?: number;
  indDistrict?: string;
  indStreet?: string;
  indBuildingNumber?: string;
  indUnitNumber?: string;
  indPostalCode?: string;
  indAdditionalCode?: string;
  employmentStatusId?: number;
  employer?: string;
  occupation?: string;
  // عنوان العمل (Individual Type 1 - خاص فقط)
  workRegionId?: number;
  workCityId?: number;
  workDistrict?: string;
  workStreet?: string;
  workBuildingNumber?: string;
  workUnitNumber?: string;
  workPostalCode?: string;
  workAdditionalCode?: string;
  // Registered Company (Type 2)
  registrationStartDate?: string;
  registrationEndDate?: string;
  regCompanyRegionId?: number;
  regCompanyCityId?: number;
  regCompanyDistrict?: string;
  regCompanyStreet?: string;
  regCompanyBuildingNumber?: string;
  regCompanyUnitNumber?: string;
  regCompanyPostalCode?: string;
  regCompanyAdditionalCode?: string;
  // Government Agency (Type 3)
  governmentAgencyId?: number;
  headquarters?: string;
  additionalStatement?: string;
  // Unregistered Company (Type 4)
  commercialRegNumber?: string;
  companyName?: string;
  countryId?: number;
  city?: string;
  description?: string;
  // Waqf (Type 7)
  waqfName?: string;
  courtDeedNumber?: string;
  courtDeedDate?: string;
  deedSource?: string;
  waqfSupervisoryTypeId?: number;
  waqfAgencyName?: string;
  waqfRegionId?: number;
  waqfCityId?: number;
  waqfDistrict?: string;
  waqfStreet?: string;
  waqfBuildingNumber?: string;
  waqfUnitNumber?: string;
  waqfPostalCode?: string;
  waqfAdditionalCode?: string;
  waqfAddressDescription?: string;
  // NGO/Society (Type 6)
  licenseNumber?: string;
  licenseSourceId?: number;
  ngoName?: string;
  licenseDate?: string;
  ngoRegionId?: number;
  ngoCityId?: number;
  ngoDistrict?: string;
  ngoStreet?: string;
  ngoBuildingNumber?: string;
  ngoUnitNumber?: string;
  ngoPostalCode?: string;
  ngoAdditionalCode?: string;
}
