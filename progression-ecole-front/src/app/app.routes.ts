
import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home.component';
import { ElevesComponent } from './pages/eleves.component';
import { CategoriesComponent } from './pages/categories.component';
import { ActivitesComponent } from './pages/activites.component';

export const routes: Routes = [
	{ path: '', component: HomeComponent },
	{ path: 'eleves', component: ElevesComponent },
	{ path: 'categories', component: CategoriesComponent },
	{ path: 'activites', component: ActivitesComponent },
];
