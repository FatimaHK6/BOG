import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-deficiencies-list',
  template: `
    <app-section-container title="النواقص" sectionId="deficiencies" icon="warning">
      <div class="deficiencies-section">
        <!-- BR05: Auto-reject deadline -->
        <div class="deadline-warning">
          <mat-icon>schedule</mat-icon>
          <div>
            <strong>الموعد النهائي للإكمال</strong>
            <p>سيتم رفض الطلب تلقائياً بعد 30 يوماً من تاريخ الطلب (BR05)</p>
          </div>
        </div>

        <!-- TODO: Load deficiencies from API -->
        <div class="empty-state">
          <p>لا توجد نواقص</p>
        </div>
      </div>
    </app-section-container>
  `,
  styles: [`
    .deficiencies-section { padding: 16px 0; }
    .deadline-warning {
      display: flex;
      gap: 16px;
      padding: 16px;
      background: #FFF3E0;
      border-radius: 4px;
      border-right: 4px solid #FF9800;
      color: #E65100;
      margin-bottom: 16px;
    }
    .deadline-warning mat-icon { flex-shrink: 0; }
    .deadline-warning strong { display: block; margin-bottom: 4px; }
    .deadline-warning p { margin: 0; font-size: 14px; }
    .empty-state { text-align: center; padding: 20px; color: #999; }
  `]
})
export class DeficienciesListComponent implements OnInit {
  @Input() requestId!: number;

  constructor() { }

  ngOnInit() {
    // TODO: Load deficiencies from API
  }
}
