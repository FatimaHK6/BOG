// Attachment Type
export interface AttachmentType {
  id: number;
  name: string;
  nameAr: string;
  isRequired: boolean;
  plaintiffTypeIds: number[];  // Which plaintiff types require this attachment
}

// Attachment Upload Response
export interface AttachmentUploadResult {
  id: number;
  fileName: string;
  fileSize: number;
  fileUrl: string;
  uploadDate: Date;
}

// Attachment Create DTO
export interface AttachmentCreateDTO {
  attachmentTypeId: number;
  file: File;
}

// Validation Constants
export const ATTACHMENT_CONSTRAINTS = {
  maxFileSize: 4 * 1024 * 1024,  // 4 MB
  allowedTypes: ['application/pdf'],
  allowedExtensions: ['.pdf']
};

// Required Attachments by Plaintiff Type
export const REQUIRED_ATTACHMENTS: { [key: number]: number[] } = {
  1: [1],      // Individual: ID Copy
  2: [1],      // Individual without ID: ID Copy
  3: [1, 2],   // Business Owner: ID Copy, Commercial Registration
  4: [2],      // Registered Company: Commercial Registration
  5: [],       // Unregistered Company: None
  6: [3],      // Government Agency: Representation Decision
  7: [4],      // Society/NGO: Registration Certificate
  8: [5]       // Waqf: Waqf Deed
};
