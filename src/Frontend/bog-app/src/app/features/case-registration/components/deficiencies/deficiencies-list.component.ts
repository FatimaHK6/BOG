import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeficiencyVM, RequestDeficiencyDTO, DeficienciesBatchUpdateDTO } from '../../models/deficiency.model';
import { DeficienciesApiService } from '../../services/deficiencies-api.service';
import { ConfirmationDialogComponent } from '../shared/confirmation-dialog/confirmation-dialog.component';
import { DeficiencyFormDialogComponent } from './deficiency-form-dialog.component';

@Component({
  selector: 'app-deficiencies-list',
  templateUrl: './deficiencies-list.component.html',
  styleUrls: ['./deficiencies-list.component.scss']
})
export class DeficienciesListComponent implements OnInit, OnDestroy {
  @Input() requestId!: number;
  @Input() canEdit = false;
  @Output() countChanged = new EventEmitter<number>();

  deficiencies: DeficiencyVM[] = [];
  loading = false;
  autoRejectDays = 30;
  displayedColumns: string[] = ['type', 'description', 'actions'];

  private destroy$ = new Subject<void>();

  constructor(
    private deficienciesApi: DeficienciesApiService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit() {
    this.loadDeficiencies();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDeficiencies() {
    if (!this.requestId) {
      return;
    }

    this.loading = true;
    this.deficienciesApi.getDeficiencies(this.requestId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.deficiencies = data;
          this.countChanged.emit(this.deficiencies.length);
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading deficiencies:', error);
          this.loading = false;
          this.snackBar.open('فشل في تحميل النواقص', 'إغلاق', { duration: 3000 });
        }
      });
  }

  deleteDeficiency(deficiency: DeficiencyVM) {
    const preview = deficiency.descriptionAr.substring(0, 50) +
                   (deficiency.descriptionAr.length > 50 ? '...' : '');
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      direction: 'rtl',
      data: {
        title: 'تأكيد الحذف',
        message: `هل أنت متأكد من حذف النقص: "${preview}"؟`,
        confirmText: 'حذف',
        cancelText: 'إلغاء',
        confirmColor: 'warn'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        // Remove from local array and update
        this.deficiencies = this.deficiencies.filter(d => d.id !== deficiency.id);
        this.countChanged.emit(this.deficiencies.length);
        this.snackBar.open('سيتم حذف النقص عند الحفظ', 'إغلاق', { duration: 3000 });
      }
    });
  }

  openAddDeficiency() {
    const dialogRef = this.dialog.open(DeficiencyFormDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.addDeficiency(result);
      }
    });
  }

  private addDeficiency(newDeficiency: RequestDeficiencyDTO) {
    // Get current deficiencies and add the new one
    const currentDeficiencies = this.deficiencies.map(d => ({
      deficiencyDescriptionId: d.deficiencyDescriptionId
    }));

    const updatedDeficiencies = [...currentDeficiencies, newDeficiency];

    const dto: DeficienciesBatchUpdateDTO = {
      deficiencies: updatedDeficiencies
    };

    this.loading = true;
    this.deficienciesApi.updateDeficiencies(this.requestId, dto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.deficiencies = result;
          this.countChanged.emit(this.deficiencies.length);
          this.loading = false;
          this.snackBar.open('تم إضافة النقص بنجاح', 'إغلاق', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error adding deficiency:', error);
          this.loading = false;
          this.snackBar.open('فشل في إضافة النقص', 'إغلاق', { duration: 3000 });
        }
      });
  }

}
