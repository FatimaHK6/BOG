import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CaseDataStateService } from '../../../services/case-data-state.service';

@Component({
  selector: 'app-contact-info-form',
  templateUrl: './contact-info-form.component.html',
  styleUrls: ['./contact-info-form.component.scss']
})
export class ContactInfoFormComponent implements OnInit, OnDestroy {
  @Input() canEdit = false;

  contactForm!: FormGroup;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private caseDataState: CaseDataStateService
  ) { }

  ngOnInit() {
    this.buildForm();
    this.setupFormValueChanges();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  buildForm() {
    const contactInfo = this.caseDataState.getContactInfo();

    this.contactForm = this.fb.group({
      primaryMobile: [
        contactInfo.primaryMobile || '',
        [
          Validators.required,
          Validators.pattern(/^05\d{8}$/)
        ]
      ],
      secondaryMobile: [
        contactInfo.secondaryMobile || '',
        [
          Validators.pattern(/^05\d{8}$/)
        ]
      ],
      email: [
        contactInfo.email || '',
        [
          Validators.email
        ]
      ]
    });

    if (!this.canEdit) {
      this.contactForm.disable();
    }
  }

  setupFormValueChanges() {
    this.contactForm.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(values => {
        // Always update state - backend will validate
        this.caseDataState.updateContactInfo(
          values.primaryMobile || '',
          values.secondaryMobile || '',
          values.email || ''
        );
      });
  }

  private isFormEmpty(): boolean {
    const values = this.contactForm.value;
    return !values.primaryMobile && !values.secondaryMobile && !values.email;
  }

  clearField(fieldName: string) {
    this.contactForm.get(fieldName)?.reset();
  }

  getErrorMessage(fieldName: string): string {
    const control = this.contactForm.get(fieldName);

    if (!control || !control.errors) {
      return '';
    }

    if (control.errors['required']) {
      return fieldName === 'primaryMobile' ? 'رقم الجوال الأساسي مطلوب' : '';
    }

    if (control.errors['pattern']) {
      return 'يجب أن يكون الرقم بصيغة صحيحة (05xxxxxxxx)';
    }

    if (control.errors['email']) {
      return 'البريد الإلكتروني غير صحيح';
    }

    return '';
  }
}
