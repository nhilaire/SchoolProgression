import { Component, signal, OnInit } from '@angular/core';
import { NgClass } from '@angular/common';
import { EleveService, Eleve } from '../services/eleve.service';
import { ActiviteService } from '../services/activite.service';
import { Activite } from '../models/activite.model';
import { CategorieService } from '../services/categorie.service';
import { Categorie } from '../models/categorie.model';
import { PeriodeService, PeriodeActivites } from '../services/periode.service';
import { forkJoin } from 'rxjs';

const PERIODES = ['P1', 'P2', 'P3', 'P4', 'P5'];
const CLASSE_ORDER: Record<string, number> = { 'Petit': 0, 'Moyen': 1, 'Grand': 2 };

interface TableRow {
  type: 'regroupement' | 'activite';
  activite: Activite;
  isChild: boolean;
}

@Component({
  selector: 'app-saisie-masse',
  standalone: true,
  imports: [NgClass],
  template: `
    <h1>Saisie en masse</h1>

    @if (notification()) {
      <div class="toast-notification" [ngClass]="notificationColor() === 'success' ? 'toast-success' : 'toast-error'">
        {{ notification() }}
      </div>
    }

    @if (isLoading()) {
      <div class="loading">Chargement des donnees...</div>
    } @else {
      @for (eleve of sortedEleves(); track eleve.id) {
        <div class="eleve-block">
          <div class="eleve-header" [ngClass]="'classe-' + (eleve.classe || 'Petit')">
            {{ getClasseIcon(eleve.classe) }} {{ eleve.prenom }}
          </div>
          <table class="activite-table">
            <thead>
              <tr>
                <th class="activite-col">Activite</th>
                @for (p of periodes; track p) {
                  <th class="periode-col">{{ p }}</th>
                }
              </tr>
            </thead>
            <tbody>
              @for (group of activitesGrouped; track group.categorie.id) {
                <tr class="categorie-row">
                  <td [attr.colspan]="periodes.length + 1" [style.background-color]="group.categorie.couleur || '#e0e0e0'">
                    {{ group.categorie.libelle }}
                  </td>
                </tr>
                @for (row of group.items; track row.activite.id) {
                  @if (row.type === 'regroupement') {
                    <tr class="regroupement-row">
                      <td [attr.colspan]="periodes.length + 1" class="regroupement-label">
                        {{ row.activite.libelleCourt }}
                      </td>
                    </tr>
                  } @else {
                    <tr>
                      <td class="activite-label" [ngClass]="{ 'activite-child': row.isChild }">
                        @if (row.isChild) { <span class="indent">&#x2514;&#x2500;</span> }
                        {{ row.activite.libelleCourt }}
                      </td>
                      @for (p of periodes; track p) {
                        <td class="periode-cell"
                            [ngClass]="{ 'cell-checked': isChecked(eleve.id, p, row.activite.id) }"
                            (click)="toggleCell(eleve.id, p, row.activite.id)">
                          @if (isChecked(eleve.id, p, row.activite.id)) {
                            <span class="check-mark">&#10003;</span>
                          }
                        </td>
                      }
                    </tr>
                  }
                }
              }
            </tbody>
          </table>
        </div>
      }

      <div class="save-bar">
        <button class="save-btn" (click)="save()" [disabled]="isSaving()">
          {{ isSaving() ? 'Enregistrement...' : 'Enregistrer' }}
        </button>
      </div>
    }
  `,
  styles: [`
    h1 { text-align: center; margin-top: 0; }

    .loading { text-align: center; padding: 2rem; color: #6c757d; }

    .eleve-block {
      margin-bottom: 2rem;
      border: 1px solid #dee2e6;
      border-radius: 8px;
      overflow: hidden;
    }

    .eleve-header {
      padding: 0.6rem 1rem;
      font-weight: 600;
      font-size: 1.1rem;
    }
    .classe-Petit { background: rgba(76, 175, 80, 0.2); }
    .classe-Moyen { background: rgba(33, 150, 243, 0.2); }
    .classe-Grand { background: rgba(156, 39, 176, 0.2); }

    .activite-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.85rem;
    }
    .activite-table th, .activite-table td {
      border: 1px solid #dee2e6;
      padding: 0.1rem 0.5rem;
      line-height: 1.2;
    }
    .activite-col { text-align: left; }
    .periode-col { text-align: center; width: 50px; }

    .categorie-row td {
      font-weight: 600;
      font-size: 0.8rem;
      color: #333;
      padding: 0.25rem 0.5rem;
    }

    .regroupement-row td {
      background: #f8f9fa;
    }
    .regroupement-label {
      font-weight: 600;
      color: #495057;
      padding-left: 0.8rem !important;
    }

    .activite-label {
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      max-width: 300px;
    }
    .activite-child { padding-left: 1.5rem !important; }
    .indent { color: #6c757d; font-family: monospace; font-size: 0.9rem; margin-right: 0.3rem; }

    .periode-cell {
      text-align: center;
      cursor: pointer;
      user-select: none;
      transition: background 0.15s;
      width: 50px;
      height: 22px;
    }
    .periode-cell:hover { background: #e9ecef; }
    .cell-checked { background: #4CAF50 !important; }
    .cell-checked:hover { background: #43a047 !important; }
    .check-mark { color: #fff; font-weight: bold; font-size: 1rem; }

    .save-bar {
      position: sticky;
      bottom: 0;
      background: #fff;
      padding: 1rem;
      text-align: center;
      border-top: 2px solid #dee2e6;
      box-shadow: 0 -2px 8px rgba(0,0,0,0.1);
    }
    .save-btn {
      background: #ff9800;
      color: #fff;
      border: none;
      padding: 0.6rem 2rem;
      border-radius: 6px;
      font-size: 1.1rem;
      font-weight: 600;
      cursor: pointer;
      transition: background 0.2s;
    }
    .save-btn:hover { background: #fb8c00; }
    .save-btn:disabled { background: #ccc; cursor: not-allowed; }

    /* Toast */
    .toast-notification {
      position: fixed; top: 20px; right: 20px; z-index: 1000;
      padding: 0.75rem 1rem; border-radius: 6px; font-weight: 500;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      animation: slideIn 0.3s ease-out, fadeOut 4s ease-in-out forwards;
      min-width: 200px; max-width: 400px;
    }
    .toast-success { color: #155724; background: #d4edda; border: 1px solid #c3e6cb; }
    .toast-error { color: #721c24; background: #f8d7da; border: 1px solid #f5c6cb; }
    @keyframes slideIn { from { transform: translateX(100%); opacity: 0; } to { transform: translateX(0); opacity: 1; } }
    @keyframes fadeOut { 0%, 85% { opacity: 1; } 100% { opacity: 0; transform: translateX(100%); } }
  `]
})
export class SaisieMasseComponent implements OnInit {
  periodes = PERIODES;

  isLoading = signal(true);
  isSaving = signal(false);
  notification = signal<string | null>(null);
  notificationColor = signal<'success' | 'error' | null>(null);
  sortedEleves = signal<Eleve[]>([]);

  activitesGrouped: { categorie: Categorie; items: TableRow[] }[] = [];

  // Etat courant : cle = "eleveId_periode", valeur = Set d'activiteIds
  private state = new Map<string, Set<string>>();
  // Etat initial pour comparer au moment de la sauvegarde
  private initialState = new Map<string, Set<string>>();

  private notificationTimeout: any = null;

  constructor(
    private eleveService: EleveService,
    private activiteService: ActiviteService,
    private categorieService: CategorieService,
    private periodeService: PeriodeService,
  ) {}

  ngOnInit(): void {
    forkJoin({
      eleves: this.eleveService.getAll(),
      activites: this.activiteService.getAll(),
      categories: this.categorieService.getAll(),
      periodeActivites: this.periodeService.getAllPeriodeActivites(),
    }).subscribe({
      next: ({ eleves, activites, categories, periodeActivites }) => {
        // Trier les eleves par classe puis prenom
        const sorted = [...eleves].sort((a, b) => {
          const classeA = CLASSE_ORDER[a.classe] ?? 99;
          const classeB = CLASSE_ORDER[b.classe] ?? 99;
          if (classeA !== classeB) return classeA - classeB;
          return (a.prenom || '').localeCompare(b.prenom || '', 'fr');
        });
        this.sortedEleves.set(sorted);

        // Construire les lignes du tableau par categorie
        this.activitesGrouped = categories
          .map(cat => {
            const catActivites = activites
              .filter(a => a.categorieId === cat.id)
              .sort((a, b) => a.ordre - b.ordre);

            const items: TableRow[] = [];
            for (const act of catActivites) {
              if (act.estRegroupement) {
                // Header de regroupement
                items.push({ type: 'regroupement', activite: act, isChild: false });
                // Activites enfants du regroupement
                const enfants = activites
                  .filter(a => a.parentId === act.id)
                  .sort((a, b) => a.ordre - b.ordre);
                for (const enfant of enfants) {
                  items.push({ type: 'activite', activite: enfant, isChild: true });
                }
              } else if (!act.parentId) {
                // Activite isolee (pas enfant d'un regroupement)
                items.push({ type: 'activite', activite: act, isChild: false });
              }
            }
            return { categorie: cat, items };
          })
          .filter(g => g.items.length > 0);

        // Construire l'etat initial
        for (const pa of periodeActivites) {
          const key = `${pa.eleveId}_${pa.periode}`;
          this.state.set(key, new Set(pa.activiteIds));
        }
        // Copie profonde de l'etat initial
        for (const [key, value] of this.state) {
          this.initialState.set(key, new Set(value));
        }

        this.isLoading.set(false);
      },
      error: () => {
        this.showNotification('Erreur lors du chargement des donnees', 'error');
        this.isLoading.set(false);
      },
    });
  }

  getClasseIcon(classe: string): string {
    switch (classe) {
      case 'Petit': return '\u{1F7E2}';
      case 'Moyen': return '\u{1F535}';
      case 'Grand': return '\u{1F7E3}';
      default: return '\u{1F7E2}';
    }
  }

  isChecked(eleveId: number | string, periode: string, activiteId: string): boolean {
    const key = `${eleveId}_${periode}`;
    return this.state.get(key)?.has(activiteId) ?? false;
  }

  toggleCell(eleveId: number | string, periode: string, activiteId: string): void {
    const key = `${eleveId}_${periode}`;
    if (!this.state.has(key)) {
      this.state.set(key, new Set());
    }
    const set = this.state.get(key)!;
    if (set.has(activiteId)) {
      set.delete(activiteId);
    } else {
      set.add(activiteId);
    }
  }

  save(): void {
    // Trouver les combinaisons eleveId/periode qui ont change
    const allKeys = new Set([...this.state.keys(), ...this.initialState.keys()]);
    const toSave: PeriodeActivites[] = [];

    for (const key of allKeys) {
      const current = this.state.get(key);
      const initial = this.initialState.get(key);
      const currentIds = current ? Array.from(current).sort() : [];
      const initialIds = initial ? Array.from(initial).sort() : [];

      if (JSON.stringify(currentIds) !== JSON.stringify(initialIds)) {
        const [eleveId, periode] = key.split('_');
        toSave.push({ eleveId, periode, activiteIds: currentIds });
      }
    }

    if (toSave.length === 0) {
      this.showNotification('Aucune modification a enregistrer', 'success');
      return;
    }

    this.isSaving.set(true);
    let completed = 0;
    let errors = 0;

    for (const data of toSave) {
      this.periodeService.save(data).subscribe({
        next: () => {
          completed++;
          if (completed + errors === toSave.length) {
            this.onSaveComplete(errors);
          }
        },
        error: () => {
          errors++;
          if (completed + errors === toSave.length) {
            this.onSaveComplete(errors);
          }
        },
      });
    }
  }

  private onSaveComplete(errors: number): void {
    this.isSaving.set(false);
    if (errors === 0) {
      // Mettre a jour l'etat initial
      this.initialState.clear();
      for (const [key, value] of this.state) {
        this.initialState.set(key, new Set(value));
      }
      this.showNotification('Enregistrement reussi !', 'success');
    } else {
      this.showNotification(`Erreur : ${errors} enregistrement(s) en echec`, 'error');
    }
  }

  private showNotification(message: string, type: 'success' | 'error'): void {
    if (this.notificationTimeout) {
      clearTimeout(this.notificationTimeout);
    }
    this.notification.set(message);
    this.notificationColor.set(type);
    this.notificationTimeout = setTimeout(() => {
      this.notification.set(null);
      this.notificationColor.set(null);
    }, 3900);
  }
}
