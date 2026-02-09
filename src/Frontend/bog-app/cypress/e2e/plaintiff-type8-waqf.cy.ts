/// <reference types="cypress" />

describe('Plaintiff Type 8 - وقف (Waqf)', () => {
  beforeEach(() => {
    cy.setupPlaintiffContext(8);
  });

  describe('Waqf Data Section', () => {
    it('should display plaintiff type as وقف', () => {
      cy.contains('وقف').should('exist');
    });

    it('should show court deed number field', () => {
      cy.get('input[formcontrolname="courtDeedNumber"]').should('exist');
    });

    it('should show waqf name field', () => {
      cy.get('input[formcontrolname="waqfName"]').should('exist');
    });

    it('should show deed date', () => {
      cy.get('input[formcontrolname="deedDate"]').should('exist');
    });

    it('should show deed source', () => {
      cy.get('input[formcontrolname="deedSource"]').should('exist');
    });

    it('should show waqf oversight type dropdown', () => {
      cy.get('mat-select[formcontrolname="waqfOversightType"]').should('exist');
    });

    it('should show waqf description textarea', () => {
      cy.get('textarea[formcontrolname="waqfDescription"]').scrollIntoView().should('exist');
    });
  });

  describe('BC03 - Waqf Oversight Rules', () => {
    it('should hide agency name when oversight is أهلية', () => {
      cy.get('mat-select[formcontrolname="waqfOversightType"]').click();
      cy.get('mat-option').contains('أهلية').click();
      cy.get('input[formcontrolname="waqfAgencyName"]').should('not.exist');
    });

    it('should show agency name when oversight is حكومية', () => {
      cy.get('mat-select[formcontrolname="waqfOversightType"]').click();
      cy.get('mat-option').contains('حكومية').click();
      cy.get('input[formcontrolname="waqfAgencyName"]').should('exist');
    });
  });

  describe('Waqf Address', () => {
    it('should show waqf address section', () => {
      cy.get('[formgroupname="waqfAddress"]').scrollIntoView().should('exist');
    });

    it('should show region and city in waqf address', () => {
      cy.get('[formgroupname="waqfAddress"] mat-select[formcontrolname="regionId"]').scrollIntoView().should('exist');
      cy.get('[formgroupname="waqfAddress"] mat-select[formcontrolname="cityId"]').should('exist');
    });
  });

  describe('Hidden Sections', () => {
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
