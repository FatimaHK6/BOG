import { NgModule } from '@angular/core';
import { NgxEditorModule } from 'ngx-editor';
import { CaseRegistrationSharedModule } from '../shared/case-registration-shared.module';

// Case data components
import { CaseDataFormComponent } from '../components/case-data/case-data-form.component';
import { CaseDataContainerComponent } from '../components/case-data/case-data-container/case-data-container.component';
import { SubjectEvidenceFormComponent } from '../components/case-data/subject-evidence/subject-evidence-form.component';
import { ClassificationsTabComponent } from '../components/case-data/classifications-tab/classifications-tab.component';
import { ClassificationSelectionDialogComponent } from '../components/case-data/classifications-tab/classification-selection-dialog/classification-selection-dialog.component';
import { ContactInfoFormComponent } from '../components/case-data/contact-info/contact-info-form.component';

// Claims components
import { ClaimsListComponent } from '../components/claims/claims-list/claims-list.component';
import { ClaimFormDialogComponent } from '../components/claims/claim-form-dialog/claim-form-dialog.component';

// Related cases components
import { RelatedCasesListComponent } from '../components/related-cases/related-cases-list/related-cases-list.component';
import { RelatedCaseFormDialogComponent } from '../components/related-cases/related-case-form-dialog/related-case-form-dialog.component';

// Attachments components
import { AttachmentsListComponent } from '../components/attachments/attachments-list.component';

// Defendants components (in case data context)
import { DefendantsListComponent } from '../components/defendants/defendants-list/defendants-list.component';
import { DefendantFormDialogComponent } from '../components/defendants/defendant-form-dialog/defendant-form-dialog.component';

// Deficiencies components
import { DeficienciesListComponent } from '../components/deficiencies/deficiencies-list.component';
import { DeficiencyFormDialogComponent } from '../components/deficiencies/deficiency-form-dialog.component';

@NgModule({
  declarations: [
    CaseDataFormComponent,
    CaseDataContainerComponent,
    SubjectEvidenceFormComponent,
    ClassificationsTabComponent,
    ClassificationSelectionDialogComponent,
    ContactInfoFormComponent,
    ClaimsListComponent,
    ClaimFormDialogComponent,
    RelatedCasesListComponent,
    RelatedCaseFormDialogComponent,
    AttachmentsListComponent,
    DefendantsListComponent,
    DefendantFormDialogComponent,
    DeficienciesListComponent,
    DeficiencyFormDialogComponent
  ],
  imports: [CaseRegistrationSharedModule, NgxEditorModule],
  exports: [
    CaseDataContainerComponent,
    CaseDataFormComponent,
    DefendantsListComponent,
    DefendantFormDialogComponent,
    DeficienciesListComponent,
    DeficiencyFormDialogComponent
  ]
})
export class CaseDataModule { }
