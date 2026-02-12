import { NgModule } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';
import { DefendantsRoutingModule } from './defendants-routing.module';

// Components
import { DefendantListComponent } from './components/defendant-list/defendant-list.component';
import { DefendantIndividualFormComponent } from './components/defendant-individual-form/defendant-individual-form.component';
import { DefendantRegisteredCompanyFormComponent } from './components/defendant-registered-company-form/defendant-registered-company-form.component';
import { DefendantGovFormComponent } from './components/defendant-gov-form/defendant-gov-form.component';
import { DefendantUnregisteredCompanyFormComponent } from './components/defendant-unregistered-company-form/defendant-unregistered-company-form.component';
import { DefendantNGOFormComponent } from './components/defendant-ngo-form/defendant-ngo-form.component';
import { DefendantWaqfFormComponent } from './components/defendant-waqf-form/defendant-waqf-form.component';
import { DefendantBusinessOwnerFormComponent } from './components/defendant-business-owner-form/defendant-business-owner-form.component';

@NgModule({
  declarations: [
    DefendantListComponent,
    DefendantIndividualFormComponent,
    DefendantRegisteredCompanyFormComponent,
    DefendantGovFormComponent,
    DefendantUnregisteredCompanyFormComponent,
    DefendantBusinessOwnerFormComponent,
    DefendantNGOFormComponent,
    DefendantWaqfFormComponent
  ],
  imports: [
    SharedModule,
    DefendantsRoutingModule
  ],
  exports: [
    DefendantListComponent
  ]
})
export class DefendantsModule { }
