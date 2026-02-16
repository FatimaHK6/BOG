import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { CaseRegistrationRequestService, CaseRegistrationRequestListVM, SearchRequestDTO, PagedResult } from '../../../../../core/services/case-registration-request.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LookupsApiService } from '../../../services/lookups-api.service';

interface RequestFilters {
  courtId: number | null;
  submissionMethod: number | null;
  statusIds: number[];
  caseTypeId: number | null;
  requestNumber: string;
  requestDate: Date | null;
}

@Component({
  selector: 'app-request-list',
  templateUrl: './request-list.component.html',
  styleUrls: ['./request-list.component.css']
})
export class RequestListComponent implements OnInit {
  requests: CaseRegistrationRequestListVM[] = [];
  isLoading = true;
  displayedColumns = ['id', 'statusNameAr', 'subjectPreview', 'plaintiffsCount', 'defendantsCount', 'createdDate', 'actions'];
  viewMode: 'table' | 'card' = 'card';
  applyingMethods: { id: number; name: string; nameAr: string }[] = [];

  // Statistics
  totalRequests = 0;
  administrativeCount = 0;
  disciplinaryCount = 0;

  // Filters
  filters: RequestFilters = {
    courtId: null,
    submissionMethod: null,
    statusIds: [],
    caseTypeId: null,
    requestNumber: '',
    requestDate: null
  };

  // Pagination
  currentPage = 1;
  pageSize = 9;
  totalPages = 1;

  constructor(
    private requestService: CaseRegistrationRequestService,
    private router: Router,
    private dialog: MatDialog,
    private notification: NotificationService,
    private lookupsService: LookupsApiService
  ) {}

  ngOnInit(): void {
    this.loadRequests();
    this.lookupsService.getApplyingMethods().subscribe({
      next: (methods) => this.applyingMethods = methods,
      error: (err) => console.error('Error loading applying methods:', err)
    });
  }

  loadRequests(): void {
    this.isLoading = true;

    const requestId = this.filters.requestNumber ? parseInt(this.filters.requestNumber, 10) : null;

    const dto: SearchRequestDTO = {
      requestId: requestId && !isNaN(requestId) ? requestId : null,
      statusId: this.filters.statusIds.length > 0 ? this.filters.statusIds[0] : null,
      courtId: this.filters.courtId,
      applyingMethodId: this.filters.submissionMethod,
      caseTypeId: this.filters.caseTypeId,
      subject: null,
      caseNumber: null,
      createdDateFrom: this.filters.requestDate ? this.filters.requestDate.toISOString() : null,
      createdDateTo: this.filters.requestDate
        ? new Date(new Date(this.filters.requestDate).setHours(23, 59, 59, 999)).toISOString()
        : null,
      submissionDateFrom: null,
      submissionDateTo: null,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: 'CreatedDate',
      sortDirection: 'desc'
    };

    console.log('[Search DTO] applyingMethodId:', dto.applyingMethodId);

    this.requestService.search(dto).subscribe({
      next: (result: PagedResult<CaseRegistrationRequestListVM>) => {
        this.requests = result.items;
        this.totalRequests = result.totalCount;
        this.totalPages = result.totalPages || 1;
        this.calculateStatistics();
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading requests:', error);
        this.isLoading = false;
      }
    });
  }

  calculateStatistics(): void {
    this.administrativeCount = this.requests.filter(r => r.caseTypeId === 1).length;
    this.disciplinaryCount = this.requests.filter(r => r.caseTypeId === 2).length;
  }

  onNewRequest(): void {
    this.router.navigate(['/case-registration', 'create']);
  }

  onEditRequest(request: CaseRegistrationRequestListVM): void {
    this.router.navigate(['/case-registration', request.id, 'edit']);
  }

  onViewRequest(request: CaseRegistrationRequestListVM): void {
    this.router.navigate(['/case-registration', request.id, 'view']);
  }

  onDeleteRequest(request: CaseRegistrationRequestListVM): void {
    const dialogData: ConfirmDialogData = {
      title: 'تأكيد الحذف',
      message: 'هل أنت متأكد من حذف هذا الطلب؟',
      confirmText: 'حذف',
      cancelText: 'إلغاء',
      confirmColor: 'warn',
      icon: 'delete'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.requestService.delete(request.id).subscribe({
          next: () => {
            this.notification.success('تم حذف الطلب بنجاح');
            this.loadRequests();
          },
          error: (error: any) => {
            console.error('Error deleting request:', error);
            this.notification.error('حدث خطأ أثناء حذف الطلب');
          }
        });
      }
    });
  }

  getStatusClass(statusId: number): string {
    switch (statusId) {
      case 1: return 'status-draft';
      case 2: return 'status-new';
      case 3: return 'status-review';
      case 4: return 'status-registered';
      case 5: return 'status-rejected';
      default: return '';
    }
  }

  getStatusName(statusId: number): string {
    switch (statusId) {
      case 1: return 'مسودة';
      case 2: return 'جديد';
      case 3: return 'استكمال نواقص';
      case 4: return 'مقيدة';
      case 5: return 'مرفوضة';
      default: return '';
    }
  }

  removeStatusFilter(statusId: number): void {
    this.filters.statusIds = this.filters.statusIds.filter(id => id !== statusId);
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadRequests();
  }

  onClearFilters(): void {
    this.filters = {
      courtId: null,
      submissionMethod: null,
      statusIds: [],
      caseTypeId: null,
      requestNumber: '',
      requestDate: null
    };
    this.currentPage = 1;
    this.loadRequests();
  }

  // Pagination methods
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    const endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage + 1 < maxPagesToShow) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadRequests();
  }

  onPreviousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadRequests();
    }
  }

  onNextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadRequests();
    }
  }

  toggleView(): void {
    this.viewMode = this.viewMode === 'table' ? 'card' : 'table';
  }
}
