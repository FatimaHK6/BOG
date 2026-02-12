import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-validation-message',
  template: `
    <div class="validation-message" [class]="'validation-' + type">
      <mat-icon>{{ getIcon() }}</mat-icon>
      <span>{{ message }}</span>
    </div>
  `,
  styles: [`
    .validation-message {
      display: flex;
      align-items: center;
      padding: 12px 16px;
      border-radius: 4px;
      margin: 8px 0;
      gap: 12px;

      mat-icon {
        font-size: 20px;
        width: 20px;
        height: 20px;
      }

      span {
        flex: 1;
        font-size: 14px;
      }

      &.validation-error {
        background: #FFEBEE;
        color: #C62828;
        border-right: 4px solid #F44336;
      }

      &.validation-warning {
        background: #FFF3E0;
        color: #E65100;
        border-right: 4px solid #FF9800;
      }

      &.validation-info {
        background: #E3F2FD;
        color: #1565C0;
        border-right: 4px solid #2196F3;
      }

      &.validation-success {
        background: #E8F5E9;
        color: #2E7D32;
        border-right: 4px solid #4CAF50;
      }
    }
  `]
})
export class ValidationMessageComponent {
  @Input() type: 'error' | 'warning' | 'info' | 'success' = 'error';
  @Input() message!: string;

  getIcon(): string {
    const icons: Record<string, string> = {
      error: 'error',
      warning: 'warning',
      info: 'info',
      success: 'check_circle'
    };
    return icons[this.type] || 'error';
  }
}
