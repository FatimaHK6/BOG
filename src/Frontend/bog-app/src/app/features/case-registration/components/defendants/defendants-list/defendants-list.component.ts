import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DefendantVM } from '../../../models/defendant.model';
import { DefendantApiService } from '../../../services/defendant-api.service';
import { RequestStateService } from '../../../services/request-state.service';
import { DefendantFormDialogComponent } from '../defendant-form-dialog/defendant-form-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-defendants-list',
  templateUrl: './defendants-list.component.html',
  styleUrls: ['./defendants-list.component.scss']
})
export class DefendantsListComponent implements OnInit {
  @Input() requestId!: number;
  @Input() canEdit = false;
  @Input() showValidation = false;

  defendants: DefendantVM[] = [];
  displayedColumns = ['type', 'name', 'identity', 'address', 'actions'];
  loading = false;

  constructor(
    private defendantApi: DefendantApiService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private requestState: RequestStateService
  ) { }

  ngOnInit() {
    this.loadDefendants();
  }

  loadDefendants() {
    this.loading = true;
    this.defendantApi.getDefendants(this.requestId).subscribe({
      next: (defendants) => {
        this.defendants = defendants;
        this.loading = false;

        const current = this.requestState.getCurrentRequest();
        if (current) {
          current.defendantsCount = defendants.length;
          this.requestState.updateRequest(current);
        }
      },
      error: (error) => {
        this.loading = false;
        this.snackBar.open('خطأ في تحميل المدعى عليهم', 'إغلاق', { duration: 3000 });
        console.error('Error loading defendants:', error);
      }
    });
  }

  openAddDefendant() {
    const dialogRef = this.dialog.open(DefendantFormDialogComponent, {
      width: '800px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: { requestId: this.requestId, mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadDefendants();
      }
    });
  }

  editDefendant(defendant: DefendantVM) {
    const dialogRef = this.dialog.open(DefendantFormDialogComponent, {
      width: '800px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: { requestId: this.requestId, defendant, mode: 'edit' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadDefendants();
      }
    });
  }

  viewDefendant(defendant: DefendantVM) {
    this.dialog.open(DefendantFormDialogComponent, {
      width: '800px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: { requestId: this.requestId, defendant, mode: 'view' }
    });
  }

  deleteDefendant(defendant: DefendantVM) {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      direction: 'rtl',
      data: {
        title: 'تأكيد الحذف',
        message: `هل أنت متأكد من حذف المدعى عليه "${defendant.fullName}"؟`,
        confirmText: 'حذف',
        cancelText: 'إلغاء',
        confirmColor: 'warn'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.defendantApi.delete(defendant.id).subscribe({
          next: () => {
            this.snackBar.open('تم حذف المدعى عليه بنجاح', 'إغلاق', { duration: 3000 });
            this.loadDefendants();
          },
          error: (error) => {
            this.snackBar.open('خطأ في حذف المدعى عليه', 'إغلاق', { duration: 3000 });
            console.error('Error deleting defendant:', error);
          }
        });
      }
    });
  }
}
