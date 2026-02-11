/// <reference types="cypress" />

describe('Plaintiff Type 2 - فرد بدون هوية (Individual without ID)', () => {
  beforeEach(() => {
    cy.setupPlaintiffContext(2);
  });

  describe('Form Sections Visibility', () => {
    it('should display plaintiff type as فرد بدون هوية', () => {
      cy.contains('فرد بدون هوية').should('exist');
    });

    it('should show document number field (optional)', () => {
      cy.get('input[formcontrolname="documentNumber"]').should('exist');
    });

    it('should NOT show identity type dropdown', () => {
      cy.get('mat-select[formcontrolname="identityTypeId"]').should('not.exist');
    });

    it('should NOT show Absher verification section', () => {
      cy.contains('أبشر').should('not.exist');
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

    it('should NOT show identity dates', () => {
      cy.get('input[formcontrolname="identityIssueDate"]').should('not.exist');
      cy.get('input[formcontrolname="identityExpiryDate"]').should('not.exist');
    });

    it('should NOT show employment section', () => {
      cy.get('mat-select[formcontrolname="employmentStatusId"]').should('not.exist');
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

  describe('Residence Address', () => {
    it('should show residence address section', () => {
      cy.get('[formgroupname="residenceAddress"]').scrollIntoView().should('exist');
    });
  });
});
