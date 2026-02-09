/// <reference types="cypress" />

describe('Plaintiff Type 4 - شركة مسجلة (Registered Company)', () => {
  beforeEach(() => {
    cy.setupPlaintiffContext(4);
  });

  describe('Company Data Section', () => {
    it('should display plaintiff type as شركة مسجلة', () => {
      cy.contains('شركة مسجلة').should('exist');
    });

    it('should show commercial registration number', () => {
      cy.get('input[formcontrolname="commercialRegNumber"]').should('exist');
    });

    it('should show company name', () => {
      cy.get('input[formcontrolname="companyName"]').should('exist');
    });

    it('should show CR start date', () => {
      cy.get('input[formcontrolname="crStartDate"]').should('exist');
    });

    it('should show CR end date', () => {
      cy.get('input[formcontrolname="crEndDate"]').should('exist');
    });
  });

  describe('Company Address', () => {
    it('should show company address section', () => {
      cy.contains('h4', 'عنوان الشركة').scrollIntoView().should('exist');
      cy.get('[formgroupname="companyAddress"]').should('exist');
    });

    it('should show all address fields', () => {
      cy.get('[formgroupname="companyAddress"] mat-select[formcontrolname="regionId"]').scrollIntoView().should('exist');
      cy.get('[formgroupname="companyAddress"] mat-select[formcontrolname="cityId"]').should('exist');
      cy.get('[formgroupname="companyAddress"] input[formcontrolname="districtId"]').should('exist');
      cy.get('[formgroupname="companyAddress"] input[formcontrolname="street"]').should('exist');
      cy.get('[formgroupname="companyAddress"] input[formcontrolname="buildingNumber"]').should('exist');
      cy.get('[formgroupname="companyAddress"] input[formcontrolname="unitNumber"]').should('exist');
      cy.get('[formgroupname="companyAddress"] input[formcontrolname="postalCode"]').should('exist');
      cy.get('[formgroupname="companyAddress"] input[formcontrolname="additionalCode"]').should('exist');
    });
  });

  describe('Hidden Sections', () => {
    it('should NOT show contact fields (mobile, email)', () => {
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
