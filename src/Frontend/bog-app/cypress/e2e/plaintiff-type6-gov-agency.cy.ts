/// <reference types="cypress" />

describe('Plaintiff Type 6 - جهة حكومية (Government Agency)', () => {
  beforeEach(() => {
    cy.setupPlaintiffContext(6);
  });

  describe('Government Agency Data Section', () => {
    it('should display plaintiff type as جهة حكومية', () => {
      cy.contains('جهة حكومية').should('exist');
    });

    it('should show government agency dropdown', () => {
      cy.get('mat-select[formcontrolname="governmentAgencyId"]').should('exist');
    });

    it('should show headquarters field', () => {
      cy.get('input[formcontrolname="headquarters"]').should('exist');
    });

    it('should auto-fill headquarters when agency is selected', () => {
      cy.get('mat-select[formcontrolname="governmentAgencyId"]').click();
      cy.get('mat-option').contains('وزارة العدل').click();
      cy.get('input[formcontrolname="headquarters"]').should('have.value', 'الرياض');
    });

    it('should show additional statement textarea', () => {
      cy.get('textarea[formcontrolname="additionalStatement"]').should('exist');
    });
  });

  describe('Hidden Sections', () => {
    it('should NOT show address section', () => {
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
