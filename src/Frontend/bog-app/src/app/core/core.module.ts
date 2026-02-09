import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

// Services
import { PlaintiffService } from './services/plaintiff.service';
import { RepresentativeService } from './services/representative.service';
import { AttachmentService } from './services/attachment.service';
import { LookupService } from './services/lookup.service';
import { AbsherService } from './services/absher.service';

@NgModule({
  imports: [
    CommonModule,
    HttpClientModule
  ],
  providers: [
    PlaintiffService,
    RepresentativeService,
    AttachmentService,
    LookupService,
    AbsherService
  ]
})
export class CoreModule {
  // Prevent reimport of CoreModule
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error('CoreModule is already loaded. Import it in the AppModule only.');
    }
  }
}
