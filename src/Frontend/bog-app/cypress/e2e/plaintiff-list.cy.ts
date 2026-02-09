/// <reference types="cypress" />

describe('Plaintiff List Page', () => {
  describe('Empty State', () => {
    beforeEach(() => {
      cy.setupApiIntercepts();
      cy.intercept('GET', '**/api/case-requests/*/plaintiffs', {
        statusCode: 200,
        body: []
      }).as('getEmptyPlaintiffs');
      cy.intercept('GET', '**/api/plaintiffs/*/representatives', {
        statusCode: 200,
        body: []
      }).as('getReps');
      cy.visit('/case-registration/plaintiffs?requestId=1');
      cy.get('app-plaintiff-list', { timeout: 15000 }).should('exist');
    });

    it('should display empty state message', () => {
      cy.contains('لا يوجد مدعين مسجلين').should('exist');
    });

    it('should show add plaintiff button in empty state', () => {
      cy.contains('button', 'إضافة مدعي').should('exist');
    });
  });

  describe('With Plaintiffs', () => {
    beforeEach(() => {
      cy.setupPlaintiffListContext();
    });

    it('should display plaintiff table', () => {
      cy.get('table[mat-table]').should('exist');
    });

    it('should show correct table columns', () => {
      cy.contains('th', 'نوع المدعي').should('exist');
      cy.contains('th', 'رقم الهوية').should('exist');
      cy.contains('th', 'اسم المدعي').should('exist');
      cy.contains('th', 'حالة المرفقات').should('exist');
      cy.contains('th', 'مقدم الطلب').should('exist');
      cy.contains('th', 'الإجراءات').should('exist');
    });

    it('should display plaintiff data in table rows', () => {
      cy.contains('td', 'فرد').should('exist');
      cy.contains('td', '1234567890').should('exist');
      cy.contains('td', 'محمد أحمد السعيد').should('exist');
    });
  });

  describe('Add Plaintiff Menu', () => {
    beforeEach(() => {
      cy.setupPlaintiffListContext();
    });

    it('should show "إضافة مدعي جديد" button', () => {
      cy.contains('button', 'إضافة مدعي جديد').should('exist');
    });

    it('should open type menu with 8 plaintiff types', () => {
      cy.contains('button', 'إضافة مدعي جديد').click();
      cy.get('.mat-mdc-menu-panel', { timeout: 5000 }).should('be.visible');
      const types = ['فرد', 'فرد بدون هوية', 'صاحب مؤسسة', 'شركة مسجلة', 'شركة غير مسجلة', 'جهة حكومية', 'جمعية/مؤسسة أهلية', 'وقف'];
      types.forEach(typeName => {
        cy.get('.mat-mdc-menu-panel').contains(typeName).should('exist');
      });
    });

    it('should navigate to add form when type is selected', () => {
      cy.contains('button', 'إضافة مدعي جديد').click();
      cy.get('.mat-mdc-menu-panel', { timeout: 5000 }).should('be.visible');
      cy.contains('[mat-menu-item]', 'فرد').first().click();
      cy.url().should('include', '/plaintiffs/add');
      cy.url().should('include', 'type=1');
    });
  });

  describe('Save as Draft', () => {
    beforeEach(() => {
      cy.setupPlaintiffListContext();
    });

    it('should show "حفظ كمسودة" button', () => {
      cy.contains('button', 'حفظ كمسودة').should('exist');
    });
  });

  describe('Representatives Section', () => {
    beforeEach(() => {
      cy.setupPlaintiffListContext();
    });

    it('should show representatives section', () => {
      cy.contains('الممثلين').should('exist');
    });
  });
});
