import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-section-container',
  template: `
    <div class="section-container" [id]="sectionId">
      <div class="section-header">
        <h2>{{ title }}</h2>
        <mat-icon class="section-icon" *ngIf="icon">{{ icon }}</mat-icon>
        <span class="section-count" *ngIf="count !== null">{{ count }}</span>
        <div class="section-actions" *ngIf="showActions">
          <ng-content select="[slot='header-actions']"></ng-content>
        </div>
      </div>
      <div class="section-content">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .section-container {
      margin-bottom: 32px;
      overflow: visible !important;

      .section-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding-bottom: 16px;
        border-bottom: 2px solid #E0E0E0;
        margin-bottom: 16px;

        h2 {
          margin: 0;
          font-size: 20px;
          font-weight: 600;
          color: #1B5E20;
          flex: 1;
        }

        .section-icon {
          font-size: 24px;
          width: 24px;
          height: 24px;
          color: #1B5E20;
          margin: 0 16px;
        }

        .section-count {
          background: #1B5E20;
          color: white;
          padding: 4px 12px;
          border-radius: 12px;
          font-size: 14px;
          font-weight: 600;
        }

        .section-actions {
          display: flex;
          gap: 8px;
          margin-left: auto;
        }
      }

      .section-content {
        padding: 8px 0;
        overflow: visible !important;
      }
    }
  `]
})
export class SectionContainerComponent {
  @Input() title!: string;
  @Input() sectionId!: string;
  @Input() icon?: string;
  @Input() count: number | null = null;
  @Input() showActions = false;
}
