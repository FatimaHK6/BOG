import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ClassificationVM, ClassificationFilter } from '../models/classification.model';

/**
 * API Service for Classification Management
 * Handles HTTP communication with the backend LookupsController
 * Provides access to 4-level hierarchical classifications
 */
@Injectable({
  providedIn: 'root'
})
export class ClassificationsApiService {
  private baseUrl = `${environment.apiUrl}/api/lookups`;

  // Cache classifications to avoid repeated API calls
  private classificationsCache$: Observable<ClassificationVM[]> | null = null;
  private cacheSubject = new BehaviorSubject<ClassificationVM[] | null>(null);

  constructor(private http: HttpClient) { }

  /**
   * Gets all active classifications with 4-level hierarchy
   * Results are cached to minimize API calls
   *
   * @returns Observable array of ClassificationVM
   */
  getClassifications(): Observable<ClassificationVM[]> {
    if (!this.classificationsCache$) {
      this.classificationsCache$ = this.http.get<ClassificationVM[]>(
        `${this.baseUrl}/classifications`
      ).pipe(
        map(classifications => this.normalizeClassifications(classifications)),
        shareReplay(1)
      );

      // Also update the cache subject for reactive access
      this.classificationsCache$.subscribe(
        classifications => this.cacheSubject.next(classifications)
      );
    }

    return this.classificationsCache$;
  }

  /**
   * Get cached classifications synchronously
   * Returns null if cache not yet loaded
   *
   * @returns Cached classifications or null
   */
  getClassificationsFromCache(): ClassificationVM[] | null {
    return this.cacheSubject.value;
  }

  /**
   * Watch for cache updates
   * Useful for reactive components
   *
   * @returns Observable of cached classifications
   */
  getClassificationsCache$(): Observable<ClassificationVM[] | null> {
    return this.cacheSubject.asObservable();
  }

  /**
   * Clear the classifications cache
   * Forces a fresh API call on next getClassifications() call
   */
  clearCache(): void {
    this.classificationsCache$ = null;
    this.cacheSubject.next(null);
  }

  /**
   * Normalize classification data from API
   * Ensures consistent structure across all classifications
   *
   * @param classifications Raw classifications from API
   * @returns Normalized classifications
   */
  private normalizeClassifications(classifications: any[]): ClassificationVM[] {
    return classifications.map((c: any) => ({
      id: c.id,
      level1: c.level1 || '',
      level2: c.level2 || '',
      level3: c.level3 || '',
      level4: c.level4 || '',
      name: c.nameEn || c.name || '',
      nameAr: c.nameAr || '',
      description: c.description || '',
      isActive: c.isActive !== false
    }));
  }

  /**
   * Build full display text from classification levels
   * Example: "عقود > عقود مدنية > عقود البيع > عقد بيع عقار"
   *
   * @param classification Classification to format
   * @returns Full display text
   */
  getClassificationDisplay(classification: ClassificationVM): string {
    const levels = [
      classification.level1,
      classification.level2,
      classification.level3,
      classification.level4
    ].filter(level => level && level.trim());

    return levels.join(' > ');
  }

  /**
   * Get unique values for a specific level
   * Useful for filtering/hierarchical selection UI
   *
   * @param level Level number (1-4)
   * @param filterByParent Optional parent level filter
   * @returns Array of unique values for the level
   */
  getUniqueValuesForLevel(
    level: 1 | 2 | 3 | 4,
    filterByParent?: { level: 1 | 2 | 3; value: string }
  ): Observable<string[]> {
    return this.getClassifications().pipe(
      map(classifications => {
        let filtered = classifications;

        // Apply parent level filter if provided
        if (filterByParent) {
          const levelKey = `level${filterByParent.level}` as keyof ClassificationVM;
          filtered = classifications.filter(
            c => c[levelKey] === filterByParent.value
          );
        }

        // Get unique values for target level
        const levelKey = `level${level}` as keyof ClassificationVM;
        const uniqueValues = new Set<string>();

        filtered.forEach(c => {
          const value = c[levelKey];
          if (value && typeof value === 'string') {
            uniqueValues.add(value.trim());
          }
        });

        return Array.from(uniqueValues).sort();
      })
    );
  }

  /**
   * Filter classifications by search criteria
   *
   * @param filter Filter criteria
   * @returns Observable of filtered classifications
   */
  filterClassifications(filter: ClassificationFilter): Observable<ClassificationVM[]> {
    return this.getClassifications().pipe(
      map(classifications => {
        return classifications.filter(c => {
          // Filter by level values
          if (filter.level1 && c.level1 !== filter.level1) return false;
          if (filter.level2 && c.level2 !== filter.level2) return false;
          if (filter.level3 && c.level3 !== filter.level3) return false;
          if (filter.level4 && c.level4 !== filter.level4) return false;

          // Filter by search text (case-insensitive)
          if (filter.searchText) {
            const searchLower = filter.searchText.toLowerCase();
            const fullDisplay = this.getClassificationDisplay(c).toLowerCase();
            return fullDisplay.includes(searchLower);
          }

          return true;
        });
      })
    );
  }

  /**
   * Get classification by ID
   *
   * @param id Classification ID
   * @returns Observable of the classification or undefined
   */
  getClassificationById(id: number): Observable<ClassificationVM | undefined> {
    return this.getClassifications().pipe(
      map(classifications => classifications.find(c => c.id === id))
    );
  }

  /**
   * Get multiple classifications by IDs
   *
   * @param ids Array of classification IDs
   * @returns Observable of array of classifications
   */
  getClassificationsByIds(ids: number[]): Observable<ClassificationVM[]> {
    return this.getClassifications().pipe(
      map(classifications => {
        const idSet = new Set(ids);
        return classifications.filter(c => idSet.has(c.id));
      })
    );
  }
}
