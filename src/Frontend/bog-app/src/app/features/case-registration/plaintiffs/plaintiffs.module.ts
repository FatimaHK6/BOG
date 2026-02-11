import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../../shared/shared.module';

// Components
import { PlaintiffListComponent } from './components/plaintiff-list/plaintiff-list.component';
import { PlaintiffFormComponent } from './components/plaintiff-form/plaintiff-form.component';
import { SetApplicantDialogComponent } from './components/set-applicant-dialog/set-applicant-dialog.component';
import { RepresentativeDialogComponent } from './components/representative-dialog/representative-dialog.component';
import { AttachmentUploadComponent } from './components/attachment-upload/attachment-upload.component';
import { AttachmentNoteDialogComponent } from './components/attachment-upload/attachment-note-dialog.component';

const routes: Routes = [
  { path: '', component: PlaintiffListComponent },
  { path: 'add', component: PlaintiffFormComponent },
  { path: ':id/edit', component: PlaintiffFormComponent },
  { path: ':id/view', component: PlaintiffFormComponent }
];

@NgModule({
  declarations: [
    PlaintiffListComponent,
    PlaintiffFormComponent,
    SetApplicantDialogComponent,
    RepresentativeDialogComponent,
    AttachmentUploadComponent,
    AttachmentNoteDialogComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ],
  exports: [
    PlaintiffListComponent,
    PlaintiffFormComponent,
    AttachmentUploadComponent
  ]
})
export class PlaintiffsModule { }
