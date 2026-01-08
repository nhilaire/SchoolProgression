import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Periode } from '../models/periode.model';

@Injectable({ providedIn: 'root' })
export class DefinitionPeriodeService {
  private baseUrl = environment.apiUrl + '/definition-periodes';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Periode[]> {
    return this.http.get<Periode[]>(this.baseUrl);
  }

  getCurrent(): Observable<Periode> {
    return this.http.get<Periode>(`${this.baseUrl}/current`);
  }

  update(periode: Periode): Observable<void> {
    return this.http.put<void>(this.baseUrl, periode);
  }
}
