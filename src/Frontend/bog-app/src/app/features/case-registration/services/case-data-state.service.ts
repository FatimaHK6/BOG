import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ClaimVM } from '../models/claim.model';
import { RelatedCaseVM } from '../models/related-case.model';

interface ContactInfo {
  primaryMobile?: string;
  secondaryMobile?: string;
  email?: string;
}

export interface CaseDataState {
  subject: string;
  evidence: string;
  claims: ClaimVM[];
  relatedCases: RelatedCaseVM[];
  classificationIds: number[];
  caseTypeId: number;
  primaryMobile: string;
  secondaryMobile: string;
  email: string;
}

/**
 * Service for managing case data state across components.
 * Provides centralized state management with localStorage backup.
 *
 * Usage:
 * - Inject into any component that needs to share case data
 * - Use updateSubject(), updateEvidence(), etc. to modify state
 * - Subscribe to state$ observable to react to changes
 */
@Injectable({
  providedIn: 'root'
})
export class CaseDataStateService {
  private initialState: CaseDataState = {
    subject: '',
    evidence: '',
    claims: [],
    relatedCases: [],
    classificationIds: [],
    caseTypeId: 1,
    primaryMobile: '',
    secondaryMobile: '',
    email: ''
  };

  private stateSubject = new BehaviorSubject<CaseDataState>(this.initialState);
  public state$: Observable<CaseDataState> = this.stateSubject.asObservable();

  // Track the current request ID to scope localStorage
  private currentRequestId: number | null = null;
  private readonly STORAGE_KEY_PREFIX = 'case-data-state';

  /**
   * Get the current localStorage key (request-specific)
   */
  private getStorageKey(): string {
    if (this.currentRequestId) {
      return `${this.STORAGE_KEY_PREFIX}-${this.currentRequestId}`;
    }
    // For new unsaved requests, use a generic draft key
    return `${this.STORAGE_KEY_PREFIX}-draft`;
  }

  constructor() {
    // DO NOT load from localStorage on initialization
    // This prevents loading stale data from a different request
    // Data will be loaded explicitly when opening an existing request
  }

  /**
   * Set the current request ID context
   * This scopes localStorage to a specific request
   * Call this BEFORE loading request data
   */
  setRequestContext(requestId: number | null): void {
    // If switching to a different request, clear the old state first
    if (this.currentRequestId !== requestId) {
      this.currentRequestId = requestId;

      // If loading an existing request, try to restore from localStorage
      if (requestId) {
        this.loadFromLocalStorage();
      } else {
        // For new requests (null/0), start with initial state
        this.stateSubject.next({ ...this.initialState });
      }
    }
  }

  /**
   * Update subject field and save to state + localStorage
   */
  updateSubject(value: string): void {
    this.updateState({ ...this.stateSubject.value, subject: value });
  }

  /**
   * Update evidence field and save to state + localStorage
   */
  updateEvidence(value: string): void {
    this.updateState({ ...this.stateSubject.value, evidence: value });
  }

  /**
   * Update claims array and save to state + localStorage
   */
  updateClaims(claims: ClaimVM[]): void {
    this.updateState({ ...this.stateSubject.value, claims });
  }

  /**
   * Update related cases array and save to state + localStorage
   */
  updateRelatedCases(relatedCases: RelatedCaseVM[]): void {
    this.updateState({ ...this.stateSubject.value, relatedCases });
  }

  /**
   * Update classification IDs array and save to state + localStorage
   */
  updateClassifications(classificationIds: number[]): void {
    this.updateState({ ...this.stateSubject.value, classificationIds });
  }

  /**
   * Update case type ID and save to state + localStorage
   */
  updateCaseTypeId(caseTypeId: number): void {
    console.log('[DEBUG] Updating caseTypeId:', caseTypeId);
    this.updateState({ ...this.stateSubject.value, caseTypeId });
  }

  /**
   * Update contact information and save to state + localStorage
   */
  updateContactInfo(primaryMobile: string, secondaryMobile: string, email: string): void {
    this.updateState({
      ...this.stateSubject.value,
      primaryMobile: primaryMobile || '',
      secondaryMobile: secondaryMobile || '',
      email: email || ''
    });
  }

  /**
   * Update primary mobile field
   */
  updatePrimaryMobile(value: string): void {
    this.updateState({
      ...this.stateSubject.value,
      primaryMobile: value || ''
    });
  }

  /**
   * Update secondary mobile field
   */
  updateSecondaryMobile(value: string): void {
    this.updateState({
      ...this.stateSubject.value,
      secondaryMobile: value || ''
    });
  }

  /**
   * Update email field
   */
  updateEmail(value: string): void {
    this.updateState({
      ...this.stateSubject.value,
      email: value || ''
    });
  }

  /**
   * Get current subject value
   */
  getSubject(): string {
    return this.stateSubject.value.subject;
  }

  /**
   * Get current evidence value
   */
  getEvidence(): string {
    return this.stateSubject.value.evidence;
  }

  /**
   * Get current claims array
   */
  getClaims(): ClaimVM[] {
    return this.stateSubject.value.claims;
  }

  /**
   * Get current related cases array
   */
  getRelatedCases(): RelatedCaseVM[] {
    return this.stateSubject.value.relatedCases;
  }

  /**
   * Get current classification IDs array
   */
  getClassificationIds(): number[] {
    return this.stateSubject.value.classificationIds;
  }

  /**
   * Get contact information
   */
  getContactInfo(): ContactInfo {
    const state = this.stateSubject.value;
    return {
      primaryMobile: state.primaryMobile,
      secondaryMobile: state.secondaryMobile,
      email: state.email
    };
  }

  /**
   * Get all data as a single object (for saving to database)
   */
  getAllData(): CaseDataState {
    return { ...this.stateSubject.value };
  }

  /**
   * Load existing request data from backend response
   * Used when opening an existing case request
   */
  loadFromRequest(request: any): void {
    const newState: CaseDataState = {
      subject: request.subject || '',
      evidence: request.evidence || '',
      claims: request.claims || [],
      relatedCases: request.relatedCases || [],
      classificationIds: request.classificationIds || [],
      caseTypeId: request.caseTypeId || 1,
      primaryMobile: request.primaryMobile || '',
      secondaryMobile: request.secondaryMobile || '',
      email: request.email || ''
    };
    this.updateState(newState);
  }

  /**
   * Reset state to initial values
   * Used when clearing form or creating new request
   */
  resetState(): void {
    this.currentRequestId = null;
    this.updateState({ ...this.initialState });
    this.clearFromLocalStorage();
  }

  /**
   * Clear state from localStorage only (useful for cleanup)
   */
  clearFromLocalStorage(): void {
    try {
      // Clear current request's localStorage
      localStorage.removeItem(this.getStorageKey());

      // Also clear draft key if exists
      localStorage.removeItem(`${this.STORAGE_KEY_PREFIX}-draft`);
    } catch (error) {
      console.warn('Failed to clear state from localStorage:', error);
    }
  }

  /**
   * Internal method to update state and save to localStorage
   */
  private updateState(newState: CaseDataState): void {
    this.stateSubject.next(newState);
    this.saveToLocalStorage(newState);
  }

  /**
   * Save state to localStorage as backup (optional recovery)
   * Useful if user refreshes page or browser crashes
   */
  private saveToLocalStorage(state: CaseDataState): void {
    try {
      localStorage.setItem(this.getStorageKey(), JSON.stringify(state));
    } catch (error) {
      console.warn('Failed to save state to localStorage:', error);
    }
  }

  /**
   * Load state from localStorage (optional backup recovery)
   * Called when setting request context
   */
  private loadFromLocalStorage(): void {
    try {
      const saved = localStorage.getItem(this.getStorageKey());
      if (saved) {
        const state = JSON.parse(saved) as CaseDataState;
        this.stateSubject.next(state);
        console.log(`Loaded state from localStorage for request ${this.currentRequestId || 'draft'}`);
      } else {
        // No saved state, use initial state
        this.stateSubject.next({ ...this.initialState });
      }
    } catch (error) {
      console.warn('Failed to load state from localStorage:', error);
      // If there's an error parsing, just use initial state
      this.stateSubject.next({ ...this.initialState });
    }
  }
}
