import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DefendantService } from '../../../../../core/services/defendant.service';
import { LookupService } from '../../../../../core/services/lookup.service';
import { GovernmentAgency } from '../../../../../core/models/lookup.model';

@Component({
  selector: 'app-defendant-gov-form',
  templateUrl: './defendant-gov-form.component.html',
  styleUrls: ['./defendant-gov-form.component.scss']
})
export class DefendantGovFormComponent implements OnInit {
  form!: FormGroup;
  requestId: number = 0;
  defendantId: number = 0;
  isEditMode = false;
  isViewMode = false;
  isLoading = false;
  isSaving = false;
  submitted = false;

  governmentAgencies: GovernmentAgency[] = [];

  constructor(
    private fb: FormBuilder,
    private defendantService: DefendantService,
    private lookupService: LookupService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    // Get requestId from query params
    this.route.queryParams.subscribe(params => {
      this.requestId = params['requestId'] ? +params['requestId'] : 0;
    });

    // Check if edit or view mode (id in route params)
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.defendantId = +params['id'];
        // Check if view mode by URL path
        const url = this.router.url;
        this.isViewMode = url.includes('/view/');
        this.isEditMode = !this.isViewMode;
      }
    });

    this.initForm();
    this.loadLookups();
  }

  private initForm(): void {
    this.form = this.fb.group({
      governmentAgencyId: [null, Validators.required],
      headquarters: ['', Validators.required],
      additionalStatement: ['', Validators.maxLength(4000)]
    });
  }

  private loadLookups(): void {
    this.isLoading = true;
    this.lookupService.getGovernmentAgencies().subscribe({
      next: (agencies) => {
        this.governmentAgencies = agencies;
        // Load defendant data if edit or view mode
        if ((this.isEditMode || this.isViewMode) && this.defendantId > 0) {
          this.loadDefendant();
        } else {
          this.isLoading = false;
        }
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  private loadDefendant(): void {
    this.defendantService.getDefendant(this.defendantId).subscribe({
      next: (defendant) => {
        this.form.patchValue({
          governmentAgencyId: defendant.governmentAgencyId,
          headquarters: defendant.headquarters,
          additionalStatement: defendant.additionalStatement
        });
        // Disable form if view mode
        if (this.isViewMode) {
          this.form.disable();
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading defendant:', err);
        this.isLoading = false;
      }
    });
  }

  onAgencyChange(agencyId: number): void {
    const agency = this.governmentAgencies.find(a => a.id === agencyId);
    if (agency) {
      // Auto-fill headquarters with agency name (as location)
      this.form.patchValue({
        headquarters: agency.nameAr
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.form.markAllAsTouched();

    if (this.form.invalid || this.requestId <= 0) {
      return;
    }

    this.isSaving = true;
    const dto = {
      defendantTypeId: 3, // Government Agency (جهة حكومية)
      governmentAgencyId: this.form.value.governmentAgencyId,
      headquarters: this.form.value.headquarters,
      additionalStatement: this.form.value.additionalStatement || null
    };

    const request$ = this.isEditMode
      ? this.defendantService.updateDefendant(this.defendantId, dto)
      : this.defendantService.createDefendant(this.requestId, dto);

    request$.subscribe({
      next: () => {
        this.isSaving = false;
        const message = this.isEditMode ? 'تم تعديل المدعى عليه بنجاح' : 'تم إنشاء المدعى عليه بنجاح';
        this.snackBar.open(message, 'إغلاق', { duration: 3000 });
        // Navigate back to defendants list
        this.router.navigate(['/case-registration/defendants'], {
          queryParams: { requestId: this.requestId }
        });
      },
      error: (err) => {
        console.error('Error saving defendant:', err);
        this.isSaving = false;
        const errorMessage = err.error?.message || err.error || 'حدث خطأ أثناء حفظ البيانات';
        this.snackBar.open(errorMessage, 'إغلاق', { duration: 5000 });
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/case-registration/defendants'], {
      queryParams: { requestId: this.requestId }
    });
  }

  // Helper for form field access
  get f() { return this.form.controls; }
}
