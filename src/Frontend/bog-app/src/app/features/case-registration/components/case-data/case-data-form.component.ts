import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CaseRegistrationApiService } from '../../services/case-registration-api.service';
import { ClassificationApiService, ClassificationVM } from '../../services/classification-api.service';
import { CaseDataStateService } from '../../services/case-data-state.service';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-case-data-form',
  templateUrl: './case-data-form.component.html',
  styleUrls: ['./case-data-form.component.scss']
})
export class CaseDataFormComponent implements OnInit, OnDestroy {
  @Input() requestId!: number;
  @Input() canEdit = false;

  caseDataForm!: UntypedFormGroup;
  saving = false;
  saveSuccess = false;
  classifications: ClassificationVM[] = [];

  private destroy$ = new Subject<void>();

  get subjectLength() {
    return this.caseDataForm.get('subject')?.value?.length || 0;
  }

  get evidenceLength() {
    return this.caseDataForm.get('evidence')?.value?.length || 0;
  }

  constructor(
    private fb: UntypedFormBuilder,
    private caseApi: CaseRegistrationApiService,
    private classificationApi: ClassificationApiService,
    private caseDataState: CaseDataStateService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit() {
    this.buildForm();
    this.loadClassifications();

    if (this.canEdit) {
      this.setupAutoSave();
    }

    // Load from state service instead of request state
    const subject = this.caseDataState.getSubject();
    const evidence = this.caseDataState.getEvidence();
    const classifications = this.caseDataState.getClassificationIds();

    this.caseDataForm.patchValue({
      subject,
      evidence,
      classifications
    });

    if (!this.canEdit) {
      this.caseDataForm.disable();
    }

    // Subscribe to state changes to update form if state changes from other components
    this.caseDataState.state$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        if (this.caseDataForm.get('subject')?.value !== state.subject) {
          this.caseDataForm.patchValue({
            subject: state.subject,
            evidence: state.evidence,
            classifications: state.classificationIds
          }, { emitEvent: false });
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClassifications() {
    console.log('Loading classifications...');
    this.classificationApi.getAll().subscribe({
      next: (classifications) => {
        console.log('Classifications loaded successfully:', classifications);
        console.log('Number of classifications:', classifications?.length);
        if (classifications && classifications.length > 0) {
          console.log('First classification:', classifications[0]);
          console.log('First classification keys:', Object.keys(classifications[0]));
        }
        this.classifications = classifications;
      },
      error: (error) => {
        console.error('Failed to load classifications', error);
        console.error('Error status:', error?.status);
        console.error('Error message:', error?.message);
        this.snackBar.open('فشل في تحميل التصنيفات', 'إغلاق', { duration: 3000 });
      }
    });
  }

  buildForm() {
    this.caseDataForm = this.fb.group({
      subject: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(4000)]],
      evidence: ['', [Validators.required, Validators.minLength(20), Validators.maxLength(4000)]],
      classifications: [[], [Validators.required, Validators.minLength(1)]]
    });
  }

  setupAutoSave() {
    this.caseDataForm.valueChanges.pipe(
      debounceTime(2000),
      distinctUntilChanged()
    ).subscribe(values => {
      this.saveForm();
    });
  }

  saveForm() {
    if (this.caseDataForm.invalid || this.saving || !this.canEdit) {
      return;
    }

    this.saving = true;
    this.saveSuccess = false;

    const subject = this.caseDataForm.get('subject')?.value;
    const evidence = this.caseDataForm.get('evidence')?.value;
    const classifications = this.caseDataForm.get('classifications')?.value || [];

    // Update state service first (for localStorage persistence)
    this.caseDataState.updateSubject(subject);
    this.caseDataState.updateEvidence(evidence);
    this.caseDataState.updateClassifications(classifications);

    const updateData = {
      subject,
      evidence,
      classificationIds: classifications
    };

    this.caseApi.update(this.requestId, updateData).subscribe({
      next: (request) => {
        // Update form with returned data
        this.caseDataForm.patchValue({
          subject: request.subject,
          evidence: request.evidence,
          classifications: (request as any).classificationIds || []
        }, { emitEvent: false });

        this.saving = false;
        this.saveSuccess = true;

        setTimeout(() => {
          this.saveSuccess = false;
        }, 3000);
      },
      error: (error) => {
        this.saving = false;
        this.snackBar.open('خطأ في حفظ بيانات الدعوى', 'إغلاق', { duration: 3000 });
      }
    });
  }
}
