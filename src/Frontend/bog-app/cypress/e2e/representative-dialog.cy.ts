/// <reference types="cypress" />

/**
 * Helper: scroll to bottom of the dialog to see type-specific sections,
 * then verify the element exists.
 */
function scrollDialogAndCheck(selector: string, assertion: 'exist' | 'not.exist') {
  // Scroll the mat-dialog-content to the bottom to reveal type-specific sections
  cy.get('mat-dialog-content').scrollTo('bottom');
  cy.wait(300); // Wait for scroll animation
  if (assertion === 'exist') {
    cy.get('mat-dialog-container').find(selector).should('exist');
  } else {
    cy.get('mat-dialog-container').find(selector).should('not.exist');
  }
}

/**
 * Helper: scroll dialog to bottom, click save, then check for mat-error.
 * The error may be covered by notification, so we use .should('exist') instead of 'be.visible'.
 */
function clickSaveAndCheckError(errorText: string) {
  // Scroll to bottom to ensure type-specific fields are touched
  cy.get('mat-dialog-content').scrollTo('bottom');
  cy.wait(200);
  // Click save - it's in mat-dialog-actions, always visible
  cy.get('mat-dialog-actions').contains('button', 'إضافة').click({ force: true });
  cy.wait(500); // Wait for validation to trigger
  // Check the error exists in the DOM (may not be "visible" due to notification overlay)
  cy.get('mat-dialog-content').scrollTo('bottom');
  cy.wait(200);
  cy.get('mat-dialog-container').contains('mat-error', errorText).should('exist');
}

describe('Representative Dialog - Type-Specific Sections', () => {

  // =====================================================
  // مصفي (Type 10) Tests
  // =====================================================
  describe('مصفي (Liquidator - Type 10)', () => {
    beforeEach(() => {
      // شركة مسجلة (type 4) allows مصفي
      cy.setupPlaintiffContext(4);
      cy.navigateToRepresentativesStep();
    });

    it('should show "بيانات مصفي" section when مصفي is selected', () => {
      cy.openRepresentativeDialog(10);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').contains('h4', 'بيانات مصفي').should('exist');
    });

    it('should hide "بيانات مصفي" section when another type is selected', () => {
      cy.openRepresentativeDialog(1); // وكيل
      cy.get('mat-dialog-container').within(() => {
        cy.contains('h4', 'بيانات مصفي').should('not.exist');
      });
    });

    it('should display 3 fields: رقم القرار, تاريخ القرار, مصدر القرار', () => {
      cy.openRepresentativeDialog(10);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('input[formcontrolname="decisionNumber"]').should('exist');
        cy.get('input[formcontrolname="decisionDate"]').should('exist');
        cy.get('input[formcontrolname="decisionSource"]').should('exist');
      });
    });

    it('should require رقم القرار field', () => {
      cy.openRepresentativeDialog(10);
      clickSaveAndCheckError('رقم القرار مطلوب');
    });

    it('should require تاريخ القرار field', () => {
      cy.openRepresentativeDialog(10);
      clickSaveAndCheckError('تاريخ القرار مطلوب');
    });

    it('should require مصدر القرار field', () => {
      cy.openRepresentativeDialog(10);
      clickSaveAndCheckError('مصدر القرار مطلوب');
    });

    it('should enforce maxLength(20) on رقم القرار', () => {
      cy.openRepresentativeDialog(10);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('input[formcontrolname="decisionNumber"]').should('have.attr', 'maxlength', '20');
      });
    });

    it('should enforce maxLength(20) on مصدر القرار', () => {
      cy.openRepresentativeDialog(10);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('input[formcontrolname="decisionSource"]').should('have.attr', 'maxlength', '20');
      });
    });
  });

  // =====================================================
  // أمين التفليسة (Type 8) Tests
  // =====================================================
  describe('أمين التفليسة (Trustee - Type 8)', () => {
    beforeEach(() => {
      // فرد (type 1) allows أمين تفليسة
      cy.setupPlaintiffContext(1);
      cy.navigateToRepresentativesStep();
    });

    it('should show "بيانات المصفي" section when أمين التفليسة is selected', () => {
      cy.openRepresentativeDialog(8);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').contains('h4', 'بيانات المصفي').should('exist');
    });

    it('should hide "بيانات المصفي" section for other types', () => {
      cy.openRepresentativeDialog(1); // وكيل
      cy.get('mat-dialog-container').within(() => {
        cy.contains('h4', 'بيانات المصفي').should('not.exist');
      });
    });

    it('should display 3 decision fields', () => {
      cy.openRepresentativeDialog(8);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('input[formcontrolname="decisionNumber"]').should('exist');
        cy.get('input[formcontrolname="decisionDate"]').should('exist');
        cy.get('input[formcontrolname="decisionSource"]').should('exist');
      });
    });

    it('should require رقم القرار field', () => {
      cy.openRepresentativeDialog(8);
      clickSaveAndCheckError('رقم القرار مطلوب');
    });

    it('should require تاريخ القرار field', () => {
      cy.openRepresentativeDialog(8);
      clickSaveAndCheckError('تاريخ القرار مطلوب');
    });

    it('should require مصدر القرار field', () => {
      cy.openRepresentativeDialog(8);
      clickSaveAndCheckError('مصدر القرار مطلوب');
    });
  });

  // =====================================================
  // حارس قضائي (Type 11) Tests
  // =====================================================
  describe('حارس قضائي (Judicial Custodian - Type 11)', () => {
    beforeEach(() => {
      // شركة مسجلة (type 4) allows حارس قضائي
      cy.setupPlaintiffContext(4);
      cy.navigateToRepresentativesStep();
    });

    it('should show "بيانات حارس قضائي" section when حارس قضائي is selected', () => {
      cy.openRepresentativeDialog(11);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').contains('h4', 'بيانات حارس قضائي').should('exist');
    });

    it('should hide "بيانات حارس قضائي" section for other types', () => {
      cy.openRepresentativeDialog(1); // وكيل
      cy.get('mat-dialog-container').within(() => {
        cy.contains('h4', 'بيانات حارس قضائي').should('not.exist');
      });
    });

    it('should display fields with correct labels', () => {
      cy.openRepresentativeDialog(11);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.contains('mat-label', 'رقم القرار / الدعوى').should('exist');
        cy.contains('mat-label', 'تاريخ القرار / الدعوى').should('exist');
        cy.contains('mat-label', 'مصدر القرار / الحكم').should('exist');
      });
    });

    it('should require رقم القرار / الدعوى field', () => {
      cy.openRepresentativeDialog(11);
      clickSaveAndCheckError('رقم القرار / الدعوى مطلوب');
    });

    it('should require تاريخ القرار / الدعوى field', () => {
      cy.openRepresentativeDialog(11);
      clickSaveAndCheckError('تاريخ القرار / الدعوى مطلوب');
    });

    it('should require مصدر القرار / الحكم field', () => {
      cy.openRepresentativeDialog(11);
      clickSaveAndCheckError('مصدر القرار / الحكم مطلوب');
    });

    it('should enforce maxLength(20) on رقم القرار / الدعوى', () => {
      cy.openRepresentativeDialog(11);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('input[formcontrolname="decisionNumber"]').should('have.attr', 'maxlength', '20');
      });
    });

    it('should enforce maxLength(20) on مصدر القرار / الحكم', () => {
      cy.openRepresentativeDialog(11);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('input[formcontrolname="decisionSource"]').should('have.attr', 'maxlength', '20');
      });
    });
  });

  // =====================================================
  // ممثل نظامي (Type 9) Tests
  // =====================================================
  describe('ممثل نظامي (Legal Representative - Type 9)', () => {
    beforeEach(() => {
      // شركة مسجلة (type 4) allows ممثل نظامي
      cy.setupPlaintiffContext(4);
      cy.navigateToRepresentativesStep();
    });

    it('should show "بيانات ممثل نظامي" section when ممثل نظامي is selected', () => {
      cy.openRepresentativeDialog(9);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').contains('h4', 'بيانات ممثل نظامي').should('exist');
    });

    it('should hide "بيانات ممثل نظامي" section for other types', () => {
      cy.openRepresentativeDialog(1); // وكيل
      cy.get('mat-dialog-container').within(() => {
        cy.contains('h4', 'بيانات ممثل نظامي').should('not.exist');
      });
    });

    it('should display 4 fields with correct labels', () => {
      cy.openRepresentativeDialog(9);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.contains('mat-label', 'مصدر مستند التمثيل').should('exist');
        cy.contains('mat-label', 'صفة الممثل').should('exist');
        cy.contains('mat-label', 'نوع مستند التمثيل').should('exist');
        cy.contains('mat-label', 'رقم مستند التمثيل').should('exist');
      });
    });

    it('should require مصدر مستند التمثيل field', () => {
      cy.openRepresentativeDialog(9);
      clickSaveAndCheckError('مصدر مستند التمثيل مطلوب');
    });

    it('should require صفة الممثل field', () => {
      cy.openRepresentativeDialog(9);
      clickSaveAndCheckError('صفة الممثل مطلوبة');
    });

    it('should require نوع مستند التمثيل field', () => {
      cy.openRepresentativeDialog(9);
      clickSaveAndCheckError('نوع مستند التمثيل مطلوب');
    });

    it('should require رقم مستند التمثيل field', () => {
      cy.openRepresentativeDialog(9);
      clickSaveAndCheckError('رقم مستند التمثيل مطلوب');
    });

    it('should provide صفة الممثل dropdown with correct options', () => {
      cy.openRepresentativeDialog(9);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('mat-select[formcontrolname="representativeCapacity"]').click();
      });
      cy.get('mat-option').contains('رئيس مجلس إدارة').should('exist');
      cy.get('mat-option').contains('مدير').should('exist');
      // Close dropdown by pressing Escape
      cy.get('body').type('{esc}');
    });

    it('should provide نوع مستند التمثيل dropdown with correct options', () => {
      cy.openRepresentativeDialog(9);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('mat-select[formcontrolname="representationDocType"]').click();
      });
      cy.get('mat-option').contains('عقد تأسيس').should('exist');
      cy.get('mat-option').contains('قرار الشركاء').should('exist');
      cy.get('mat-option').contains('غير ذلك').should('exist');
      cy.get('body').type('{esc}');
    });

    it('should enforce maxLength(20) on رقم مستند التمثيل', () => {
      cy.openRepresentativeDialog(9);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('input[formcontrolname="representationDocNumber"]').should('have.attr', 'maxlength', '20');
      });
    });
  });

  // =====================================================
  // Cross-Type Visibility Tests
  // =====================================================
  describe('Cross-Type Visibility', () => {
    beforeEach(() => {
      // شركة مسجلة (type 4) allows multiple representative types
      cy.setupPlaintiffContext(4);
      cy.navigateToRepresentativesStep();
    });

    it('should show only مصفي section when مصفي is selected (not others)', () => {
      cy.openRepresentativeDialog(10);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').contains('h4', 'بيانات مصفي').should('exist');
      cy.get('mat-dialog-container').contains('h4', 'بيانات حارس قضائي').should('not.exist');
      cy.get('mat-dialog-container').contains('h4', 'بيانات ممثل نظامي').should('not.exist');
    });

    it('should show only حارس قضائي section when حارس قضائي is selected (not others)', () => {
      cy.openRepresentativeDialog(11);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').contains('h4', 'بيانات حارس قضائي').should('exist');
      cy.get('mat-dialog-container').contains('h4', 'بيانات مصفي').should('not.exist');
      cy.get('mat-dialog-container').contains('h4', 'بيانات ممثل نظامي').should('not.exist');
    });

    it('should show only ممثل نظامي section when ممثل نظامي is selected (not others)', () => {
      cy.openRepresentativeDialog(9);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').contains('h4', 'بيانات ممثل نظامي').should('exist');
      cy.get('mat-dialog-container').contains('h4', 'بيانات مصفي').should('not.exist');
      cy.get('mat-dialog-container').contains('h4', 'بيانات حارس قضائي').should('not.exist');
    });

    it('should not show any type-specific section when وكيل (type 1) is selected', () => {
      cy.openRepresentativeDialog(1);
      cy.get('mat-dialog-container').within(() => {
        cy.contains('h4', 'بيانات مصفي').should('not.exist');
        cy.contains('h4', 'بيانات حارس قضائي').should('not.exist');
        cy.contains('h4', 'بيانات ممثل نظامي').should('not.exist');
      });
    });
  });

  // =====================================================
  // Common Fields Tests
  // =====================================================
  describe('Common Fields', () => {
    beforeEach(() => {
      cy.setupPlaintiffContext(4);
      cy.navigateToRepresentativesStep();
    });

    it('should always show identity fields regardless of representative type', () => {
      cy.openRepresentativeDialog(10);
      cy.get('mat-dialog-container').within(() => {
        cy.get('mat-select[formcontrolname="identityTypeId"]').should('exist');
        cy.get('input[formcontrolname="identityNumber"]').should('exist');
      });
    });

    it('should always show name fields regardless of representative type', () => {
      cy.openRepresentativeDialog(10);
      cy.get('mat-dialog-container').within(() => {
        cy.get('input[formcontrolname="firstName"]').should('exist');
        cy.get('input[formcontrolname="fatherName"]').should('exist');
        cy.get('input[formcontrolname="grandfatherName"]').should('exist');
        cy.get('input[formcontrolname="familyName"]').should('exist');
      });
    });

    it('should always show contact info fields regardless of representative type', () => {
      cy.openRepresentativeDialog(10);
      cy.get('mat-dialog-content').scrollTo('bottom');
      cy.get('mat-dialog-container').within(() => {
        cy.get('input[formcontrolname="mobileNumber"]').should('exist');
        cy.get('input[formcontrolname="email"]').should('exist');
      });
    });

    it('should show the save button as "إضافة" in add mode', () => {
      cy.openRepresentativeDialog(10);
      cy.get('mat-dialog-actions').contains('button', 'إضافة').should('exist');
    });
  });
});
