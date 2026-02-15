export interface DeficiencyVM {
  id: number;
  caseRegistrationRequestId: number;
  deficiencyDescriptionId: number;
  deficiencyTypeId: number;
  deficiencyTypeName: string;
  deficiencyTypeNameAr: string;
  descriptionAr: string;
  descriptionEn?: string;
  displayOrder: number;
  createdDate: Date;
  modifiedDate: Date;
}

export interface RequestDeficiencyDTO {
  deficiencyDescriptionId: number;
}

export interface DeficienciesBatchUpdateDTO {
  deficiencies: RequestDeficiencyDTO[];
}

export interface DeficiencyTypeVM {
  id: number;
  name: string;
  nameAr: string;
  displayOrder: number;
}

export interface DeficiencyDescriptionVM {
  id: number;
  deficiencyTypeId: number;
  descriptionAr: string;
  descriptionEn?: string;
  displayOrder: number;
}
