import { NgModule } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { RequestListComponent } from './components/request-list/request-list.component';

@NgModule({
  declarations: [
    RequestListComponent
  ],
  imports: [
    SharedModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  exports: [
    RequestListComponent
  ]
})
export class RequestsModule { }
