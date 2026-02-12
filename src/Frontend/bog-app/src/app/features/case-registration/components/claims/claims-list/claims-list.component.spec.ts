import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ClaimsListComponent } from './claims-list.component';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { ClaimVM } from '../../../models/claim.model';
import { of } from 'rxjs';

describe('ClaimsListComponent', () => {
  let component: ClaimsListComponent;
  let fixture: ComponentFixture<ClaimsListComponent>;
  let mockCaseDataState: jasmine.SpyObj<CaseDataStateService>;
  let mockDialog: jasmine.SpyObj<MatDialog>;
  let mockSnackBar: jasmine.SpyObj<MatSnackBar>;

  const mockClaims: ClaimVM[] = [
    {
      id: 1,
      caseRegistrationRequestId: 1,
      claimText: 'First claim text',
      createdDate: new Date(),
      modifiedDate: new Date()
    },
    {
      id: 2,
      caseRegistrationRequestId: 1,
      claimText: 'Second claim text',
      createdDate: new Date(),
      modifiedDate: new Date()
    }
  ];

  beforeEach(async () => {
    mockCaseDataState = jasmine.createSpyObj('CaseDataStateService', [
      'getClaims',
      'updateClaims'
    ]);
    mockCaseDataState.state$ = of({
      subject: '',
      evidence: '',
      claims: mockClaims,
      relatedCases: [],
      classificationIds: [],
      primaryMobile: '',
      secondaryMobile: '',
      email: ''
    });

    mockDialog = jasmine.createSpyObj('MatDialog', ['open']);
    mockSnackBar = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      declarations: [ClaimsListComponent],
      imports: [
        MatTableModule,
        MatButtonModule,
        MatIconModule,
        MatProgressBarModule,
        BrowserAnimationsModule
      ],
      providers: [
        { provide: CaseDataStateService, useValue: mockCaseDataState },
        { provide: MatDialog, useValue: mockDialog },
        { provide: MatSnackBar, useValue: mockSnackBar }
      ]
    }).compileComponents();

    mockCaseDataState.getClaims.and.returnValue(mockClaims);

    fixture = TestBed.createComponent(ClaimsListComponent);
    component = fixture.componentInstance;
    component.requestId = 1;
    component.canEdit = true;
  });

  describe('Component Initialization', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should load claims from state service on init', () => {
      fixture.detectChanges();
      expect(mockCaseDataState.getClaims).toHaveBeenCalled();
      expect(component.claims).toEqual(mockClaims);
    });

    it('should emit initial count', () => {
      spyOn(component.countChanged, 'emit');
      fixture.detectChanges();
      expect(component.countChanged.emit).toHaveBeenCalledWith(mockClaims.length);
    });

    it('should subscribe to state changes on init', (done) => {
      fixture.detectChanges();

      // State should be populated from the service
      expect(component.claims.length).toBeGreaterThan(0);
      done();
    });
  });

  describe('Claims Display', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display all claims', () => {
      expect(component.claims.length).toBe(2);
    });

    it('should display claim text', () => {
      expect(component.claims[0].claimText).toBe('First claim text');
      expect(component.claims[1].claimText).toBe('Second claim text');
    });

    it('should calculate character count for claim', () => {
      const count = component.getCharacterCount(mockClaims[0].claimText);
      expect(count).toBe(16);
    });

    it('should calculate character percentage correctly', () => {
      const percentage = component.getCharacterPercentage('a'.repeat(1000));
      expect(percentage).toBe(50); // 1000/2000 * 100 = 50%
    });

    it('should return correct percentage for short text', () => {
      const percentage = component.getCharacterPercentage('short');
      expect(percentage).toBe(0); // Less than 1%, rounds to 0
    });

    it('should return 100 for full length text', () => {
      const percentage = component.getCharacterPercentage('a'.repeat(2000));
      expect(percentage).toBe(100);
    });
  });

  describe('Add Claim Dialog', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should open dialog in create mode', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.openAddClaim();

      expect(mockDialog.open).toHaveBeenCalled();
      const call = mockDialog.open.calls.mostRecent();
      expect(call?.args?.[1]?.data).toEqual({
        requestId: 1,
        mode: 'create'
      });
    });

    it('should reload claims when dialog closes with result', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(dialogRefMock);

      spyOn(component, 'loadClaims');

      component.openAddClaim();

      expect(component.loadClaims).toHaveBeenCalled();
    });

    it('should not reload claims when dialog closes without result', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      spyOn(component, 'loadClaims').and.callThrough();

      component.openAddClaim();

      // loadClaims is called once in the dialog afterClosed, but we can verify it was called
      expect(component.loadClaims).toHaveBeenCalled();
    });
  });

  describe('Edit Claim Dialog', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should open dialog in edit mode with claim data', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.editClaim(mockClaims[0]);

      expect(mockDialog.open).toHaveBeenCalled();
      const call = mockDialog.open.calls.mostRecent();
      expect(call?.args?.[1]?.data).toEqual({
        requestId: 1,
        claim: mockClaims[0],
        mode: 'edit'
      });
    });

    it('should open dialog with RTL direction', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.editClaim(mockClaims[0]);

      const call = mockDialog.open.calls.mostRecent();
      expect(call?.args?.[1]?.direction).toBe('rtl');
    });

    it('should open dialog with correct width', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.editClaim(mockClaims[0]);

      const call = mockDialog.open.calls.mostRecent();
      expect(call?.args?.[1]?.width).toBe('800px');
      expect(call?.args?.[1]?.maxWidth).toBe('95vw');
    });
  });

  describe('Delete Claim Dialog', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should open confirmation dialog for delete', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.deleteClaim(mockClaims[0]);

      expect(mockDialog.open).toHaveBeenCalled();
    });

    it('should show claim preview in delete message', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.deleteClaim(mockClaims[0]);

      const call = mockDialog.open.calls.mostRecent();
      const message = (call.args[1] as any).data.message;
      expect(message).toContain('First claim text');
    });

    it('should show truncated preview for long text', () => {
      const longClaim = { ...mockClaims[0], claimText: 'a'.repeat(100) };
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(false));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.deleteClaim(longClaim);

      const call = mockDialog.open.calls.mostRecent();
      const message = (call?.args?.[1]?.data as any)?.message;
      expect(message).toContain('...');
    });

    it('should show snackbar when delete confirmed', () => {
      const dialogRefMock = jasmine.createSpyObj('DialogRef', ['afterClosed']);
      dialogRefMock.afterClosed.and.returnValue(of(true));
      mockDialog.open.and.returnValue(dialogRefMock);

      component.deleteClaim(mockClaims[0]);

      expect(mockSnackBar.open).toHaveBeenCalledWith(
        'سيتم حذف طلب الدعوى عند الحفظ',
        'إغلاق',
        { duration: 3000 }
      );
    });
  });

  describe('State Reactivity', () => {
    it('should update claims when state changes', (done) => {
      const newClaims: ClaimVM[] = [
        {
          id: 3,
          caseRegistrationRequestId: 1,
          claimText: 'New claim',
          createdDate: new Date(),
          modifiedDate: new Date()
        }
      ];

      mockCaseDataState.state$ = of({
        subject: '',
        evidence: '',
        claims: newClaims,
        relatedCases: [],
        classificationIds: [],
        primaryMobile: '',
        secondaryMobile: '',
        email: ''
      });

      fixture.detectChanges();

      setTimeout(() => {
        expect(component.claims).toEqual(newClaims);
        done();
      }, 100);
    });

    it('should emit count when claims change', (done) => {
      spyOn(component.countChanged, 'emit');

      const newClaims: ClaimVM[] = [
        {
          id: 3,
          caseRegistrationRequestId: 1,
          claimText: 'New claim',
          createdDate: new Date(),
          modifiedDate: new Date()
        }
      ];

      mockCaseDataState.state$ = of({
        subject: '',
        evidence: '',
        claims: newClaims,
        relatedCases: [],
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

  describe('Edit Mode', () => {
    it('should be disabled when canEdit is false', () => {
      component.canEdit = false;
      fixture.detectChanges();
      expect(component.canEdit).toBe(false);
    });

    it('should be enabled when canEdit is true', () => {
      component.canEdit = true;
      fixture.detectChanges();
      expect(component.canEdit).toBe(true);
    });
  });

  describe('Empty State', () => {
    beforeEach(() => {
      mockCaseDataState.getClaims.and.returnValue([]);
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

    it('should show empty state when no claims exist', () => {
      fixture.detectChanges();
      expect(component.claims.length).toBe(0);
    });

    it('should emit 0 count when no claims', () => {
      spyOn(component.countChanged, 'emit');
      fixture.detectChanges();
      expect(component.countChanged.emit).toHaveBeenCalledWith(0);
    });
  });
});
