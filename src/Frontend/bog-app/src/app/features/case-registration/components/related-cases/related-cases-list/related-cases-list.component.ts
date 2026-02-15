import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RelatedCaseVM } from '../../../models/related-case.model';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { ConfirmationDialogComponent } from '../../shared/confirmation-dialog/confirmation-dialog.component';
import { RelatedCaseFormDialogComponent } from '../related-case-form-dialog/related-case-form-dialog.component';
import { DeficiencyFormDialogComponent } from '../../deficiencies/deficiency-form-dialog.component';
import { DeficienciesApiService } from '../../../services/deficiencies-api.service';
import { RequestDeficiencyDTO, DeficienciesBatchUpdateDTO } from '../../../models/deficiency.model';
import { Subject } from 'rxjs';
import { takeUntil, map, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-related-cases-list',
  templateUrl: './related-cases-list.component.html',
  styleUrls: ['./related-cases-list.component.scss']
})
export class RelatedCasesListComponent implements OnInit, OnDestroy {
  @Input() requestId!: number;
  @Input() canEdit = false;
  @Input() showValidation = false;
  @Output() countChanged = new EventEmitter<number>();

  relatedCases: RelatedCaseVM[] = [];
  displayedColumns = ['court', 'caseNumber', 'caseYear', 'actions'];
  loading = false;

  private destroy$ = new Subject<void>();

  get hasRelatedCases(): boolean {
    return this.relatedCases.length > 0;
  }

  constructor(
    private caseDataState: CaseDataStateService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private deficienciesApi: DeficienciesApiService
  ) { }

  ngOnInit() {
    this.loadRelatedCases();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadRelatedCases() {
    // Load from state service instead of API
    this.relatedCases = this.caseDataState.getRelatedCases();
    this.countChanged.emit(this.relatedCases.length);

    // Subscribe to state changes
    this.caseDataState.state$
      .pipe(
        map(state => state.relatedCases),
        distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
        takeUntil(this.destroy$)
      )
      .subscribe(relatedCases => {
        this.relatedCases = relatedCases;
        this.countChanged.emit(relatedCases.length);
      });
  }

  openAddRelatedCase() {
    const dialogRef = this.dialog.open(RelatedCaseFormDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: { requestId: this.requestId, mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadRelatedCases();
      }
    });
  }

  editRelatedCase(relatedCase: RelatedCaseVM) {
    const dialogRef = this.dialog.open(RelatedCaseFormDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: { requestId: this.requestId, relatedCase, mode: 'edit' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadRelatedCases();
      }
    });
  }

  deleteRelatedCase(relatedCase: RelatedCaseVM) {
    const caseInfo = `${relatedCase.caseNumber}/${relatedCase.caseYear}`;
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      direction: 'rtl',
      data: {
        title: 'تأكيد الحذف',
        message: `هل أنت متأكد من حذف الدعوى المرتبطة: "${caseInfo}"؟`,
        confirmText: 'حذف',
        cancelText: 'إلغاء',
        confirmColor: 'warn'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.snackBar.open('سيتم حذف الدعوى المرتبطة عند الحفظ', 'إغلاق', { duration: 3000 });
      }
    });
  }

  openDeficiencyDialog() {
    this.openDeficiencyDialogForType(4); // Type 4 = RelatedCases
  }

  private openDeficiencyDialogForType(typeId: number) {
    const dialogRef = this.dialog.open(DeficiencyFormDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: {
        mode: 'create',
        preSelectedTypeId: typeId
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.addDeficiency(result);
      }
    });
  }

  private addDeficiency(newDeficiency: RequestDeficiencyDTO) {
    this.deficienciesApi.getDeficiencies(this.requestId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (currentDeficiencies) => {
          const existingDescriptionIds = currentDeficiencies.map(d => ({
            deficiencyDescriptionId: d.deficiencyDescriptionId
          }));

          const updatedDeficiencies = [...existingDescriptionIds, newDeficiency];

          const dto: DeficienciesBatchUpdateDTO = {
            deficiencies: updatedDeficiencies
          };

          this.deficienciesApi.updateDeficiencies(this.requestId, dto)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.snackBar.open('تم إضافة النقص بنجاح', 'إغلاق', { duration: 3000 });
              },
              error: (error) => {
                console.error('Error adding deficiency:', error);
                this.snackBar.open('فشل في إضافة النقص', 'إغلاق', { duration: 3000 });
              }
            });
        },
        error: (error) => {
          console.error('Error fetching deficiencies:', error);
          this.snackBar.open('فشل في جلب النواقص الحالية', 'إغلاق', { duration: 3000 });
        }
      });
  }
}
