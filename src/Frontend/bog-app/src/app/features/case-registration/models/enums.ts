export enum DefendantType {
  Natural = 1,
  Company = 2,
  GovernmentEntity = 3,
  NonProfit = 4,
  Unknown = 5,
  Other = 6
}

export enum IdentityType {
  NationalId = 1,
  Iqama = 2,
  Passport = 3,
  GCC_Id = 4
}

export enum DecisionType {
  Register = 'Register',
  SendToJudge = 'SendToJudge',
  Reject = 'Reject',
  RequestCompletion = 'RequestCompletion'
}

export const DefendantTypeLabels: Record<DefendantType, string> = {
  [DefendantType.Natural]: 'طبيعي',
  [DefendantType.Company]: 'شركة',
  [DefendantType.GovernmentEntity]: 'جهة حكومية',
  [DefendantType.NonProfit]: 'جمعية أهلية',
  [DefendantType.Unknown]: 'مجهول',
  [DefendantType.Other]: 'أخرى'
};

export const IdentityTypeLabels: Record<IdentityType, string> = {
  [IdentityType.NationalId]: 'هوية وطنية',
  [IdentityType.Iqama]: 'إقامة',
  [IdentityType.Passport]: 'جواز سفر',
  [IdentityType.GCC_Id]: 'هوية خليجية'
};
