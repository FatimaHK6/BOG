/// <reference types="cypress" />

describe('Plaintiff Form Stepper & Cross-Cutting', () => {

  describe('Stepper Navigation', () => {
    beforeEach(() => {
      cy.setupPlaintiffContext(1);
    });

    it('should show 4 step headers', () => {
      cy.get('mat-step-header').should('have.length', 4);
    });

    it('should start on step 1 (البيانات الشخصية)', () => {
      cy.contains('h3', 'البيانات الشخصية').should('be.visible');
    });

    it('should navigate to step 2 (بيانات الممثلين)', () => {
      cy.navigateToStep(1);
      cy.contains('h3', 'بيانات الممثلين').should('be.visible');
    });

    it('should navigate to step 3 (المرفقات)', () => {
      cy.navigateToStep(2);
      cy.contains('المرفقات').should('exist');
    });

    it('should navigate to step 4 (بيانات إضافية)', () => {
      cy.navigateToStep(3);
      cy.contains('بيانات إضافية').should('exist');
    });

    it('should navigate back to step 1 from step 3', () => {
      cy.navigateToStep(2);
      cy.navigateToStep(0);
      cy.contains('h3', 'البيانات الشخصية').should('be.visible');
    });
  });

  describe('Step 2 - Representatives', () => {
    it('should show add representative button for فرد (type 1)', () => {
      cy.setupPlaintiffContext(1);
      cy.navigateToStep(1);
      cy.contains('button', 'إضافة ممثّل للمدّعي').should('exist');
    });

    it('should show allowed representative types for شركة مسجلة (type 4)', () => {
      cy.setupPlaintiffContext(4);
      cy.navigateToStep(1);
      cy.contains('button', 'إضافة ممثّل للمدّعي').click();
      cy.get('.mat-mdc-menu-panel', { timeout: 5000 }).should('be.visible');
      // شركة مسجلة allows: وكيل, مصفي, أمين التفليسة, حارس قضائي, ممثل نظامي
      cy.get('.mat-mdc-menu-panel').contains('وكيل').should('exist');
    });
  });

  describe('Step 4 - Additional Data', () => {
    beforeEach(() => {
      cy.setupPlaintiffContext(1);
      cy.navigateToStep(3);
    });

    it('should show isApplicant checkbox', () => {
      cy.get('[formcontrolname="isApplicant"], mat-checkbox').should('exist');
    });

    it('should show addSelectedAddress checkbox', () => {
      cy.get('[formcontrolname="addSelectedAddress"], mat-checkbox').should('exist');
    });
  });

  describe('No Type Parameter - Redirect', () => {
    it('should redirect to list when no type param is provided', () => {
      cy.setupApiIntercepts();
      cy.visit('/case-registration/plaintiffs/add?requestId=1');
      // Should redirect to plaintiff list
      cy.url().should('include', '/case-registration/plaintiffs');
      cy.url().should('not.include', '/add');
    });
  });

  describe('Form Header', () => {
    it('should show "إضافة مدعي جديد" header in add mode', () => {
      cy.setupPlaintiffContext(1);
      cy.contains('h2', 'إضافة مدعي جديد').should('exist');
    });
  });
});
