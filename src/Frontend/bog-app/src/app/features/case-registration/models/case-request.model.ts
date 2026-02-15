import { RelatedCaseVM } from './related-case.model';
import { RequestDeficiencyDTO } from './deficiency.model';

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
  classificationIds?: number[];
  relatedCases?: RelatedCaseVM[];
  primaryMobile?: string;
  secondaryMobile?: string;
  email?: string;
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
  classificationIds?: number[];
  primaryMobile?: string;
  secondaryMobile?: string;
  email?: string;
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

export interface ClassificationVM {
  id: number;
  name: string;
  nameAr: string;
  description?: string;
}

export interface CaseClassificationVM {
  id: number;
  nameAr: string;
  nameEn?: string;
  code?: string;
}

export interface CaseTypeVM {
  id: number;
  name: string;
  nameAr?: string;
  description?: string;
}

export interface RequestDecisionDTO {
  decisionType: string | number;
  caseTypeId: number;
  notes?: string;
  deficiencies?: RequestDeficiencyDTO[];
}
