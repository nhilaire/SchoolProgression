import { Component, signal, effect, OnDestroy } from '@angular/core';
import { NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../services/api.service';
import { CategorieService } from '../services/categorie.service';
import { Categorie } from '../models/categorie.model';
import { ActiviteService } from '../services/activite.service';
import { Activite } from '../models/activite.model';
import { PeriodeService } from '../services/periode.service';
import { EleveService, Eleve } from '../services/eleve.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [NgClass, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnDestroy {
  generateDocx() {
    if (!this.selectedPeriode()) 
      return;
    this.periodeService.generate(this.selectedPeriode()).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Progression_${this.selectedPeriode()}.docx`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.showNotification("Erreur lors de la génération du document", 'error');
      }
    });
  }
  eleves = signal<Eleve[]>([]);
  selectedEleve = signal<number | null>(null);
  periodesActivites: { periode: string; activiteIds: string[] }[] = [];

  getPeriodeForActivite(id: string): string | null {
  // Trouver toutes les périodes associées à l'activité
  const periodes = this.periodesActivites.filter(p => p.activiteIds.includes(id)).map(p => p.periode);
  if (periodes.length === 0) return null;
  // Trier selon l'ordre P1 < P2 < ... < P5
  const ordre = ['P1', 'P2', 'P3', 'P4', 'P5'];
  periodes.sort((a, b) => ordre.indexOf(a) - ordre.indexOf(b));
  return periodes[periodes.length - 1];
  }
  savePeriodeActivites() {
    if (!this.selectedEleve()) return;
    const activiteIds: string[] = [];
    for (const id of Object.keys(this.selectedActivites)) {
      // On ajoute l'activité à la période sélectionnée même si elle existe déjà pour une autre période
      if (this.selectedActivites[id]) {
        activiteIds.push(id);
      }
    }
    const data = {
      eleveId: String(this.selectedEleve()),
      periode: this.selectedPeriode(),
      activiteIds
    };
    this.periodeService.save(data).subscribe({
      next: () => {
        this.showNotification('Enregistrement réussi !', 'success');
      },
      error: () => {
        this.showNotification("Erreur lors de l'enregistrement", 'error');
      }
    });
  }
  activites = signal<Activite[]>([]);
  status = signal<'ok' | 'ko' | null>(null);
  categories = signal<Categorie[]>([]);
  selectedTab = signal<string | null>(null);
  selectedPeriode = signal<string>('P1');
  selectedActivites: { [id: string]: boolean } = {};
  notification = signal<string | null>(null);
  notificationColor = signal<'success' | 'error' | null>(null);
  private notificationTimeout: any = null;

  constructor(
    private apiService: ApiService,
    private categorieService: CategorieService,
    private activiteService: ActiviteService,
    private periodeService: PeriodeService,
    private eleveService: EleveService
  ) {
      // Charger tous les élèves
      this.eleveService.getAll().subscribe({
        next: (eleves) => this.eleves.set(eleves),
        error: () => this.eleves.set([])
      });
      // Charger toutes les activités
      this.activiteService.getAll().subscribe({
        next: (acts) => this.activites.set(acts),
        error: () => this.activites.set([])
      });

      // Vérifier la connexion API
      this.apiService.ping().subscribe({
        next: (res) => {
          if (res.replace(/"/g, '').trim() === 'pong') {
            this.status.set('ok');
          } else {
            this.status.set('ko');
          }
        },
        error: () => {
          this.status.set('ko');
        },
      });

      // Charger les catégories
      this.categorieService.getAll().subscribe({
        next: (cats) => {
          this.categories.set(cats);
          if (cats.length > 0) {
            this.selectedTab.set(cats[0].id);
          }
        },
        error: () => {
          this.categories.set([]);
        }
      });

    // Charger toutes les activités réalisées (toutes périodes confondues) au démarrage
    effect(() => {
      if (!this.selectedEleve()) {
        this.periodesActivites = [];
        this.selectedActivites = {};
        return;
      }
      this.periodeService.getAll(String(this.selectedEleve())).subscribe({
        next: (datas) => {
          this.periodesActivites = datas;
          this.selectedActivites = {};
          // Ne pas pré-cocher les activités déjà réalisées
        },
        error: () => {
          this.selectedActivites = {};
          this.periodesActivites = [];
        }
      });
    });
  }

  getActivitesForSelectedCategorie(): Activite[] {
    const catId = this.selectedTab();
    if (!catId) return [];
    return this.activites().filter(a => a.categorieId === catId);
  }

  getEnfantsForActivite(parentId: string): Activite[] {
    return this.activites().filter(a => a.parentId === parentId).sort((a, b) => a.ordre - b.ordre);
  }

  getActiviteItemClass(activite: Activite): string {
    if (activite.estRegroupement) {
      return 'regroupement-item';
    } else if (activite.parentId) {
      return 'activite-enfant-item';
    } else {
      return 'activite-isolee-item';
    }
  }

  private showNotification(message: string, type: 'success' | 'error'): void {
    // Nettoyer le timeout précédent s'il existe
    if (this.notificationTimeout) {
      clearTimeout(this.notificationTimeout);
    }

    // Afficher la notification
    this.notification.set(message);
    this.notificationColor.set(type);

    // Programmer la disparition après 4 secondes (légèrement avant la fin de l'animation)
    this.notificationTimeout = setTimeout(() => {
      this.notification.set(null);
      this.notificationColor.set(null);
      this.notificationTimeout = null;
    }, 3900);
  }

  ngOnDestroy(): void {
    if (this.notificationTimeout) {
      clearTimeout(this.notificationTimeout);
    }
  }
}
