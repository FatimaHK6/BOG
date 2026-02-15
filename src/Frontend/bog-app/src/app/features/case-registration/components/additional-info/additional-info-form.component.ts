import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { AdditionalInfoApiService } from '../../services/additional-info-api.service';
import { LookupsApiService } from '../../services/lookups-api.service';
import { AdditionalInfoVM, AdditionalInfoDTO, NotificationMethodVM, GovernmentEntityVM } from '../../models/additional-info.model';
import { DeficiencyFormDialogComponent } from '../deficiencies/deficiency-form-dialog.component';
import { DeficienciesApiService } from '../../services/deficiencies-api.service';
import { RequestDeficiencyDTO, DeficienciesBatchUpdateDTO } from '../../models/deficiency.model';

/**
 * Additional Info Form Component - UC 6.5.1.1.15
 * Displays three types of additional information based on case registration context:
 * - Type 1: Management Decision Cancellation (إلغاء قرار إداري)
 * - Type 2: Service/Retirement Rights (حقوق خدمة/تقاعدية)
 * - Type 3: Trademark Registration Request Acceptance/Rejection Appeals (دعاوى إلغاء قبول أو رفض طلب تسجيل علامة تجارية)
 */
@Component({
  selector: 'app-additional-info-form',
  templateUrl: './additional-info-form.component.html',
  styleUrls: ['./additional-info-form.component.scss']
})
export class AdditionalInfoFormComponent implements OnInit, OnDestroy {
  @Input() requestId!: number;
  @Input() canEdit = false;

  infoForm: FormGroup;
  loading = false;
  saving = false;

  // Lookup data
  notificationMethods: NotificationMethodVM[] = [];
  governmentEntities: GovernmentEntityVM[] = [];

  // Auto-save state
  private destroy$ = new Subject<void>();
  private saveInProgress = false;

  constructor(
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private additionalInfoApi: AdditionalInfoApiService,
    private lookupsApi: LookupsApiService,
    private dialog: MatDialog,
    private deficienciesApi: DeficienciesApiService
  ) {
    this.infoForm = this.createForm();
  }

  ngOnInit() {
    if (!this.canEdit) {
      this.infoForm.disable();
    } else {
      this.setupAutoSave();
    }

    this.loadLookups();
    this.loadAdditionalInfo();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Create form with all type-specific fields
   * Type 1: 5 fields
   * Type 2: 6 fields
   * Type 3: 2 fields
   */
  private createForm(): FormGroup {
    return this.fb.group({
      // Type 1: Management Decision Cancellation (إلغاء قرار إداري)
      decisionNumber: ['', [Validators.maxLength(50)]],
      decisionDate: [null],
      notificationDate: [null],
      notificationMethodId: [null],
      issuingAuthorityId: [null],

      // Type 2: Service/Retirement Rights (حقوق خدمة/تقاعدية)
      hasComplaint: [false],
      complaintNumber: ['', [Validators.maxLength(50)]],
      complaintDate: [null],
      complaintAuthorityId: [null],
      complaintDecisionDate: [null],
      systemResult: ['', [Validators.maxLength(500)]],

      // Type 3: Trademark Dispute (نزاع علامة تجارية)
      requestNumber: ['', [Validators.maxLength(50)]],
      requestDate: [null]
    });
  }

  /**
   * Setup auto-save: Debounce form changes by 2 seconds, then save
   */
  private setupAutoSave() {
    this.infoForm.valueChanges
      .pipe(
        debounceTime(2000),
        distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.saveAdditionalInfo();
      });
  }

  /**
   * Load lookup data (notification methods, government entities)
   */
  private loadLookups() {
    this.lookupsApi.getNotificationMethods()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (methods) => {
          this.notificationMethods = methods;
        },
        error: (error) => {
          console.error('Error loading notification methods', error);
          this.snackBar.open('خطأ في تحميل طرق الإبلاغ', 'إغلاق', { duration: 3000 });
        }
      });

    this.lookupsApi.getGovernmentEntities()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (entities) => {
          this.governmentEntities = entities;
        },
        error: (error) => {
          console.error('Error loading government entities', error);
          this.snackBar.open('خطأ في تحميل الجهات الحكومية', 'إغلاق', { duration: 3000 });
        }
      });
  }

  /**
   * Load existing additional info for this case
   */
  private loadAdditionalInfo() {
    if (!this.requestId || this.requestId <= 0) {
      return;
    }

    this.loading = true;
    this.additionalInfoApi.getAdditionalInfo(this.requestId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.populateForm(data);
          this.loading = false;
        },
        error: (error) => {
          // 404 is expected for new records without additional info
          if (error.status !== 404) {
            console.error('Error loading additional info', error);
            this.snackBar.open('خطأ في تحميل المعلومات الإضافية', 'إغلاق', { duration: 3000 });
          }
          this.loading = false;
        }
      });
  }

  /**
   * Populate form with loaded data
   */
  private populateForm(data: AdditionalInfoVM) {
    if (!data) {
      return;
    }

    // Type 1: Management Decision
    if (data.decisionNumber || data.decisionDate) {
      this.infoForm.patchValue({
        decisionNumber: data.decisionNumber,
        decisionDate: data.decisionDate ? new Date(data.decisionDate) : null,
        notificationDate: data.notificationDate ? new Date(data.notificationDate) : null,
        notificationMethodId: data.notificationMethodId,
        issuingAuthorityId: data.issuingAuthorityId
      });
    }

    // Type 2: Service/Retirement Rights
    if (data.hasComplaint !== null || data.complaintNumber || data.complaintDate) {
      this.infoForm.patchValue({
        hasComplaint: data.hasComplaint || false,
        complaintNumber: data.complaintNumber,
        complaintDate: data.complaintDate ? new Date(data.complaintDate) : null,
        complaintAuthorityId: data.complaintAuthorityId,
        complaintDecisionDate: data.complaintDecisionDate ? new Date(data.complaintDecisionDate) : null,
        systemResult: data.systemResult
      });
    }

    // Type 3: Trademark
    if (data.requestNumber || data.requestDate) {
      this.infoForm.patchValue({
        requestNumber: data.requestNumber,
        requestDate: data.requestDate ? new Date(data.requestDate) : null
      });
    }
  }

  /**
   * Save additional info with intelligent type detection
   * Backend will detect which types have data and save accordingly
   */
  private saveAdditionalInfo() {
    if (!this.requestId || this.requestId <= 0) {
      return;
    }

    if (this.saveInProgress) {
      return; // Prevent duplicate saves
    }

    // Check if any field has data
    if (!this.hasAnyData()) {
      return; // No need to save if all fields are empty
    }

    this.saveInProgress = true;
    this.saving = true;

    const dto: AdditionalInfoDTO = this.infoForm.getRawValue();

    this.additionalInfoApi.saveAdditionalInfo(this.requestId, dto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.saveInProgress = false;
          this.saving = false;
          // Silently save - no snackbar notification for auto-save
        },
        error: (error) => {
          this.saveInProgress = false;
          this.saving = false;
          console.error('Error saving additional info', error);
          this.snackBar.open('خطأ في حفظ المعلومات الإضافية', 'إغلاق', { duration: 3000 });
        }
      });
  }

  /**
   * Check if any field has data (at least one field is filled)
   */
  private hasAnyData(): boolean {
    const value = this.infoForm.getRawValue();

    // Type 1 check
    if (value.decisionNumber || value.decisionDate || value.notificationDate ||
        value.notificationMethodId || value.issuingAuthorityId) {
      return true;
    }

    // Type 2 check
    if (value.hasComplaint === true || value.complaintNumber || value.complaintDate ||
        value.complaintAuthorityId || value.complaintDecisionDate || value.systemResult) {
      return true;
    }

    // Type 3 check
    if (value.requestNumber || value.requestDate) {
      return true;
    }

    return false;
  }

  /**
   * Get lookup name by ID (for display in form after save)
   */
  getNotificationMethodName(id: number | null): string {
    if (!id) return '';
    const found = this.notificationMethods.find(m => m.id === id);
    return found ? found.nameAr : '';
  }

  getGovernmentEntityName(id: number | null): string {
    if (!id) return '';
    const found = this.governmentEntities.find(e => e.id === id);
    return found ? found.nameAr : '';
  }

  openDeficiencyDialog() {
    this.openDeficiencyDialogForType(6); // Type 6 = AdditionalCaseInfo
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
