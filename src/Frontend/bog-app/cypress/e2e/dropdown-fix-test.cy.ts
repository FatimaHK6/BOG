/**
 * E2E Test for Dropdown Fix
 * Verifies that dropdowns appear near the trigger, not at the bottom of the page
 */

describe('Dropdown Fix Verification', () => {
  const apiUrl = 'http://localhost:5001/api';

  before(() => {
    // Clear browser storage
    cy.window().then(win => {
      win.localStorage.clear();
      win.sessionStorage.clear();
    });
  });

  it('Should display dropdown correctly in defendant form dialog', () => {
    // Navigate to case registration list
    cy.visit('/case-registration/list', { timeout: 10000 });

    // Wait for page to load
    cy.contains('قائمة طلبات التسجيل').should('be.visible');

    // Click create new request button
    cy.contains('button', 'طلب جديد').click();

    // Wait for navigation to /create
    cy.url().should('include', '/create', { timeout: 3000 });

    // Wait for navigation to /edit and page to load
    cy.url().should('include', '/edit', { timeout: 10000 });
    cy.contains('إجراءات الطلب', { timeout: 5000 }).should('be.visible');

    // Scroll to defendants section
    cy.contains('المدعى عليهم').scrollIntoView();

    // Click add defendant button
    cy.contains('إضافة مدعى عليه').click();

    // Wait for dialog to open
    cy.get('mat-dialog-container', { timeout: 5000 }).should('be.visible');

    // Get the position of the defendant type dropdown trigger
    cy.get('mat-select[formcontrolname="defendantTypeId"]')
      .then(($select) => {
        const selectRect = $select[0].getBoundingClientRect();
        cy.log(`Defendant Type Select Position: Top=${selectRect.top}, Left=${selectRect.left}`);

        // Click the dropdown to open it
        cy.get('mat-select[formcontrolname="defendantTypeId"]').click();

        // Wait for dropdown panel to appear
        cy.get('.mat-select-panel', { timeout: 5000 }).should('be.visible');

        // Get dropdown panel position
        cy.get('.mat-select-panel').then(($panel) => {
          const panelRect = $panel[0].getBoundingClientRect();
          cy.log(`Dropdown Panel Position: Top=${panelRect.top}, Left=${panelRect.left}`);

          // Verify dropdown is near the trigger (within 300px)
          // If dropdown was at bottom, the distance would be much larger
          const distance = Math.abs(panelRect.top - (selectRect.top + selectRect.height));
          expect(distance).to.be.lessThan(300);

          // Verify dropdown is visible on screen
          expect(panelRect.top).to.be.greaterThan(0);
          expect(panelRect.top).to.be.lessThan(window.innerHeight - 50);

          cy.log(`✅ Dropdown appears correctly! Distance from trigger: ${distance}px`);
        });

        // Verify dropdown options are visible
        cy.contains('mat-option', 'طبيعي').should('be.visible');
        cy.contains('mat-option', 'شركة').should('be.visible');
        cy.contains('mat-option', 'جهة حكومية').should('be.visible');
      });
  });

  it('Should display identity type dropdown correctly', () => {
    // Navigate and create request
    cy.visit('/case-registration/list', { timeout: 10000 });
    cy.contains('button', 'طلب جديد').click();
    cy.url().should('include', '/edit', { timeout: 10000 });

    // Open defendant form
    cy.contains('المدعى عليهم').scrollIntoView();
    cy.contains('إضافة مدعى عليه').click();
    cy.get('mat-dialog-container', { timeout: 5000 }).should('be.visible');

    // First select "طبيعي" (Natural) to show identity fields
    cy.get('mat-select[formcontrolname="defendantTypeId"]').click();
    cy.contains('mat-option', 'طبيعي').click();

    // Wait for identity fields to appear
    cy.get('mat-select[formcontrolname="identityTypeId"]', { timeout: 3000 }).should('be.visible');

    // Click identity type dropdown
    cy.get('mat-select[formcontrolname="identityTypeId"]').click();

    // Verify dropdown panel appears
    cy.get('.mat-select-panel', { timeout: 5000 }).should('be.visible');

    // Verify options are visible
    cy.contains('mat-option', 'هوية وطنية').should('be.visible');
    cy.contains('mat-option', 'إقامة').should('be.visible');

    cy.log('✅ Identity type dropdown appears correctly!');
  });

  it('Should allow dropdown selection', () => {
    // Navigate and create request
    cy.visit('/case-registration/list', { timeout: 10000 });
    cy.contains('button', 'طلب جديد').click();
    cy.url().should('include', '/edit', { timeout: 10000 });

    // Open defendant form
    cy.contains('المدعى عليهم').scrollIntoView();
    cy.contains('إضافة مدعى عليه').click();
    cy.get('mat-dialog-container', { timeout: 5000 }).should('be.visible');

    // Click and select defendant type
    cy.get('mat-select[formcontrolname="defendantTypeId"]').click();
    cy.contains('mat-option', 'شركة').click({ force: true });

    // Verify selection was made
    cy.get('mat-select[formcontrolname="defendantTypeId"]').should('contain', 'شركة');

    // Verify commercial registration field appears (company-specific)
    cy.get('mat-form-field', { timeout: 3000 })
      .contains('رقم السجل التجاري')
      .should('be.visible');

    cy.log('✅ Dropdown selection works correctly!');
  });
});
