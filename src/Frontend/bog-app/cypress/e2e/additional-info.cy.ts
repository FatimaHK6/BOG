/**
 * E2E Tests for Additional Info Feature (UC 6.5.1.1.15)
 * Tests all three types of additional information:
 * - Type 1: Management Decision Cancellation (إلغاء قرار إداري)
 * - Type 2: Service/Retirement Rights (حقوق خدمة/تقاعدية)
 * - Type 3: Trademark Dispute (نزاع علامة تجارية)
 */

describe('Additional Info Feature - UC 6.5.1.1.15', () => {
  const baseUrl = 'http://localhost:4200';
  const requestId = 1; // Use existing request ID for testing

  beforeEach(() => {
    // Mock API responses for lookups
    cy.intercept('GET', '**/api/lookups/notification-methods', {
      statusCode: 200,
      body: [
        { id: 1, name: 'Email', nameAr: 'البريد الإلكتروني' },
        { id: 2, name: 'SMS', nameAr: 'رسالة نصية' },
        { id: 3, name: 'Official Gazette', nameAr: 'الجريدة الرسمية' }
      ]
    }).as('getNotificationMethods');

    cy.intercept('GET', '**/api/lookups/government-entities', {
      statusCode: 200,
      body: [
        { id: 1, name: 'Ministry of Justice', nameAr: 'وزارة العدل' },
        { id: 2, name: 'Ministry of Interior', nameAr: 'وزارة الداخلية' },
        { id: 3, name: 'Ministry of Finance', nameAr: 'وزارة المالية' }
      ]
    }).as('getGovernmentEntities');

    cy.intercept('GET', `**/api/case-requests/${requestId}/additional-info`, {
      statusCode: 404,
      body: {}
    }).as('getAdditionalInfo');

    cy.intercept('PUT', `**/api/case-requests/${requestId}/additional-info`, (req) => {
      req.reply({
        statusCode: 200,
        body: req.body
      });
    }).as('saveAdditionalInfo');

    // Navigate to case registration page
    cy.visit(`${baseUrl}/case-registration/request-details/${requestId}`);
    cy.wait(['@getNotificationMethods', '@getGovernmentEntities', '@getAdditionalInfo']);
  });

  describe('Page Load and Initialization', () => {
    it('should display all three additional info sections', () => {
      cy.get('.section-box').should('have.length', 3);
      cy.get('.type1-section').should('contain', 'دعاوى إلغاء القرارات الإدارية');
      cy.get('.type2-section').should('contain', 'دعاوى الحقوق المتعلقة بالخدمة والتقاعد');
      cy.get('.type3-section').should('contain', 'نزاع علامة تجارية');
    });

    it('should load lookup data from API', () => {
      // Wait for requests
      cy.wait('@getNotificationMethods');
      cy.wait('@getGovernmentEntities');

      // Verify dropdowns are populated
      cy.get('mat-select[formControlName="notificationMethodId"]')
        .click()
        .get('mat-option')
        .should('have.length.greaterThan', 0);
    });

    it('should display form in editable state', () => {
      cy.get('input[formControlName="decisionNumber"]')
        .should('not.have.attr', 'disabled');
      cy.get('mat-select[formControlName="notificationMethodId"]')
        .should('not.have.attr', 'disabled');
    });

    it('should render section icons', () => {
      cy.get('.type1-section .section-icon').should('contain', 'gavel');
      cy.get('.type2-section .section-icon').should('contain', 'work');
      cy.get('.type3-section .section-icon').should('contain', 'store');
    });
  });

  describe('Type 1: Management Decision Cancellation', () => {
    it('should fill and save Type 1 fields', () => {
      // Fill Type 1 form
      cy.get('input[formControlName="decisionNumber"]')
        .type('DEC-2025-001');

      cy.get('input[formControlName="decisionDate"]')
        .click()
        .get('.mat-calendar-body-cell')
        .first()
        .click();

      cy.get('mat-select[formControlName="notificationMethodId"]')
        .click()
        .get('mat-option')
        .first()
        .click();

      cy.get('input[formControlName="notificationDate"]')
        .click()
        .get('.mat-calendar-body-cell')
        .eq(5)
        .click();

      cy.get('mat-select[formControlName="issuingAuthorityId"]')
        .click()
        .get('mat-option')
        .first()
        .click();

      // Wait for auto-save (2+ seconds)
      cy.wait(2200);
      cy.wait('@saveAdditionalInfo');

      // Verify save was called with correct data
      cy.get('@saveAdditionalInfo').then((interception) => {
        expect(interception.request.body.decisionNumber).to.equal('DEC-2025-001');
        expect(interception.request.body.notificationMethodId).to.be.greaterThan(0);
        expect(interception.request.body.issuingAuthorityId).to.be.greaterThan(0);
      });
    });

    it('should validate decision number max length', () => {
      cy.get('input[formControlName="decisionNumber"]')
        .type('A'.repeat(60));

      // Should truncate at 50 characters
      cy.get('input[formControlName="decisionNumber"]')
        .should('have.value', 'A'.repeat(50));
    });

    it('should require date selection for datepicker', () => {
      cy.get('input[formControlName="decisionNumber"]').type('DEC-001');

      // Type 1 is optional, so just verify datepicker works
      cy.get('input[formControlName="decisionDate"]').should('be.empty');
    });
  });

  describe('Type 2: Service/Retirement Rights', () => {
    it('should fill and save Type 2 fields with complaint', () => {
      // Select "Yes" for has complaint
      cy.get('mat-select[formControlName="hasComplaint"]')
        .click()
        .get('mat-option')
        .contains('نعم')
        .click();

      cy.get('input[formControlName="complaintNumber"]')
        .type('COMP-2025-001');

      cy.get('input[formControlName="complaintDate"]')
        .click()
        .get('.mat-calendar-body-cell')
        .first()
        .click();

      cy.get('mat-select[formControlName="complaintAuthorityId"]')
        .click()
        .get('mat-option')
        .first()
        .click();

      cy.get('input[formControlName="complaintDecisionDate"]')
        .click()
        .get('.mat-calendar-body-cell')
        .eq(3)
        .click();

      cy.get('textarea[formControlName="systemResult"]')
        .type('Complaint has been reviewed and approved');

      // Wait for auto-save
      cy.wait(2200);
      cy.wait('@saveAdditionalInfo');

      cy.get('@saveAdditionalInfo').then((interception) => {
        expect(interception.request.body.hasComplaint).to.equal(true);
        expect(interception.request.body.complaintNumber).to.equal('COMP-2025-001');
      });
    });

    it('should handle Type 2 without complaint', () => {
      cy.get('mat-select[formControlName="hasComplaint"]')
        .click()
        .get('mat-option')
        .contains('لا')
        .click();

      cy.get('input[formControlName="complaintNumber"]').should('be.empty');

      cy.wait(2200);
      cy.wait('@saveAdditionalInfo');

      cy.get('@saveAdditionalInfo').then((interception) => {
        expect(interception.request.body.hasComplaint).to.equal(false);
      });
    });

    it('should validate system result max length', () => {
      cy.get('textarea[formControlName="systemResult"]')
        .type('A'.repeat(600));

      // Should truncate at 500 characters
      cy.get('textarea[formControlName="systemResult"]')
        .should('have.value', 'A'.repeat(500));
    });

    it('should show character count for system result', () => {
      cy.get('textarea[formControlName="systemResult"]')
        .type('Test result');

      cy.get('.mat-form-field-hint')
        .should('contain', '11/500');
    });
  });

  describe('Type 3: Trademark Dispute', () => {
    it('should fill and save Type 3 fields', () => {
      cy.get('input[formControlName="requestNumber"]')
        .type('TM-2025-001');

      cy.get('input[formControlName="requestDate"]')
        .click()
        .get('.mat-calendar-body-cell')
        .first()
        .click();

      // Wait for auto-save
      cy.wait(2200);
      cy.wait('@saveAdditionalInfo');

      cy.get('@saveAdditionalInfo').then((interception) => {
        expect(interception.request.body.requestNumber).to.equal('TM-2025-001');
      });
    });

    it('should validate request number max length', () => {
      cy.get('input[formControlName="requestNumber"]')
        .type('A'.repeat(60));

      cy.get('input[formControlName="requestNumber"]')
        .should('have.value', 'A'.repeat(50));
    });
  });

  describe('Multiple Types Simultaneously', () => {
    it('should allow filling multiple types at once', () => {
      // Fill Type 1
      cy.get('input[formControlName="decisionNumber"]')
        .type('DEC-001');

      // Fill Type 2
      cy.get('mat-select[formControlName="hasComplaint"]')
        .click()
        .get('mat-option')
        .contains('نعم')
        .click();

      cy.get('input[formControlName="complaintNumber"]')
        .type('COMP-001');

      // Fill Type 3
      cy.get('input[formControlName="requestNumber"]')
        .type('TM-001');

      // Wait for auto-save
      cy.wait(2200);
      cy.wait('@saveAdditionalInfo');

      cy.get('@saveAdditionalInfo').then((interception) => {
        const body = interception.request.body;
        expect(body.decisionNumber).to.equal('DEC-001');
        expect(body.complaintNumber).to.equal('COMP-001');
        expect(body.requestNumber).to.equal('TM-001');
      });
    });

    it('should update form when switching between types', () => {
      // Fill Type 1
      cy.get('input[formControlName="decisionNumber"]').type('DEC-001');

      // Verify Type 2 is empty
      cy.get('input[formControlName="complaintNumber"]').should('be.empty');

      // Fill Type 2
      cy.get('input[formControlName="complaintNumber"]').type('COMP-001');

      // Verify both are filled
      cy.get('input[formControlName="decisionNumber"]').should('have.value', 'DEC-001');
      cy.get('input[formControlName="complaintNumber"]').should('have.value', 'COMP-001');
    });
  });

  describe('Auto-save Functionality', () => {
    it('should auto-save after 2 seconds of inactivity', () => {
      cy.get('input[formControlName="decisionNumber"]')
        .type('DEC-AUTO-SAVE', { delay: 100 });

      // Should not save immediately
      cy.get('@saveAdditionalInfo.all').should('have.length', 0);

      // Wait for debounce
      cy.wait(2200);
      cy.wait('@saveAdditionalInfo');

      // Should have saved once
      cy.get('@saveAdditionalInfo.all').should('have.length', 1);
    });

    it('should not auto-save empty form', () => {
      // Don't fill anything
      cy.wait(2200);

      // Should not have auto-saved
      cy.get('@saveAdditionalInfo.all').should('have.length', 0);
    });

    it('should show auto-save indicator', () => {
      cy.get('input[formControlName="decisionNumber"]')
        .type('DEC-INDICATOR');

      // Immediately check for saving indicator
      cy.get('.save-indicator', { timeout: 2500 })
        .should('contain', 'جاري الحفظ التلقائي');
    });

    it('should debounce multiple rapid changes', () => {
      cy.get('input[formControlName="decisionNumber"]')
        .type('D')
        .wait(500)
        .type('E')
        .wait(500)
        .type('C')
        .wait(500)
        .type('-001');

      // Wait for debounce after last keystroke
      cy.wait(2200);
      cy.wait('@saveAdditionalInfo');

      // Should only have saved once despite multiple keystrokes
      cy.get('@saveAdditionalInfo.all').should('have.length', 1);
    });
  });

  describe('Loading and Error States', () => {
    it('should handle lookup loading errors', () => {
      // Intercept with error
      cy.intercept('GET', '**/api/lookups/notification-methods', {
        statusCode: 500,
        body: { message: 'Server error' }
      }).as('methodsError');

      cy.reload();

      // Should show error message
      cy.get('snack-bar-container').should('contain', 'خطأ في تحميل طرق الإبلاغ');
    });

    it('should handle save errors', () => {
      cy.intercept('PUT', `**/api/case-requests/${requestId}/additional-info`, {
        statusCode: 500,
        body: { message: 'Save failed' }
      }).as('saveFailed');

      cy.get('input[formControlName="decisionNumber"]')
        .type('DEC-ERROR');

      cy.wait(2200);
      cy.wait('@saveFailed');

      // Should show error message
      cy.get('snack-bar-container').should('contain', 'خطأ في حفظ المعلومات الإضافية');
    });
  });

  describe('Form Validation and Constraints', () => {
    it('should enforce field length constraints', () => {
      const longText = 'A'.repeat(100);

      cy.get('input[formControlName="decisionNumber"]').type(longText);
      cy.get('input[formControlName="decisionNumber"]')
        .should('have.value', longText.substring(0, 50));

      cy.get('input[formControlName="complaintNumber"]').type(longText);
      cy.get('input[formControlName="complaintNumber"]')
        .should('have.value', longText.substring(0, 50));

      cy.get('input[formControlName="requestNumber"]').type(longText);
      cy.get('input[formControlName="requestNumber"]')
        .should('have.value', longText.substring(0, 50));
    });

    it('should allow empty optional fields', () => {
      // Don't fill any fields
      cy.get('input[formControlName="decisionNumber"]').should('be.empty');
      cy.get('input[formControlName="complaintNumber"]').should('be.empty');
      cy.get('input[formControlName="requestNumber"]').should('be.empty');

      // Form should still be valid
      cy.get('input[formControlName="decisionNumber"]').should('not.have.attr', 'aria-invalid');
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA labels', () => {
      cy.get('mat-form-field').first()
        .should('have.attr', 'appearance', 'outline');
    });

    it('should be keyboard navigable', () => {
      cy.get('input[formControlName="decisionNumber"]')
        .focus()
        .should('be.focused')
        .type('DEC-001');

      cy.get('body').tab();
      cy.focused().should('be.visible');
    });

    it('should have Arabic right-to-left support', () => {
      cy.get('.section-header').should('contain', 'دعاوى');
      cy.get('.section-header').should('have.css', 'text-align', 'right');
    });
  });

  describe('Responsive Design', () => {
    it('should display 2-column grid on desktop', () => {
      cy.viewport(1920, 1080);
      cy.get('.grid-2col').should('have.css', 'grid-template-columns', '1fr 1fr');
    });

    it('should display single column on mobile', () => {
      cy.viewport(375, 812); // iPhone size
      // Grid should adapt to single column due to media query
      cy.get('.grid-2col').should('be.visible');
    });
  });
});
