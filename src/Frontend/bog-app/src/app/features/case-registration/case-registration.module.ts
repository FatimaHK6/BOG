import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'plaintiffs',
    pathMatch: 'full'
  },
  {
    path: 'requests',
    loadChildren: () => import('./requests/requests.module').then(m => m.RequestsModule)
  },
  {
    path: 'plaintiffs',
    loadChildren: () => import('./plaintiffs/plaintiffs.module').then(m => m.PlaintiffsModule)
  }
  // Future modules will be added here:
  // { path: 'representatives', loadChildren: () => import('./representatives/representatives.module').then(m => m.RepresentativesModule) },
  // { path: 'attachments', loadChildren: () => import('./attachments/attachments.module').then(m => m.AttachmentsModule) }
];

@NgModule({
  declarations: [],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class CaseRegistrationModule { }
