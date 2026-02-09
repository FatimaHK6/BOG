import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

// Material modules
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatMenuModule } from '@angular/material/menu';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NgxEditorModule } from 'ngx-editor';

import { CaseRegistrationRoutingModule } from './case-registration-routing.module';

// Shared components
import { SectionContainerComponent } from './components/shared/section-container/section-container.component';
import { ValidationMessageComponent } from './components/shared/validation-message/validation-message.component';
import { ConfirmationDialogComponent } from './components/shared/confirmation-dialog/confirmation-dialog.component';

// Services
import { CaseRegistrationApiService } from './services/case-registration-api.service';
import { DefendantApiService } from './services/defendant-api.service';
import { RequestStateService } from './services/request-state.service';
import { ClassificationApiService } from './services/classification-api.service';
import { AttachmentApiService } from './services/attachment-api.service';
import { AdditionalInfoApiService } from './services/additional-info-api.service';
import { LookupsApiService } from './services/lookups-api.service';
import { ClaimApiService } from './services/claim-api.service';
import { RelatedCaseApiService } from './services/related-case-api.service';
import { CaseDataStateService } from './services/case-data-state.service';
import { ClassificationsApiService } from './services/classifications-api.service';

// Page components (will be created)
import { RequestDetailsComponent } from './pages/request-details/request-details.component';
import { RequestListComponent } from './pages/request-list/request-list.component';

// Defendants components
import { DefendantsListComponent } from './components/defendants/defendants-list/defendants-list.component';
import { DefendantFormDialogComponent } from './components/defendants/defendant-form-dialog/defendant-form-dialog.component';

// Case Data, Attachments, Additional Info components (will be created)
import { CaseDataFormComponent } from './components/case-data/case-data-form.component';
import { AttachmentsListComponent } from './components/attachments/attachments-list.component';
import { AdditionalInfoFormComponent } from './components/additional-info/additional-info-form.component';
import { DeficienciesListComponent } from './components/deficiencies/deficiencies-list.component';
import { ClaimsListComponent } from './components/claims/claims-list/claims-list.component';
import { ClaimFormDialogComponent } from './components/claims/claim-form-dialog/claim-form-dialog.component';
import { RelatedCasesListComponent } from './components/related-cases/related-cases-list/related-cases-list.component';
import { RelatedCaseFormDialogComponent } from './components/related-cases/related-case-form-dialog/related-case-form-dialog.component';
import { ContactInfoFormComponent } from './components/case-data/contact-info/contact-info-form.component';
import { SubjectEvidenceFormComponent } from './components/case-data/subject-evidence/subject-evidence-form.component';
import { CaseDataContainerComponent } from './components/case-data/case-data-container/case-data-container.component';
import { ClassificationsTabComponent } from './components/case-data/classifications-tab/classifications-tab.component';
import { ClassificationSelectionDialogComponent } from './components/case-data/classifications-tab/classification-selection-dialog/classification-selection-dialog.component';
import { RequestCompletionComponent } from './components/request-completion/request-completion.component';

@NgModule({
  declarations: [
    // Shared components
    SectionContainerComponent,
    ValidationMessageComponent,
    ConfirmationDialogComponent,
    // Page components
    RequestDetailsComponent,
    RequestListComponent,
    // Defendants components
    DefendantsListComponent,
    DefendantFormDialogComponent,
    // Other components
    CaseDataFormComponent,
    AttachmentsListComponent,
    AdditionalInfoFormComponent,
    DeficienciesListComponent,
    ClaimsListComponent,
    ClaimFormDialogComponent,
    RelatedCasesListComponent,
    RelatedCaseFormDialogComponent,
    // Case Data Sub-components (Phase 3)
    ContactInfoFormComponent,
    SubjectEvidenceFormComponent,
    // Case Data Container & Classifications (Phase 4)
    CaseDataContainerComponent,
    ClassificationsTabComponent,
    ClassificationSelectionDialogComponent,
    // Request Completion (Phase 5)
    RequestCompletionComponent
  ],
  imports: [
    CommonModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    CaseRegistrationRoutingModule,
    NgxEditorModule,
    // Material modules
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatOptionModule,
    MatTableModule,
    MatPaginatorModule,
    MatDialogModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatMenuModule,
    MatTabsModule,
    MatCheckboxModule,
    MatChipsModule,
    MatListModule,
    MatProgressBarModule,
    MatTooltipModule
  ],
  providers: [
    CaseRegistrationApiService,
    DefendantApiService,
    RequestStateService,
    ClassificationApiService,
    AttachmentApiService,
    AdditionalInfoApiService,
    LookupsApiService,
    ClaimApiService,
    RelatedCaseApiService,
    CaseDataStateService,
    ClassificationsApiService
  ]
})
export class CaseRegistrationModule { }
