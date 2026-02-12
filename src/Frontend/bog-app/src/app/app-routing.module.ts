import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    redirectTo: '/case-registration/list',
    pathMatch: 'full'
  },
  {
    path: 'case-registration',
    loadChildren: () => import('./features/case-registration/case-registration.module').then(m => m.CaseRegistrationModule)
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
