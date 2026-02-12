export interface DefendantVM {
  id: number;
  defendantTypeId: number;
  defendantTypeName: string;
  fullName: string;
  identityNumber?: string;
  identityTypeName?: string;
  identityTypeId?: number;
  addressText?: string;
  dataSourceId?: number;
  dataSourceName?: string;
  createdDate: Date;
}

export interface DefendantCreateDTO {
  defendantTypeId: number;
  fullName: string;
  identityNumber?: string;
  identityTypeId?: number;
  addressText?: string;
  commercialRegNumber?: string;
  governmentAgencyId?: number;
  additionalStatement?: string;
}

export interface DefendantUpdateDTO extends DefendantCreateDTO {}
