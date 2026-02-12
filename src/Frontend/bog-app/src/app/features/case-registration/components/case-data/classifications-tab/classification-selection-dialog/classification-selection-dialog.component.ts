import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormControl } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil, startWith } from 'rxjs/operators';
import { ClassificationsApiService } from '../../../../services/classifications-api.service';
import { ClassificationVM } from '../../../../models/classification.model';

@Component({
  selector: 'app-classification-selection-dialog',
  templateUrl: './classification-selection-dialog.component.html',
  styleUrls: ['./classification-selection-dialog.component.scss']
})
export class ClassificationSelectionDialogComponent implements OnInit, OnDestroy {
  // Data
  allClassifications: ClassificationVM[] = [];
  filteredClassifications: ClassificationVM[] = [];

  // Selection
  selectedIds = new Set<number>();

  // Search and Filtering
  searchControl = new FormControl('');
  level1Control = new FormControl('');
  level2Control = new FormControl('');
  level3Control = new FormControl('');
  level4Control = new FormControl('');

  // Unique values for dropdowns
  level1Values: string[] = [];
  level2Values: string[] = [];
  level3Values: string[] = [];
  level4Values: string[] = [];

  // Loading and States
  loading = true;
  noResults = false;

  // Display
  displayedColumns = ['select', 'level1', 'level2', 'level3', 'level4'];

  private destroy$ = new Subject<void>();

  constructor(
    private classificationsApi: ClassificationsApiService,
    public dialogRef: MatDialogRef<ClassificationSelectionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    // Initialize selectedIds from data
    if (data?.selectedIds) {
      data.selectedIds.forEach((id: number) => this.selectedIds.add(id));
    }
  }

  ngOnInit() {
    this.loadClassifications();
    this.setupSearchAndFilters();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load all classifications from API
   */
  private loadClassifications() {
    this.loading = true;

    this.classificationsApi.getClassifications()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (classifications) => {
          this.allClassifications = classifications;
          this.filteredClassifications = classifications;
          this.loading = false;
          this.updateLevelFilters();
        },
        error: (error) => {
          this.loading = false;
          console.error('Error loading classifications:', error);
        }
      });
  }

  /**
   * Setup search and filter controls
   */
  private setupSearchAndFilters() {
    // Search filter
    this.searchControl.valueChanges
      .pipe(
        startWith(''),
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(() => this.applyFilters());

    // Level filters
    [this.level1Control, this.level2Control, this.level3Control, this.level4Control]
      .forEach(control => {
        control.valueChanges
          .pipe(
            startWith(''),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
          )
          .subscribe(() => {
            this.updateLevelFilters();
            this.applyFilters();
          });
      });
  }

  /**
   * Apply all active filters to classifications
   */
  private applyFilters() {
    let result = this.allClassifications;

    // Filter by search text
    const search = this.searchControl.value?.toLowerCase() || '';
    if (search) {
      result = result.filter(c => {
        const display = this.classificationsApi.getClassificationDisplay(c).toLowerCase();
        return display.includes(search);
      });
    }

    // Filter by level 1
    const level1 = this.level1Control.value;
    if (level1) {
      result = result.filter(c => c.level1 === level1);
    }

    // Filter by level 2
    const level2 = this.level2Control.value;
    if (level2) {
      result = result.filter(c => c.level2 === level2);
    }

    // Filter by level 3
    const level3 = this.level3Control.value;
    if (level3) {
      result = result.filter(c => c.level3 === level3);
    }

    // Filter by level 4
    const level4 = this.level4Control.value;
    if (level4) {
      result = result.filter(c => c.level4 === level4);
    }

    this.filteredClassifications = result;
    this.noResults = result.length === 0;
  }

  /**
   * Update unique values for level filters based on current filters
   */
  private updateLevelFilters() {
    // Level 1 values
    this.classificationsApi.getUniqueValuesForLevel(1)
      .pipe(takeUntil(this.destroy$))
      .subscribe(values => this.level1Values = values);

    // Level 2 values (filtered by level 1 if selected)
    if (this.level1Control.value) {
      this.classificationsApi.getUniqueValuesForLevel(2, {
        level: 1,
        value: this.level1Control.value
      })
        .pipe(takeUntil(this.destroy$))
        .subscribe(values => this.level2Values = values);
    } else {
      this.classificationsApi.getUniqueValuesForLevel(2)
        .pipe(takeUntil(this.destroy$))
        .subscribe(values => this.level2Values = values);
    }

    // Level 3 values
    if (this.level2Control.value) {
      this.classificationsApi.getUniqueValuesForLevel(3, {
        level: 2,
        value: this.level2Control.value
      })
        .pipe(takeUntil(this.destroy$))
        .subscribe(values => this.level3Values = values);
    } else {
      this.classificationsApi.getUniqueValuesForLevel(3)
        .pipe(takeUntil(this.destroy$))
        .subscribe(values => this.level3Values = values);
    }

    // Level 4 values
    if (this.level3Control.value) {
      this.classificationsApi.getUniqueValuesForLevel(4, {
        level: 3,
        value: this.level3Control.value
      })
        .pipe(takeUntil(this.destroy$))
        .subscribe(values => this.level4Values = values);
    } else {
      this.classificationsApi.getUniqueValuesForLevel(4)
        .pipe(takeUntil(this.destroy$))
        .subscribe(values => this.level4Values = values);
    }
  }

  /**
   * Toggle selection of a classification
   */
  toggleSelection(id: number) {
    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
  }

  /**
   * Check if a classification is selected
   */
  isSelected(id: number): boolean {
    return this.selectedIds.has(id);
  }

  /**
   * Select all visible classifications
   */
  selectAll() {
    this.filteredClassifications.forEach(c => this.selectedIds.add(c.id));
  }

  /**
   * Deselect all visible classifications
   */
  deselectAll() {
    this.filteredClassifications.forEach(c => this.selectedIds.delete(c.id));
  }

  /**
   * Get selection stats
   */
  get selectionCount(): number {
    return this.selectedIds.size;
  }

  /**
   * Clear all filters
   */
  clearFilters() {
    this.searchControl.reset('');
    this.level1Control.reset('');
    this.level2Control.reset('');
    this.level3Control.reset('');
    this.level4Control.reset('');
  }

  /**
   * Save and close dialog
   */
  onSave() {
    const selectedArray = Array.from(this.selectedIds);
    this.dialogRef.close(selectedArray);
  }

  /**
   * Cancel and close dialog
   */
  onCancel() {
    this.dialogRef.close();
  }
}
