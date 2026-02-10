export interface RelatedCaseVM {
  id: number;
  caseRegistrationRequestId: number;
  courtId?: number;
  courtName?: string; // For display
  caseNumber: number;
  caseYear: number;
  createdDate: Date;
  modifiedDate: Date;
}

export interface RelatedCaseDTO {
  courtId?: number;
  caseNumber: number;
  caseYear: number;
}

export interface RelatedCasesBatchUpdateDTO {
  relatedCases: RelatedCaseDTO[];
}

// For Court lookup dropdown
export interface CourtLookup {
  id: number;
  nameAr: string;
  name: string;
  regionId: number;
  cityId: number;
}
