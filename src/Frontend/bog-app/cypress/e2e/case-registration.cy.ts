/**
 * E2E Tests for Case Registration Module (Developer-B)
 * Tests scenarios: Defendants, Case Data, Attachments, Request Actions
 */

describe('Case Registration Module - E2E Tests', () => {
  const apiUrl = 'http://localhost:5001/api';

  before(() => {
    // Clear browser storage before tests
    cy.window().then(win => {
      win.localStorage.clear();
      win.sessionStorage.clear();
    });
  });

  /**
   * Scenario 1: Navigation to Case Registration
   */
  describe('Scenario 1: Navigation', () => {
    it('Should navigate to Case Registration when clicking the button', () => {
      cy.visit('/');

      // Verify header is visible
      cy.contains('نظام إدارة الدعاوى').should('be.visible');

      // Click Case Registration navigation
      cy.contains('إدارة الدعاوى').click();

      // Wait for page to load
      cy.url().should('include', '/case-registration');

      // Verify request list page loads
      cy.contains('قائمة طلبات التسجيل', { timeout: 5000 }).should('be.visible');
    });

    it('Should show create new request button', () => {
      cy.visit('/case-registration/list');

      // Verify button exists
      cy.contains('طلب جديد').should('be.visible');
    });
  });

  /**
   * Scenario 2: Create New Request
   */
  describe('Scenario 2: Create New Request', () => {
    it('Should create a new case request', () => {
      cy.visit('/case-registration/list', { timeout: 10000 });

      // Click create new request button
      cy.contains('button', 'طلب جديد').click();

      // Wait for page to navigate to create view first
      cy.url().should('include', '/create', { timeout: 3000 });

      // Wait for request to be created and navigated to edit
      cy.url().should('include', '/edit', { timeout: 10000 });

      // Verify request details page loads
      cy.contains('إجراءات الطلب', { timeout: 5000 }).should('be.visible');
    });

    it('Should show request ID after creation', () => {
      cy.visit('/case-registration/list', { timeout: 10000 });
      cy.contains('button', 'طلب جديد').click();

      // Wait for create redirect
      cy.url().should('include', '/create', { timeout: 3000 });

      // Verify URL has request ID and /edit path
      cy.url().should('match', /\/case-registration\/\d+\/edit/, { timeout: 10000 });
    });
  });

  /**
   * Scenario 3: Defendants Management - Add
   */
  describe('Scenario 3: Add Defendant', () => {
    beforeEach(() => {
      // Create a new request for each test
      cy.visit('/case-registration/list');
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');
    });

    it('Should open add defendant dialog', () => {
      // Scroll to defendants section
      cy.contains('المدعى عليهم').scrollIntoView();

      // Click add defendant button
      cy.contains('إضافة مدعى عليه').click();

      // Verify dialog opens
      cy.get('[mat-dialog-title]', { timeout: 3000 }).should('be.visible');
    });

    it('Should add a natural person defendant successfully', () => {
      cy.contains('المدعى عليهم').scrollIntoView();
      cy.contains('إضافة مدعى عليه').click();

      // Wait for dialog
      cy.get('mat-dialog-container', { timeout: 3000 }).should('be.visible');

      // Fill form
      cy.get('mat-select[formcontrolname="defendantTypeId"]').click();
      cy.contains('mat-option', 'طبيعي').click();

      cy.get('input[formcontrolname="fullName"]')
        .type('أحمد محمد الشريف');

      cy.get('mat-select[formcontrolname="identityTypeId"]').click();
      cy.contains('mat-option', 'هوية وطنية').click();

      cy.get('input[formcontrolname="identityNumber"]')
        .type('1234567890');

      cy.get('textarea[formcontrolname="addressText"]')
        .type('الرياض، حي النخيل');

      // Submit form
      cy.contains('button', 'إضافة').click();

      // Verify defendant added (check table)
      cy.contains('أحمد محمد الشريف', { timeout: 3000 }).should('be.visible');
    });

    it('Should display count badge after adding defendant', () => {
      cy.contains('المدعى عليهم').scrollIntoView();
      cy.contains('إضافة مدعى عليه').click();

      cy.get('mat-select[formcontrolname="defendantTypeId"]').click();
      cy.contains('mat-option', 'طبيعي').click();
      cy.get('input[formcontrolname="fullName"]').type('علي أحمد');
      cy.get('input[formcontrolname="identityNumber"]').type('0987654321');
      cy.contains('button', 'إضافة').click();

      // Verify count badge shows "1"
      cy.contains('المدعى عليهم').parent()
        .contains('1')
        .should('be.visible');
    });

    it('Should add a company defendant', () => {
      cy.contains('المدعى عليهم').scrollIntoView();
      cy.contains('إضافة مدعى عليه').click();

      cy.get('mat-select[formcontrolname="defendantTypeId"]').click();
      cy.contains('mat-option', 'شركة').click();

      cy.get('input[formcontrolname="fullName"]')
        .type('شركة النور للتجارة');

      cy.get('input[formcontrolname="commercialRegNumber"]')
        .type('123456');

      cy.contains('button', 'إضافة').click();

      cy.contains('شركة النور للتجارة', { timeout: 3000 }).should('be.visible');
    });
  });

  /**
   * Scenario 4: Defendants Management - Duplicate Detection (ERR013)
   */
  describe('Scenario 4: Duplicate Detection (ERR013)', () => {
    beforeEach(() => {
      cy.visit('/case-registration/list');
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');
    });

    it('Should show ERR013 when adding duplicate defendant', () => {
      cy.contains('المدعى عليهم').scrollIntoView();

      // Add first defendant
      cy.contains('إضافة مدعى عليه').click();
      cy.get('mat-select[formcontrolname="defendantTypeId"]').click();
      cy.contains('mat-option', 'طبيعي').click();
      cy.get('input[formcontrolname="fullName"]').type('محمد أحمد');
      cy.get('input[formcontrolname="identityNumber"]').type('1111111111');
      cy.contains('button', 'إضافة').click();

      cy.contains('محمد أحمد', { timeout: 3000 }).should('be.visible');

      // Try to add same defendant again
      cy.contains('إضافة مدعى عليه').click();
      cy.get('mat-select[formcontrolname="defendantTypeId"]').click();
      cy.contains('mat-option', 'طبيعي').click();
      cy.get('input[formcontrolname="fullName"]').type('علي علي');
      cy.get('input[formcontrolname="identityNumber"]').type('1111111111');
      cy.contains('button', 'إضافة').click();

      // Verify ERR013 error appears
      cy.contains('ERR013', { timeout: 3000 }).should('be.visible');
    });
  });

  /**
   * Scenario 5: Case Data Management
   */
  describe('Scenario 5: Case Data Form', () => {
    beforeEach(() => {
      cy.visit('/case-registration/list');
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');
    });

    it('Should fill case data form', () => {
      cy.contains('بيانات الدعوى').scrollIntoView();

      cy.get('textarea[formcontrolname="subject"]')
        .type('دعوى استرجاع مبالغ مالية مستحقة');

      cy.get('textarea[formcontrolname="evidence"]')
        .type('عقود وفواتير وتحويلات بنكية');

      // Verify character count displayed
      cy.contains('/4000').should('be.visible');
    });

    it('Should enforce character limits', () => {
      cy.contains('بيانات الدعوى').scrollIntoView();

      // Try to type more than 4000 characters
      const longText = 'a'.repeat(5000);
      cy.get('textarea[formcontrolname="subject"]')
        .type(longText);

      // Verify input is limited
      cy.get('textarea[formcontrolname="subject"]')
        .invoke('val')
        .then(val => {
          expect((val as string).length).to.be.at.most(4000);
        });
    });
  });

  /**
   * Scenario 6: Request Actions - Submit
   */
  describe('Scenario 6: Submit Request', () => {
    it('Should submit a complete request', () => {
      cy.visit('/case-registration/list');
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      // Add defendant
      cy.contains('المدعى عليهم').scrollIntoView();
      cy.contains('إضافة مدعى عليه').click();
      cy.get('mat-select[formcontrolname="defendantTypeId"]').click();
      cy.contains('mat-option', 'طبيعي').click();
      cy.get('input[formcontrolname="fullName"]').type('اختبار');
      cy.get('input[formcontrolname="identityNumber"]').type('9999999999');
      cy.contains('button', 'إضافة').click();

      // Add case data
      cy.contains('بيانات الدعوى').scrollIntoView();
      cy.get('textarea[formcontrolname="subject"]')
        .type('موضوع الاختبار');
      cy.get('textarea[formcontrolname="evidence"]')
        .type('الأدلة');

      // Click submit button
      cy.contains('إرسال الطلب').scrollIntoView().click();

      // Verify success message
      cy.contains('تم إرسال الطلب بنجاح', { timeout: 5000 })
        .should('be.visible');
    });
  });

  /**
   * Scenario 7: Request List & Search
   */
  describe('Scenario 7: Request List', () => {
    it('Should display request list', () => {
      cy.visit('/case-registration/list');

      cy.contains('قائمة طلبات التسجيل').should('be.visible');
      cy.get('table').should('be.visible');
    });

    it('Should have search filters', () => {
      cy.visit('/case-registration/list');

      // Verify search fields
      cy.get('input[formcontrolname="requestNumber"]').should('be.visible');
      cy.get('mat-select[formcontrolname="status"]').should('be.visible');
      cy.contains('button', 'search').should('be.visible');
    });

    it('Should have pagination controls', () => {
      cy.visit('/case-registration/list');

      // Verify paginator
      cy.get('mat-paginator').should('be.visible');
    });
  });

  /**
   * Scenario 8: Responsive Design
   */
  describe('Scenario 8: Responsive Design', () => {
    it('Should display correctly on desktop', () => {
      cy.viewport(1920, 1080);
      cy.visit('/');

      cy.contains('نظام إدارة الدعاوى').should('be.visible');
      cy.contains('إدارة الدعاوى').should('be.visible');
    });

    it('Should display correctly on tablet', () => {
      cy.viewport('ipad-2');
      cy.visit('/');

      cy.contains('نظام إدارة الدعاوى').should('be.visible');
    });

    it('Should display correctly on mobile', () => {
      cy.viewport('iphone-x');
      cy.visit('/');

      cy.contains('نظام إدارة الدعاوى').should('be.visible');
    });
  });

  /**
   * Scenario 9: RTL Layout
   */
  describe('Scenario 9: RTL Layout', () => {
    it('Should have RTL direction', () => {
      cy.visit('/');

      cy.get('app-root').should('have.attr', 'dir', 'rtl');
    });

    it('Should display Arabic text correctly', () => {
      cy.visit('/');

      cy.contains('نظام إدارة الدعاوى').should('be.visible');
      cy.contains('إدارة الدعاوى').should('be.visible');
    });
  });

  /**
   * Scenario 10: Material Theme
   */
  describe('Scenario 10: Material Theme', () => {
    it('Should use correct primary color', () => {
      cy.visit('/');

      // Check header background (green theme)
      cy.get('.app-header')
        .should('have.css', 'background-color')
        .and('include', 'rgb(27, 94, 32)'); // #1B5E20
    });

    it('Should render Material icons', () => {
      cy.visit('/case-registration/list');

      // Verify icons are rendered (not showing text)
      cy.get('mat-icon').should('be.visible');
    });
  });
});
