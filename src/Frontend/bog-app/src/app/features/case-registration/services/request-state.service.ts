import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { CaseRequestVM } from '../models/case-request.model';
import { CaseRegistrationApiService } from './case-registration-api.service';

@Injectable({
  providedIn: 'root'
})
export class RequestStateService {
  private currentRequestSubject = new BehaviorSubject<CaseRequestVM | null>(null);
  public currentRequest$ = this.currentRequestSubject.asObservable();

  private validationErrorsSubject = new BehaviorSubject<string[]>([]);
  public validationErrors$ = this.validationErrorsSubject.asObservable();

  constructor(private api: CaseRegistrationApiService) { }

  loadRequest(id: number): Observable<CaseRequestVM> {
    return this.api.getById(id).pipe(
      tap(request => this.currentRequestSubject.next(request)),
      catchError(error => {
        console.error('Error loading request:', error);
        return throwError(() => error);
      })
    );
  }

  updateRequest(request: CaseRequestVM) {
    this.currentRequestSubject.next(request);
  }

  refreshRequest() {
    const current = this.currentRequestSubject.value;
    if (current?.id) {
      this.loadRequest(current.id).subscribe();
    }
  }

  setValidationErrors(errors: string[]) {
    this.validationErrorsSubject.next(errors);
  }

  clearValidationErrors() {
    this.validationErrorsSubject.next([]);
  }

  // Permission checks
  canEdit(): boolean {
    const request = this.currentRequestSubject.value;
    return request?.requestStatusId === 1 || request?.requestStatusId === 2 || request?.requestStatusId === 6; // Draft, New, or PendingCompletion
  }

  canSubmit(): boolean {
    const request = this.currentRequestSubject.value;
    return request?.requestStatusId === 1; // Draft only
  }

  canTakeAction(): boolean {
    const request = this.currentRequestSubject.value;
    return [2, 7, 9].includes(request?.requestStatusId || 0); // New, OnJudgeDesk, AutoRejected
  }

  getCurrentRequest(): CaseRequestVM | null {
    return this.currentRequestSubject.value;
  }

  updateAttachmentsCount(requestId: number, count: number): void {
    const request = this.currentRequestSubject.value;
    if (request && request.id === requestId) {
      request.attachmentsCount = count;
      this.currentRequestSubject.next(request);
    }
  }
}
