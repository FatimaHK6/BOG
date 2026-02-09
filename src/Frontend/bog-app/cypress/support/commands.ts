/// <reference types="cypress" />

// API intercept data
const mockPlaintiffTypes = [
  { id: 1, name: 'Individual', nameAr: 'فرد' },
  { id: 2, name: 'IndividualWithoutId', nameAr: 'فرد بدون هوية' },
  { id: 3, name: 'BusinessOwner', nameAr: 'صاحب مؤسسة' },
  { id: 4, name: 'RegisteredCompany', nameAr: 'شركة مسجلة' },
  { id: 5, name: 'UnregisteredCompany', nameAr: 'شركة غير مسجلة' },
  { id: 6, name: 'GovernmentAgency', nameAr: 'جهة حكومية' },
  { id: 7, name: 'NGO', nameAr: 'جمعية/مؤسسة أهلية' },
  { id: 8, name: 'Waqf', nameAr: 'وقف' }
];

const mockRepresentativeTypes = [
  { id: 1, name: 'Agent', nameAr: 'وكيل' },
  { id: 2, name: 'Guardian', nameAr: 'ولي' },
  { id: 3, name: 'Custodian', nameAr: 'وصي' },
  { id: 4, name: 'Executor', nameAr: 'ناظر' },
  { id: 5, name: 'HeirRepresentative', nameAr: 'ممثل الورثة' },
  { id: 6, name: 'CompanyRepresentative', nameAr: 'ممثل الشركة' },
  { id: 7, name: 'AgencyRepresentative', nameAr: 'ممثل الجهة' },
  { id: 8, name: 'Trustee', nameAr: 'أمين التفليسة' },
  { id: 9, name: 'LegalRepresentative', nameAr: 'ممثل نظامي' },
  { id: 10, name: 'Liquidator', nameAr: 'مصفي' },
  { id: 11, name: 'JudicialCustodian', nameAr: 'حارس قضائي' }
];

const mockIdentityTypes = [
  { id: 1, name: 'NationalId', nameAr: 'هوية وطنية' },
  { id: 2, name: 'ResidentId', nameAr: 'هوية مقيم' }
];

const mockRegions = [
  { id: 1, name: 'Riyadh', nameAr: 'الرياض' },
  { id: 2, name: 'Makkah', nameAr: 'مكة المكرمة' }
];

const mockCities = [
  { id: 1, name: 'Riyadh', nameAr: 'الرياض', regionId: 1 },
  { id: 2, name: 'Jeddah', nameAr: 'جدة', regionId: 2 }
];

const mockNationalities = [
  { id: 1, name: 'Saudi', nameAr: 'سعودي' },
  { id: 2, name: 'Egyptian', nameAr: 'مصري' }
];

const mockGovernmentAgencies = [
  { id: 1, name: 'MOJ', nameAr: 'وزارة العدل', headquarters: 'الرياض' }
];

const mockLicenseSources = [
  { id: 1, name: 'MOI', nameAr: 'وزارة الداخلية' },
  { id: 2, name: 'MOMRA', nameAr: 'وزارة الشؤون البلدية' }
];

const mockPlaintiffsList = [
  {
    id: 1, plaintiffTypeId: 1, plaintiffTypeName: 'فرد', plaintiffTypeNameAr: 'فرد',
    identityNumber: '1234567890', fullName: 'محمد أحمد السعيد', displayName: 'محمد أحمد السعيد',
    representativesCount: 1, attachmentsCount: 1, attachmentStatus: 'complete', isApplicant: true
  },
  {
    id: 2, plaintiffTypeId: 4, plaintiffTypeName: 'شركة مسجلة', plaintiffTypeNameAr: 'شركة مسجلة',
    identityNumber: '2020202020', fullName: 'شركة الرياض', displayName: 'شركة الرياض',
    representativesCount: 0, attachmentsCount: 0, attachmentStatus: 'incomplete', isApplicant: false
  }
];

declare global {
  namespace Cypress {
    interface Chainable {
      setupApiIntercepts(): Chainable<void>;
      setupPlaintiffContext(plaintiffTypeId: number): Chainable<void>;
      setupPlaintiffListContext(): Chainable<void>;
      navigateToRepresentativesStep(): Chainable<void>;
      navigateToStep(stepIndex: number): Chainable<void>;
      openRepresentativeDialog(repTypeId: number): Chainable<void>;
      fillCommonFields(): Chainable<void>;
      fillMussaffiFields(data?: { decisionNumber?: string; decisionDate?: string; decisionSource?: string }): Chainable<void>;
      fillTrusteeFields(data?: { decisionNumber?: string; decisionDate?: string; decisionSource?: string }): Chainable<void>;
      fillJudicialCustodianFields(data?: { decisionNumber?: string; decisionDate?: string; decisionSource?: string }): Chainable<void>;
      fillLegalRepresentativeFields(data?: {
        representationDocSource?: string;
        representativeCapacity?: string;
        representationDocType?: string;
        representationDocNumber?: string;
      }): Chainable<void>;
      fillAddressFields(formGroupName: string, data?: any): Chainable<void>;
      selectMatSelect(formControlName: string, optionText: string): Chainable<void>;
    }
  }
}

Cypress.Commands.add('setupApiIntercepts', () => {
  // Lookup endpoints used by both plaintiff form and representative dialog
  cy.intercept('GET', '**/api/lookups/plaintiff-types', {
    statusCode: 200,
    body: mockPlaintiffTypes
  }).as('getPlaintiffTypes');

  cy.intercept('GET', '**/api/lookups/representative-types', {
    statusCode: 200,
    body: mockRepresentativeTypes
  }).as('getRepresentativeTypes');

  cy.intercept('GET', '**/api/lookups/identity-types', {
    statusCode: 200,
    body: mockIdentityTypes
  }).as('getIdentityTypes');

  cy.intercept('GET', '**/api/lookups/regions', {
    statusCode: 200,
    body: mockRegions
  }).as('getRegions');

  cy.intercept('GET', '**/api/lookups/regions/*/cities', {
    statusCode: 200,
    body: mockCities
  }).as('getCities');

  cy.intercept('GET', '**/api/lookups/nationalities', {
    statusCode: 200,
    body: mockNationalities
  }).as('getNationalities');

  cy.intercept('GET', '**/api/lookups/government-agencies', {
    statusCode: 200,
    body: mockGovernmentAgencies
  }).as('getGovernmentAgencies');

  cy.intercept('GET', '**/api/lookups/countries', {
    statusCode: 200,
    body: [{ id: 1, name: 'Saudi Arabia', nameAr: 'المملكة العربية السعودية' }]
  }).as('getCountries');

  cy.intercept('GET', '**/api/lookups/license-sources', {
    statusCode: 200,
    body: mockLicenseSources
  }).as('getLicenseSources');

  // Plaintiff and representative API calls
  cy.intercept('GET', '**/api/plaintiffs*', {
    statusCode: 200,
    body: []
  }).as('getPlaintiffs');

  cy.intercept('POST', '**/api/plaintiffs*', {
    statusCode: 200,
    body: { id: 1 }
  }).as('createPlaintiff');

  cy.intercept('POST', '**/api/representatives*', {
    statusCode: 200,
    body: { id: 1 }
  }).as('createRepresentative');

  // Case registration request
  cy.intercept('GET', '**/api/case-registration-requests*', {
    statusCode: 200,
    body: { id: 1, isDraft: true }
  }).as('getCaseRequest');

  cy.intercept('POST', '**/api/case-registration-requests*', {
    statusCode: 200,
    body: { id: 1 }
  }).as('createCaseRequest');
});

Cypress.Commands.add('setupPlaintiffContext', (plaintiffTypeId: number) => {
  cy.setupApiIntercepts();
  // Navigate to the plaintiff form (add mode) with type and requestId query params
  cy.visit(`/case-registration/plaintiffs/add?type=${plaintiffTypeId}&requestId=1`);
  // Wait for the plaintiff form to fully render
  cy.get('app-plaintiff-form', { timeout: 15000 }).should('exist');
  cy.get('mat-horizontal-stepper', { timeout: 10000 }).should('exist');
});

Cypress.Commands.add('navigateToRepresentativesStep', () => {
  // Click on step 2 (بيانات الممثلين) in the stepper
  // The stepper has [linear]="false" so we can skip to any step
  cy.get('mat-step-header').eq(1).click();
  // Wait for the representatives step content to be visible
  cy.contains('h3', 'بيانات الممثلين', { timeout: 5000 }).should('be.visible');
});

Cypress.Commands.add('openRepresentativeDialog', (repTypeId: number) => {
  // Find the add representative button and click it to open the menu
  cy.contains('button', 'إضافة ممثّل للمدّعي').click();
  // Wait for the menu to appear and click the representative type
  const typeName = mockRepresentativeTypes.find(t => t.id === repTypeId)?.nameAr || '';
  cy.get('.mat-mdc-menu-panel', { timeout: 5000 }).should('be.visible');
  cy.contains('.mat-mdc-menu-item, [mat-menu-item]', typeName).click();
  // Wait for dialog to open
  cy.get('mat-dialog-container', { timeout: 5000 }).should('be.visible');
});

Cypress.Commands.add('fillCommonFields', () => {
  cy.get('mat-dialog-container').within(() => {
    cy.get('input[formcontrolname="identityNumber"]').clear().type('1234567890');
    cy.get('input[formcontrolname="firstName"]').clear().type('أحمد');
    cy.get('input[formcontrolname="fatherName"]').clear().type('محمد');
    cy.get('input[formcontrolname="familyName"]').clear().type('الشمري');
    cy.get('input[formcontrolname="birthDate"]').clear().type('01/01/1990');
    cy.get('mat-select[formcontrolname="gender"]').click();
  });
  cy.get('mat-option').contains('ذكر').click();
  cy.get('mat-dialog-container').within(() => {
    cy.get('input[formcontrolname="identityIssueDate"]').clear().type('01/01/2020');
    cy.get('input[formcontrolname="identityExpiryDate"]').clear().type('01/01/2030');
    cy.get('mat-select[formcontrolname="residenceRegionId"]').click();
  });
  cy.get('mat-option').first().click();
  cy.get('mat-dialog-container').within(() => {
    cy.get('mat-select[formcontrolname="residenceCityId"]').click();
  });
  cy.get('mat-option').first().click();
  cy.get('mat-dialog-container').within(() => {
    cy.get('input[formcontrolname="residenceDistrict"]').clear().type('حي النزهة');
    cy.get('input[formcontrolname="residenceStreet"]').clear().type('شارع الملك فهد');
    cy.get('input[formcontrolname="residenceBuildingNumber"]').clear().type('1234');
    cy.get('input[formcontrolname="residenceUnitNumber"]').clear().type('1');
    cy.get('input[formcontrolname="residencePostalCode"]').clear().type('12345');
    cy.get('input[formcontrolname="residenceAdditionalCode"]').clear().type('6789');
    cy.get('mat-select[formcontrolname="employmentStatus"]').click();
  });
  cy.get('mat-option').contains('بدون عمل').click();
  cy.get('mat-dialog-container').within(() => {
    cy.get('input[formcontrolname="mobileNumber"]').clear().type('0512345678');
  });
});

Cypress.Commands.add('fillMussaffiFields', (data = {}) => {
  const defaults = {
    decisionNumber: '12345',
    decisionDate: '01/15/2025',
    decisionSource: 'المحكمة التجارية'
  };
  const d = { ...defaults, ...data };
  cy.get('mat-dialog-container').within(() => {
    cy.get('input[formcontrolname="decisionNumber"]').clear().type(d.decisionNumber);
    cy.get('input[formcontrolname="decisionDate"]').clear().type(d.decisionDate);
    cy.get('input[formcontrolname="decisionSource"]').clear().type(d.decisionSource);
  });
});

Cypress.Commands.add('fillTrusteeFields', (data = {}) => {
  const defaults = {
    decisionNumber: '67890',
    decisionDate: '03/20/2025',
    decisionSource: 'محكمة التنفيذ'
  };
  const d = { ...defaults, ...data };
  cy.get('mat-dialog-container').within(() => {
    cy.get('input[formcontrolname="decisionNumber"]').clear().type(d.decisionNumber);
    cy.get('input[formcontrolname="decisionDate"]').clear().type(d.decisionDate);
    cy.get('input[formcontrolname="decisionSource"]').clear().type(d.decisionSource);
  });
});

Cypress.Commands.add('fillJudicialCustodianFields', (data = {}) => {
  const defaults = {
    decisionNumber: '11111',
    decisionDate: '06/10/2025',
    decisionSource: 'المحكمة العامة'
  };
  const d = { ...defaults, ...data };
  cy.get('mat-dialog-container').within(() => {
    cy.get('input[formcontrolname="decisionNumber"]').clear().type(d.decisionNumber);
    cy.get('input[formcontrolname="decisionDate"]').clear().type(d.decisionDate);
    cy.get('input[formcontrolname="decisionSource"]').clear().type(d.decisionSource);
  });
});

Cypress.Commands.add('fillLegalRepresentativeFields', (data = {}) => {
  const defaults = {
    representationDocSource: 'وزارة التجارة',
    representativeCapacity: 'رئيس مجلس إدارة',
    representationDocType: 'عقد تأسيس',
    representationDocNumber: 'DOC-2025-001'
  };
  const d = { ...defaults, ...data };
  cy.get('mat-dialog-container').within(() => {
    cy.get('input[formcontrolname="representationDocSource"]').clear().type(d.representationDocSource);
    cy.get('mat-select[formcontrolname="representativeCapacity"]').click();
  });
  cy.get('mat-option').contains(d.representativeCapacity).click();
  cy.get('mat-dialog-container').within(() => {
    cy.get('mat-select[formcontrolname="representationDocType"]').click();
  });
  cy.get('mat-option').contains(d.representationDocType).click();
  cy.get('mat-dialog-container').within(() => {
    cy.get('input[formcontrolname="representationDocNumber"]').clear().type(d.representationDocNumber);
  });
});

Cypress.Commands.add('setupPlaintiffListContext', () => {
  cy.setupApiIntercepts();
  // Override plaintiffs intercept with list data
  cy.intercept('GET', '**/api/case-requests/*/plaintiffs', {
    statusCode: 200,
    body: mockPlaintiffsList
  }).as('getPlaintiffsList');
  cy.intercept('GET', '**/api/plaintiffs/*/representatives', {
    statusCode: 200,
    body: []
  }).as('getRepresentativesList');
  cy.intercept('PUT', '**/api/case-registration-requests/*', {
    statusCode: 200,
    body: { id: 1 }
  }).as('updateCaseRequest');
  cy.intercept('DELETE', '**/api/plaintiffs/*', {
    statusCode: 200,
    body: {}
  }).as('deletePlaintiff');
  cy.visit('/case-registration/plaintiffs?requestId=1');
  cy.get('app-plaintiff-list', { timeout: 15000 }).should('exist');
});

Cypress.Commands.add('navigateToStep', (stepIndex: number) => {
  cy.get('mat-step-header').eq(stepIndex).click();
  cy.wait(300);
});

Cypress.Commands.add('fillAddressFields', (formGroupName: string, data: any = {}) => {
  const defaults = {
    regionId: 1,
    cityId: 1,
    districtId: 'حي النزهة',
    street: 'شارع الملك فهد',
    buildingNumber: '1234',
    unitNumber: '1',
    postalCode: '12345',
    additionalCode: '6789'
  };
  const d = { ...defaults, ...data };

  // Select region
  cy.get(`[formgroupname="${formGroupName}"] mat-select[formcontrolname="regionId"]`).click();
  cy.get('mat-option').first().click();
  cy.wait(200);

  // Select city
  cy.get(`[formgroupname="${formGroupName}"] mat-select[formcontrolname="cityId"]`).click();
  cy.get('mat-option').first().click();

  // Fill text fields
  cy.get(`[formgroupname="${formGroupName}"] input[formcontrolname="districtId"]`).clear().type(d.districtId);
  cy.get(`[formgroupname="${formGroupName}"] input[formcontrolname="street"]`).clear().type(d.street);
  cy.get(`[formgroupname="${formGroupName}"] input[formcontrolname="buildingNumber"]`).clear().type(d.buildingNumber);
  cy.get(`[formgroupname="${formGroupName}"] input[formcontrolname="unitNumber"]`).clear().type(d.unitNumber);
  cy.get(`[formgroupname="${formGroupName}"] input[formcontrolname="postalCode"]`).clear().type(d.postalCode);
  cy.get(`[formgroupname="${formGroupName}"] input[formcontrolname="additionalCode"]`).clear().type(d.additionalCode);
});

Cypress.Commands.add('selectMatSelect', (formControlName: string, optionText: string) => {
  cy.get(`mat-select[formcontrolname="${formControlName}"]`).click();
  cy.get('mat-option').contains(optionText).click();
});

export {};
