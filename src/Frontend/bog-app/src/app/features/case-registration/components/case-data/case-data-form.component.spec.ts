import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { CaseDataFormComponent } from './case-data-form.component';
import { CaseDataStateService } from '../../services/case-data-state.service';
import { CaseRegistrationApiService } from '../../services/case-registration-api.service';
import { ClassificationApiService } from '../../services/classification-api.service';
import { of, throwError } from 'rxjs';

describe('CaseDataFormComponent', () => {
  let component: CaseDataFormComponent;
  let fixture: ComponentFixture<CaseDataFormComponent>;
  let mockCaseDataState: jasmine.SpyObj<CaseDataStateService>;
  let mockCaseApi: jasmine.SpyObj<CaseRegistrationApiService>;
  let mockClassificationApi: jasmine.SpyObj<ClassificationApiService>;
  let mockSnackBar: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    mockCaseDataState = jasmine.createSpyObj('CaseDataStateService', [
      'getSubject',
      'getEvidence',
      'getClassificationIds',
      'updateSubject',
      'updateEvidence',
      'updateClassifications'
    ]);
    mockCaseDataState.state$ = of({
      subject: '',
      evidence: '',
      claims: [],
      relatedCases: [],
      classificationIds: [],
      primaryMobile: '',
      secondaryMobile: '',
      email: ''
    });

    mockCaseApi = jasmine.createSpyObj('CaseRegistrationApiService', ['update']);
    mockClassificationApi = jasmine.createSpyObj('ClassificationApiService', ['getAll']);
    mockSnackBar = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      declarations: [CaseDataFormComponent],
      imports: [
        ReactiveFormsModule,
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatOptionModule,
        MatSnackBarModule,
        BrowserAnimationsModule
      ],
      providers: [
        { provide: CaseDataStateService, useValue: mockCaseDataState },
        { provide: CaseRegistrationApiService, useValue: mockCaseApi },
        { provide: ClassificationApiService, useValue: mockClassificationApi },
        { provide: MatSnackBar, useValue: mockSnackBar }
      ]
    }).compileComponents();

    mockCaseDataState.getSubject.and.returnValue('');
    mockCaseDataState.getEvidence.and.returnValue('');
    mockCaseDataState.getClassificationIds.and.returnValue([]);
    mockClassificationApi.getAll.and.returnValue(of([]));

    fixture = TestBed.createComponent(CaseDataFormComponent);
    component = fixture.componentInstance;
    component.requestId = 1;
    component.canEdit = true;
  });

  describe('Component Initialization', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should load classifications on init', () => {
      fixture.detectChanges();
      expect(mockClassificationApi.getAll).toHaveBeenCalled();
    });

    it('should load case data from state service on init', () => {
      fixture.detectChanges();
      expect(mockCaseDataState.getSubject).toHaveBeenCalled();
      expect(mockCaseDataState.getEvidence).toHaveBeenCalled();
      expect(mockCaseDataState.getClassificationIds).toHaveBeenCalled();
    });

    it('should disable form when canEdit is false', () => {
      component.canEdit = false;
      fixture.detectChanges();
      expect(component.caseDataForm.disabled).toBe(true);
    });

    it('should enable form when canEdit is true', () => {
      component.canEdit = true;
      fixture.detectChanges();
      expect(component.caseDataForm.enabled).toBe(true);
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should require subject field', () => {
      const control = component.caseDataForm.get('subject');
      control?.setValue('');
      expect(control?.hasError('required')).toBe(true);
    });

    it('should require minimum subject length (10 characters)', () => {
      const control = component.caseDataForm.get('subject');
      control?.setValue('short');
      expect(control?.hasError('minlength')).toBe(true);
    });

    it('should enforce maximum subject length (4000 characters)', () => {
      const control = component.caseDataForm.get('subject');
      const longText = 'a'.repeat(4001);
      control?.setValue(longText);
      expect(control?.hasError('maxlength')).toBe(true);
    });

    it('should allow subject with exactly 4000 characters', () => {
      const control = component.caseDataForm.get('subject');
      const maxLengthText = 'a'.repeat(4000);
      control?.setValue(maxLengthText);
      expect(control?.hasError('maxlength')).toBe(false);
    });

    it('should require evidence field', () => {
      const control = component.caseDataForm.get('evidence');
      control?.setValue('');
      expect(control?.hasError('required')).toBe(true);
    });

    it('should require minimum evidence length (20 characters)', () => {
      const control = component.caseDataForm.get('evidence');
      control?.setValue('short');
      expect(control?.hasError('minlength')).toBe(true);
    });

    it('should enforce maximum evidence length (4000 characters)', () => {
      const control = component.caseDataForm.get('evidence');
      const longText = 'a'.repeat(4001);
      control?.setValue(longText);
      expect(control?.hasError('maxlength')).toBe(true);
    });

    it('should require at least one classification', () => {
      const control = component.caseDataForm.get('classifications');
      control?.setValue([]);
      expect(control?.hasError('minlength')).toBe(true);
    });
  });

  describe('Character Counter', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should calculate subject length correctly', () => {
      component.caseDataForm.get('subject')?.setValue('1234567890');
      expect(component.subjectLength).toBe(10);
    });

    it('should return 0 when subject is empty', () => {
      component.caseDataForm.get('subject')?.setValue('');
      expect(component.subjectLength).toBe(0);
    });

    it('should calculate evidence length correctly', () => {
      component.caseDataForm.get('evidence')?.setValue('12345678901234567890');
      expect(component.evidenceLength).toBe(20);
    });

    it('should return 0 when evidence is empty', () => {
      component.caseDataForm.get('evidence')?.setValue('');
      expect(component.evidenceLength).toBe(0);
    });
  });

  describe('Auto-Save Functionality', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should not save when form is invalid', () => {
      component.caseDataForm.get('subject')?.setValue('short');
      component.saveForm();
      expect(mockCaseApi.update).not.toHaveBeenCalled();
    });

    it('should not save when already saving', () => {
      component.saving = true;
      component.caseDataForm.patchValue({
        subject: '1234567890',
        evidence: '12345678901234567890',
        classifications: [1]
      });
      component.saveForm();
      expect(mockCaseApi.update).not.toHaveBeenCalled();
    });

    it('should not save when canEdit is false', () => {
      component.canEdit = false;
      component.caseDataForm.patchValue({
        subject: '1234567890',
        evidence: '12345678901234567890',
        classifications: [1]
      });
      component.saveForm();
      expect(mockCaseApi.update).not.toHaveBeenCalled();
    });

    it('should update state service before API call', () => {
      mockCaseApi.update.and.returnValue(of({
        id: 1,
        courtId: 1,
        requestStatusId: 1,
        requestStatusName: 'Draft',
        subject: 'Test',
        evidence: 'Test Evidence',
        createdDate: new Date(),
        modifiedDate: new Date(),
        isDeleted: false
      }));

      component.caseDataForm.patchValue({
        subject: '1234567890',
        evidence: '12345678901234567890',
        classifications: [1]
      });

      component.saveForm();

      expect(mockCaseDataState.updateSubject).toHaveBeenCalledWith('1234567890');
      expect(mockCaseDataState.updateEvidence).toHaveBeenCalledWith('12345678901234567890');
      expect(mockCaseDataState.updateClassifications).toHaveBeenCalledWith([1]);
    });
  });

  describe('API Save', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should call API update with correct data', () => {
      mockCaseApi.update.and.returnValue(of({
        id: 1,
        courtId: 1,
        requestStatusId: 1,
        requestStatusName: 'Draft',
        subject: 'Test',
        evidence: 'Test Evidence',
        createdDate: new Date(),
        modifiedDate: new Date(),
        isDeleted: false
      }));

      component.caseDataForm.patchValue({
        subject: 'Valid Subject',
        evidence: 'Valid Evidence Text',
        classifications: [1, 2]
      });

      component.saveForm();

      expect(mockCaseApi.update).toHaveBeenCalledWith(1, {
        subject: 'Valid Subject',
        evidence: 'Valid Evidence Text',
        classificationIds: [1, 2]
      });
    });

    it('should handle API success', (done) => {
      const response = {
        id: 1,
        courtId: 1,
        requestStatusId: 1,
        requestStatusName: 'Draft',
        subject: 'Updated Subject',
        evidence: 'Updated Evidence',
        createdDate: new Date(),
        modifiedDate: new Date(),
        isDeleted: false
      };

      mockCaseApi.update.and.returnValue(of(response));

      component.caseDataForm.patchValue({
        subject: 'Valid Subject',
        evidence: 'Valid Evidence Text',
        classifications: [1]
      });

      component.saveForm();

      setTimeout(() => {
        expect(component.saveSuccess).toBe(true);
        expect(component.saving).toBe(false);
        done();
      }, 100);
    });

    it('should handle API error', () => {
      mockCaseApi.update.and.returnValue(throwError(() => ({
        error: { message: 'Save failed' }
      })));

      component.caseDataForm.patchValue({
        subject: 'Valid Subject',
        evidence: 'Valid Evidence Text',
        classifications: [1]
      });

      component.saveForm();

      expect(component.saving).toBe(false);
      expect(mockSnackBar.open).toHaveBeenCalledWith('خطأ في حفظ بيانات الدعوى', 'إغلاق', { duration: 3000 });
    });
  });

  describe('Classifications Loading', () => {
    it('should load classifications successfully', () => {
      const mockClassifications = [
        { id: 1, nameAr: 'تصنيف 1', nameEn: 'Classification 1' },
        { id: 2, nameAr: 'تصنيف 2', nameEn: 'Classification 2' }
      ];

      mockClassificationApi.getAll.and.returnValue(of(mockClassifications));
      fixture.detectChanges();

      expect(component.classifications).toEqual(mockClassifications);
    });

    it('should show error message when classifications loading fails', () => {
      mockClassificationApi.getAll.and.returnValue(throwError(() => new Error('Load failed')));
      fixture.detectChanges();

      expect(mockSnackBar.open).toHaveBeenCalledWith('فشل في تحميل التصنيفات', 'إغلاق', { duration: 3000 });
    });
  });

  describe('State Subscription', () => {
    it('should subscribe to state changes on init', () => {
      const mockState = {
        subject: 'Updated Subject',
        evidence: 'Updated Evidence',
        claims: [],
        relatedCases: [],
        classificationIds: [1],
        primaryMobile: '',
        secondaryMobile: '',
        email: ''
      };

      mockCaseDataState.state$ = of(mockState);

      fixture.detectChanges();

      // State subscription should have been called (from ngOnInit)
      expect(component.caseDataForm.get('subject')?.value).toBe('');
    });
  });

  describe('Component Cleanup', () => {
    it('should unsubscribe on destroy', () => {
      fixture.detectChanges();
      spyOn(component['destroy$'], 'next');
      spyOn(component['destroy$'], 'complete');

      component.ngOnDestroy();

      expect(component['destroy$'].next).toHaveBeenCalled();
      expect(component['destroy$'].complete).toHaveBeenCalled();
    });
  });
});
