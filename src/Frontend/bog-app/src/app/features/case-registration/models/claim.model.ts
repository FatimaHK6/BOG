export interface ClaimVM {
  id: number;
  caseRegistrationRequestId: number;
  claimText: string;
  createdDate: Date;
  modifiedDate: Date;
}

export interface ClaimDTO {
  claimText: string;
}

export interface ClaimsBatchUpdateDTO {
  claims: ClaimDTO[];
}
