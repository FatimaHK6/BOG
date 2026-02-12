import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Components
import { DefendantListComponent } from './components/defendant-list/defendant-list.component';
import { DefendantIndividualFormComponent } from './components/defendant-individual-form/defendant-individual-form.component';
import { DefendantRegisteredCompanyFormComponent } from './components/defendant-registered-company-form/defendant-registered-company-form.component';
import { DefendantUnregisteredCompanyFormComponent } from './components/defendant-unregistered-company-form/defendant-unregistered-company-form.component';
import { DefendantGovFormComponent } from './components/defendant-gov-form/defendant-gov-form.component';
import { DefendantNGOFormComponent } from './components/defendant-ngo-form/defendant-ngo-form.component';
import { DefendantWaqfFormComponent } from './components/defendant-waqf-form/defendant-waqf-form.component';
import { DefendantBusinessOwnerFormComponent } from './components/defendant-business-owner-form/defendant-business-owner-form.component';

const routes: Routes = [
  {
    path: '',
    component: DefendantListComponent,
    data: { title: 'قائمة المدعى عليهم' }
  },
  // Add defendant routes
  {
    path: 'add/individual',
    component: DefendantIndividualFormComponent,
    data: { title: 'إضافة مدعى عليه - فرد' }
  },
  {
    path: 'add/registered-company',
    component: DefendantRegisteredCompanyFormComponent,
    data: { title: 'إضافة مدعى عليه - شركة مسجلة' }
  },
  {
    path: 'add/unregistered-company',
    component: DefendantUnregisteredCompanyFormComponent,
    data: { title: 'إضافة مدعى عليه - شركة غير مسجلة' }
  },
  {
    path: 'add/government-agency',
    component: DefendantGovFormComponent,
    data: { title: 'إضافة مدعى عليه - جهة حكومية' }
  },
  {
    path: 'add/ngo',
    component: DefendantNGOFormComponent,
    data: { title: 'إضافة مدعى عليه - جمعية/مؤسسة أهلية' }
  },
  {
    path: 'add/waqf',
    component: DefendantWaqfFormComponent,
    data: { title: 'إضافة مدعى عليه - وقف' }
  },
  {
    path: 'add/business-owner',
    component: DefendantBusinessOwnerFormComponent,
    data: { title: 'إضافة مدعى عليه - صاحب مؤسسة' }
  },
  // View defendant routes
  {
    path: 'view/individual/:id',
    component: DefendantIndividualFormComponent,
    data: { title: 'عرض المدعى عليه - فرد' }
  },
  {
    path: 'view/registered-company/:id',
    component: DefendantRegisteredCompanyFormComponent,
    data: { title: 'عرض المدعى عليه - شركة مسجلة' }
  },
  {
    path: 'view/unregistered-company/:id',
    component: DefendantUnregisteredCompanyFormComponent,
    data: { title: 'عرض المدعى عليه - شركة غير مسجلة' }
  },
  {
    path: 'view/government-agency/:id',
    component: DefendantGovFormComponent,
    data: { title: 'عرض المدعى عليه - جهة حكومية' }
  },
  {
    path: 'view/ngo/:id',
    component: DefendantNGOFormComponent,
    data: { title: 'عرض المدعى عليه - جمعية/مؤسسة أهلية' }
  },
  {
    path: 'view/waqf/:id',
    component: DefendantWaqfFormComponent,
    data: { title: 'عرض المدعى عليه - وقف' }
  },
  {
    path: 'view/business-owner/:id',
    component: DefendantBusinessOwnerFormComponent,
    data: { title: 'عرض المدعى عليه - صاحب مؤسسة' }
  },
  // Edit defendant routes
  {
    path: 'edit/individual/:id',
    component: DefendantIndividualFormComponent,
    data: { title: 'تعديل المدعى عليه - فرد' }
  },
  {
    path: 'edit/registered-company/:id',
    component: DefendantRegisteredCompanyFormComponent,
    data: { title: 'تعديل المدعى عليه - شركة مسجلة' }
  },
  {
    path: 'edit/unregistered-company/:id',
    component: DefendantUnregisteredCompanyFormComponent,
    data: { title: 'تعديل المدعى عليه - شركة غير مسجلة' }
  },
  {
    path: 'edit/government-agency/:id',
    component: DefendantGovFormComponent,
    data: { title: 'تعديل المدعى عليه - جهة حكومية' }
  },
  {
    path: 'edit/ngo/:id',
    component: DefendantNGOFormComponent,
    data: { title: 'تعديل المدعى عليه - جمعية/مؤسسة أهلية' }
  },
  {
    path: 'edit/waqf/:id',
    component: DefendantWaqfFormComponent,
    data: { title: 'تعديل المدعى عليه - وقف' }
  },
  {
    path: 'edit/business-owner/:id',
    component: DefendantBusinessOwnerFormComponent,
    data: { title: 'تعديل المدعى عليه - صاحب مؤسسة' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DefendantsRoutingModule { }
