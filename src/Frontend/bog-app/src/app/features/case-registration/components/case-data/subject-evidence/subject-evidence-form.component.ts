import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Editor, Toolbar } from 'ngx-editor';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CaseDataStateService } from '../../../services/case-data-state.service';
import { DeficiencyFormDialogComponent } from '../../deficiencies/deficiency-form-dialog.component';
import { DeficienciesApiService } from '../../../services/deficiencies-api.service';
import { RequestDeficiencyDTO, DeficienciesBatchUpdateDTO } from '../../../models/deficiency.model';

@Component({
  selector: 'app-subject-evidence-form',
  templateUrl: './subject-evidence-form.component.html',
  styleUrls: ['./subject-evidence-form.component.scss']
})
export class SubjectEvidenceFormComponent implements OnInit, OnDestroy {
  @Input() canEdit = false;
  @Input() requestId!: number;

  subjectEvidenceForm!: FormGroup;

  // Rich text editors
  subjectEditor!: Editor;
  evidenceEditor!: Editor;

  // Toolbar configuration
  toolbar: Toolbar = [
    ['bold', 'italic', 'underline', 'strike'],
    ['bullet_list', 'ordered_list'],
    [{ heading: ['h1', 'h2', 'h3', 'h4', 'h5', 'h6'] }],
    ['link'],
    ['text_color', 'background_color'],
    ['align_left', 'align_center', 'align_right', 'align_justify']
  ];

  readonly maxCharacters = {
    subject: 4000,
    evidence: 4000
  };

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private caseDataState: CaseDataStateService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private deficienciesApi: DeficienciesApiService
  ) { }

  ngOnInit() {
    this.subjectEditor = new Editor();
    this.evidenceEditor = new Editor();
    this.buildForm();
    this.setupFormValueChanges();
  }

  ngOnDestroy() {
    this.subjectEditor.destroy();
    this.evidenceEditor.destroy();
    this.destroy$.next();
    this.destroy$.complete();
  }

  buildForm() {
    const subject = this.caseDataState.getSubject();
    const evidence = this.caseDataState.getEvidence();

    this.subjectEvidenceForm = this.fb.group({
      subject: [
        subject || '',
        [
          Validators.required,
          Validators.maxLength(this.maxCharacters.subject)
        ]
      ],
      evidence: [
        evidence || '',
        [
          Validators.required,
          Validators.maxLength(this.maxCharacters.evidence)
        ]
      ]
    });

    if (!this.canEdit) {
      this.subjectEvidenceForm.disable();
    }
  }

  setupFormValueChanges() {
    this.subjectEvidenceForm.valueChanges
      .pipe(
        debounceTime(1000),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(values => {
        this.caseDataState.updateSubject(values.subject || '');
        this.caseDataState.updateEvidence(values.evidence || '');
      });
  }

  getCharacterCount(fieldName: 'subject' | 'evidence'): number {
    const control = this.subjectEvidenceForm.get(fieldName);
    return control?.value?.length || 0;
  }

  getCharacterPercentage(fieldName: 'subject' | 'evidence'): number {
    const count = this.getCharacterCount(fieldName);
    return Math.round((count / this.maxCharacters[fieldName]) * 100);
  }

  getMaxCharacters(fieldName: 'subject' | 'evidence'): number {
    return this.maxCharacters[fieldName];
  }

  hasError(fieldName: string, errorType: string): boolean {
    const control = this.subjectEvidenceForm.get(fieldName);
    return !!(control && control.hasError(errorType) && (control.dirty || control.touched));
  }

  getErrorMessage(fieldName: string): string {
    const control = this.subjectEvidenceForm.get(fieldName);

    if (!control || !control.errors) {
      return '';
    }

    if (control.errors['required']) {
      return fieldName === 'subject' ? 'موضوع الدعوى مطلوب' : 'أسانيد الدعوى مطلوبة';
    }

    if (control.errors['maxlength']) {
      const max = this.maxCharacters[fieldName as 'subject' | 'evidence'];
      return `يجب ألا يتجاوز ${max} حرف`;
    }

    return '';
  }

  openSubjectDeficiencyDialog() {
    this.openDeficiencyDialogForType(1); // Type 1 = CaseSubject
  }

  openGroundsDeficiencyDialog() {
    this.openDeficiencyDialogForType(3); // Type 3 = CaseGrounds
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
