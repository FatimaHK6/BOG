/**
 * View Model for Additional Information
 * Represents the data returned from the API
 */
export interface AdditionalInfoVM {
  id: number;
  caseRegistrationRequestId: number;

  // Type 1: إلغاء قرار إداري (Management Decision Cancellation) - 5 Fields
  decisionNumber?: string;
  decisionDate?: Date;
  notificationDate?: Date;
  notificationMethod?: string;        // Arabic display name
  notificationMethodId?: number;      // Lookup ID
  issuingAuthority?: string;           // Arabic display name
  issuingAuthorityId?: number;        // Lookup ID

  // Type 2: حقوق خدمة/تقاعدية (Service/Retirement Rights) - 6 Fields
  hasComplaint?: boolean;
  complaintNumber?: string;
  complaintDate?: Date;
  complaintAuthority?: string;         // Arabic display name
  complaintAuthorityId?: number;      // Lookup ID
  complaintDecisionDate?: Date;
  systemResult?: string;

  // Type 3: نزاع علامة تجارية (Trademark Dispute) - 2 Fields
  requestNumber?: string;
  requestDate?: Date;

  createdDate: Date;
  modifiedDate: Date;
}

/**
 * Data Transfer Object for Creating/Updating Additional Information
 * Sent to the API in PUT requests
 */
export interface AdditionalInfoDTO {
  // Type 1: إلغاء قرار إداري (Management Decision Cancellation)
  decisionNumber?: string;
  decisionDate?: Date;
  notificationDate?: Date;
  notificationMethodId?: number;      // Lookup ID
  issuingAuthorityId?: number;        // Lookup ID

  // Type 2: حقوق خدمة/تقاعدية (Service/Retirement Rights)
  hasComplaint?: boolean;
  complaintNumber?: string;
  complaintDate?: Date;
  complaintAuthorityId?: number;      // Lookup ID
  complaintDecisionDate?: Date;
  systemResult?: string;

  // Type 3: نزاع علامة تجارية (Trademark Dispute)
  requestNumber?: string;
  requestDate?: Date;
}

/**
 * Notification Method Lookup
 * Used for Type 1: طريقة العلم بالقرار
 */
export interface NotificationMethodVM {
  id: number;
  name: string;
  nameAr: string;
  description?: string;
}

/**
 * Government Entity Lookup
 * Used for Type 1 (Issuing Authority) and Type 2 (Complaint Authority)
 */
export interface GovernmentEntityVM {
  id: number;
  name: string;
  nameAr: string;
  code?: string;
  description?: string;
}
