import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { CaseRegistrationSharedModule } from './shared/case-registration-shared.module';
import { CaseDataModule } from './case-data/case-data.module';
import { PlaintiffsModule } from './plaintiffs/plaintiffs.module';
import { DefendantsModule } from './defendants/defendants.module';
import { RequestsModule } from './requests/requests.module';
import { CaseRegistrationRoutingModule } from './case-registration-routing.module';

// Page components
import { RequestDetailsComponent } from './pages/request-details/request-details.component';

// Remaining components
import { AdditionalInfoFormComponent } from './components/additional-info/additional-info-form.component';
import { RequestCompletionComponent } from './components/request-completion/request-completion.component';
import { DeficienciesSelectionDialogComponent } from './components/request-completion/deficiencies-selection-dialog.component';

@NgModule({
  declarations: [
    RequestDetailsComponent,
    AdditionalInfoFormComponent,
    RequestCompletionComponent,
    DeficienciesSelectionDialogComponent
  ],
  imports: [
    CaseRegistrationSharedModule,
    CaseDataModule,
    PlaintiffsModule,
    DefendantsModule,
    RequestsModule,
    SharedModule,
    CaseRegistrationRoutingModule
  ]
})
export class CaseRegistrationModule { }
