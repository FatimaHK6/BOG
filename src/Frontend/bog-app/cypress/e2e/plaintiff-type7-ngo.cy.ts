/// <reference types="cypress" />

describe('Plaintiff Type 7 - جمعية/مؤسسة أهلية (NGO)', () => {
  beforeEach(() => {
    cy.setupPlaintiffContext(7);
  });

  describe('NGO Data Section', () => {
    it('should display plaintiff type as جمعية/مؤسسة أهلية', () => {
      cy.contains('جمعية/مؤسسة أهلية').should('exist');
    });

    it('should show NGO name field', () => {
      cy.get('input[formcontrolname="ngoName"]').should('exist');
    });

    it('should show license number field', () => {
      cy.get('input[formcontrolname="licenseNumber"]').should('exist');
    });

    it('should show license source dropdown', () => {
      cy.get('mat-select[formcontrolname="licenseSourceId"]').should('exist');
    });

    it('should show license date', () => {
      cy.get('input[formcontrolname="licenseDate"]').should('exist');
    });
  });

  describe('NGO Address', () => {
    it('should show NGO address section', () => {
      cy.get('[formgroupname="ngoAddress"]').scrollIntoView().should('exist');
    });

    it('should show all address fields in NGO address', () => {
      cy.get('[formgroupname="ngoAddress"] mat-select[formcontrolname="regionId"]').scrollIntoView().should('exist');
      cy.get('[formgroupname="ngoAddress"] mat-select[formcontrolname="cityId"]').should('exist');
      cy.get('[formgroupname="ngoAddress"] input[formcontrolname="districtId"]').should('exist');
      cy.get('[formgroupname="ngoAddress"] input[formcontrolname="street"]').should('exist');
      cy.get('[formgroupname="ngoAddress"] input[formcontrolname="buildingNumber"]').should('exist');
      cy.get('[formgroupname="ngoAddress"] input[formcontrolname="unitNumber"]').should('exist');
      cy.get('[formgroupname="ngoAddress"] input[formcontrolname="postalCode"]').should('exist');
      cy.get('[formgroupname="ngoAddress"] input[formcontrolname="additionalCode"]').should('exist');
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

    it('should NOT show employment section', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').should('not.exist');
    });
  });
});
