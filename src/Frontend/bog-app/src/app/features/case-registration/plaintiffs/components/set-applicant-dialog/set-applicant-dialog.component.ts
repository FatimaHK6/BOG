import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { PlaintiffListVM } from '../../../../../core/models/plaintiff.model';

export interface SetApplicantDialogData {
  plaintiff: PlaintiffListVM;
}

@Component({
  selector: 'app-set-applicant-dialog',
  templateUrl: './set-applicant-dialog.component.html',
  styleUrls: ['./set-applicant-dialog.component.scss']
})
export class SetApplicantDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<SetApplicantDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SetApplicantDialogData
  ) { }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}
