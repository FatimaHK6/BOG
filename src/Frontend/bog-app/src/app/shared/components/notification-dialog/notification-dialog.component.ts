import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface NotificationDialogData {
  type: NotificationType;
  title?: string;
  message: string;
  buttonText?: string;
}

@Component({
  selector: 'app-notification-dialog',
  templateUrl: './notification-dialog.component.html',
  styleUrls: ['./notification-dialog.component.scss']
})
export class NotificationDialogComponent {

  // Type-specific configurations
  private readonly typeConfig = {
    success: {
      icon: 'check_circle',
      title: 'تمت العملية بنجاح',
      buttonText: 'حسناً'
    },
    error: {
      icon: 'error',
      title: 'حدث خطأ',
      buttonText: 'حسناً'
    },
    warning: {
      icon: 'warning',
      title: 'تنبيه',
      buttonText: 'حسناً'
    },
    info: {
      icon: 'info',
      title: 'معلومة',
      buttonText: 'حسناً'
    }
  };

  icon: string;
  title: string;
  buttonText: string;

  constructor(
    public dialogRef: MatDialogRef<NotificationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: NotificationDialogData
  ) {
    const config = this.typeConfig[data.type];
    this.icon = config.icon;
    this.title = data.title || config.title;
    this.buttonText = data.buttonText || config.buttonText;
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
