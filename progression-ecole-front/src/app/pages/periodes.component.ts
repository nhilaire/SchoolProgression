import { Component, signal } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { DefinitionPeriodeService } from '../services/definition-periode.service';
import { Periode } from '../models/periode.model';

@Component({
  selector: 'app-periodes',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './periodes.component.html',
  styleUrls: ['./periodes.component.css']
})
export class PeriodesComponent {
  periodes = signal<Periode[]>([]);
  selected = signal<Periode | null>(null);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  editDateDebut = new FormControl('');
  editDateFin = new FormControl('');

  constructor(private periodeService: DefinitionPeriodeService) {
    this.load();
  }

  load() {
    this.periodeService.getAll().subscribe({
      next: (data) => this.periodes.set(data),
      error: () => this.error.set('Erreur de chargement des périodes')
    });
  }

  select(periode: Periode) {
    this.selected.set({ ...periode });
    this.editDateDebut.setValue(this.formatDateForInput(periode.dateDebut));
    this.editDateFin.setValue(this.formatDateForInput(periode.dateFin));
    this.error.set(null);
    this.success.set(null);
  }

  save() {
    const sel = this.selected();
    if (sel) {
      const dateDebut = this.editDateDebut.value;
      const dateFin = this.editDateFin.value;

      if (!dateDebut || !dateFin) {
        this.error.set('Les deux dates sont requises');
        return;
      }

      if (new Date(dateDebut) >= new Date(dateFin)) {
        this.error.set('La date de début doit être avant la date de fin');
        return;
      }

      const updated: Periode = {
        ...sel,
        dateDebut: dateDebut,
        dateFin: dateFin
      };

      this.periodeService.update(updated).subscribe({
        next: () => {
          this.load();
          this.selected.set(null);
          this.success.set('Période mise à jour avec succès');
          this.error.set(null);
        },
        error: () => this.error.set('Erreur lors de la modification')
      });
    }
  }

  cancel() {
    this.selected.set(null);
    this.error.set(null);
  }

  formatDateForInput(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toISOString().split('T')[0];
  }

  formatDateForDisplay(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('fr-FR');
  }
}
