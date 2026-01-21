import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { NotificationDialogComponent, NotificationDialogData, NotificationType } from '../../shared/components/notification-dialog/notification-dialog.component';

/**
 * Unified Notification Service
 * Displays notifications as modal dialogs for better UX
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  constructor(private dialog: MatDialog) {}

  /**
   * Show a success notification
   * @param message The message to display
   * @param title Optional custom title
   */
  success(message: string, title?: string): MatDialogRef<NotificationDialogComponent> {
    return this.show('success', message, title);
  }

  /**
   * Show an error notification
   * @param message The message to display
   * @param title Optional custom title
   */
  error(message: string, title?: string): MatDialogRef<NotificationDialogComponent> {
    return this.show('error', message, title);
  }

  /**
   * Show a warning notification
   * @param message The message to display
   * @param title Optional custom title
   */
  warning(message: string, title?: string): MatDialogRef<NotificationDialogComponent> {
    return this.show('warning', message, title);
  }

  /**
   * Show an info notification
   * @param message The message to display
   * @param title Optional custom title
   */
  info(message: string, title?: string): MatDialogRef<NotificationDialogComponent> {
    return this.show('info', message, title);
  }

  /**
   * Show a validation error notification
   * @param message The validation message to display
   */
  validation(message: string): MatDialogRef<NotificationDialogComponent> {
    return this.show('warning', message, 'تنبيه');
  }

  /**
   * Handle API error and show appropriate notification
   * @param error The error object from HTTP response
   * @param fallbackMessage Default message if error doesn't contain one
   */
  handleError(error: any, fallbackMessage: string = 'حدث خطأ غير متوقع'): MatDialogRef<NotificationDialogComponent> {
    const message = error?.error?.message || error?.message || fallbackMessage;
    return this.error(message);
  }

  /**
   * Internal method to show notification dialog
   */
  private show(
    type: NotificationType,
    message: string,
    title?: string
  ): MatDialogRef<NotificationDialogComponent> {
    const data: NotificationDialogData = {
      type,
      message,
      title
    };

    return this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data,
      disableClose: false,
      panelClass: 'notification-dialog-container',
      direction: 'rtl'
    });
  }

  /**
   * Dismiss all open dialogs
   */
  dismiss(): void {
    this.dialog.closeAll();
  }
}
