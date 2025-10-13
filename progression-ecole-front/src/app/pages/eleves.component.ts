import { Component, signal } from '@angular/core';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { CdkDragDrop, moveItemInArray, DragDropModule } from '@angular/cdk/drag-drop';
import { Eleve, EleveService } from '../services/eleve.service';

@Component({
  selector: 'app-eleves',
  standalone: true,
  imports: [ReactiveFormsModule, DragDropModule],
  templateUrl: './eleves.component.html',
  styleUrls: ['./eleves.component.css']
})
export class ElevesComponent {
  eleves = signal<Eleve[]>([]);
  selected = signal<Eleve | null>(null);
  error = signal<string | null>(null);

  addPrenom = new FormControl('');
  addClasse = new FormControl('');
  editPrenom = new FormControl('');
  editClasse = new FormControl('');

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
    this.editPrenom.setValue(eleve.prenom);
    this.editClasse.setValue(eleve.classe || 'Petit');
    this.error.set(null);
  }

  save() {
    const sel = this.selected();
    if (sel) {
      const updated: Eleve = {
        ...sel,
        nom: '_', // Valeur par défaut masquée
        prenom: this.editPrenom.value ?? '',
        classe: this.editClasse.value ?? 'Petit'
      };
      this.eleveService.update(updated).subscribe({
        next: () => { this.load(); this.selected.set(null); },
        error: () => this.error.set('Erreur lors de la modification')
      });
    }
  }

  add() {
    const prenom = this.addPrenom.value ?? '';
    const classe = this.addClasse.value ?? '';
    console.log('Add button clicked. Prénom:', prenom, 'Classe:', classe);
    if (prenom && classe) {
      console.log('Calling EleveService.add with:', { nom: '_', prenom, classe });
      this.eleveService.add({ nom: '_', prenom, classe } as Eleve).subscribe({
        next: (res) => {
          console.log('Add success:', res);
          this.load();
          this.addPrenom.setValue('');
          this.addClasse.setValue('');
        },
        error: (err) => {
          console.error('Add error:', err);
          this.error.set("Erreur lors de l'ajout");
        }
      });
    } else {
      console.warn('Prénom ou classe manquant');
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

  onDropEleves(event: CdkDragDrop<Eleve[]>) {
    const currentEleves = [...this.eleves()];
    moveItemInArray(currentEleves, event.previousIndex, event.currentIndex);
    
    // Mise à jour immédiate de l'affichage
    this.eleves.set(currentEleves);
    
    // Sauvegarde sur le serveur
    this.eleveService.reorganize(currentEleves).subscribe({
      next: () => {
        // Pas besoin de recharger, on a déjà mis à jour l'affichage
        console.log('Réorganisation des élèves réussie');
      },
      error: (err) => {
        console.error('Erreur lors de la réorganisation:', err);
        this.error.set('Erreur lors de la réorganisation');
        // En cas d'erreur, on recharge pour revenir à l'état précédent
        this.load();
      }
    });
  }
}
