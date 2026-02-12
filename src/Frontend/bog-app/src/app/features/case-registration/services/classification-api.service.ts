import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface ClassificationVM {
  id: number;
  nameAr: string;
  nameEn: string;
  description?: string;
}

@Injectable({ providedIn: 'root' })
export class ClassificationApiService {
  private apiUrl = `${environment.apiUrl}/api/lookups/classifications`;

  constructor(private http: HttpClient) {}

  /**
   * Gets all active classifications for the dropdown.
   */
  getAll(): Observable<ClassificationVM[]> {
    return this.http.get<ClassificationVM[]>(this.apiUrl);
  }
}
