import { TestBed } from '@angular/core/testing';
import { CaseDataStateService, CaseDataState } from './case-data-state.service';
import { ClaimVM } from '../models/claim.model';
import { RelatedCaseVM } from '../models/related-case.model';

describe('CaseDataStateService', () => {
  let service: CaseDataStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CaseDataStateService);
    // Clear localStorage before each test
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Subject Updates', () => {
    it('should update subject and emit state', (done) => {
      const testSubject = 'Test case subject';
      service.updateSubject(testSubject);

      service.state$.subscribe(state => {
        if (state.subject === testSubject) {
          expect(state.subject).toBe(testSubject);
          done();
        }
      });
    });

    it('should persist subject to localStorage', () => {
      const testSubject = 'Persisted subject';
      service.updateSubject(testSubject);

      const stored = localStorage.getItem('case-data-state');
      expect(stored).toBeTruthy();
      const parsed = JSON.parse(stored!);
      expect(parsed.subject).toBe(testSubject);
    });
  });

  describe('Evidence Updates', () => {
    it('should update evidence', (done) => {
      const testEvidence = 'Case evidence details';
      service.updateEvidence(testEvidence);

      service.state$.subscribe(state => {
        if (state.evidence === testEvidence) {
          expect(state.evidence).toBe(testEvidence);
          done();
        }
      });
    });
  });

  describe('Claims Management', () => {
    it('should update claims array', (done) => {
      const testClaims: ClaimVM[] = [
        {
          id: 1,
          caseRegistrationRequestId: 1,
          claimText: 'Claim 1',
          createdDate: new Date(),
          modifiedDate: new Date()
        }
      ];

      service.updateClaims(testClaims);

      service.state$.subscribe(state => {
        if (state.claims.length > 0) {
          expect(state.claims.length).toBe(1);
          expect(state.claims[0].claimText).toBe('Claim 1');
          done();
        }
      });
    });

    it('should get claims via getter', () => {
      const testClaim: ClaimVM = {
        id: 1,
        caseRegistrationRequestId: 1,
        claimText: 'Test claim',
        createdDate: new Date(),
        modifiedDate: new Date()
      };

      service.updateClaims([testClaim]);
      const claims = service.getClaims();

      expect(claims.length).toBe(1);
      expect(claims[0].claimText).toBe('Test claim');
    });
  });

  describe('Related Cases Management', () => {
    it('should update related cases array', (done) => {
      const testRelatedCases: RelatedCaseVM[] = [
        {
          id: 1,
          caseRegistrationRequestId: 1,
          courtId: 1,
          caseNumber: 12345,
          caseYear: 1445,
          createdDate: new Date(),
          modifiedDate: new Date()
        }
      ];

      service.updateRelatedCases(testRelatedCases);

      service.state$.subscribe(state => {
        if (state.relatedCases.length > 0) {
          expect(state.relatedCases.length).toBe(1);
          expect(state.relatedCases[0].caseNumber).toBe(12345);
          done();
        }
      });
    });
  });

  describe('Classifications Management', () => {
    it('should update classification IDs', (done) => {
      const classificationIds = [1, 2, 3];
      service.updateClassifications(classificationIds);

      service.state$.subscribe(state => {
        if (state.classificationIds.length > 0) {
          expect(state.classificationIds).toEqual(classificationIds);
          done();
        }
      });
    });

    it('should get classifications via getter', () => {
      const classificationIds = [5, 6, 7];
      service.updateClassifications(classificationIds);
      const ids = service.getClassificationIds();

      expect(ids).toEqual(classificationIds);
    });
  });

  describe('Contact Information Management', () => {
    it('should update all contact fields', (done) => {
      const primaryMobile = '0501234567';
      const secondaryMobile = '0559876543';
      const email = 'test@example.com';

      service.updateContactInfo(primaryMobile, secondaryMobile, email);

      service.state$.subscribe(state => {
        if (state.primaryMobile === primaryMobile) {
          expect(state.primaryMobile).toBe(primaryMobile);
          expect(state.secondaryMobile).toBe(secondaryMobile);
          expect(state.email).toBe(email);
          done();
        }
      });
    });

    it('should update primary mobile individually', () => {
      service.updatePrimaryMobile('0501111111');
      const contact = service.getContactInfo();

      expect(contact.primaryMobile).toBe('0501111111');
    });

    it('should update secondary mobile individually', () => {
      service.updateSecondaryMobile('0502222222');
      const contact = service.getContactInfo();

      expect(contact.secondaryMobile).toBe('0502222222');
    });

    it('should update email individually', () => {
      service.updateEmail('newemail@test.com');
      const contact = service.getContactInfo();

      expect(contact.email).toBe('newemail@test.com');
    });

    it('should handle empty contact fields', () => {
      service.updateContactInfo('', '', '');
      const contact = service.getContactInfo();

      expect(contact.primaryMobile).toBe('');
      expect(contact.secondaryMobile).toBe('');
      expect(contact.email).toBe('');
    });
  });

  describe('State Retrieval', () => {
    it('should get all data', () => {
      service.updateSubject('Test subject');
      service.updateEvidence('Test evidence');
      service.updateClassifications([1, 2]);

      const allData = service.getAllData();

      expect(allData.subject).toBe('Test subject');
      expect(allData.evidence).toBe('Test evidence');
      expect(allData.classificationIds).toEqual([1, 2]);
    });
  });

  describe('Load from Request', () => {
    it('should load existing request data', () => {
      const mockRequest = {
        subject: 'Loaded subject',
        evidence: 'Loaded evidence',
        claims: [],
        relatedCases: [],
        classificationIds: [1, 2, 3],
        primaryMobile: '0505555555',
        secondaryMobile: '0506666666',
        email: 'loaded@example.com'
      };

      service.loadFromRequest(mockRequest);

      expect(service.getSubject()).toBe('Loaded subject');
      expect(service.getEvidence()).toBe('Loaded evidence');
      expect(service.getClassificationIds()).toEqual([1, 2, 3]);
      expect(service.getContactInfo().primaryMobile).toBe('0505555555');
    });

    it('should handle missing fields in request', () => {
      const mockRequest = {
        subject: 'Subject only'
      };

      service.loadFromRequest(mockRequest);

      expect(service.getSubject()).toBe('Subject only');
      expect(service.getEvidence()).toBe('');
      expect(service.getClaims()).toEqual([]);
      expect(service.getClassificationIds()).toEqual([]);
    });
  });

  describe('Reset State', () => {
    it('should reset to initial state', () => {
      service.updateSubject('Modified subject');
      service.updateEvidence('Modified evidence');
      service.updateClassifications([1, 2, 3]);

      service.resetState();

      expect(service.getSubject()).toBe('');
      expect(service.getEvidence()).toBe('');
      expect(service.getClassificationIds()).toEqual([]);
    });

    it('should clear localStorage on reset', () => {
      service.updateSubject('Test');
      expect(localStorage.getItem('case-data-state')).toBeTruthy();

      service.resetState();

      expect(localStorage.getItem('case-data-state')).toBeNull();
    });
  });

  describe('LocalStorage Persistence', () => {
    it('should load state from localStorage on initialization', () => {
      // Set up localStorage
      const state: CaseDataState = {
        subject: 'Stored subject',
        evidence: 'Stored evidence',
        claims: [],
        relatedCases: [],
        classificationIds: [1, 2],
        primaryMobile: '0501234567',
        secondaryMobile: '',
        email: 'stored@example.com'
      };
      localStorage.setItem('case-data-state', JSON.stringify(state));

      // Create new service instance
      const newService = TestBed.inject(CaseDataStateService);

      expect(newService.getSubject()).toBe('Stored subject');
      expect(newService.getEvidence()).toBe('Stored evidence');
      expect(newService.getClassificationIds()).toEqual([1, 2]);
    });

    it('should handle corrupted localStorage gracefully', () => {
      localStorage.setItem('case-data-state', 'invalid json');

      // Should not throw error
      const newService = new CaseDataStateService();
      expect(newService.getSubject()).toBe('');
    });
  });

  describe('Clear LocalStorage', () => {
    it('should clear localStorage without resetting state', () => {
      service.updateSubject('Test subject');
      expect(localStorage.getItem('case-data-state')).toBeTruthy();

      service.clearFromLocalStorage();

      expect(localStorage.getItem('case-data-state')).toBeNull();
      // State should still be in memory
      expect(service.getSubject()).toBe('Test subject');
    });
  });

  describe('Observable Subscription', () => {
    it('should emit state changes through observable', (done) => {
      let emissionCount = 0;

      service.state$.subscribe(state => {
        emissionCount++;
        if (emissionCount === 2) { // Initial state + one update
          expect(state.subject).toBe('Observable test');
          done();
        }
      });

      service.updateSubject('Observable test');
    });

    it('should allow multiple subscribers', () => {
      let subscriber1Called = false;
      let subscriber2Called = false;

      service.state$.subscribe(() => {
        subscriber1Called = true;
      });

      service.state$.subscribe(() => {
        subscriber2Called = true;
      });

      service.updateSubject('Multi-subscriber test');

      expect(subscriber1Called).toBeTruthy();
      expect(subscriber2Called).toBeTruthy();
    });
  });
});
