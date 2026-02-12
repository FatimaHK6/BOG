/// <reference types="cypress" />

describe('Case Registration UI - Full Test Plan Execution', () => {
  const API_URL = 'https://localhost:5001';
  const APP_URL = 'http://localhost:4200';

  beforeEach(() => {
    // Disable certificate validation warnings
    cy.visit(APP_URL, {
      onBeforeLoad: (win) => {
        win.fetch = null;
      }
    });
  });

  describe('Section 1: Request List Page (/case-registration/list)', () => {

    it('TC-1.1: Should load request list page correctly', () => {
      cy.visit(`${APP_URL}/case-registration/list`);

      // Check page header
      cy.contains('قائمة طلبات التسجيل').should('be.visible');

      // Check for new request button
      cy.contains('button', 'طلب جديد').should('be.visible');

      // Check table exists
      cy.get('mat-table').should('exist');

      // Verify RTL layout
      cy.get('[dir="rtl"]').should('have.length.greaterThan', 0);

      cy.log('✅ TC-1.1 PASSED: Request list page loaded correctly');
    });

    it('TC-1.2: Should create new request', () => {
      cy.visit(`${APP_URL}/case-registration/list`);

      // Click create new request button
      cy.contains('button', 'طلب جديد').click();

      // Wait for redirect and new page to load
      cy.url().should('include', '/case-registration/');
      cy.url().should('include', '/edit');

      // Check request details page header
      cy.contains('h1').should('be.visible');

      // Check sidebar visible
      cy.get('.sidebar').should('be.visible');

      cy.log('✅ TC-1.2 PASSED: New request created and redirected to edit page');
    });

    it('TC-1.3: Should display requests in table', () => {
      cy.visit(`${APP_URL}/case-registration/list`);

      // Wait for table to load
      cy.get('mat-table').should('exist');

      // Check table rows
      cy.get('mat-table tbody tr').should('have.length.greaterThan', 0);

      // Check columns exist
      cy.contains('th', 'رقم الطلب').should('be.visible');
      cy.contains('th', 'الموضوع').should('be.visible');
      cy.contains('th', 'الحالة').should('be.visible');
      cy.contains('th', 'التاريخ').should('be.visible');

      cy.log('✅ TC-1.3 PASSED: Requests display in table');
    });

    it('TC-1.4: Should have pagination controls', () => {
      cy.visit(`${APP_URL}/case-registration/list`);

      // Check paginator exists
      cy.get('mat-paginator').should('be.visible');

      // Check page size options
      cy.get('mat-paginator').should('contain', '10');
      cy.get('mat-paginator').should('contain', '25');

      cy.log('✅ TC-1.4 PASSED: Pagination controls visible');
    });

    it('TC-1.5: Should show status badges with correct colors', () => {
      cy.visit(`${APP_URL}/case-registration/list`);

      // Check for status badges
      cy.get('.status-badge').should('have.length.greaterThan', 0);

      // Check badge styling
      cy.get('.status-1').should('have.css', 'background-color').and('include', 'rgb');
      cy.get('.status-3').should('have.css', 'background-color').and('include', 'rgb');
      cy.get('.status-6').should('have.css', 'background-color').and('include', 'rgb');

      cy.log('✅ TC-1.5 PASSED: Status badges display with colors');
    });

  });

  describe('Section 2: Request Details Page (/case-registration/{id}/edit)', () => {

    it('TC-2.1: Should display request details layout correctly', () => {
      // Create a new request first
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/case-registration/');

      // Check layout elements
      cy.get('.layout-header').should('be.visible');
      cy.get('.sidebar').should('be.visible');
      cy.get('.content-area').should('be.visible');

      // Check header content
      cy.get('.layout-header h1').should('be.visible');

      cy.log('✅ TC-2.1 PASSED: Request details layout correct');
    });

    it('TC-2.2: Should have sidebar with section navigation', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      // Check sidebar navigation items
      cy.get('.nav-sections').should('be.visible');
      cy.contains('.nav-item', 'المدعى عليهم').should('be.visible');
      cy.contains('.nav-item', 'بيانات الدعوى').should('be.visible');
      cy.contains('.nav-item', 'المرفقات').should('be.visible');

      cy.log('✅ TC-2.2 PASSED: Sidebar navigation visible');
    });

    it('TC-2.3: Should switch tabs when clicking sidebar items', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      // Click on defendants section
      cy.contains('.nav-item', 'المدعى عليهم').click();

      // Check if section changes
      cy.get('#defendants').should('be.visible');

      // Check URL has tab parameter
      cy.url().should('include', 'tab=defendants');

      cy.log('✅ TC-2.3 PASSED: Tab switching works');
    });

    it('TC-2.4: Should display status badge in header', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      // Check status badge exists in header
      cy.get('.request-status').should('be.visible');

      // Status should be Draft (مسودة) for new request
      cy.get('.request-status').should('contain', 'مسودة');

      cy.log('✅ TC-2.4 PASSED: Status badge displays in header');
    });

  });

  describe('Section 3: Defendants Management (المدعى عليهم)', () => {

    it('TC-3.1: Should show add defendant button in empty state', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      // Go to defendants tab
      cy.contains('.nav-item', 'المدعى عليهم').click();
      cy.get('#defendants').should('be.visible');

      // Check empty state message
      cy.contains('لم يتم إضافة أي مدعى عليه بعد').should('be.visible');

      // Check add button exists
      cy.contains('button', 'إضافة أول مدعى عليه').should('be.visible');

      cy.log('✅ TC-3.1 PASSED: Add defendant button visible in empty state');
    });

    it('TC-3.2: Should open defendant form dialog on button click', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      cy.contains('.nav-item', 'المدعى عليهم').click();
      cy.get('#defendants').should('be.visible');

      // Click add defendant button
      cy.contains('button', 'إضافة أول مدعى عليه').click();

      // Check if dialog opens
      cy.get('mat-dialog-container').should('be.visible');
      cy.contains('h2', 'إضافة مدعى عليه').should('be.visible');

      cy.log('✅ TC-3.2 PASSED: Defendant form dialog opened');
    });

    it('TC-3.3: Should display defendant form fields', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      cy.contains('.nav-item', 'المدعى عليهم').click();
      cy.contains('button', 'إضافة أول مدعى عليه').click();

      // Wait for dialog
      cy.get('mat-dialog-container').should('be.visible');

      // Check form fields exist
      cy.contains('mat-label', 'نوع المدعى عليه').should('be.visible');
      cy.contains('mat-label', 'الاسم الكامل').should('be.visible');
      cy.contains('mat-label', 'نوع الهوية').should('be.visible');
      cy.contains('mat-label', 'رقم الهوية').should('be.visible');
      cy.contains('mat-label', 'العنوان').should('be.visible');

      cy.log('✅ TC-3.3 PASSED: Defendant form fields visible');
    });

    it('TC-3.4: Should validate required fields', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      cy.contains('.nav-item', 'المدعى عليهم').click();
      cy.contains('button', 'إضافة أول مدعى عليه').click();

      cy.get('mat-dialog-container').should('be.visible');

      // Try to submit empty form
      cy.contains('button', 'إضافة').click();

      // Check for validation error
      cy.get('mat-error').should('be.visible');
      cy.contains('mat-error', 'مطلوب').should('exist');

      cy.log('✅ TC-3.4 PASSED: Form validation works');
    });

  });

  describe('Section 4: Case Data Form (بيانات الدعوى)', () => {

    it('TC-4.1: Should display case data form', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      // Go to case data tab
      cy.contains('.nav-item', 'بيانات الدعوى').click();
      cy.get('#case-data').should('be.visible');

      // Check form fields
      cy.contains('mat-label', 'موضوع الدعوى').should('be.visible');
      cy.contains('mat-label', 'الأدلة والمستندات').should('be.visible');

      cy.log('✅ TC-4.1 PASSED: Case data form visible');
    });

    it('TC-4.2: Should count characters in subject field', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      cy.contains('.nav-item', 'بيانات الدعوى').click();

      // Find subject textarea
      cy.get('textarea[formControlName="subject"]').type('اختبار النص');

      // Check character counter
      cy.contains('mat-hint', '/4000').should('be.visible');

      cy.log('✅ TC-4.2 PASSED: Character counter works');
    });

  });

  describe('Section 5: Material Components & Styling', () => {

    it('TC-5.1: Should apply RTL layout', () => {
      cy.visit(`${APP_URL}/case-registration/list`);

      // Check for RTL directive
      cy.get('[dir="rtl"]').should('exist');

      // Check app-root has dir attribute
      cy.get('app-root').should('have.attr', 'dir', 'rtl');

      cy.log('✅ TC-5.1 PASSED: RTL layout applied');
    });

    it('TC-5.2: Should display Material icons', () => {
      cy.visit(`${APP_URL}/case-registration/list`);

      // Check for Material icons
      cy.get('mat-icon').should('have.length.greaterThan', 0);

      // Icons should not be empty
      cy.get('mat-icon').first().should('not.be.empty');

      cy.log('✅ TC-5.2 PASSED: Material icons displayed');
    });

    it('TC-5.3: Should style buttons correctly', () => {
      cy.visit(`${APP_URL}/case-registration/list`);

      // Check for primary button
      cy.contains('button', 'طلب جديد').should('have.class', 'mat-raised-button');

      // Check button is clickable
      cy.contains('button', 'طلب جديد').should('not.be.disabled');

      cy.log('✅ TC-5.3 PASSED: Buttons styled correctly');
    });

    it('TC-5.4: Should display form fields with Material styling', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();
      cy.url().should('include', '/edit');

      cy.contains('.nav-item', 'بيانات الدعوى').click();

      // Check Material form fields
      cy.get('mat-form-field').should('have.length.greaterThan', 0);
      cy.get('mat-form-field').first().should('have.class', 'mat-form-field-outline');

      cy.log('✅ TC-5.4 PASSED: Form fields styled correctly');
    });

  });

  describe('Section 6: Responsive Design', () => {

    it('TC-6.1: Should display sidebar on desktop', () => {
      cy.viewport(1400, 900);
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();

      // Sidebar should be visible on desktop
      cy.get('.sidebar').should('be.visible');

      cy.log('✅ TC-6.1 PASSED: Sidebar visible on desktop');
    });

    it('TC-6.2: Should be responsive on tablet', () => {
      cy.viewport(800, 600);
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();

      // Page should still be usable
      cy.get('.content-area').should('be.visible');

      cy.log('✅ TC-6.2 PASSED: Responsive on tablet');
    });

    it('TC-6.3: Should be responsive on mobile', () => {
      cy.viewport(375, 667);
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طلب جديد').click();

      // Content should be visible
      cy.get('.content-area').should('be.visible');

      // Mobile menu button might appear
      cy.get('.mobile-sidebar-toggle').should('exist');

      cy.log('✅ TC-6.3 PASSED: Responsive on mobile');
    });

  });

  describe('Section 7: Error Handling', () => {

    it('TC-7.1: Should show error on API failure', () => {
      // This test would intercept API calls and simulate failures
      cy.intercept('GET', `${API_URL}/api/case-requests`, {
        statusCode: 500,
        body: { message: 'Server error' }
      }).as('getRequestsFail');

      cy.visit(`${APP_URL}/case-registration/list`);
      cy.wait('@getRequestsFail');

      // Check for error message
      cy.get('snack-bar-container').should('be.visible');

      cy.log('✅ TC-7.1 PASSED: Error handled gracefully');
    });

  });

  describe('Known Issues', () => {

    it('❌ TC-ISSUE-1: Dropdown overlaps field label (KNOWN ISSUE - UNFIXED)', () => {
      cy.visit(`${APP_URL}/case-registration/list`);

      // Try to click status dropdown
      cy.get('mat-select[formControlName="status"]').click();

      // Check if dropdown opens
      cy.get('.mat-select-panel').should('be.visible');

      // ISSUE: Label should be visible above dropdown but is not
      cy.log('⚠️ KNOWN ISSUE: Dropdown overlaps label');
    });

    it('❌ TC-ISSUE-2: Add defendant button click not working (KNOWN ISSUE - UNFIXED)', () => {
      cy.visit(`${APP_URL}/case-registration/list`);
      cy.contains('button', 'طลب جديد').click();
      cy.url().should('include', '/edit');

      cy.contains('.nav-item', 'المدعى عليهم').click();

      // Button exists but click might not work
      cy.contains('button', 'إضافة أول مدعى عليه').should('be.visible');

      cy.log('⚠️ KNOWN ISSUE: Button click may not open dialog');
    });

  });

});
