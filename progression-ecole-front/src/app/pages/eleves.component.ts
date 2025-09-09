import { Component, signal } from '@angular/core';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Eleve, EleveService } from '../services/eleve.service';

@Component({
  selector: 'app-eleves',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './eleves.component.html',
  styleUrls: ['./eleves.component.css']
})
export class ElevesComponent {
  eleves = signal<Eleve[]>([]);
  selected = signal<Eleve | null>(null);
  error = signal<string | null>(null);

  addNom = new FormControl('');
  addPrenom = new FormControl('');
  editNom = new FormControl('');
  editPrenom = new FormControl('');

  constructor(private eleveService: EleveService) {
    this.load();
  }

  load() {
    this.eleveService.getAll().subscribe({
      next: (data) => this.eleves.set(data),
      error: () => this.error.set('Erreur de chargement des élèves')
    });
  }

  select(eleve: Eleve) {
    this.selected.set({ ...eleve });
  this.editNom.setValue(eleve.nom);
  this.editPrenom.setValue(eleve.prenom);
    this.error.set(null);
  }

  save() {
    const sel = this.selected();
    if (sel) {
      const updated: Eleve = {
  ...sel,
  nom: this.editNom.value ?? '',
  prenom: this.editPrenom.value ?? ''
      };
      this.eleveService.update(updated).subscribe({
  next: () => { this.load(); this.selected.set(null); },
        error: () => this.error.set('Erreur lors de la modification')
      });
    }
  }

  add() {
    const nom = this.addNom.value ?? '';
    const prenom = this.addPrenom.value ?? '';
    console.log('Add button clicked. Nom:', nom, 'Prénom:', prenom);
    if (nom && prenom) {
      console.log('Calling EleveService.add with:', { nom, prenom });
      this.eleveService.add({ nom, prenom } as Eleve).subscribe({
        next: (res) => {
          console.log('Add success:', res);
          this.load();
          this.addNom.setValue('');
          this.addPrenom.setValue('');
        },
        error: (err) => {
          console.error('Add error:', err);
          this.error.set("Erreur lors de l'ajout");
        }
      });
    } else {
      console.warn('Nom ou prénom manquant');
    }
  }

  deleteEleve(id: number) {
    console.log('Suppression élève id:', id);
    this.eleveService.delete(id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Erreur lors de la suppression')
    });
  }

  cancel() {
    this.selected.set(null);
    this.error.set(null);
  }
}
