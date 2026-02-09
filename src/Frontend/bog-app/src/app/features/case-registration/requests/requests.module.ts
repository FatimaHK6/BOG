import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../../shared/shared.module';
import { RequestListComponent } from './components/request-list/request-list.component';

const routes: Routes = [
  {
    path: '',
    component: RequestListComponent
  }
];

@NgModule({
  declarations: [
    RequestListComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class RequestsModule { }
