import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../../shared/shared.module';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
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
    RouterModule.forChild(routes),
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatIconModule
  ]
})
export class RequestsModule { }
