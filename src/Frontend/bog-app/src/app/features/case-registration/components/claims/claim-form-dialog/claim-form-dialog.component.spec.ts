import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ClaimFormDialogComponent } from './claim-form-dialog.component';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { ClaimVM } from '../../../models/claim.model';

describe('ClaimFormDialogComponent', () => {
  let component: ClaimFormDialogComponent;
  let fixture: ComponentFixture<ClaimFormDialogComponent>;
  let mockCaseDataState: jasmine.SpyObj<CaseDataStateService>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<ClaimFormDialogComponent>>;
  let mockSnackBar: jasmine.SpyObj<MatSnackBar>;

  const mockClaim: ClaimVM = {
    id: 1,
    caseRegistrationRequestId: 1,
    claimText: 'Original claim text',
    createdDate: new Date(),
    modifiedDate: new Date()
  };

  beforeEach(async () => {
    mockCaseDataState = jasmine.createSpyObj('CaseDataStateService', [
      'getClaims',
      'updateClaims'
    ]);
    mockCaseDataState.getClaims.and.returnValue([mockClaim]);

    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);
    mockSnackBar = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      declarations: [ClaimFormDialogComponent],
      imports: [
        ReactiveFormsModule,
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatProgressBarModule,
        BrowserAnimationsModule
      ],
      providers: [
        { provide: CaseDataStateService, useValue: mockCaseDataState },
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MatSnackBar, useValue: mockSnackBar },
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            requestId: 1,
            mode: 'create'
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ClaimFormDialogComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization - Create Mode', () => {
    beforeEach(() => {
      component.mode = 'create';
      component.requestId = 1;
      fixture.detectChanges();
    });

    it('should create the component in create mode', () => {
      expect(component).toBeTruthy();
      expect(component.mode).toBe('create');
    });

    it('should initialize empty form in create mode', () => {
      expect(component.claimForm.get('claimText')?.value).toBe('');
    });

    it('should have enabled form in create mode', () => {
      expect(component.claimForm.enabled).toBe(true);
    });
  });

  describe('Component Initialization - Edit Mode', () => {
    beforeEach(() => {
      component.mode = 'edit';
      component.requestId = 1;
      component.claim = mockClaim;
      fixture.detectChanges();
    });

    it('should initialize form with claim text in edit mode', () => {
      expect(component.claimForm.get('claimText')?.value).toBe('Original claim text');
    });

    it('should have enabled form in edit mode', () => {
      expect(component.claimForm.enabled).toBe(true);
    });
  });

  describe('Component Initialization - View Mode', () => {
    beforeEach(() => {
      component.mode = 'view';
      component.requestId = 1;
      component.claim = mockClaim;
      fixture.detectChanges();
    });

    it('should disable form in view mode', () => {
      expect(component.claimForm.disabled).toBe(true);
    });

    it('should display claim text in view mode', () => {
      expect(component.claimForm.get('claimText')?.value).toBe('Original claim text');
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should require claim text', () => {
      const control = component.claimForm.get('claimText');
      control?.setValue('');
      expect(control?.hasError('required')).toBe(true);
    });

    it('should enforce maximum 2000 characters', () => {
      const control = component.claimForm.get('claimText');
      const longText = 'a'.repeat(2001);
      control?.setValue(longText);
      expect(control?.hasError('maxlength')).toBe(true);
    });

    it('should accept text with exactly 2000 characters', () => {
      const control = component.claimForm.get('claimText');
      const text = 'a'.repeat(2000);
      control?.setValue(text);
      expect(control?.hasError('maxlength')).toBe(false);
    });

    it('should accept valid claim text', () => {
      const control = component.claimForm.get('claimText');
      control?.setValue('Valid claim text');
      expect(control?.invalid).toBe(false);
    });
  });

  describe('Character Counter', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should calculate character count correctly', () => {
      component.claimForm.get('claimText')?.setValue('Hello World');
      expect(component.claimTextLength).toBe(11);
    });

    it('should return 0 for empty text', () => {
      component.claimForm.get('claimText')?.setValue('');
      expect(component.claimTextLength).toBe(0);
    });

    it('should calculate percentage correctly', () => {
      component.claimForm.get('claimText')?.setValue('a'.repeat(1000));
      expect(component.characterPercentage).toBe(50); // 1000/2000 * 100 = 50%
    });

    it('should return 100 for maximum length', () => {
      component.claimForm.get('claimText')?.setValue('a'.repeat(2000));
      expect(component.characterPercentage).toBe(100);
    });

    it('should update percentage on text change', () => {
      const control = component.claimForm.get('claimText');

      control?.setValue('a'.repeat(100));
      expect(component.characterPercentage).toBe(5);

      control?.setValue('a'.repeat(500));
      expect(component.characterPercentage).toBe(25);

      control?.setValue('a'.repeat(1500));
      expect(component.characterPercentage).toBe(75);
    });
  });

  describe('Save - Create Mode', () => {
    beforeEach(() => {
      component.mode = 'create';
      component.requestId = 1;
      fixture.detectChanges();
    });

    it('should not save invalid form', () => {
      component.claimForm.get('claimText')?.setValue('');
      component.onSave();
      expect(mockCaseDataState.updateClaims).not.toHaveBeenCalled();
    });

    it('should show error for invalid form', () => {
      component.claimForm.get('claimText')?.setValue('');
      component.onSave();
      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'يرجى ملء جميع الحقول المطلوبة',
        'إغلاق',
        { duration: 3000 }
      );
    });

    it('should create new claim with correct properties', () => {
      component.claimForm.get('claimText')?.setValue('New claim text');
      component.onSave();

      expect(mockCaseDataState.updateClaims).toHaveBeenCalled();
      const updatedClaims = mockCaseDataState.updateClaims.calls.mostRecent().args[0];

      // Should have original claim plus new one
      expect(updatedClaims.length).toBeGreaterThan(1);
      const newClaim = updatedClaims[updatedClaims.length - 1];
      expect(newClaim.claimText).toBe('New claim text');
      expect(newClaim.caseRegistrationRequestId).toBe(1);
    });

    it('should close dialog on successful save', () => {
      component.claimForm.get('claimText')?.setValue('New claim text');
      component.onSave();

      expect(mockDialogRef.close).toHaveBeenCalledWith(true);
    });

    it('should show success message on save', () => {
      component.claimForm.get('claimText')?.setValue('New claim text');
      component.onSave();

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'تم إضافة طلب الدعوى بنجاح',
        'إغلاق',
        { duration: 3000 }
      );
    });
  });

  describe('Save - Edit Mode', () => {
    beforeEach(() => {
      component.mode = 'edit';
      component.requestId = 1;
      component.claim = mockClaim;
      fixture.detectChanges();
    });

    it('should update existing claim', () => {
      component.claimForm.get('claimText')?.setValue('Updated claim text');
      component.onSave();

      expect(mockCaseDataState.updateClaims).toHaveBeenCalled();
      const updatedClaims = mockCaseDataState.updateClaims.calls.mostRecent().args[0];
      const editedClaim = updatedClaims.find(c => c.id === mockClaim.id);

      expect(editedClaim?.claimText).toBe('Updated claim text');
    });

    it('should preserve claim ID on update', () => {
      component.claimForm.get('claimText')?.setValue('Updated claim text');
      component.onSave();

      const updatedClaims = mockCaseDataState.updateClaims.calls.mostRecent().args[0];
      const editedClaim = updatedClaims.find(c => c.id === mockClaim.id);

      expect(editedClaim?.id).toBe(mockClaim.id);
    });

    it('should close dialog on successful update', () => {
      component.claimForm.get('claimText')?.setValue('Updated claim text');
      component.onSave();

      expect(mockDialogRef.close).toHaveBeenCalledWith(true);
    });

    it('should show update success message', () => {
      component.claimForm.get('claimText')?.setValue('Updated claim text');
      component.onSave();

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'تم تحديث طلب الدعوى بنجاح',
        'إغلاق',
        { duration: 3000 }
      );
    });
  });

  describe('Save - View Mode', () => {
    beforeEach(() => {
      component.mode = 'view';
      component.requestId = 1;
      component.claim = mockClaim;
      fixture.detectChanges();
    });

    it('should not save in view mode (form is disabled)', () => {
      expect(component.claimForm.disabled).toBe(true);
    });
  });

  describe('Cancel', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should close dialog without result', () => {
      component.onCancel();
      expect(mockDialogRef.close).toHaveBeenCalledWith();
    });
  });

  describe('Text Change Handler', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should update form validity on text change', () => {
      const control = component.claimForm.get('claimText');

      control?.setValue('short');
      component.onTextChange();
      expect(control?.invalid).toBe(false); // 5 characters is valid

      control?.setValue('a'.repeat(2001));
      component.onTextChange();
      expect(control?.invalid).toBe(true); // Over 2000 is invalid
    });
  });

  describe('Claim Text Maximum Length', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have maximum 2000 characters constant', () => {
      expect(component.maxCharacters).toBe(2000);
    });

    it('should validate against maximum constant', () => {
      const control = component.claimForm.get('claimText');
      control?.setValue('a'.repeat(component.maxCharacters + 1));
      expect(control?.hasError('maxlength')).toBe(true);
    });
  });

  describe('New Claim ID Generation', () => {
    beforeEach(() => {
      component.mode = 'create';
      component.requestId = 1;
      fixture.detectChanges();
    });

    it('should generate ID greater than existing claims', () => {
      mockCaseDataState.getClaims.and.returnValue([
        { ...mockClaim, id: 5 },
        { ...mockClaim, id: 3 }
      ]);

      component.claimForm.get('claimText')?.setValue('New claim');
      component.onSave();

      const updatedClaims = mockCaseDataState.updateClaims.calls.mostRecent().args[0];
      const newClaim = updatedClaims[updatedClaims.length - 1];

      expect(newClaim.id).toBeGreaterThan(5);
    });

    it('should start with ID 1 when no claims exist', () => {
      mockCaseDataState.getClaims.and.returnValue([]);

      component.claimForm.get('claimText')?.setValue('New claim');
      component.onSave();

      const updatedClaims = mockCaseDataState.updateClaims.calls.mostRecent().args[0];
      const newClaim = updatedClaims[0];

      expect(newClaim.id).toBe(1);
    });
  });

  describe('Form State Persistence', () => {
    beforeEach(() => {
      component.mode = 'edit';
      component.requestId = 1;
      component.claim = mockClaim;
      fixture.detectChanges();
    });

    it('should not clear form on text change', () => {
      const initialValue = component.claimForm.get('claimText')?.value;
      component.onTextChange();
      const currentValue = component.claimForm.get('claimText')?.value;

      expect(currentValue).toBe(initialValue);
    });
  });

  describe('Error State Management', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should initialize without server error', () => {
      expect(component.serverError).toBeNull();
    });

    it('should clear server error on new save attempt', () => {
      component.serverError = 'Previous error';
      component.claimForm.get('claimText')?.setValue('New text');
      component.onSave();

      expect(component.serverError).toBeNull();
    });
  });

  describe('Saving State', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should set saving flag during save', () => {
      component.claimForm.get('claimText')?.setValue('Valid text');
      component.saving = false;
      component.onSave();

      // After save completes
      expect(component.saving).toBe(false);
    });
  });
});
