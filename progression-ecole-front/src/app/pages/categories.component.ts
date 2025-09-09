import { Component, signal } from '@angular/core';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { CategorieService } from '../services/categorie.service';
import { Categorie } from '../models/categorie.model';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.css']
})
export class CategoriesComponent {
  categories = signal<Categorie[]>([]);
  selected = signal<Categorie | null>(null);
  error = signal<string | null>(null);

  addLibelle = new FormControl('');
  editLibelle = new FormControl('');

  constructor(private categorieService: CategorieService) {
    this.load();
  }

  load() {
    this.categorieService.getAll().subscribe({
      next: (data) => this.categories.set(data),
      error: () => this.error.set('Erreur de chargement des catégories')
    });
  }

  select(categorie: Categorie) {
    this.selected.set({ ...categorie });
    this.editLibelle.setValue(categorie.libelle);
    this.error.set(null);
  }

  save() {
    const sel = this.selected();
    if (sel) {
      const updated: Categorie = {
        ...sel,
        libelle: this.editLibelle.value ?? ''
      };
      this.categorieService.update(updated).subscribe({
        next: () => { this.load(); this.selected.set(null); },
        error: () => this.error.set('Erreur lors de la modification')
      });
    }
  }

  add() {
    const libelle = this.addLibelle.value ?? '';
    if (libelle) {
      this.categorieService.add({ libelle } as Categorie).subscribe({
        next: (res) => {
          this.load();
          this.addLibelle.setValue('');
        },
        error: (err) => {
          this.error.set("Erreur lors de l'ajout");
        }
      });
    } else {
      this.error.set('Libellé manquant');
    }
  }

  deleteCategorie(id: string) {
    this.categorieService.delete(id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Erreur lors de la suppression')
    });
  }

  cancel() {
    this.selected.set(null);
    this.error.set(null);
  }
}
