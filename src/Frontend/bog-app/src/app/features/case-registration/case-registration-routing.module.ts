import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RequestListComponent } from './requests/components/request-list/request-list.component';
import { RequestDetailsComponent } from './pages/request-details/request-details.component';

const routes: Routes = [
  {
    path: '',
    children: [
      { path: '', redirectTo: 'list', pathMatch: 'full' },
      {
        path: 'list',
        component: RequestListComponent,
        data: { title: 'قائمة طلبات التسجيل' }
      },
      {
        path: 'create',
        component: RequestDetailsComponent,
        data: { mode: 'create', title: 'طلب تسجيل دعوى جديد' }
      },
      {
        path: ':id/edit',
        component: RequestDetailsComponent,
        data: { mode: 'edit', title: 'تعديل طلب تسجيل الدعوى' }
      },
      {
        path: ':id/view',
        component: RequestDetailsComponent,
        data: { mode: 'view', title: 'عرض طلب تسجيل الدعوى' }
      },
      {
        path: 'defendants',
        loadChildren: () => import('./defendants/defendants.module').then(m => m.DefendantsModule)
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CaseRegistrationRoutingModule { }
