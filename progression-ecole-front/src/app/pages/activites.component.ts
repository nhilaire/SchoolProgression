import { Component, signal } from '@angular/core';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { Activite } from '../models/activite.model';
import { ActiviteService } from '../services/activite.service';
import { CategorieService } from '../services/categorie.service';
import { Categorie } from '../models/categorie.model';

@Component({
  selector: 'app-activites',
  standalone: true,
  imports: [ReactiveFormsModule, DragDropModule],
  templateUrl: './activites.component.html',
  styleUrls: ['./activites.component.css']
})
export class ActivitesComponent {
  activites = signal<Activite[]>([]);
  regroupements = signal<Activite[]>([]);
  activitesIsolees = signal<Activite[]>([]);
  enfantsMap = signal<Map<string, Activite[]>>(new Map());
  
  categories = signal<Categorie[]>([]);
  selected = signal<Activite | null>(null);
  error = signal<string | null>(null);
  addingToRegroupement = signal<string | null>(null);

  // Contrôles pour les activités isolées
  addLibelleCourt = new FormControl('');
  addLibelleLong = new FormControl('');
  addCategorieId = new FormControl('');

  // Contrôles pour les regroupements
  addRegroupementLibelleCourt = new FormControl('');
  addRegroupementLibelleLong = new FormControl('');
  addRegroupementCategorieId = new FormControl('');

  // Contrôles d'édition
  editLibelleCourt = new FormControl('');
  editLibelleLong = new FormControl('');
  editCategorieId = new FormControl('');
  editOrdre = new FormControl('');

  constructor(private activiteService: ActiviteService, private categorieService: CategorieService) {
    this.load();
    this.loadCategories();
  }

  getCategorieLibelle(categorieId: string): string {
    const cat = this.categories().find((c: Categorie) => c.id === categorieId);
    return cat ? cat.libelle : 'Non défini';
  }

  load() {
    // Charger tous les types d'activités
    this.loadRegroupements();
    this.loadActivitesIsolees();
    this.loadAllActivites();
  }

  loadRegroupements() {
    this.activiteService.getRegroupements().subscribe({
      next: (data) => {
        this.regroupements.set(data);
        // Charger les enfants pour chaque regroupement
        data.forEach(regroupement => {
          this.loadEnfants(regroupement.id);
        });
      },
      error: () => this.error.set('Erreur de chargement des regroupements')
    });
  }

  loadActivitesIsolees() {
    this.activiteService.getActivitesIsolees().subscribe({
      next: (data) => this.activitesIsolees.set(data),
      error: () => this.error.set('Erreur de chargement des activités isolées')
    });
  }

  loadAllActivites() {
    this.activiteService.getAll().subscribe({
      next: (data) => this.activites.set(data),
      error: () => this.error.set('Erreur de chargement des activités')
    });
  }

  loadEnfants(parentId: string) {
    this.activiteService.getEnfants(parentId).subscribe({
      next: (enfants) => {
        const currentMap = new Map(this.enfantsMap());
        currentMap.set(parentId, enfants);
        this.enfantsMap.set(currentMap);
      },
      error: () => this.error.set('Erreur de chargement des activités enfants')
    });
  }

  getEnfants(parentId: string): Activite[] {
    return this.enfantsMap().get(parentId) || [];
  }

  loadCategories() {
    this.categorieService.getAll().subscribe({
      next: (data) => this.categories.set(data),
      error: () => this.error.set('Erreur de chargement des catégories')
    });
  }

  select(activite: Activite) {
    this.selected.set({ ...activite });
    this.editLibelleCourt.setValue(activite.libelleCourt);
    this.editLibelleLong.setValue(activite.libelleLong);
    this.editCategorieId.setValue(activite.categorieId);
    this.editOrdre.setValue(String(activite.ordre));
    this.error.set(null);
  }

  save() {
    const sel = this.selected();
    if (sel) {
      const updated: Activite = {
        ...sel,
        libelleCourt: this.editLibelleCourt.value ?? '',
        libelleLong: this.editLibelleLong.value ?? '',
        categorieId: this.editCategorieId.value ?? '',
        ordre: Number(this.editOrdre.value ?? 0)
      };
      this.activiteService.update(updated).subscribe({
        next: () => { 
          this.load(); 
          this.selected.set(null); 
        },
        error: () => this.error.set('Erreur lors de la modification')
      });
    }
  }

  addRegroupement() {
    const libelleCourt = this.addRegroupementLibelleCourt.value ?? '';
    const libelleLong = this.addRegroupementLibelleLong.value ?? '';
    const categorieId = this.addRegroupementCategorieId.value ?? '';

    if (libelleCourt && libelleLong && categorieId) {
      const regroupement: Activite = {
        id: '',
        libelleCourt,
        libelleLong,
        categorieId,
        ordre: this.regroupements().length + 1,
        estRegroupement: true,
        parentId: null
      };

      this.activiteService.createRegroupement(regroupement).subscribe({
        next: () => {
          this.load();
          this.addRegroupementLibelleCourt.setValue('');
          this.addRegroupementLibelleLong.setValue('');
          this.addRegroupementCategorieId.setValue('');
        },
        error: () => this.error.set("Erreur lors de la création du regroupement")
      });
    } else {
      this.error.set('Tous les champs sont obligatoires');
    }
  }

  addActiviteIsolee() {
    const libelleCourt = this.addLibelleCourt.value ?? '';
    const libelleLong = this.addLibelleLong.value ?? '';
    const categorieId = this.addCategorieId.value ?? '';

    if (libelleCourt && libelleLong && categorieId) {
      const activite: Activite = {
        id: '',
        libelleCourt,
        libelleLong,
        categorieId,
        ordre: this.activitesIsolees().length + 1,
        estRegroupement: false,
        parentId: null
      };

      this.activiteService.add(activite).subscribe({
        next: () => {
          this.load();
          this.addLibelleCourt.setValue('');
          this.addLibelleLong.setValue('');
          this.addCategorieId.setValue('');
        },
        error: () => this.error.set("Erreur lors de l'ajout de l'activité")
      });
    } else {
      this.error.set('Tous les champs sont obligatoires');
    }
  }

  startAddingToRegroupement(regroupementId: string) {
    this.addingToRegroupement.set(regroupementId);
    this.addLibelleCourt.setValue('');
    this.addLibelleLong.setValue('');
  }

  addActiviteToRegroupement(parentId: string) {
    const libelleCourt = this.addLibelleCourt.value ?? '';
    const libelleLong = this.addLibelleLong.value ?? '';

    if (libelleCourt && libelleLong) {
      const regroupement = this.regroupements().find(r => r.id === parentId);
      if (!regroupement) return;

      const enfantsActuels = this.getEnfants(parentId);
      const activite: Activite = {
        id: '',
        libelleCourt,
        libelleLong,
        categorieId: regroupement.categorieId, // Même catégorie que le parent
        ordre: enfantsActuels.length + 1,
        estRegroupement: false,
        parentId: parentId
      };

      this.activiteService.addActiviteToRegroupement(activite).subscribe({
        next: () => {
          this.loadEnfants(parentId);
          this.addingToRegroupement.set(null);
          this.addLibelleCourt.setValue('');
          this.addLibelleLong.setValue('');
        },
        error: () => this.error.set("Erreur lors de l'ajout de l'activité au regroupement")
      });
    } else {
      this.error.set('Tous les champs sont obligatoires');
    }
  }

  cancelAddingToRegroupement() {
    this.addingToRegroupement.set(null);
    this.addLibelleCourt.setValue('');
    this.addLibelleLong.setValue('');
  }

  deleteActivite(id: string) {
    this.activiteService.delete(id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Erreur lors de la suppression')
    });
  }

  cancel() {
    this.selected.set(null);
    this.error.set(null);
  }

  // Gardé pour compatibilité avec l'ancien système
  get sortedActivites(): Activite[] {
    return [...this.activites()].sort((a, b) => a.ordre - b.ordre);
  }

  onDrop(event: any) {
    // TODO: Implémenter le drag & drop pour les regroupements si nécessaire
  }
}