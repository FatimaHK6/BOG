export interface CaseRequestVM {
  id: number;
  courtId: number;
  requestNumber?: string;
  requestStatusId: number;
  requestStatusName: string;
  subject: string;
  evidence: string;
  createdDate: Date;
  modifiedDate: Date;
  plaintiffsCount?: number;
  defendantsCount?: number;
  attachmentsCount?: number;
  deficienciesCount?: number;
  isDeleted: boolean;
}

export interface CaseRequestCreateDTO {
  courtId: number;
  subject: string;
  evidence: string;
}

export interface CaseRequestUpdateDTO {
  courtId?: number;
  subject?: string;
  evidence?: string;
  requestStatusId?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageSize: number;
  pageNumber: number;
  totalPages: number;
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
}
