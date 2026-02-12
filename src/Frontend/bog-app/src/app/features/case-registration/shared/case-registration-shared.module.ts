import { NgModule } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';

// Shared components
import { SectionContainerComponent } from '../components/shared/section-container/section-container.component';
import { ValidationMessageComponent } from '../components/shared/validation-message/validation-message.component';
import { ConfirmationDialogComponent } from '../components/shared/confirmation-dialog/confirmation-dialog.component';

@NgModule({
  declarations: [
    SectionContainerComponent,
    ValidationMessageComponent,
    ConfirmationDialogComponent
  ],
  imports: [SharedModule],
  exports: [
    SectionContainerComponent,
    ValidationMessageComponent,
    ConfirmationDialogComponent,
    SharedModule  // Re-export for convenience
  ]
})
export class CaseRegistrationSharedModule { }
