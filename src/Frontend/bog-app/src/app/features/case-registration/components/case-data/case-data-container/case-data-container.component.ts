import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { CaseRegistrationApiService } from '../../../services/case-registration-api.service';
import { ClaimsApiService } from '../../../services/claims-api.service';
import { RelatedCaseApiService } from '../../../services/related-case-api.service';
import { MatSnackBar } from '@angular/material/snack-bar';


@Component({
  selector: 'app-case-data-container',
  templateUrl: './case-data-container.component.html',
  styleUrls: ['./case-data-container.component.scss']
})
export class CaseDataContainerComponent implements OnInit, OnDestroy, OnChanges {
  @Input() requestId!: number;
  @Input() canEdit = false;
  @Input() activeTabId: string = 'subject-evidence';
  @Output() saveComplete = new EventEmitter<{ success: boolean; error?: string }>();

  // Saving state
  isSaving = false;
  saveSuccess = false;
  saveError: string = '';

  // Counts for badge display
  claimsCount = 0;
  relatedCasesCount = 0;
  classificationsCount = 0;

  private destroy$ = new Subject<void>();

  constructor(
    private caseDataState: CaseDataStateService,
    private caseRegistrationApi: CaseRegistrationApiService,
    private claimsApi: ClaimsApiService,
    private relatedCaseApi: RelatedCaseApiService,
    private router: Router,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit() {
    if (!this.requestId) {
      console.error('requestId is required for CaseDataContainerComponent');
      return;
    }

    // Subscribe to state changes to update counts
    this.caseDataState.state$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.claimsCount = state.claims?.length || 0;
        this.relatedCasesCount = state.relatedCases?.length || 0;
        this.classificationsCount = state.classificationIds?.length || 0;
      });
  }

  ngOnChanges(changes: SimpleChanges) {
    // Handle any necessary changes when inputs change
    if (changes['activeTabId'] && !changes['activeTabId'].firstChange) {
      // Tab ID has changed, any necessary logic here
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Save all data from all tabs
   */
  saveAllData() {
    if (!this.canEdit) {
      this.snackBar.open('لا يمكنك حفظ البيانات في وضع العرض', 'إغلاق', { duration: 3000 });
      this.saveComplete.emit({ success: false, error: 'لا يمكنك حفظ البيانات في وضع العرض' });
      return;
    }

    this.isSaving = true;
    this.saveSuccess = false;
    this.saveError = '';

    // Get current state
    const state = this.caseDataState.getAllData();

    // Create payload for main request data (WITHOUT related cases)
    const requestPayload = {
      subject: state.subject,
      evidence: state.evidence,
      classificationIds: state.classificationIds,
      primaryMobile: state.primaryMobile,
      secondaryMobile: state.secondaryMobile,
      email: state.email
    };

    // Create payload for claims
    const claimsPayload = state.claims.map(c => ({ claimText: c.claimText }));

    // Create payload for related cases
    const relatedCasesPayload = state.relatedCases.map(rc => ({
      courtId: rc.courtId,
      caseNumber: rc.caseNumber,
      caseYear: rc.caseYear
    }));

    // Save main request data first
    console.log('[DEBUG] Saving classifications:', requestPayload.classificationIds);
    console.log('[DEBUG] Full request payload:', requestPayload);
    this.caseRegistrationApi.update(this.requestId, requestPayload).subscribe({
      next: (response) => {
        console.log('[DEBUG] Save response classificationIds:', response.classificationIds);
        console.log('[DEBUG] Full save response:', response);

        // CRITICAL: Update state from response to ensure UI reflects saved data
        const savedClassifications = response.classificationIds || [];
        this.caseDataState.updateClassifications(savedClassifications);
        console.log('[DEBUG] State updated with classifications:', savedClassifications);

        // Then save claims using dedicated API
        this.claimsApi.updateClaims(this.requestId, claimsPayload).subscribe({
          next: () => {
            // Then save related cases using dedicated API
            this.relatedCaseApi.updateRelatedCases(this.requestId, { relatedCases: relatedCasesPayload }).subscribe({
              next: () => {
                this.isSaving = false;
                this.saveSuccess = true;

                // Show success message
                this.snackBar.open('تم حفظ البيانات بنجاح', 'إغلاق', { duration: 3000 });

                // Emit save complete event
                this.saveComplete.emit({ success: true });

                // Reset success indicator after 2 seconds
                setTimeout(() => {
                  this.saveSuccess = false;
                }, 2000);
              },
              error: (error: any) => {
                this.isSaving = false;
                this.saveError = 'فشل في حفظ الدعاوى المرتبطة';
                console.error('Error saving related cases:', error);
                this.snackBar.open('فشل في حفظ الدعاوى المرتبطة', 'إغلاق', { duration: 5000 });
                this.saveComplete.emit({ success: false, error: this.saveError || undefined });
              }
            });
          },
          error: (error: any) => {
            this.isSaving = false;
            this.saveError = 'فشل في حفظ الطلبات';
            console.error('Error saving claims:', error);
            this.snackBar.open('فشل في حفظ الطلبات', 'إغلاق', { duration: 5000 });
            this.saveComplete.emit({ success: false, error: this.saveError || undefined });
          }
        });
      },
      error: (error: any) => {
        this.isSaving = false;

        // Extract detailed error message
        let errorMessage = 'حدث خطأ أثناء حفظ البيانات';

        if (error?.error?.message) {
          errorMessage = error.error.message;
        }

        // Log full error for debugging
        console.error('Error saving case data:', error);
        console.error('Error response:', error?.error);

        this.saveError = errorMessage;
        this.snackBar.open(errorMessage, 'إغلاق', {
          duration: 7000,  // Longer for detailed messages
          panelClass: ['error-snackbar']
        });

        this.saveComplete.emit({ success: false, error: errorMessage });
      }
    });
  }
}
