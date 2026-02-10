
import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home.component';
import { ElevesComponent } from './pages/eleves.component';
import { CategoriesComponent } from './pages/categories.component';
import { ActivitesComponent } from './pages/activites.component';
import { PeriodesComponent } from './pages/periodes.component';
import { SaisieMasseComponent } from './pages/saisie-masse.component';

export const routes: Routes = [
	{ path: '', component: HomeComponent },
	{ path: 'eleves', component: ElevesComponent },
	{ path: 'categories', component: CategoriesComponent },
	{ path: 'activites', component: ActivitesComponent },
	{ path: 'periodes', component: PeriodesComponent },
	{ path: 'saisie-masse', component: SaisieMasseComponent },
];
