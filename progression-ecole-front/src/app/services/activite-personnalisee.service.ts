import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ActivitePersonnalisee } from '../models/activite.model';

@Injectable({
  providedIn: 'root'
})
export class ActivitePersonnaliseeService {
  private baseUrl = environment.apiUrl + '/activite-personnalisee';

  constructor(private http: HttpClient) {}

  getByEleveAndPeriode(eleveId: string, periode: string): Observable<ActivitePersonnalisee[]> {
    return this.http.get<ActivitePersonnalisee[]>(`${this.baseUrl}/eleve/${eleveId}/periode/${periode}`);
  }

  save(activite: ActivitePersonnalisee): Observable<any> {
    return this.http.post(this.baseUrl, activite);
  }

  delete(activiteId: string, eleveId: string, periode: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/activite/${activiteId}/eleve/${eleveId}/periode/${periode}`);
  }
}