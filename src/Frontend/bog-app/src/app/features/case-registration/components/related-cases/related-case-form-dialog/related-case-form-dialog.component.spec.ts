import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RelatedCaseFormDialogComponent } from './related-case-form-dialog.component';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { LookupsApiService } from '../../../services/lookups-api.service';
import { RelatedCaseVM, CourtLookup } from '../../../models/related-case.model';
import { of, throwError } from 'rxjs';

describe('RelatedCaseFormDialogComponent', () => {
  let component: RelatedCaseFormDialogComponent;
  let fixture: ComponentFixture<RelatedCaseFormDialogComponent>;
  let mockCaseDataState: jasmine.SpyObj<CaseDataStateService>;
  let mockLookupsApi: jasmine.SpyObj<LookupsApiService>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<RelatedCaseFormDialogComponent>>;
  let mockSnackBar: jasmine.SpyObj<MatSnackBar>;

  const mockCourts: CourtLookup[] = [
    { id: 1, name: 'Riyadh Court', nameAr: 'محكمة الرياض', regionId: 1, cityId: 1 },
    { id: 2, name: 'Jeddah Court', nameAr: 'محكمة جدة', regionId: 2, cityId: 2 }
  ];

  const mockRelatedCase: RelatedCaseVM = {
    id: 1,
    caseRegistrationRequestId: 1,
    courtId: 1,
    courtName: 'محكمة الرياض',
    caseNumber: 12345,
    caseYear: 1445,
    createdDate: new Date(),
    modifiedDate: new Date()
  };

  beforeEach(async () => {
    mockCaseDataState = jasmine.createSpyObj('CaseDataStateService', [
      'getRelatedCases',
      'updateRelatedCases'
    ]);
    mockCaseDataState.getRelatedCases.and.returnValue([mockRelatedCase]);

    mockLookupsApi = jasmine.createSpyObj('LookupsApiService', ['getCourts']);
    mockLookupsApi.getCourts.and.returnValue(of(mockCourts));

    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);
    mockSnackBar = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      declarations: [RelatedCaseFormDialogComponent],
      imports: [
        ReactiveFormsModule,
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatOptionModule,
        MatButtonModule,
        BrowserAnimationsModule
      ],
      providers: [
        { provide: CaseDataStateService, useValue: mockCaseDataState },
        { provide: LookupsApiService, useValue: mockLookupsApi },
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

    fixture = TestBed.createComponent(RelatedCaseFormDialogComponent);
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
      expect(component.relatedCaseForm.get('courtId')?.value).toBeNull();
      expect(component.relatedCaseForm.get('caseNumber')?.value).toBe('');
      expect(component.relatedCaseForm.get('caseYear')?.value).toBe('');
    });

    it('should load courts on init', () => {
      expect(mockLookupsApi.getCourts).toHaveBeenCalled();
      expect(component.courts).toEqual(mockCourts);
    });
  });

  describe('Component Initialization - Edit Mode', () => {
    beforeEach(() => {
      component.mode = 'edit';
      component.requestId = 1;
      component.relatedCase = mockRelatedCase;
      fixture.detectChanges();
    });

    it('should initialize form with related case data in edit mode', () => {
      expect(component.relatedCaseForm.get('courtId')?.value).toBe(1);
      expect(component.relatedCaseForm.get('caseNumber')?.value).toBe(12345);
      expect(component.relatedCaseForm.get('caseYear')?.value).toBe(1445);
    });

    it('should have enabled form in edit mode', () => {
      expect(component.relatedCaseForm.enabled).toBe(true);
    });
  });

  describe('Component Initialization - View Mode', () => {
    beforeEach(() => {
      component.mode = 'view';
      component.requestId = 1;
      component.relatedCase = mockRelatedCase;
      fixture.detectChanges();
    });

    it('should disable form in view mode', () => {
      expect(component.relatedCaseForm.disabled).toBe(true);
    });

    it('should display related case data in view mode', () => {
      expect(component.relatedCaseForm.get('courtId')?.value).toBe(1);
      expect(component.relatedCaseForm.get('caseNumber')?.value).toBe(12345);
      expect(component.relatedCaseForm.get('caseYear')?.value).toBe(1445);
    });
  });

  describe('Form Validation - Case Number', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should require case number', () => {
      const control = component.relatedCaseForm.get('caseNumber');
      control?.setValue('');
      expect(control?.hasError('required')).toBe(true);
    });

    it('should validate case number as numeric only', () => {
      const control = component.relatedCaseForm.get('caseNumber');
      control?.setValue('abc123');
      expect(control?.hasError('pattern')).toBe(true);
    });

    it('should accept valid numeric case number', () => {
      const control = component.relatedCaseForm.get('caseNumber');
      control?.setValue('123456');
      expect(control?.invalid).toBe(false);
    });

    it('should accept single digit case number', () => {
      const control = component.relatedCaseForm.get('caseNumber');
      control?.setValue('1');
      expect(control?.invalid).toBe(false);
    });
  });

  describe('Form Validation - Case Year', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should require case year', () => {
      const control = component.relatedCaseForm.get('caseYear');
      control?.setValue('');
      expect(control?.hasError('required')).toBe(true);
    });

    it('should validate case year as 4-digit number', () => {
      const control = component.relatedCaseForm.get('caseYear');
      control?.setValue('145'); // 3 digits
      expect(control?.hasError('pattern')).toBe(true);
    });

    it('should accept valid 4-digit year', () => {
      const control = component.relatedCaseForm.get('caseYear');
      control?.setValue('1445');
      expect(control?.invalid).toBe(false);
    });

    it('should reject 5-digit year', () => {
      const control = component.relatedCaseForm.get('caseYear');
      control?.setValue('14450');
      expect(control?.hasError('pattern')).toBe(true);
    });

    it('should accept Hijri years (1300-1500 range)', () => {
      const control = component.relatedCaseForm.get('caseYear');
      control?.setValue('1350');
      expect(control?.invalid).toBe(false);
    });
  });

  describe('Court Loading', () => {
    it('should load courts successfully', () => {
      fixture.detectChanges();
      expect(component.courts).toEqual(mockCourts);
      expect(component.loadingCourts).toBe(false);
    });

    it('should handle court loading error', () => {
      mockLookupsApi.getCourts.and.returnValue(throwError(() => new Error('Load failed')));
      component.loadCourts();

      expect(mockSnackBar.open).toHaveBeenCalledWith('خطأ في تحميل المحاكم', 'إغلاق', { duration: 3000 });
    });

    it('should set loading flag while fetching courts', () => {
      component.loadingCourts = true;
      component.loadCourts();
      expect(component.loadingCourts).toBe(false); // Set to false when done
    });
  });

  describe('Get Court Name', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should return court name in Arabic', () => {
      const name = component.getCourtName(1);
      expect(name).toBe('محكمة الرياض');
    });

    it('should return court name for second court', () => {
      const name = component.getCourtName(2);
      expect(name).toBe('محكمة جدة');
    });

    it('should return empty string for unknown court', () => {
      const name = component.getCourtName(999);
      expect(name).toBe('');
    });
  });

  describe('Save - Create Mode', () => {
    beforeEach(() => {
      component.mode = 'create';
      component.requestId = 1;
      fixture.detectChanges();
    });

    it('should not save invalid form', () => {
      component.relatedCaseForm.get('caseNumber')?.setValue('');
      component.onSave();
      expect(mockCaseDataState.updateRelatedCases).not.toHaveBeenCalled();
    });

    it('should show error for invalid form', () => {
      component.relatedCaseForm.get('caseNumber')?.setValue('');
      component.onSave();
      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'يرجى ملء جميع الحقول المطلوبة',
        'إغلاق',
        { duration: 3000 }
      );
    });

    it('should create new related case with correct properties', () => {
      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: 54321,
        caseYear: 1445
      });

      component.onSave();

      expect(mockCaseDataState.updateRelatedCases).toHaveBeenCalled();
      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];

      expect(updatedCases.length).toBeGreaterThan(1);
      const newCase = updatedCases[updatedCases.length - 1];
      expect(newCase.caseNumber).toBe(54321);
      expect(newCase.caseYear).toBe(1445);
      expect(newCase.courtId).toBe(1);
      expect(newCase.caseRegistrationRequestId).toBe(1);
    });

    it('should close dialog on successful save', () => {
      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: 54321,
        caseYear: 1445
      });

      component.onSave();

      expect(mockDialogRef.close).toHaveBeenCalledWith(true);
    });

    it('should show success message on save', () => {
      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: 54321,
        caseYear: 1445
      });

      component.onSave();

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'تم إضافة الدعوى المرتبطة بنجاح',
        'إغلاق',
        { duration: 3000 }
      );
    });

    it('should include court name in new case', () => {
      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: 54321,
        caseYear: 1445
      });

      component.onSave();

      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
      const newCase = updatedCases[updatedCases.length - 1];
      expect(newCase.courtName).toBe('محكمة الرياض');
    });
  });

  describe('Save - Edit Mode', () => {
    beforeEach(() => {
      component.mode = 'edit';
      component.requestId = 1;
      component.relatedCase = mockRelatedCase;
      fixture.detectChanges();
    });

    it('should update existing related case', () => {
      component.relatedCaseForm.patchValue({
        courtId: 2,
        caseNumber: 99999,
        caseYear: 1446
      });

      component.onSave();

      expect(mockCaseDataState.updateRelatedCases).toHaveBeenCalled();
      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
      const editedCase = updatedCases.find(c => c.id === mockRelatedCase.id);

      expect(editedCase?.caseNumber).toBe(99999);
      expect(editedCase?.caseYear).toBe(1446);
      expect(editedCase?.courtId).toBe(2);
    });

    it('should preserve case ID on update', () => {
      component.relatedCaseForm.patchValue({
        courtId: 2,
        caseNumber: 99999,
        caseYear: 1446
      });

      component.onSave();

      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
      const editedCase = updatedCases.find(c => c.id === mockRelatedCase.id);

      expect(editedCase?.id).toBe(mockRelatedCase.id);
    });

    it('should close dialog on successful update', () => {
      component.relatedCaseForm.patchValue({
        courtId: 2,
        caseNumber: 99999,
        caseYear: 1446
      });

      component.onSave();

      expect(mockDialogRef.close).toHaveBeenCalledWith(true);
    });

    it('should show update success message', () => {
      component.relatedCaseForm.patchValue({
        courtId: 2,
        caseNumber: 99999,
        caseYear: 1446
      });

      component.onSave();

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'تم تحديث الدعوى المرتبطة بنجاح',
        'إغلاق',
        { duration: 3000 }
      );
    });

    it('should update court name on edit', () => {
      component.relatedCaseForm.patchValue({
        courtId: 2,
        caseNumber: 99999,
        caseYear: 1446
      });

      component.onSave();

      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
      const editedCase = updatedCases.find(c => c.id === mockRelatedCase.id);

      expect(editedCase?.courtName).toBe('محكمة جدة');
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

  describe('Court Dropdown', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have optional court field', () => {
      const control = component.relatedCaseForm.get('courtId');
      control?.setValue(null);
      expect(control?.invalid).toBe(false);
    });

    it('should populate court dropdown with courts', () => {
      expect(component.courts.length).toBe(2);
    });

    it('should display court names in dropdown', () => {
      const courtNames = component.courts.map(c => c.nameAr);
      expect(courtNames).toContain('محكمة الرياض');
      expect(courtNames).toContain('محكمة جدة');
    });
  });

  describe('Form Numeric Conversion', () => {
    beforeEach(() => {
      component.mode = 'create';
      fixture.detectChanges();
    });

    it('should convert case number to integer', () => {
      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: '12345',
        caseYear: '1445'
      });

      component.onSave();

      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
      const newCase = updatedCases[updatedCases.length - 1];

      expect(typeof newCase.caseNumber).toBe('number');
      expect(newCase.caseNumber).toBe(12345);
    });

    it('should convert case year to integer', () => {
      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: 12345,
        caseYear: '1445'
      });

      component.onSave();

      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
      const newCase = updatedCases[updatedCases.length - 1];

      expect(typeof newCase.caseYear).toBe('number');
      expect(newCase.caseYear).toBe(1445);
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
      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: 12345,
        caseYear: 1445
      });
      component.onSave();

      expect(component.serverError).toBeNull();
    });
  });

  describe('New Related Case ID Generation', () => {
    beforeEach(() => {
      component.mode = 'create';
      fixture.detectChanges();
    });

    it('should generate ID greater than existing cases', () => {
      mockCaseDataState.getRelatedCases.and.returnValue([
        { ...mockRelatedCase, id: 5 },
        { ...mockRelatedCase, id: 3 }
      ]);

      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: 54321,
        caseYear: 1445
      });

      component.onSave();

      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
      const newCase = updatedCases[updatedCases.length - 1];

      expect(newCase.id).toBeGreaterThan(5);
    });

    it('should start with ID 1 when no cases exist', () => {
      mockCaseDataState.getRelatedCases.and.returnValue([]);

      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: 54321,
        caseYear: 1445
      });

      component.onSave();

      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
      const newCase = updatedCases[0];

      expect(newCase.id).toBe(1);
    });
  });

  describe('Date Handling', () => {
    beforeEach(() => {
      component.mode = 'create';
      fixture.detectChanges();
    });

    it('should set created date for new case', () => {
      component.relatedCaseForm.patchValue({
        courtId: 1,
        caseNumber: 54321,
        caseYear: 1445
      });

      const beforeSave = new Date();
      component.onSave();
      const afterSave = new Date();

      const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
      const newCase = updatedCases[updatedCases.length - 1];

      expect(newCase.createdDate).toBeDefined();
      expect(newCase.createdDate.getTime()).toBeGreaterThanOrEqual(beforeSave.getTime());
      expect(newCase.createdDate.getTime()).toBeLessThanOrEqual(afterSave.getTime());
    });

    it('should update modified date for edited case', (done) => {
      component.mode = 'edit';
      component.relatedCase = mockRelatedCase;

      setTimeout(() => {
        component.relatedCaseForm.patchValue({
          courtId: 2,
          caseNumber: 99999,
          caseYear: 1446
        });

        const beforeSave = new Date();
        component.onSave();
        const afterSave = new Date();

        const updatedCases = mockCaseDataState.updateRelatedCases.calls.mostRecent().args[0];
        const editedCase = updatedCases.find(c => c.id === mockRelatedCase.id);

        expect(editedCase?.modifiedDate.getTime()).toBeGreaterThanOrEqual(beforeSave.getTime());
        expect(editedCase?.modifiedDate.getTime()).toBeLessThanOrEqual(afterSave.getTime());

        done();
      }, 10);
    });
  });
});
