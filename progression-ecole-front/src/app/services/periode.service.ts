import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PeriodeActivites {
  eleveId: string;
  periode: string;
  activiteIds: string[];
}

@Injectable({ providedIn: 'root' })
export class PeriodeService {
  generate(periode: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${periode}/generate`, { responseType: 'blob' });
  }
  getAll(eleveId: string): Observable<PeriodeActivites[]> {
    return this.http.get<PeriodeActivites[]>(`${this.baseUrl}/eleve/${eleveId}`);
  }

  getAllPeriodeActivites(): Observable<PeriodeActivites[]> {
    return this.http.get<PeriodeActivites[]>(this.baseUrl);
  }
  private baseUrl = environment.apiUrl + '/periode';

  constructor(private http: HttpClient) {}

  save(data: PeriodeActivites): Observable<void> {
    return this.http.post<void>(this.baseUrl, data);
  }

  get(eleveId: string, periode: string): Observable<PeriodeActivites> {
    return this.http.get<PeriodeActivites>(`${this.baseUrl}/eleve/${eleveId}/periode/${periode}`);
  }
}
