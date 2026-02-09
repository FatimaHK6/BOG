import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DefendantListVM } from '../../../../../core/models/defendant.model';
import { DefendantService } from '../../../../../core/services/defendant.service';
import { CaseRegistrationRequestService } from '../../../../../core/services/case-registration-request.service';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

interface DefendantType {
  id: number;
  nameAr: string;
}

@Component({
  selector: 'app-defendant-list',
  templateUrl: './defendant-list.component.html',
  styleUrls: ['./defendant-list.component.scss']
})
export class DefendantListComponent implements OnInit, AfterViewInit {
  requestId: number = 0;
  defendants: DefendantListVM[] = [];
  dataSource = new MatTableDataSource<DefendantListVM>([]);
  isLoading = false;
  isSaving = false;

  // Defendant types matching database DefendantTypes table
  defendantTypes: DefendantType[] = [
    { id: 1, nameAr: 'فرد' },
    { id: 2, nameAr: 'شركة مسجلة في المملكة' },
    { id: 3, nameAr: 'جهة حكومية' },
    { id: 4, nameAr: 'شركة غير مسجلة في المملكة' },
    { id: 5, nameAr: 'صاحب مؤسسة' },
    { id: 6, nameAr: 'جمعية/مؤسسة أهلية' },
    { id: 7, nameAr: 'وقف' }
  ];

  @ViewChild('paginator') paginator!: MatPaginator;

  displayedColumns = [
    'defendantType',
    'displayName',
    'actions'
  ];

  constructor(
    private defendantService: DefendantService,
    private requestService: CaseRegistrationRequestService,
    private router: Router,
    private route: ActivatedRoute,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const newRequestId = params['requestId'] ? +params['requestId'] : 0;
      if (newRequestId !== this.requestId) {
        this.requestId = newRequestId;
        if (this.requestId > 0) {
          this.loadDefendants();
        } else {
          this.defendants = [];
          this.dataSource.data = [];
        }
      }
    });
  }

  ngAfterViewInit(): void {
    // Paginator is set after data loads due to *ngIf
  }

  onSaveAsDraft(): void {
    if (this.requestId <= 0) {
      this.snackBar.open('لا يوجد طلب محدد', 'إغلاق', { duration: 3000 });
      return;
    }

    this.isSaving = true;
    this.requestService.update(this.requestId, { saveAsDraft: true }).subscribe({
      next: () => {
        this.router.navigate(['/case-registration/requests']).then(() => {
          this.isSaving = false;
          this.snackBar.open('تم حفظ الطلب كمسودة', 'إغلاق', { duration: 3000 });
        });
      },
      error: (error) => {
        console.error('Error saving draft:', error);
        this.isSaving = false;
        this.snackBar.open('حدث خطأ أثناء حفظ المسودة', 'إغلاق', { duration: 3000 });
      }
    });
  }

  private setPaginator(): void {
    setTimeout(() => {
      if (this.paginator) {
        this.dataSource.paginator = this.paginator;
      }
    });
  }

  loadDefendants(): void {
    if (this.requestId <= 0) return;

    this.isLoading = true;
    this.defendantService.getDefendants(this.requestId).subscribe({
      next: (data) => {
        this.defendants = data;
        this.dataSource.data = this.defendants;
        this.isLoading = false;
        this.setPaginator();
      },
      error: (err) => {
        console.error('Error loading defendants:', err);
        this.isLoading = false;
        this.defendants = [];
        this.dataSource.data = [];
      }
    });
  }

  onSelectDefendantType(type: DefendantType): void {
    // Navigate to add defendant form based on type
    const typeRoutes: { [key: number]: string } = {
      1: 'individual',             // فرد
      2: 'registered-company',     // شركة مسجلة في المملكة
      3: 'government-agency',      // جهة حكومية
      4: 'unregistered-company',   // شركة غير مسجلة في المملكة
      5: 'business-owner',         // صاحب مؤسسة
      6: 'ngo',                    // جمعية/مؤسسة أهلية
      7: 'waqf'                    // وقف
    };

    const route = typeRoutes[type.id];
    if (route) {
      this.router.navigate(['/case-registration/defendants/add', route], {
        queryParams: { requestId: this.requestId }
      });
    } else {
      console.log('Form not implemented for type:', type.id, type.nameAr);
    }
  }

  onViewDefendant(defendant: DefendantListVM): void {
    // Navigate to view defendant based on type
    const typeRoutes: { [key: number]: string } = {
      1: 'individual',             // فرد
      2: 'registered-company',     // شركة مسجلة في المملكة
      3: 'government-agency',      // جهة حكومية
      4: 'unregistered-company',   // شركة غير مسجلة في المملكة
      5: 'business-owner',         // صاحب مؤسسة
      6: 'ngo',                    // جمعية/مؤسسة أهلية
      7: 'waqf'                    // وقف
    };

    const route = typeRoutes[defendant.defendantTypeId];
    if (route) {
      this.router.navigate(['/case-registration/defendants/view', route, defendant.id], {
        queryParams: { requestId: this.requestId }
      });
    } else {
      this.snackBar.open('عرض هذا النوع غير متاح حالياً', 'إغلاق', { duration: 3000 });
    }
  }

  onEditDefendant(defendant: DefendantListVM): void {
    // Navigate to edit defendant based on type
    const typeRoutes: { [key: number]: string } = {
      1: 'individual',             // فرد
      2: 'registered-company',     // شركة مسجلة في المملكة
      3: 'government-agency',      // جهة حكومية
      4: 'unregistered-company',   // شركة غير مسجلة في المملكة
      5: 'business-owner',         // صاحب مؤسسة
      6: 'ngo',                    // جمعية/مؤسسة أهلية
      7: 'waqf'                    // وقف
    };

    const route = typeRoutes[defendant.defendantTypeId];
    if (route) {
      this.router.navigate(['/case-registration/defendants/edit', route, defendant.id], {
        queryParams: { requestId: this.requestId }
      });
    } else {
      this.snackBar.open('تعديل هذا النوع غير متاح حالياً', 'إغلاق', { duration: 3000 });
    }
  }

  onDeleteDefendant(defendant: DefendantListVM): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'تأكيد الحذف',
        message: `هل أنت متأكد من حذف المدعى عليه "${defendant.displayName}"؟`,
        confirmText: 'حذف',
        cancelText: 'إلغاء'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.defendantService.deleteDefendant(defendant.id).subscribe({
          next: () => {
            this.snackBar.open('تم حذف المدعى عليه بنجاح', 'إغلاق', { duration: 3000 });
            this.loadDefendants();
          },
          error: (err) => {
            console.error('Error deleting defendant:', err);
            this.snackBar.open('حدث خطأ أثناء حذف المدعى عليه', 'إغلاق', { duration: 3000 });
          }
        });
      }
    });
  }
}
