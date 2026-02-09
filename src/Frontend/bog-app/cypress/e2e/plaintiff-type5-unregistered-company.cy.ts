/// <reference types="cypress" />

describe('Plaintiff Type 5 - شركة غير مسجلة (Unregistered Company)', () => {
  beforeEach(() => {
    cy.setupPlaintiffContext(5);
  });

  describe('Company Data Section', () => {
    it('should display plaintiff type as شركة غير مسجلة', () => {
      cy.contains('شركة غير مسجلة').should('exist');
    });

    it('should show commercial registration number (maxlength 20)', () => {
      cy.get('input[formcontrolname="commercialRegNumber"]').should('exist');
    });

    it('should show unregistered company name', () => {
      cy.get('input[formcontrolname="unregisteredCompanyName"]').should('exist');
    });

    it('should show country dropdown', () => {
      cy.get('mat-select[formcontrolname="unregisteredCountryId"]').should('exist');
    });

    it('should show city text input', () => {
      cy.get('input[formcontrolname="unregisteredCity"]').should('exist');
    });

    it('should show description textarea (maxlength 1000)', () => {
      cy.get('textarea[formcontrolname="unregisteredDescription"]').should('exist');
    });
  });

  describe('Hidden Sections', () => {
    it('should NOT show national address section', () => {
      cy.get('[formgroupname="residenceAddress"]').should('not.exist');
      cy.get('[formgroupname="companyAddress"]').should('not.exist');
    });

    it('should NOT show contact fields', () => {
      cy.get('input[formcontrolname="mobileNumber"]').should('not.exist');
      cy.get('input[formcontrolname="email"]').should('not.exist');
    });

    it('should NOT show identity fields', () => {
      cy.get('input[formcontrolname="identityNumber"]').should('not.exist');
      cy.get('input[formcontrolname="firstName"]').should('not.exist');
    });
  });
});
