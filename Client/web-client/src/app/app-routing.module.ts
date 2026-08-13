import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MoodFormComponent } from './components/mood-form/mood-form.component';
import { AdminViewComponent } from './components/admin-view/admin-view.component';

const routes: Routes = [
  { path: '', component: MoodFormComponent },
  { path: 'admin', component: AdminViewComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }