import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Eleve {
  id: number;
  nom: string;
  prenom: string;
  // Ajoutez d'autres champs si nécessaire
}

@Injectable({ providedIn: 'root' })
export class EleveService {
  private baseUrl = environment.apiUrl + '/eleve';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Eleve[]> {
    return this.http.get<Eleve[]>(this.baseUrl);
  }

  add(eleve: Eleve): Observable<Eleve> {
    return this.http.post<Eleve>(this.baseUrl, eleve);
  }

  update(eleve: Eleve): Observable<Eleve> {
    return this.http.put<Eleve>(this.baseUrl, eleve);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
