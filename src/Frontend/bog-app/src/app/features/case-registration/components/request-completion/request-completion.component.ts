import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { switchMap, tap, catchError } from 'rxjs/operators';
import { forkJoin, throwError } from 'rxjs';
import { CaseRegistrationApiService } from '../../services/case-registration-api.service';
import { RequestStateService } from '../../services/request-state.service';
import { CaseDataStateService } from '../../services/case-data-state.service';
import { ClaimsApiService } from '../../services/claims-api.service';
import { RelatedCaseApiService } from '../../services/related-case-api.service';
import { ConfirmationDialogComponent } from '../shared/confirmation-dialog/confirmation-dialog.component';
import { DecisionType } from '../../models/enums';
import { CaseTypeVM, RequestDecisionDTO } from '../../models/case-request.model';

@Component({
  selector: 'app-request-completion',
  templateUrl: './request-completion.component.html',
  styleUrls: ['./request-completion.component.scss']
})
export class RequestCompletionComponent implements OnInit {
  @Input() requestId!: number;
  @Input() currentStatus!: number;

  completionForm!: FormGroup;
  caseTypes: CaseTypeVM[] = [];
  isSubmitting = false;
  isLoadingCaseTypes = false;

  // Decision options with Arabic labels
  decisionOptions = [
    { value: DecisionType.Register, label: 'قيد الدعوى' },
    { value: DecisionType.SendToJudge, label: 'العرض على رئيس المحكمة' },
    { value: DecisionType.Reject, label: 'التوجيه بعدم قيد الطلب' },
    { value: DecisionType.RequestCompletion, label: 'استكمال النواقص' }
  ];

  constructor(
    private fb: FormBuilder,
    private apiService: CaseRegistrationApiService,
    private requestState: RequestStateService,
    private caseDataState: CaseDataStateService,
    private claimsApi: ClaimsApiService,
    private relatedCaseApi: RelatedCaseApiService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.initializeForm();
    this.loadCaseTypes();
  }

  private initializeForm() {
    this.completionForm = this.fb.group({
      decisionType: ['', Validators.required],
      caseTypeId: [1, Validators.required],  // Default to 1 (إداري - Administrative)
      notes: ['', [Validators.maxLength(4000)]]
    });
  }

  private loadCaseTypes() {
    this.isLoadingCaseTypes = true;
    this.apiService.getCaseTypes().subscribe({
      next: (types) => {
        this.caseTypes = types;
        this.isLoadingCaseTypes = false;
      },
      error: (error) => {
        this.isLoadingCaseTypes = false;
        console.error('Failed to load case types', error);
        // Provide default case types as fallback
        this.caseTypes = [
          { id: 1, name: 'Administrative', nameAr: 'إداري', description: 'Administrative case' },
          { id: 2, name: 'Disciplinary', nameAr: 'تأديبي', description: 'Disciplinary case' }
        ];
        this.snackBar.open('فشل تحميل أنواع الدعاوى - استخدام البيانات الافتراضية', 'إغلاق', { duration: 3000 });
      }
    });
  }

  /**
   * Save all pending changes from state to database.
   * Ensures validations run against current data, not stale database values.
   */
  private saveAllPendingChanges(): Observable<any> {
    console.log('[AUTO-SAVE] Saving pending changes before action...');

    const state = this.caseDataState.getAllData();

    // Build main request payload
    const requestPayload = {
      subject: state.subject,
      evidence: state.evidence,
      classificationIds: state.classificationIds,
      primaryMobile: state.primaryMobile,
      secondaryMobile: state.secondaryMobile,
      email: state.email
    };

    console.log('[AUTO-SAVE] Classifications being saved:', state.classificationIds);

    // Build claims and related cases payloads
    const claimsPayload = state.claims.map(c => ({ claimText: c.claimText }));
    const relatedCasesPayload = state.relatedCases.map(rc => ({
      courtId: rc.courtId,
      caseNumber: rc.caseNumber,
      caseYear: rc.caseYear
    }));

    // Execute all saves in parallel using forkJoin
    return forkJoin({
      request: this.apiService.update(this.requestId, requestPayload),
      claims: this.claimsApi.updateClaims(this.requestId, claimsPayload),
      relatedCases: this.relatedCaseApi.updateRelatedCases(this.requestId, {
        relatedCases: relatedCasesPayload
      })
    }).pipe(
      tap((results) => {
        console.log('[AUTO-SAVE] All changes saved successfully');
        console.log('[AUTO-SAVE] Response classificationIds:', results.request.classificationIds);

        // Update state with saved values for consistency
        if (results.request.classificationIds) {
          this.caseDataState.updateClassifications(results.request.classificationIds);
        }
      }),
      catchError((error) => {
        console.error('[AUTO-SAVE] Failed to save changes:', error);
        return throwError(() => ({ operation: 'save', error }));
      })
    );
  }

  get canComplete(): boolean {
    // Allow completion if status is Draft (1), New (3), or PendingCompletion (6)
    // Status 6 allows موظف القيد to take action after deficiencies are identified
    return [1, 3, 6].includes(this.currentStatus);
  }

  onSubmit() {
    if (!this.completionForm.valid) {
      this.snackBar.open('يرجى تعبئة جميع الحقول المطلوبة', 'إغلاق', { duration: 3000 });
      return;
    }

    if (!this.canComplete) {
      this.snackBar.open('لا يمكن إنهاء الطلب في هذه الحالة', 'إغلاق', { duration: 3000 });
      return;
    }

    // Show confirmation dialog (CON02)
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'تأكيد العملية',
        message: 'هل أنت متأكد من الحفظ؟',
        confirmText: 'نعم',
        cancelText: 'لا'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return; // User clicked Cancel
      }

      const decision: RequestDecisionDTO = {
        decisionType: this.completionForm.value.decisionType,
        caseTypeId: this.completionForm.value.caseTypeId,
        notes: this.completionForm.value.notes || undefined
      };

      this.isSubmitting = true;

      // ALWAYS auto-save before executing action to ensure data consistency
      const actionPipeline = this.saveAllPendingChanges().pipe(
        tap(() => console.log('[AUTO-SAVE] Changes saved, proceeding to action...')),
        switchMap(() => this.apiService.completeRequest(this.requestId, decision))
      );

      // Execute pipeline
      actionPipeline.subscribe({
        next: (result) => {
          this.isSubmitting = false;
          this.requestState.updateRequest(result);

          // Reload full request details
          this.apiService.getById(this.requestId).subscribe({
            next: (updatedRequest: any) => {
              this.requestState.updateRequest(updatedRequest);

              // Show success message
              const messages: Record<string, string> = {
                [DecisionType.Register]: 'تم قيد الدعوى بنجاح',
                [DecisionType.SendToJudge]: 'تم العرض على رئيس المحكمة بنجاح',
                [DecisionType.Reject]: 'تم حفظ الطلب بنجاح',
                [DecisionType.RequestCompletion]: 'تم طلب استكمال النواقص'
              };

              this.snackBar.open(
                messages[decision.decisionType] || 'تمت العملية بنجاح',
                'إغلاق',
                { duration: 5000 }
              );

              // Reset form
              this.completionForm.reset({
                decisionType: '',
                caseTypeId: 1,
                notes: ''
              });
            },
            error: (reloadError: any) => {
              console.error('Error reloading request:', reloadError);
              // Still show success even if reload fails
              const messages: Record<string, string> = {
                [DecisionType.Register]: 'تم قيد الدعوى بنجاح',
                [DecisionType.SendToJudge]: 'تم العرض على رئيس المحكمة بنجاح',
                [DecisionType.Reject]: 'تم حفظ الطلب بنجاح',
                [DecisionType.RequestCompletion]: 'تم طلب استكمال النواقص'
              };

              this.snackBar.open(
                messages[decision.decisionType] || 'تمت العملية بنجاح',
                'إغلاق',
                { duration: 5000 }
              );
            }
          });
        },
        error: (errorObj) => {
          this.isSubmitting = false;

          // Determine which operation failed
          if (errorObj.operation === 'save') {
            console.error('[AUTO-SAVE] Save operation failed:', errorObj.error);

            let errorMessage = 'فشل حفظ التغييرات قبل تنفيذ الإجراء';
            if (errorObj.error?.error?.message) {
              errorMessage = errorObj.error.error.message;
            }

            this.snackBar.open(errorMessage, 'إغلاق', {
              duration: 8000,
              panelClass: 'error-snackbar'
            });
          } else {
            // Action execution failed
            console.error('Failed to complete request', errorObj);

            let errorMessage = 'فشل إنهاء الطلب';
            if (errorObj.error?.message) {
              errorMessage = errorObj.error.message;
              // Split multiple errors for better readability
              if (errorMessage.includes('|')) {
                const errors = errorMessage.split('|').map((e: string) => e.trim());
                errorMessage = errors.join('\n');
              }
            }

            this.snackBar.open(errorMessage, 'إغلاق', {
              duration: 8000,
              panelClass: 'error-snackbar'
            });
          }
        }
      });
    });
  }
}
