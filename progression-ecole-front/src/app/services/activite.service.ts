import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Activite } from '../models/activite.model';

@Injectable({ providedIn: 'root' })
export class ActiviteService {
  private baseUrl = environment.apiUrl + '/activite';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Activite[]> {
    return this.http.get<Activite[]>(this.baseUrl);
  }

  add(activite: Activite): Observable<Activite> {
    return this.http.post<Activite>(this.baseUrl, activite);
  }

  update(activite: Activite): Observable<Activite> {
    return this.http.put<Activite>(this.baseUrl, activite);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getEnfants(parentId: string): Observable<Activite[]> {
    return this.http.get<Activite[]>(`${this.baseUrl}/enfants/${parentId}`);
  }

  getRegroupements(): Observable<Activite[]> {
    return this.http.get<Activite[]>(`${this.baseUrl}/regroupements`);
  }

  getActivitesIsolees(): Observable<Activite[]> {
    return this.http.get<Activite[]>(`${this.baseUrl}/isolees`);
  }

  createRegroupement(regroupement: Activite): Observable<Activite> {
    return this.http.post<Activite>(`${this.baseUrl}/regroupement`, regroupement);
  }

  addActiviteToRegroupement(activite: Activite): Observable<Activite> {
    return this.http.post<Activite>(`${this.baseUrl}/activite-enfant`, activite);
  }
}
