import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ClaimVM } from '../../../models/claim.model';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { ConfirmationDialogComponent } from '../../shared/confirmation-dialog/confirmation-dialog.component';
import { ClaimFormDialogComponent } from '../claim-form-dialog/claim-form-dialog.component';
import { DeficiencyFormDialogComponent } from '../../deficiencies/deficiency-form-dialog.component';
import { DeficienciesApiService } from '../../../services/deficiencies-api.service';
import { RequestDeficiencyDTO, DeficienciesBatchUpdateDTO } from '../../../models/deficiency.model';
import { Subject } from 'rxjs';
import { takeUntil, map, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-claims-list',
  templateUrl: './claims-list.component.html',
  styleUrls: ['./claims-list.component.scss']
})
export class ClaimsListComponent implements OnInit, OnDestroy {
  @Input() requestId!: number;
  @Input() canEdit = false;
  @Input() showValidation = false;
  @Output() countChanged = new EventEmitter<number>();

  claims: ClaimVM[] = [];
  loading = false;

  private destroy$ = new Subject<void>();

  constructor(
    private caseDataState: CaseDataStateService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private deficienciesApi: DeficienciesApiService
  ) { }

  ngOnInit() {
    this.loadClaims();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClaims() {
    // Load from state service instead of API
    this.claims = this.caseDataState.getClaims();
    this.countChanged.emit(this.claims.length);

    // Subscribe to state changes
    this.caseDataState.state$
      .pipe(
        map(state => state.claims),
        distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
        takeUntil(this.destroy$)
      )
      .subscribe(claims => {
        this.claims = claims;
        this.countChanged.emit(claims.length);
      });
  }

  openAddClaim() {
    const dialogRef = this.dialog.open(ClaimFormDialogComponent, {
      width: '800px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: { requestId: this.requestId, mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadClaims();
      }
    });
  }

  editClaim(claim: ClaimVM) {
    const dialogRef = this.dialog.open(ClaimFormDialogComponent, {
      width: '800px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: { requestId: this.requestId, claim, mode: 'edit' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadClaims();
      }
    });
  }

  deleteClaim(claim: ClaimVM) {
    const preview = claim.claimText.substring(0, 50) + (claim.claimText.length > 50 ? '...' : '');
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      direction: 'rtl',
      data: {
        title: 'تأكيد الحذف',
        message: `هل أنت متأكد من حذف طلب الدعوى: "${preview}"؟`,
        confirmText: 'حذف',
        cancelText: 'إلغاء',
        confirmColor: 'warn'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        // In a real implementation, this would call delete on the API
        // For now, we just show a message
        this.snackBar.open('سيتم حذف طلب الدعوى عند الحفظ', 'إغلاق', { duration: 3000 });
      }
    });
  }

  getCharacterCount(text: string): number {
    return text.length;
  }

  getCharacterPercentage(text: string): number {
    return Math.round((text.length / 2000) * 100);
  }

  openDeficiencyDialog() {
    this.openDeficiencyDialogForType(2); // Type 2 = CaseClaims
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
