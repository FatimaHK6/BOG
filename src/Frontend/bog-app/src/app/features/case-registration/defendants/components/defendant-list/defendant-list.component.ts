import { Component, OnInit, ViewChild, AfterViewInit, Input } from '@angular/core';
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
  @Input() requestId: number = 0;
  @Input() canEdit: boolean = true;
  @Input() showValidation: boolean = false;

  // Inline form state management
  showForm: boolean = false;
  formMode: 'add' | 'edit' | 'view' = 'add';
  selectedDefendantType: number | null = null;
  selectedDefendantId: number | null = null;

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
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    // Load defendants if requestId is provided
    if (this.requestId > 0) {
      this.loadDefendants();
    }
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
        this.isSaving = false;
        this.snackBar.open('تم حفظ الطلب كمسودة', 'إغلاق', { duration: 3000 });
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
    // Show form inline
    this.selectedDefendantType = type.id;
    this.formMode = 'add';
    this.selectedDefendantId = null;
    this.showForm = true;
  }

  onViewDefendant(defendant: DefendantListVM): void {
    // Show form inline in view mode
    this.selectedDefendantType = defendant.defendantTypeId;
    this.formMode = 'view';
    this.selectedDefendantId = defendant.id;
    this.showForm = true;
  }

  onEditDefendant(defendant: DefendantListVM): void {
    // Show form inline in edit mode
    this.selectedDefendantType = defendant.defendantTypeId;
    this.formMode = 'edit';
    this.selectedDefendantId = defendant.id;
    this.showForm = true;
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

  onFormSaved(): void {
    this.showForm = false;
    this.loadDefendants();
  }

  onFormCancelled(): void {
    this.showForm = false;
  }
}
