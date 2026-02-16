import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { CaseRegistrationRequestService, CaseRegistrationRequestListVM } from '../../../../../core/services/case-registration-request.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { STATUS_DRAFT, STATUS_NEW, STATUS_UNDER_REVIEW, STATUS_REGISTERED, STATUS_REJECTED, STATUS_PENDING_COMPLETION, STATUS_ON_JUDGE_DESK, STATUS_COMPLETED, STATUS_AUTO_REJECTED, STATUS_CANCELLED } from '../../../models/status-constants';

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
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading = true;
    this.requestService.getAll().subscribe({
      next: (requests: CaseRegistrationRequestListVM[]) => {
        this.requests = requests;
        this.calculateStatistics();
        this.totalPages = Math.ceil(this.requests.length / this.pageSize) || 1;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading requests:', error);
        this.isLoading = false;
      }
    });
  }

  calculateStatistics(): void {
    this.totalRequests = this.requests.length;
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
      case STATUS_DRAFT: return 'status-draft';
      case STATUS_NEW: return 'status-new';
      case STATUS_UNDER_REVIEW: return 'status-review';
      case STATUS_REGISTERED: return 'status-registered';
      case STATUS_REJECTED: return 'status-rejected';
      case STATUS_PENDING_COMPLETION: return 'status-pending';
      case STATUS_ON_JUDGE_DESK: return 'status-judge';
      case STATUS_COMPLETED: return 'status-completed';
      case STATUS_AUTO_REJECTED: return 'status-auto-rejected';
      case STATUS_CANCELLED: return 'status-cancelled';
      default: return '';
    }
  }

  getStatusName(statusId: number): string {
    // Return Arabic status names to match database seed
    switch (statusId) {
      case STATUS_DRAFT: return 'مسودة';
      case STATUS_NEW: return 'جديد';
      case STATUS_UNDER_REVIEW: return 'قيد المراجعة';
      case STATUS_REGISTERED: return 'مقيد';
      case STATUS_REJECTED: return 'مرفوض';
      case STATUS_PENDING_COMPLETION: return 'بانتظار الاستكمال';
      case STATUS_ON_JUDGE_DESK: return 'على مكتب القاضي';
      case STATUS_COMPLETED: return 'مكتمل';
      case STATUS_AUTO_REJECTED: return 'مرفوض تلقائياً';
      case STATUS_CANCELLED: return 'ملغي';
      default: return '';
    }
  }

  /**
   * Checks if a request is in an editable status.
   * Editable statuses: Draft (1), New (2), PendingCompletion (6)
   */
  isEditable(request: CaseRegistrationRequestListVM): boolean {
    return [STATUS_DRAFT, STATUS_NEW, STATUS_PENDING_COMPLETION].includes(request.requestStatusId);
  }

  removeStatusFilter(statusId: number): void {
    this.filters.statusIds = this.filters.statusIds.filter(id => id !== statusId);
  }

  onSearch(): void {
    // TODO: Implement API search with filters
    console.log('Searching with filters:', this.filters);
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
    // TODO: Implement pagination API call
  }

  onPreviousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  onNextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  toggleView(): void {
    this.viewMode = this.viewMode === 'table' ? 'card' : 'table';
  }
}
