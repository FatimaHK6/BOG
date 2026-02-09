import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CaseRegistrationApiService } from '../../services/case-registration-api.service';
import { RequestStateService } from '../../services/request-state.service';
import { CaseDataStateService } from '../../services/case-data-state.service';
import { ClaimsApiService } from '../../services/claims-api.service';
import { RelatedCaseApiService } from '../../services/related-case-api.service';
import { CaseRequestVM, CaseRequestCreateDTO } from '../../models/case-request.model';
import { RequestStatusLabels, RequestStatus } from '../../models/enums';
import { CaseDataContainerComponent } from '../../components/case-data/case-data-container/case-data-container.component';

@Component({
  selector: 'app-request-details',
  templateUrl: './request-details.component.html',
  styleUrls: ['./request-details.component.scss']
})
export class RequestDetailsComponent implements OnInit, OnDestroy {
  @ViewChild(CaseDataContainerComponent) caseDataContainer?: CaseDataContainerComponent;

  requestId: number = 0;
  mode: 'create' | 'edit' | 'view' = 'create';
  pageTitle = '';
  loading = true;
  showValidation = false;

  currentStatus = 1;
  currentStatusName = 'مسودة';
  activeSection = 'defendants';

  // Metadata bar properties
  courtName: string = '';
  registrationMethod: string = '';
  caseType: string = '';
  caseRegistrationNumber: string = '';
  caseRegistrationDate: Date | null = null;
  createdDate: Date | null = null;

  plaintiffsCount = 0;
  defendantsCount = 0;
  claimsCount = 0;
  relatedCasesCount = 0;
  attachmentsCount = 0;
  deficienciesCount = 0;
  hasDeficiencies = false;

  canEdit = true;
  canSave = true;

  isMobileView = false;
  sidebarOpen = false;

  // Save state management
  isSaving = false;
  saveSuccess = false;

  // Expandable case data section state
  caseDataExpanded = true;
  activeCaseDataTab = 'subject-evidence';

  // Case data sub-tabs configuration
  caseDataTabs = [
    {
      id: 'subject-evidence',
      label: 'موضوع وأسانيد الدعوى',
      icon: 'description'
    },
    {
      id: 'claims',
      label: 'طلبات الدعوى',
      icon: 'format_list_numbered',
      showCount: true
    },
    {
      id: 'related-cases',
      label: 'الدعاوى المرتبطة',
      icon: 'link',
      showCount: true
    },
    {
      id: 'classifications',
      label: 'تصنيف الدعوى',
      icon: 'category',
      showCount: true
    },
    {
      id: 'contact-info',
      label: 'بيانات التواصل',
      icon: 'phone'
    },
    {
      id: 'attachments',
      label: 'المرفقات',
      icon: 'attach_file',
      showCount: true
    }
  ];

  // Counts for case data sub-tabs badges
  caseDataCounts = {
    claims: 0,
    relatedCases: 0,
    classifications: 0,
    attachments: 0
  };

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private requestState: RequestStateService,
    private caseRegistrationApi: CaseRegistrationApiService,
    private caseDataState: CaseDataStateService,
    private claimsApi: ClaimsApiService,
    private relatedCaseApi: RelatedCaseApiService
  ) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.mode = data['mode'] || 'create';
      this.pageTitle = data['title'] || 'طلب تسجيل دعوى';
    });

    // Check for tab parameter in route
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeSection = params['tab'];
      }
      if (params['subTab']) {
        this.activeCaseDataTab = params['subTab'];
      }
    });

    this.route.params.subscribe(params => {
      if (params['id']) {
        this.requestId = +params['id'];

        // CRITICAL: Set request context BEFORE loading data
        // This ensures localStorage is scoped to this specific request
        this.caseDataState.setRequestContext(this.requestId);

        this.loadRequest();
      } else {
        // New request - clear context
        this.caseDataState.setRequestContext(null);
        this.createNewRequest();
      }
    });

    this.requestState.currentRequest$
      .pipe(takeUntil(this.destroy$))
      .subscribe(request => {
        if (request) {
          this.updateFromRequest(request);
        }
      });

    // Subscribe to case data state for counts
    this.caseDataState.state$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.caseDataCounts = {
          claims: state.claims?.length || 0,
          relatedCases: state.relatedCases?.length || 0,
          classifications: state.classificationIds?.length || 0,
          attachments: 0
        };
      });

    this.setupResponsive();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadRequest() {
    this.loading = true;
    this.requestState.loadRequest(this.requestId).subscribe({
      next: (loadedRequest) => {
        // CRITICAL: Preserve and restore classifications from the loaded request
        // This ensures classifications are not cleared during the reload cycle
        const classificationIds = loadedRequest.classificationIds || [];
        console.log('[DEBUG] Loaded request classifications:', classificationIds);

        // After loading main request, load claims separately
        this.claimsApi.getClaims(this.requestId).subscribe({
          next: (claims) => {
            // Update state service with loaded claims
            this.caseDataState.updateClaims(claims);

            // Then load related cases separately
            this.relatedCaseApi.getRelatedCases(this.requestId).subscribe({
              next: (relatedCases) => {
                // Update state service with loaded related cases
                this.caseDataState.updateRelatedCases(relatedCases);

                // CRITICAL: Re-ensure classifications are set after all other data loads
                // This prevents classifications from being cleared if state subscription
                // processes updates in an unexpected order
                this.caseDataState.updateClassifications(classificationIds);
                console.log('[DEBUG] Classifications restored after full reload:', classificationIds);

                this.loading = false;
              },
              error: (error) => {
                console.error('Error loading related cases:', error);
                this.loading = false;
              }
            });
          },
          error: (error) => {
            console.error('Error loading claims:', error);
            this.loading = false;
          }
        });
      },
      error: (error) => {
        this.loading = false;
        console.error('Error loading request:', error);
      }
    });
  }

  createNewRequest() {
    this.loading = true;

    // Clear state for new request
    this.caseDataState.setRequestContext(null);
    this.caseDataState.resetState();

    const dto: CaseRequestCreateDTO = {
      courtId: 1,
      subject: 'طلب تسجيل دعوى جديد',
      evidence: 'سيتم إضافة التفاصيل والأدلة المتعلقة بالقضية لاحقاً.'
    };

    this.caseRegistrationApi.create(dto).subscribe({
      next: (request) => {
        this.requestId = request.id;

        // Set context to the new request ID
        this.caseDataState.setRequestContext(request.id);

        this.requestState.updateRequest(request);
        this.loading = false;

        this.router.navigate(['/case-registration', request.id, 'edit'], { replaceUrl: true });
      },
      error: (error) => {
        this.loading = false;
        console.error('Error creating request:', error);
      }
    });
  }

  updateFromRequest(request: CaseRequestVM) {
    this.currentStatus = request.requestStatusId;
    this.currentStatusName = request.requestStatusName;
    this.plaintiffsCount = request.plaintiffsCount || 0;
    this.defendantsCount = request.defendantsCount || 0;
    this.attachmentsCount = request.attachmentsCount || 0;
    this.hasDeficiencies = request.requestStatusId === 8;

    // Populate metadata bar properties
    this.createdDate = request.createdDate ? new Date(request.createdDate) : null;
    // courtName, registrationMethod, caseType, caseRegistrationNumber, caseRegistrationDate
    // will be populated when backend provides this data

    this.canEdit = this.mode !== 'view' && [1, 8].includes(request.requestStatusId);
    this.canSave = this.canEdit;

    // Initialize CaseDataStateService with request data
    // Note: Do NOT call loadFromRequest() here as it overwrites claims and related cases that are loaded separately via API
    // Instead, update individual fields while preserving claims and related cases that will be loaded by loadRequest()
    this.caseDataState.updateSubject(request.subject || '');
    this.caseDataState.updateEvidence(request.evidence || '');
    this.caseDataState.updateClassifications(request.classificationIds || []);
    this.caseDataState.updateContactInfo(
      request.primaryMobile || '',
      request.secondaryMobile || '',
      request.email || ''
    );
  }

  scrollToSection(sectionId: string) {
    // Update active section
    this.activeSection = sectionId;

    // Update URL with tab query parameter
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: sectionId },
      queryParamsHandling: 'merge',
      fragment: sectionId
    });

    // Close sidebar on mobile after selection
    if (this.isMobileView) {
      this.sidebarOpen = false;
    }
  }

  toggleCaseDataExpanded() {
    this.caseDataExpanded = !this.caseDataExpanded;
  }

  selectCaseDataTab(tabId: string) {
    this.activeCaseDataTab = tabId;
    this.activeSection = 'case-data';

    // Update URL with query params
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: 'case-data', subTab: tabId },
      queryParamsHandling: 'merge'
    });

    // Close sidebar on mobile after selection
    if (this.isMobileView) {
      this.sidebarOpen = false;
    }
  }

  getCaseDataTabCount(tabId: string): number | null {
    switch (tabId) {
      case 'claims':
        return this.caseDataCounts.claims > 0 ? this.caseDataCounts.claims : null;
      case 'related-cases':
        return this.caseDataCounts.relatedCases > 0 ? this.caseDataCounts.relatedCases : null;
      case 'classifications':
        return this.caseDataCounts.classifications > 0 ? this.caseDataCounts.classifications : null;
      case 'attachments':
        return this.caseDataCounts.attachments > 0 ? this.caseDataCounts.attachments : null;
      default:
        return null;
    }
  }

  /**
   * Get the display name of the currently active tab/section
   * When in case-data section with a sub-tab, shows: "بيانات الدعوى - [sub-tab name]"
   */
  getActiveTabName(): string {
    const tabNames: {[key: string]: string} = {
      'defendants': 'المدعى عليهم',
      'case-data': 'بيانات الدعوى',
      'additional-info': 'معلومات إضافية',
      'deficiencies': 'النواقص',
      'completion': 'إنهاء الطلب'
    };

    // If in case-data section, append the active sub-tab name
    if (this.activeSection === 'case-data' && this.activeCaseDataTab) {
      const subTabNames: {[key: string]: string} = {
        'subject-evidence': 'موضوع وأسانيد الدعوى',
        'claims': 'طلبات الدعوى',
        'related-cases': 'الدعاوى المرتبطة',
        'classifications': 'تصنيف الدعوى',
        'contact-info': 'بيانات التواصل',
        'attachments': 'المرفقات'
      };
      const subTabName = subTabNames[this.activeCaseDataTab];
      return subTabName ? `بيانات الدعوى - ${subTabName}` : 'بيانات الدعوى';
    }

    return tabNames[this.activeSection] || 'طلب تسجيل دعوى';
  }

  /**
   * Check if there's a next tab in the case data tabs sequence
   */
  hasNextCaseDataTab(): boolean {
    const tabOrder = ['subject-evidence', 'claims', 'related-cases', 'classifications', 'contact-info', 'attachments'];
    const currentIndex = tabOrder.indexOf(this.activeCaseDataTab);
    return currentIndex >= 0 && currentIndex < tabOrder.length - 1;
  }

  /**
   * Navigate to the next case data tab
   */
  goToNextCaseDataTab(): void {
    const tabOrder = ['subject-evidence', 'claims', 'related-cases', 'classifications', 'contact-info', 'attachments'];
    const currentIndex = tabOrder.indexOf(this.activeCaseDataTab);

    if (currentIndex >= 0 && currentIndex < tabOrder.length - 1) {
      const nextTabId = tabOrder[currentIndex + 1];
      this.selectCaseDataTab(nextTabId);
    }
  }

  setupResponsive() {
    this.checkMobileView();
    window.addEventListener('resize', () => this.checkMobileView());
  }

  checkMobileView() {
    this.isMobileView = window.innerWidth < 768;
  }

  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
  }

  saveRequest() {
    console.log('Save requested from header bar, active section:', this.activeSection);

    // If we're on case-data section, delegate to case-data-container
    if (this.activeSection === 'case-data' && this.caseDataContainer) {
      this.isSaving = true;
      this.saveSuccess = false;

      // Call child component's save method
      this.caseDataContainer.saveAllData();
    } else {
      // For other sections, you can add save logic here if needed
      console.log('Save for section:', this.activeSection);
      this.isSaving = false;
    }
  }

  onSaveComplete(event: { success: boolean; error?: string }) {
    this.isSaving = false;

    if (event.success) {
      this.saveSuccess = true;

      // CRITICAL: Reload request from backend after successful save
      // This ensures UI displays the actual saved data, not cached state
      this.loadRequest();

      // Reset success indicator after 2 seconds
      setTimeout(() => {
        this.saveSuccess = false;
      }, 2000);
    } else {
      console.error('Save failed:', event.error);
    }
  }

  navigateBack() {
    this.router.navigate(['/case-registration']);
  }

  /**
   * Get Arabic status label from enum
   */
  get arabicStatusName(): string {
    return RequestStatusLabels[this.currentStatus as RequestStatus] || 'غير معروف';
  }
}
