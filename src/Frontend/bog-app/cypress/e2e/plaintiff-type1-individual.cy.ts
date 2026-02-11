/// <reference types="cypress" />

describe('Plaintiff Type 1 - فرد (Individual)', () => {
  beforeEach(() => {
    cy.setupPlaintiffContext(1);
  });

  describe('Form Sections Visibility', () => {
    it('should display plaintiff type as فرد', () => {
      cy.contains('فرد').should('exist');
    });

    it('should show identity type dropdown', () => {
      cy.get('mat-select[formcontrolname="identityTypeId"]').should('exist');
    });

    it('should show identity number with Absher search button', () => {
      cy.get('input[formcontrolname="identityNumber"]').should('exist');
      cy.get('input[formcontrolname="identityNumber"]').parents('mat-form-field').find('button').should('exist');
    });

    it('should show Absher verification warning (BR08)', () => {
      cy.contains('يجب الضغط على زر البحث للتحقق من البيانات عبر أبشر').should('exist');
    });

    it('should show name fields', () => {
      cy.get('input[formcontrolname="firstName"]').should('exist');
      cy.get('input[formcontrolname="fatherName"]').should('exist');
      cy.get('input[formcontrolname="grandfatherName"]').should('exist');
      cy.get('input[formcontrolname="clanName"]').should('exist');
      cy.get('input[formcontrolname="familyName"]').should('exist');
    });

    it('should show birth date and gender', () => {
      cy.get('input[formcontrolname="birthDate"]').should('exist');
      cy.get('mat-select[formcontrolname="gender"]').should('exist');
    });

    it('should show nationality dropdown', () => {
      cy.get('mat-select[formcontrolname="nationalityId"]').should('exist');
    });

    it('should show identity dates', () => {
      cy.get('input[formcontrolname="identityIssueDate"]').should('exist');
      cy.get('input[formcontrolname="identityExpiryDate"]').should('exist');
    });

    it('should show employment section', () => {
      cy.contains('h4', 'بيانات العمل').should('exist');
      cy.get('mat-select[formcontrolname="employmentStatusId"]').should('exist');
    });
  });

  describe('Contact Fields', () => {
    it('should show mobile number field', () => {
      cy.get('input[formcontrolname="mobileNumber"]').scrollIntoView().should('exist');
    });

    it('should show email field', () => {
      cy.get('input[formcontrolname="email"]').scrollIntoView().should('exist');
    });
  });

  describe('BC01 - Employment Status Rules', () => {
    it('should show employer and profession when status is حكومي', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').click();
      cy.get('mat-option').contains('حكومي').click();
      cy.get('input[formcontrolname="employer"]').should('exist');
      cy.get('input[formcontrolname="profession"]').should('exist');
    });

    it('should show employer and profession when status is خاص', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').click();
      cy.get('mat-option').contains('خاص').click();
      cy.get('input[formcontrolname="employer"]').should('exist');
      cy.get('input[formcontrolname="profession"]').should('exist');
    });

    it('should hide employer and profession when status is بدون عمل', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').click();
      cy.get('mat-option').contains('بدون عمل').click();
      cy.get('input[formcontrolname="employer"]').should('not.exist');
      cy.get('input[formcontrolname="profession"]').should('not.exist');
    });
  });

  describe('BC02 - Work Address Rules', () => {
    it('should show work address only when status is خاص', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').click();
      cy.get('mat-option').contains('خاص').click();
      cy.contains('h4', 'عنوان العمل').scrollIntoView().should('exist');
    });

    it('should hide work address when status is حكومي', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').click();
      cy.get('mat-option').contains('حكومي').click();
      cy.contains('h4', 'عنوان العمل').should('not.exist');
    });

    it('should hide work address when status is بدون عمل', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').click();
      cy.get('mat-option').contains('بدون عمل').click();
      cy.contains('h4', 'عنوان العمل').should('not.exist');
    });
  });

  describe('Residence Address', () => {
    it('should show residence address section with all fields', () => {
      cy.get('[formgroupname="residenceAddress"]').scrollIntoView().should('exist');
      cy.get('[formgroupname="residenceAddress"] mat-select[formcontrolname="regionId"]').should('exist');
      cy.get('[formgroupname="residenceAddress"] mat-select[formcontrolname="cityId"]').should('exist');
      cy.get('[formgroupname="residenceAddress"] input[formcontrolname="districtId"]').should('exist');
      cy.get('[formgroupname="residenceAddress"] input[formcontrolname="street"]').should('exist');
      cy.get('[formgroupname="residenceAddress"] input[formcontrolname="buildingNumber"]').should('exist');
      cy.get('[formgroupname="residenceAddress"] input[formcontrolname="unitNumber"]').should('exist');
      cy.get('[formgroupname="residenceAddress"] input[formcontrolname="postalCode"]').should('exist');
      cy.get('[formgroupname="residenceAddress"] input[formcontrolname="additionalCode"]').should('exist');
    });
  });
});
