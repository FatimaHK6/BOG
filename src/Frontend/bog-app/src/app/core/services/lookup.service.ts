import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { shareReplay, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  LookupItem,
  IdentityType,
  Nationality,
  Region,
  City,
  GovernmentAgency,
  Court,
  LicenseSource
} from '../models/lookup.model';
import { PlaintiffType } from '../models/plaintiff.model';
import { RepresentativeType } from '../models/representative.model';

@Injectable({
  providedIn: 'root'
})
export class LookupService {
  private readonly apiUrl = `${environment.apiUrl}/api/lookups`;

  // Cache observables
  private plaintiffTypes$?: Observable<PlaintiffType[]>;
  private representativeTypes$?: Observable<RepresentativeType[]>;
  private identityTypes$?: Observable<IdentityType[]>;
  private nationalities$?: Observable<Nationality[]>;
  private regions$?: Observable<Region[]>;
  private governmentAgencies$?: Observable<GovernmentAgency[]>;
  private countries$?: Observable<any[]>;
  private licenseSources$?: Observable<LicenseSource[]>;

  constructor(private http: HttpClient) { }

  getPlaintiffTypes(): Observable<PlaintiffType[]> {
    if (!this.plaintiffTypes$) {
      this.plaintiffTypes$ = this.http.get<PlaintiffType[]>(`${this.apiUrl}/plaintiff-types`)
        .pipe(
          catchError(() => of(this.getDemoPlaintiffTypes())),
          shareReplay(1)
        );
    }
    return this.plaintiffTypes$;
  }

  getRepresentativeTypes(): Observable<RepresentativeType[]> {
    if (!this.representativeTypes$) {
      this.representativeTypes$ = this.http.get<RepresentativeType[]>(`${this.apiUrl}/representative-types`)
        .pipe(
          catchError(() => of(this.getDemoRepresentativeTypes())),
          shareReplay(1)
        );
    }
    return this.representativeTypes$;
  }

  getIdentityTypes(): Observable<IdentityType[]> {
    if (!this.identityTypes$) {
      this.identityTypes$ = this.http.get<IdentityType[]>(`${this.apiUrl}/identity-types`)
        .pipe(
          catchError(() => of(this.getDemoIdentityTypes())),
          shareReplay(1)
        );
    }
    return this.identityTypes$;
  }

  getNationalities(): Observable<Nationality[]> {
    if (!this.nationalities$) {
      this.nationalities$ = this.http.get<Nationality[]>(`${this.apiUrl}/nationalities`)
        .pipe(
          catchError(() => of(this.getDemoNationalities())),
          shareReplay(1)
        );
    }
    return this.nationalities$;
  }

  getRegions(): Observable<Region[]> {
    if (!this.regions$) {
      this.regions$ = this.http.get<Region[]>(`${this.apiUrl}/regions`)
        .pipe(
          catchError(() => of(this.getDemoRegions())),
          shareReplay(1)
        );
    }
    return this.regions$;
  }

  getCitiesByRegion(regionId: number): Observable<City[]> {
    return this.http.get<City[]>(`${this.apiUrl}/regions/${regionId}/cities`)
      .pipe(catchError(() => of(this.getDemoCities(regionId))));
  }

  getGovernmentAgencies(): Observable<GovernmentAgency[]> {
    if (!this.governmentAgencies$) {
      this.governmentAgencies$ = this.http.get<GovernmentAgency[]>(`${this.apiUrl}/government-agencies`)
        .pipe(
          catchError(() => of(this.getDemoGovernmentAgencies())),
          shareReplay(1)
        );
    }
    return this.governmentAgencies$;
  }

  getCountries(): Observable<any[]> {
    if (!this.countries$) {
      this.countries$ = this.http.get<any[]>(`${this.apiUrl}/countries`)
        .pipe(
          catchError(() => of(this.getDemoCountries())),
          shareReplay(1)
        );
    }
    return this.countries$;
  }

  getLicenseSources(): Observable<LicenseSource[]> {
    if (!this.licenseSources$) {
      this.licenseSources$ = this.http.get<LicenseSource[]>(`${this.apiUrl}/license-sources`)
        .pipe(
          catchError(() => of(this.getDemoLicenseSources())),
          shareReplay(1)
        );
    }
    return this.licenseSources$;
  }

  getCourtsByCity(cityId: number): Observable<Court[]> {
    return this.http.get<Court[]>(`${this.apiUrl}/cities/${cityId}/courts`)
      .pipe(catchError(() => of(this.getDemoCourts())));
  }

  clearCache(): void {
    this.plaintiffTypes$ = undefined;
    this.representativeTypes$ = undefined;
    this.identityTypes$ = undefined;
    this.nationalities$ = undefined;
    this.regions$ = undefined;
    this.governmentAgencies$ = undefined;
    this.countries$ = undefined;
    this.licenseSources$ = undefined;
  }

  // ============== Demo Data ==============

  private getDemoPlaintiffTypes(): PlaintiffType[] {
    // IDs must match backend PlaintiffTypes table (exactly 8 types per SRS Section 1.3)
    return [
      { id: 1, name: 'Individual', nameAr: 'فرد' },
      { id: 2, name: 'IndividualWithoutId', nameAr: 'فرد بدون هوية' },
      { id: 3, name: 'BusinessOwner', nameAr: 'صاحب مؤسسة' },
      { id: 4, name: 'RegisteredCompany', nameAr: 'شركة مسجلة' },
      { id: 5, name: 'UnregisteredCompany', nameAr: 'شركة غير مسجلة' },
      { id: 6, name: 'GovernmentAgency', nameAr: 'جهة حكومية' },
      { id: 7, name: 'Society', nameAr: 'جمعية/مؤسسة أهلية' },
      { id: 8, name: 'Waqf', nameAr: 'وقف' }
    ];
  }

  private getDemoRepresentativeTypes(): RepresentativeType[] {
    return [
      { id: 1, name: 'Agent/Lawyer', nameAr: 'وكيل' },
      { id: 2, name: 'Guardian', nameAr: 'ولي' },
      { id: 3, name: 'Custodian', nameAr: 'وصي' },
      { id: 4, name: 'Executor', nameAr: 'ناظر' },
      { id: 5, name: 'Heir Representative', nameAr: 'ممثل الورثة' },
      { id: 6, name: 'Company Representative', nameAr: 'ممثل الشركة' },
      { id: 7, name: 'Agency Representative', nameAr: 'ممثل الجهة' },
      { id: 8, name: 'Trustee', nameAr: 'أمين التفليسة' },
      { id: 9, name: 'Legal Representative', nameAr: 'ممثل نظامي' }
    ];
  }

  private getDemoIdentityTypes(): IdentityType[] {
    return [
      { id: 1, name: 'National ID', nameAr: 'هوية وطنية' },
      { id: 2, name: 'Residency', nameAr: 'إقامة' },
      { id: 3, name: 'Passport', nameAr: 'جواز سفر' }
    ];
  }

  private getDemoNationalities(): Nationality[] {
    return [
      { id: 1, name: 'Saudi', nameAr: 'سعودي' },
      { id: 2, name: 'Emirati', nameAr: 'إماراتي' },
      { id: 3, name: 'Kuwaiti', nameAr: 'كويتي' },
      { id: 4, name: 'Qatari', nameAr: 'قطري' },
      { id: 5, name: 'Bahraini', nameAr: 'بحريني' },
      { id: 6, name: 'Omani', nameAr: 'عماني' },
      { id: 7, name: 'Egyptian', nameAr: 'مصري' },
      { id: 8, name: 'Jordanian', nameAr: 'أردني' },
      { id: 9, name: 'Syrian', nameAr: 'سوري' },
      { id: 10, name: 'Yemeni', nameAr: 'يمني' }
    ];
  }

  private getDemoRegions(): Region[] {
    return [
      { id: 1, name: 'Riyadh', nameAr: 'الرياض' },
      { id: 2, name: 'Makkah', nameAr: 'مكة المكرمة' },
      { id: 3, name: 'Madinah', nameAr: 'المدينة المنورة' },
      { id: 4, name: 'Qassim', nameAr: 'القصيم' },
      { id: 5, name: 'Eastern Province', nameAr: 'المنطقة الشرقية' },
      { id: 6, name: 'Asir', nameAr: 'عسير' },
      { id: 7, name: 'Tabuk', nameAr: 'تبوك' },
      { id: 8, name: 'Hail', nameAr: 'حائل' },
      { id: 9, name: 'Northern Borders', nameAr: 'الحدود الشمالية' },
      { id: 10, name: 'Jazan', nameAr: 'جازان' },
      { id: 11, name: 'Najran', nameAr: 'نجران' },
      { id: 12, name: 'Bahah', nameAr: 'الباحة' },
      { id: 13, name: 'Jawf', nameAr: 'الجوف' }
    ];
  }

  private getDemoCities(regionId: number): City[] {
    const citiesByRegion: { [key: number]: City[] } = {
      1: [
        { id: 1, name: 'Riyadh', nameAr: 'الرياض', regionId: 1 },
        { id: 2, name: 'Al Kharj', nameAr: 'الخرج', regionId: 1 },
        { id: 3, name: 'Diriyah', nameAr: 'الدرعية', regionId: 1 },
        { id: 4, name: 'Al Majmaah', nameAr: 'المجمعة', regionId: 1 }
      ],
      2: [
        { id: 5, name: 'Makkah', nameAr: 'مكة المكرمة', regionId: 2 },
        { id: 6, name: 'Jeddah', nameAr: 'جدة', regionId: 2 },
        { id: 7, name: 'Taif', nameAr: 'الطائف', regionId: 2 }
      ],
      3: [
        { id: 8, name: 'Madinah', nameAr: 'المدينة المنورة', regionId: 3 },
        { id: 9, name: 'Yanbu', nameAr: 'ينبع', regionId: 3 }
      ],
      5: [
        { id: 10, name: 'Dammam', nameAr: 'الدمام', regionId: 5 },
        { id: 11, name: 'Dhahran', nameAr: 'الظهران', regionId: 5 },
        { id: 12, name: 'Khobar', nameAr: 'الخبر', regionId: 5 },
        { id: 13, name: 'Al Ahsa', nameAr: 'الأحساء', regionId: 5 }
      ]
    };
    return citiesByRegion[regionId] || [
      { id: 100, name: 'Default City', nameAr: 'مدينة افتراضية', regionId: regionId }
    ];
  }

  private getDemoGovernmentAgencies(): GovernmentAgency[] {
    return [
      { id: 1, name: 'Ministry of Interior', nameAr: 'وزارة الداخلية' },
      { id: 2, name: 'Ministry of Finance', nameAr: 'وزارة المالية' },
      { id: 3, name: 'Ministry of Education', nameAr: 'وزارة التعليم' },
      { id: 4, name: 'Ministry of Health', nameAr: 'وزارة الصحة' },
      { id: 5, name: 'Ministry of Justice', nameAr: 'وزارة العدل' },
      { id: 6, name: 'Ministry of Human Resources', nameAr: 'وزارة الموارد البشرية' },
      { id: 7, name: 'Ministry of Commerce', nameAr: 'وزارة التجارة' },
      { id: 8, name: 'ZATCA', nameAr: 'الهيئة العامة للزكاة والدخل' },
      { id: 9, name: 'Public Pension Agency', nameAr: 'المؤسسة العامة للتقاعد' },
      { id: 10, name: 'GOSI', nameAr: 'المؤسسة العامة للتأمينات الاجتماعية' }
    ];
  }

  private getDemoCourts(): Court[] {
    return [
      { id: 1, name: 'Administrative Court - Riyadh', nameAr: 'المحكمة الإدارية بالرياض', cityId: 1 },
      { id: 2, name: 'Administrative Appeals Court - Riyadh', nameAr: 'محكمة الاستئناف الإدارية بالرياض', cityId: 1 }
    ];
  }

  private getDemoCountries(): any[] {
    return [
      { id: 1, name: 'Saudi Arabia', nameAr: 'المملكة العربية السعودية', isoCode: 'SA' },
      { id: 2, name: 'United Arab Emirates', nameAr: 'الإمارات العربية المتحدة', isoCode: 'AE' },
      { id: 3, name: 'Kuwait', nameAr: 'الكويت', isoCode: 'KW' },
      { id: 4, name: 'Qatar', nameAr: 'قطر', isoCode: 'QA' },
      { id: 5, name: 'Bahrain', nameAr: 'البحرين', isoCode: 'BH' },
      { id: 6, name: 'Oman', nameAr: 'عمان', isoCode: 'OM' },
      { id: 7, name: 'Egypt', nameAr: 'مصر', isoCode: 'EG' },
      { id: 8, name: 'Jordan', nameAr: 'الأردن', isoCode: 'JO' },
      { id: 9, name: 'Lebanon', nameAr: 'لبنان', isoCode: 'LB' },
      { id: 10, name: 'Syria', nameAr: 'سوريا', isoCode: 'SY' }
    ];
  }

  private getDemoLicenseSources(): LicenseSource[] {
    return [
      { id: 1, name: 'Ministry of Human Resources', nameAr: 'وزارة الموارد البشرية' },
      { id: 2, name: 'Ministry of Interior', nameAr: 'وزارة الداخلية' },
      { id: 3, name: 'Ministry of Commerce', nameAr: 'وزارة التجارة' }
    ];
  }
}
