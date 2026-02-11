import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { CaseRegistrationSharedModule } from './shared/case-registration-shared.module';
import { CaseDataModule } from './case-data/case-data.module';

// Page components
import { RequestDetailsComponent } from './pages/request-details/request-details.component';

// Remaining components
import { AdditionalInfoFormComponent } from './components/additional-info/additional-info-form.component';
import { DeficienciesListComponent } from './components/deficiencies/deficiencies-list.component';
import { RequestCompletionComponent } from './components/request-completion/request-completion.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'requests',
    pathMatch: 'full'
  },
  {
    path: 'requests',
    loadChildren: () => import('./requests/requests.module').then(m => m.RequestsModule)
  },
  {
    path: 'plaintiffs',
    loadChildren: () => import('./plaintiffs/plaintiffs.module').then(m => m.PlaintiffsModule)
  },
  {
    path: 'defendants',
    loadChildren: () => import('./defendants/defendants.module').then(m => m.DefendantsModule)
  }
  // Future modules will be added here:
  // { path: 'representatives', loadChildren: () => import('./representatives/representatives.module').then(m => m.RepresentativesModule) },
  // { path: 'attachments', loadChildren: () => import('./attachments/attachments.module').then(m => m.AttachmentsModule) }
];

@NgModule({
  declarations: [
    RequestDetailsComponent,
    AdditionalInfoFormComponent,
    DeficienciesListComponent,
    RequestCompletionComponent
  ],
  imports: [
    CaseRegistrationSharedModule,
    CaseDataModule,
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class CaseRegistrationModule { }
