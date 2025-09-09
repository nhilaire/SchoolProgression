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
  get sortedActivites(): Activite[] {
    return [...this.activites()].sort((a, b) => a.ordre - b.ordre);
  }
  categories = signal<Categorie[]>([]);
  selected = signal<Activite | null>(null);
  error = signal<string | null>(null);
  addLibelleCourt = new FormControl('');
  addLibelleLong = new FormControl('');
  addCategorieId = new FormControl('');
  addOrdre = new FormControl('');
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
    this.activiteService.getAll().subscribe({
      next: (data) => this.activites.set(data),
      error: () => this.error.set('Erreur de chargement des activités')
    });
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
        next: () => { this.load(); this.selected.set(null); },
        error: () => this.error.set('Erreur lors de la modification')
      });
    }
  }

  add() {
    const libelleCourt = this.addLibelleCourt.value ?? '';
    const libelleLong = this.addLibelleLong.value ?? '';
    const categorieId = this.addCategorieId.value ?? '';
    // Détermine le plus haut ordre actuel
    const maxOrdre = this.activites().length > 0 ? Math.max(...this.activites().map(a => a.ordre)) : 0;
    const ordre = maxOrdre + 1;
    if (libelleCourt && libelleLong && categorieId) {
      this.activiteService.add({ id: '', libelleCourt, libelleLong, categorieId, ordre } as Activite).subscribe({
        next: (res) => {
          this.load();
          this.addLibelleCourt.setValue('');
          this.addLibelleLong.setValue('');
          this.addCategorieId.setValue('');
        },
        error: (err) => {
          this.error.set("Erreur lors de l'ajout");
        }
      });
    } else {
      this.error.set('Tous les champs sont obligatoires');
      console.warn('add() - Champs manquants:', { libelleCourt, libelleLong, categorieId });
    }
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

  onDrop(event: any) {
    const sorted = [...this.sortedActivites];
    moveItemInArray(sorted, event.previousIndex, event.currentIndex);
    sorted.forEach((a: Activite, idx: number) => a.ordre = idx);
    sorted.forEach((a: Activite) => {
      this.activiteService.update(a).subscribe();
    });
    this.activites.set([...sorted]);
  }
}