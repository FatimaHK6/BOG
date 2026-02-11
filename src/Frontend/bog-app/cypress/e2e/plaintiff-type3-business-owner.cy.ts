/// <reference types="cypress" />

describe('Plaintiff Type 3 - صاحب مؤسسة (Business Owner)', () => {
  beforeEach(() => {
    cy.setupPlaintiffContext(3);
  });

  describe('Personal Data Section', () => {
    it('should display plaintiff type as صاحب مؤسسة', () => {
      cy.contains('صاحب مؤسسة').should('exist');
    });

    it('should show "بيانات صاحب المؤسسة" section title', () => {
      cy.contains('h4', 'بيانات صاحب المؤسسة').should('exist');
    });

    it('should show identity type dropdown with Absher', () => {
      cy.get('mat-select[formcontrolname="identityTypeId"]').should('exist');
      cy.get('input[formcontrolname="identityNumber"]').should('exist');
    });

    it('should show name fields', () => {
      cy.get('input[formcontrolname="firstName"]').should('exist');
      cy.get('input[formcontrolname="fatherName"]').should('exist');
      cy.get('input[formcontrolname="grandfatherName"]').should('exist');
      cy.get('input[formcontrolname="clanName"]').should('exist');
      cy.get('input[formcontrolname="familyName"]').should('exist');
    });

    it('should show birth date, gender, nationality', () => {
      cy.get('input[formcontrolname="birthDate"]').should('exist');
      cy.get('mat-select[formcontrolname="gender"]').should('exist');
      cy.get('mat-select[formcontrolname="nationalityId"]').should('exist');
    });

    it('should show identity dates', () => {
      cy.get('input[formcontrolname="identityIssueDate"]').should('exist');
      cy.get('input[formcontrolname="identityExpiryDate"]').should('exist');
    });

    it('should show mobile number and email', () => {
      cy.get('input[formcontrolname="mobileNumber"]').first().scrollIntoView().should('exist');
      cy.get('input[formcontrolname="email"]').first().scrollIntoView().should('exist');
    });
  });

  describe('Business Data Section', () => {
    it('should show "بيانات المؤسسة" section title', () => {
      cy.contains('h4', 'بيانات المؤسسة').scrollIntoView().should('exist');
    });

    it('should show commercial registration number field', () => {
      cy.get('input[formcontrolname="commercialRegNumber"]').scrollIntoView().should('exist');
    });

    it('should show company name field', () => {
      cy.get('input[formcontrolname="companyName"]').scrollIntoView().should('exist');
    });

    it('should show CR start and end dates', () => {
      cy.get('input[formcontrolname="crStartDate"]').scrollIntoView().should('exist');
      cy.get('input[formcontrolname="crEndDate"]').scrollIntoView().should('exist');
    });
  });

  describe('Employment Section', () => {
    it('should show employment section', () => {
      cy.contains('h4', 'بيانات العمل').scrollIntoView().should('exist');
      cy.get('mat-select[formcontrolname="employmentStatusId"]').scrollIntoView().should('exist');
    });
  });

  describe('Address Sections', () => {
    it('should show residence address section', () => {
      cy.contains('h4', 'عنوان صاحب المؤسسة').scrollIntoView().should('exist');
      cy.get('[formgroupname="residenceAddress"]').should('exist');
    });

    it('should show business address section', () => {
      cy.contains('h4', 'عنوان المؤسسة').scrollIntoView().should('exist');
      cy.get('[formgroupname="businessAddress"]').should('exist');
    });

    it('should show work address when employment status is خاص (BC02)', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').scrollIntoView().click();
      cy.get('mat-option').contains('خاص').click();
      cy.contains('h4', 'عنوان العمل').scrollIntoView().should('exist');
      cy.get('[formgroupname="workAddress"]').should('exist');
    });

    it('should hide work address when employment status is بدون عمل', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').scrollIntoView().click();
      cy.get('mat-option').contains('بدون عمل').click();
      cy.get('[formgroupname="workAddress"]').should('not.exist');
    });
  });
});
