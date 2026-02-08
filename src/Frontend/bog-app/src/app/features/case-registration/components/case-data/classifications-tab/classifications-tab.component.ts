import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { ClassificationsApiService } from '../../../services/classifications-api.service';
import { ClassificationVM } from '../../../models/classification.model';
import { ClassificationSelectionDialogComponent } from './classification-selection-dialog/classification-selection-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-classifications-tab',
  templateUrl: './classifications-tab.component.html',
  styleUrls: ['./classifications-tab.component.scss']
})
export class ClassificationsTabComponent implements OnInit, OnDestroy {
  @Input() canEdit = false;
  @Output() countChanged = new EventEmitter<number>();

  selectedClassifications: ClassificationVM[] = [];
  loading = false;

  // Table column configuration
  displayedColumns: string[] = ['level1', 'level2', 'level3', 'level4', 'actions'];

  private destroy$ = new Subject<void>();

  constructor(
    private caseDataState: CaseDataStateService,
    private classificationsApi: ClassificationsApiService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit() {
    // Subscribe to state changes ONCE, unconditionally
    // This ensures the component always reacts to classification updates
    this.caseDataState.state$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.loadClassificationsFromIds(state.classificationIds);
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load classifications from given IDs
   * Called reactively whenever state changes
   */
  private loadClassificationsFromIds(classificationIds: number[]) {
    // Handle empty state
    if (classificationIds.length === 0) {
      console.log('[DEBUG] Classifications cleared - setting to empty array');
      this.selectedClassifications = [];
      this.countChanged.emit(0);
      this.loading = false;
      return;
    }

    // Load classifications from API
    console.log('[DEBUG] Loading classifications with IDs:', classificationIds);
    this.loading = true;

    this.classificationsApi.getClassificationsByIds(classificationIds)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (classifications) => {
          console.log('[DEBUG] Successfully loaded classifications:', classifications);
          this.selectedClassifications = classifications;
          this.countChanged.emit(classifications.length);
          this.loading = false;
        },
        error: (error) => {
          this.loading = false;
          console.error('[DEBUG] Error loading classifications:', error);
          this.snackBar.open('خطأ في تحميل التصنيفات', 'إغلاق', { duration: 3000 });
        }
      });
  }

  /**
   * Open classification selection dialog
   */
  openSelectionDialog() {
    const dialogRef = this.dialog.open(ClassificationSelectionDialogComponent, {
      width: '800px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: {
        selectedIds: this.caseDataState.getClassificationIds()
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Update state with selected classification IDs
        this.caseDataState.updateClassifications(result);
        this.snackBar.open('تم تحديث التصنيفات بنجاح', 'إغلاق', { duration: 3000 });
      }
    });
  }

  /**
   * Remove a classification
   */
  removeClassification(classification: ClassificationVM) {
    const displayText = this.classificationsApi.getClassificationDisplay(classification);

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      direction: 'rtl',
      data: {
        title: 'تأكيد الحذف',
        message: `هل أنت متأكد من حذف التصنيف: "${displayText}"؟`,
        confirmText: 'حذف',
        cancelText: 'إلغاء',
        confirmColor: 'warn'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        const currentIds = this.caseDataState.getClassificationIds();
        const updatedIds = currentIds.filter(id => id !== classification.id);
        this.caseDataState.updateClassifications(updatedIds);
        this.snackBar.open('تم حذف التصنيف بنجاح', 'إغلاق', { duration: 3000 });
      }
    });
  }

  /**
   * Get formatted display text for classification
   */
  getClassificationDisplay(classification: ClassificationVM): string {
    return this.classificationsApi.getClassificationDisplay(classification);
  }
}
