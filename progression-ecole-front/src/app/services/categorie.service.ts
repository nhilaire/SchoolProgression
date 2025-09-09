import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Categorie } from '../models/categorie.model';

@Injectable({ providedIn: 'root' })
export class CategorieService {
  private baseUrl = environment.apiUrl + '/categories';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Categorie[]> {
    return this.http.get<Categorie[]>(this.baseUrl);
  }

  add(categorie: Categorie): Observable<Categorie> {
    return this.http.post<Categorie>(this.baseUrl, categorie);
  }

  update(categorie: Categorie): Observable<Categorie> {
    return this.http.put<Categorie>(this.baseUrl, categorie);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
