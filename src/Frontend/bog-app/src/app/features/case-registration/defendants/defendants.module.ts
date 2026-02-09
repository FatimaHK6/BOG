import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../../shared/shared.module';

// Components
import { DefendantListComponent } from './components/defendant-list/defendant-list.component';
import { DefendantIndividualFormComponent } from './components/defendant-individual-form/defendant-individual-form.component';
import { DefendantRegisteredCompanyFormComponent } from './components/defendant-registered-company-form/defendant-registered-company-form.component';
import { DefendantGovFormComponent } from './components/defendant-gov-form/defendant-gov-form.component';
import { DefendantUnregisteredCompanyFormComponent } from './components/defendant-unregistered-company-form/defendant-unregistered-company-form.component';
import { DefendantNGOFormComponent } from './components/defendant-ngo-form/defendant-ngo-form.component';
import { DefendantWaqfFormComponent } from './components/defendant-waqf-form/defendant-waqf-form.component';
import { DefendantBusinessOwnerFormComponent } from './components/defendant-business-owner-form/defendant-business-owner-form.component';

const routes: Routes = [
  { path: '', component: DefendantListComponent },
  // Individual (Type 1)
  { path: 'add/individual', component: DefendantIndividualFormComponent },
  { path: 'edit/individual/:id', component: DefendantIndividualFormComponent },
  { path: 'view/individual/:id', component: DefendantIndividualFormComponent },
  // Registered Company (Type 2)
  { path: 'add/registered-company', component: DefendantRegisteredCompanyFormComponent },
  { path: 'edit/registered-company/:id', component: DefendantRegisteredCompanyFormComponent },
  { path: 'view/registered-company/:id', component: DefendantRegisteredCompanyFormComponent },
  // Government Agency (Type 3)
  { path: 'add/government-agency', component: DefendantGovFormComponent },
  { path: 'edit/government-agency/:id', component: DefendantGovFormComponent },
  { path: 'view/government-agency/:id', component: DefendantGovFormComponent },
  // Unregistered Company (Type 4)
  { path: 'add/unregistered-company', component: DefendantUnregisteredCompanyFormComponent },
  { path: 'edit/unregistered-company/:id', component: DefendantUnregisteredCompanyFormComponent },
  { path: 'view/unregistered-company/:id', component: DefendantUnregisteredCompanyFormComponent },
  // Business Owner (Type 5)
  { path: 'add/business-owner', component: DefendantBusinessOwnerFormComponent },
  { path: 'edit/business-owner/:id', component: DefendantBusinessOwnerFormComponent },
  { path: 'view/business-owner/:id', component: DefendantBusinessOwnerFormComponent },
  // NGO/Society (Type 6)
  { path: 'add/ngo', component: DefendantNGOFormComponent },
  { path: 'edit/ngo/:id', component: DefendantNGOFormComponent },
  { path: 'view/ngo/:id', component: DefendantNGOFormComponent },
  // Waqf (Type 7)
  { path: 'add/waqf', component: DefendantWaqfFormComponent },
  { path: 'edit/waqf/:id', component: DefendantWaqfFormComponent },
  { path: 'view/waqf/:id', component: DefendantWaqfFormComponent }
];

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
    RouterModule.forChild(routes)
  ],
  exports: [
    DefendantListComponent
  ]
})
export class DefendantsModule { }
