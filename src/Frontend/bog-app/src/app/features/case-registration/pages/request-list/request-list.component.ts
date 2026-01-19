import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CaseRegistrationApiService } from '../../services/case-registration-api.service';
import { CaseRequestVM, PagedResult } from '../../models/case-request.model';

@Component({
  selector: 'app-request-list',
  template: `
    <div class="request-list" dir="rtl">
      <div class="page-header">
        <h1>قائمة طلبات التسجيل</h1>
        <button mat-raised-button color="primary" (click)="createNewRequest()">
          <mat-icon>add</mat-icon>
          طلب جديد
        </button>
      </div>

      <!-- Search Filters -->
      <mat-card class="filters-card">
        <form [formGroup]="searchForm" class="search-form">
          <mat-form-field appearance="outline">
            <mat-label>رقم الطلب</mat-label>
            <input matInput formControlName="requestNumber">
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>الحالة</mat-label>
            <mat-select
              formControlName="status"
              panelClass="dropdown-panel"
              disableOptionCentering>
              <mat-option [value]="null">-- الكل --</mat-option>
              <mat-option [value]="1">مسودة</mat-option>
              <mat-option [value]="3">جديد</mat-option>
              <mat-option [value]="6">مسجل</mat-option>
            </mat-select>
          </mat-form-field>

          <button mat-icon-button (click)="search()" title="بحث">
            <mat-icon>search</mat-icon>
          </button>
        </form>
      </mat-card>

      <!-- Requests Table -->
      <mat-card class="table-card">
        <div class="table-container" *ngIf="!loading">
          <table mat-table [dataSource]="requests" class="requests-table">
            <ng-container matColumnDef="requestNumber">
              <th mat-header-cell *matHeaderCellDef>رقم الطلب</th>
              <td mat-cell *matCellDef="let request">{{ request.requestNumber }}</td>
            </ng-container>

            <ng-container matColumnDef="subject">
              <th mat-header-cell *matHeaderCellDef>الموضوع</th>
              <td mat-cell *matCellDef="let request">{{ request.subject | slice:0:50 }}</td>
            </ng-container>

            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>الحالة</th>
              <td mat-cell *matCellDef="let request">
                <span class="status-badge" [class]="'status-' + request.requestStatusId">
                  {{ request.requestStatusName }}
                </span>
              </td>
            </ng-container>

            <ng-container matColumnDef="date">
              <th mat-header-cell *matHeaderCellDef>التاريخ</th>
              <td mat-cell *matCellDef="let request">
                {{ request.createdDate | date:'yyyy-MM-dd' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>الإجراءات</th>
              <td mat-cell *matCellDef="let request">
                <button mat-icon-button (click)="viewRequest(request.id)" title="عرض">
                  <mat-icon>visibility</mat-icon>
                </button>
                <button mat-icon-button (click)="editRequest(request.id)" title="تعديل">
                  <mat-icon>edit</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>

          <!-- Pagination -->
          <mat-paginator
            [length]="totalCount"
            [pageSize]="pageSize"
            [pageSizeOptions]="[10, 25, 50]"
            (page)="onPageChange($event)">
          </mat-paginator>
        </div>

        <!-- Loading State -->
        <div class="loading-state" *ngIf="loading">
          <mat-spinner></mat-spinner>
        </div>

        <!-- Empty State -->
        <div class="empty-state" *ngIf="!loading && !requests.length">
          <mat-icon>inbox</mat-icon>
          <p>لا توجد طلبات</p>
        </div>
      </mat-card>
    </div>
  `,
  styles: [`
    .request-list { padding: 24px; direction: rtl; }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }
    .page-header h1 { margin: 0; color: #1B5E20; }
    .filters-card, .table-card { margin-bottom: 16px; overflow: visible; }
    .search-form { display: flex; gap: 16px; padding: 16px; overflow: visible; }
    .search-form mat-form-field { flex: 1; overflow: visible; }
    .table-container { padding: 16px; }
    .requests-table { width: 100%; }
    .status-badge {
      display: inline-block;
      padding: 4px 12px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 600;
    }
    .status-1 { background: #9E9E9E; color: white; }
    .status-3 { background: #2196F3; color: white; }
    .status-6 { background: #4CAF50; color: white; }
    .loading-state, .empty-state { text-align: center; padding: 40px; }
    .empty-state mat-icon { font-size: 64px; color: #BDBDBD; }
  `]
})
export class RequestListComponent implements OnInit {
  requests: CaseRequestVM[] = [];
  displayedColumns = ['requestNumber', 'subject', 'status', 'date', 'actions'];
  searchForm!: FormGroup;
  loading = false;

  totalCount = 0;
  pageSize = 10;
  pageNumber = 1;

  constructor(
    private fb: FormBuilder,
    private caseApi: CaseRegistrationApiService,
    private router: Router,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit() {
    this.searchForm = this.fb.group({
      requestNumber: [''],
      status: [null]
    });

    this.loadRequests();
  }

  loadRequests() {
    this.loading = true;
    const params = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      ...this.searchForm.value
    };

    this.caseApi.getRequests(params).subscribe({
      next: (result: PagedResult<CaseRequestVM>) => {
        console.log('API Response:', result);
        this.requests = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load requests:', error);
        this.loading = false;
        this.snackBar.open('خطأ في تحميل الطلبات', 'إغلاق', { duration: 3000 });
      }
    });
  }

  search() {
    this.pageNumber = 1;
    this.loadRequests();
  }

  onPageChange(event: PageEvent) {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadRequests();
  }

  createNewRequest() {
    this.router.navigate(['/case-registration/create']);
  }

  viewRequest(id: number) {
    this.router.navigate(['/case-registration', id, 'view']);
  }

  editRequest(id: number) {
    this.router.navigate(['/case-registration', id, 'edit']);
  }
}
