import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RelatedCasesListComponent } from './related-cases-list.component';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { RelatedCaseVM } from '../../../models/related-case.model';
import { of } from 'rxjs';

describe('RelatedCasesListComponent', () => {
  let component: RelatedCasesListComponent;
  let fixture: ComponentFixture<RelatedCasesListComponent>;
  let mockCaseDataState: jasmine.SpyObj<CaseDataStateService>;
  let mockDialog: jasmine.SpyObj<MatDialog>;
  let mockSnackBar: jasmine.SpyObj<MatSnackBar>;

  const mockRelatedCases: RelatedCaseVM[] = [
    {
      id: 1,
      caseRegistrationRequestId: 1,
      courtId: 1,
      courtName: 'محكمة الرياض',
      caseNumber: 12345,
      caseYear: 1445,
      createdDate: new Date(),
      modifiedDate: new Date()
    },
    {
      id: 2,
      caseRegistrationRequestId: 1,
      courtId: 2,
      courtName: 'محكمة جدة',
      caseNumber: 67890,
      caseYear: 1445,
      createdDate: new Date(),
      modifiedDate: new Date()
    }
  ];

  beforeEach(async () => {
    mockCaseDataState = jasmine.createSpyObj('CaseDataStateService', [
      'getRelatedCases',
      'updateRelatedCases'
    ]);
    mockCaseDataState.state$ = of({
      subject: '',
      evidence: '',
      claims: [],
      relatedCases: mockRelatedCases,
      classificationIds: [],
      primaryMobile: '',
      secondaryMobile: '',
      email: ''
    });

    mockDialog = jasmine.createSpyObj('MatDialog', ['open']);
    mockSnackBar = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      declarations: [RelatedCasesListComponent],
      imports: [
        MatTableModule,
        MatButtonModule,
        MatIconModule,
        BrowserAnimationsModule
      ],
      providers: [
        { provide: CaseDataStateService, useValue: mockCaseDataState },
        { provide: MatDialog, useValue: mockDialog },
        { provide: MatSnackBar, useValue: mockSnackBar }
      ]
    }).compileComponents();

    mockCaseDataState.getRelatedCases.and.returnValue(mockRelatedCases);

    fixture = TestBed.createComponent(RelatedCasesListComponent);
    component = fixture.componentInstance;
    component.requestId = 1;
    component.canEdit = true;
  });

  describe('Component Initialization', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should load related cases from state service on init', () => {
      fixture.detectChanges();
      expect(mockCaseDataState.getRelatedCases).toHaveBeenCalled();
      expect(component.relatedCases).toEqual(mockRelatedCases);
    });

    it('should emit initial count', () => {
      spyOn(component.countChanged, 'emit');
      fixture.detectChanges();
      expect(component.countChanged.emit).toHaveBeenCalledWith(mockRelatedCases.length);
    });

    it('should define displayed columns', () => {
      expect(component.displayedColumns).toEqual(['court', 'caseNumber', 'caseYear', 'actions']);
    });

    it('should subscribe to state changes on init', (done) => {
      fixture.detectChanges();

      expect(component.relatedCases.length).toBeGreaterThan(0);
      done();
    });
  });

  describe('Related Cases Display', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display all related cases', () => {
      expect(component.relatedCases.length).toBe(2);
    });

    it('should display case information correctly', () => {
      expect(component.relatedCases[0].caseNumber).toBe(12345);
      expect(component.relatedCases[0].caseYear).toBe(1445);
      expect(component.relatedCases[1].caseNumber).toBe(67890);
      expect(component.relatedCases[1].caseYear).toBe(1445);
    });

    it('should display court names', () => {
      expect(component.relatedCases[0].courtName).toBe('محكمة الرياض');
      expect(component.relatedCases[1].courtName).toBe('محكمة جدة');
    });
  });

  describe('Add Related Case Dialog', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should open dialog in create mode', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.openAddRelatedCase();

      expect(mockDialog.open).toHaveBeenCalled();
      const call = mockDialog.open.calls.mostRecent();
      expect((call.args[1] as any).data).toEqual({
        requestId: 1,
        mode: 'create'
      });
    });

    it('should open dialog with correct configuration', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.openAddRelatedCase();

      const call = mockDialog.open.calls.mostRecent();
      expect((call.args[1] as any).width).toBe('600px');
      expect((call.args[1] as any).maxWidth).toBe('95vw');
      expect((call.args[1] as any).direction).toBe('rtl');
    });

    it('should reload related cases when dialog closes with result', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(dialogRefMock);

      spyOn(component, 'loadRelatedCases');

      component.openAddRelatedCase();

      expect(component.loadRelatedCases).toHaveBeenCalled();
    });
  });

  describe('Edit Related Case Dialog', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should open dialog in edit mode with case data', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.editRelatedCase(mockRelatedCases[0]);

      expect(mockDialog.open).toHaveBeenCalled();
      const call = mockDialog.open.calls.mostRecent();
      expect((call.args[1] as any).data).toEqual({
        requestId: 1,
        relatedCase: mockRelatedCases[0],
        mode: 'edit'
      });
    });

    it('should open dialog with RTL direction', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.editRelatedCase(mockRelatedCases[0]);

      const call = mockDialog.open.calls.mostRecent();
      expect((call.args[1] as any).direction).toBe('rtl');
    });

    it('should reload related cases on successful edit', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(dialogRefMock);

      spyOn(component, 'loadRelatedCases');

      component.editRelatedCase(mockRelatedCases[0]);

      expect(component.loadRelatedCases).toHaveBeenCalled();
    });
  });

  describe('Delete Related Case Dialog', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should open confirmation dialog for delete', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.deleteRelatedCase(mockRelatedCases[0]);

      expect(mockDialog.open).toHaveBeenCalled();
    });

    it('should show case info in delete message', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.deleteRelatedCase(mockRelatedCases[0]);

      const call = mockDialog.open.calls.mostRecent();
      const message = (call.args[1] as any).data.message;
      expect(message).toContain('12345/1445');
    });

    it('should show snackbar when delete confirmed', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.deleteRelatedCase(mockRelatedCases[0]);

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'سيتم حذف الدعوى المرتبطة عند الحفظ',
        'إغلاق',
        { duration: 3000 }
      );
    });

    it('should include deletion confirmation title', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.deleteRelatedCase(mockRelatedCases[0]);

      const call = mockDialog.open.calls.mostRecent();
      expect((call.args[1] as any).data.title).toBe('تأكيد الحذف');
    });
  });

  describe('State Reactivity', () => {
    it('should update related cases when state changes', (done) => {
      const newRelatedCases: RelatedCaseVM[] = [
        {
          id: 3,
          caseRegistrationRequestId: 1,
          courtId: 3,
          courtName: 'محكمة الدمام',
          caseNumber: 11111,
          caseYear: 1445,
          createdDate: new Date(),
          modifiedDate: new Date()
        }
      ];

      mockCaseDataState.state$ = of({
        subject: '',
        evidence: '',
        claims: [],
        relatedCases: newRelatedCases,
        classificationIds: [],
        primaryMobile: '',
        secondaryMobile: '',
        email: ''
      });

      fixture.detectChanges();

      setTimeout(() => {
        expect(component.relatedCases).toEqual(newRelatedCases);
        done();
      }, 100);
    });

    it('should emit count when related cases change', (done) => {
      spyOn(component.countChanged, 'emit');

      const newRelatedCases: RelatedCaseVM[] = [
        {
          id: 3,
          caseRegistrationRequestId: 1,
          courtId: 3,
          courtName: 'محكمة الدمام',
          caseNumber: 11111,
          caseYear: 1445,
          createdDate: new Date(),
          modifiedDate: new Date()
        }
      ];

      mockCaseDataState.state$ = of({
        subject: '',
        evidence: '',
        claims: [],
        relatedCases: newRelatedCases,
        classificationIds: [],
        primaryMobile: '',
        secondaryMobile: '',
        email: ''
      });

      fixture.detectChanges();

      setTimeout(() => {
        expect(component.countChanged.emit).toHaveBeenCalled();
        done();
      }, 100);
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

  describe('Edit Mode Control', () => {
    it('should respect canEdit flag', () => {
      component.canEdit = false;
      fixture.detectChanges();
      expect(component.canEdit).toBe(false);
    });

    it('should enable editing when canEdit is true', () => {
      component.canEdit = true;
      fixture.detectChanges();
      expect(component.canEdit).toBe(true);
    });
  });

  describe('Empty State', () => {
    beforeEach(() => {
      mockCaseDataState.getRelatedCases.and.returnValue([]);
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
    });

    it('should show empty state when no related cases exist', () => {
      fixture.detectChanges();
      expect(component.relatedCases.length).toBe(0);
    });

    it('should emit 0 count when no related cases', () => {
      spyOn(component.countChanged, 'emit');
      fixture.detectChanges();
      expect(component.countChanged.emit).toHaveBeenCalledWith(0);
    });
  });

  describe('Loading State', () => {
    it('should initialize with loading flag', () => {
      expect(component.loading).toBe(false);
    });
  });

  describe('Case Information Formatting', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should format case info as number/year', () => {
      const caseInfo = `${mockRelatedCases[0].caseNumber}/${mockRelatedCases[0].caseYear}`;
      expect(caseInfo).toBe('12345/1445');
    });
  });

  describe('Dialog Mode Consistency', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should pass correct mode to create dialog', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.openAddRelatedCase();

      const call = mockDialog.open.calls.mostRecent();
      expect((call.args[1] as any).data.mode).toBe('create');
    });

    it('should pass correct mode to edit dialog', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.editRelatedCase(mockRelatedCases[0]);

      const call = mockDialog.open.calls.mostRecent();
      expect((call.args[1] as any).data.mode).toBe('edit');
    });
  });
});
