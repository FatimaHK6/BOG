import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CaseRegistrationApiService } from '../../services/case-registration-api.service';
import { RequestStateService } from '../../services/request-state.service';
import { CaseRequestVM, CaseRequestCreateDTO } from '../../models/case-request.model';

@Component({
  selector: 'app-request-details',
  templateUrl: './request-details.component.html',
  styleUrls: ['./request-details.component.scss']
})
export class RequestDetailsComponent implements OnInit, OnDestroy {
  requestId: number = 0;
  mode: 'create' | 'edit' | 'view' = 'create';
  pageTitle = '';
  loading = true;
  showValidation = false;

  currentStatus = 1;
  currentStatusName = 'مسودة';
  activeSection = 'defendants';

  plaintiffsCount = 0;
  defendantsCount = 0;
  attachmentsCount = 0;
  deficienciesCount = 0;
  hasDeficiencies = false;

  canEdit = true;
  canSave = true;

  isMobileView = false;
  sidebarOpen = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private requestState: RequestStateService,
    private caseRegistrationApi: CaseRegistrationApiService
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
    });

    this.route.params.subscribe(params => {
      if (params['id']) {
        this.requestId = +params['id'];
        this.loadRequest();
      } else {
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

    this.setupResponsive();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadRequest() {
    this.loading = true;
    this.requestState.loadRequest(this.requestId).subscribe({
      next: () => {
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        console.error('Error loading request:', error);
      }
    });
  }

  createNewRequest() {
    this.loading = true;
    const dto: CaseRequestCreateDTO = {
      courtId: 1,
      subject: 'طلب تسجيل دعوى جديد',
      evidence: 'سيتم إضافة التفاصيل والأدلة المتعلقة بالقضية لاحقاً.'
    };

    this.caseRegistrationApi.create(dto).subscribe({
      next: (request) => {
        this.requestId = request.id;
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

    this.canEdit = this.mode !== 'view' && [1, 8].includes(request.requestStatusId);
    this.canSave = this.canEdit;
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
    console.log('Manual save requested');
  }

  navigateBack() {
    this.router.navigate(['/case-registration']);
  }
}
