import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DeficiencyTypeVM, DeficiencyDescriptionVM } from '../../models/deficiency.model';
import { LookupsApiService } from '../../services/lookups-api.service';
import { MatSnackBar } from '@angular/material/snack-bar';

interface DialogData {
  selectedDeficiencies?: number[]; // Array of DeficiencyDescriptionId
}

interface DeficiencySelection {
  typeId: number;
  descriptions: DeficiencyDescriptionVM[];
  selectedDescriptionIds: number[];
}

@Component({
  selector: 'app-deficiencies-selection-dialog',
  templateUrl: './deficiencies-selection-dialog.component.html',
  styleUrls: ['./deficiencies-selection-dialog.component.scss']
})
export class DeficienciesSelectionDialogComponent implements OnInit, OnDestroy {
  deficiencyTypes: DeficiencyTypeVM[] = [];
  deficiencyDescriptions: DeficiencyDescriptionVM[] = [];
  deficienciesByType: Map<number, DeficiencyDescriptionVM[]> = new Map();

  loading = false;
  selectedDescriptionIds: number[] = [];
  expandedTypeIds: Set<number> = new Set();

  private destroy$ = new Subject<void>();

  constructor(
    public dialogRef: MatDialogRef<DeficienciesSelectionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private lookupsApi: LookupsApiService,
    private snackBar: MatSnackBar
  ) {
    // Initialize with existing selections
    if (data?.selectedDeficiencies) {
      this.selectedDescriptionIds = [...data.selectedDeficiencies];
    }
  }

  ngOnInit() {
    this.loadDeficiencies();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDeficiencies() {
    this.loading = true;

    // Load deficiency types first
    this.lookupsApi.getDeficiencyTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (types) => {
          this.deficiencyTypes = types;
          this.expandedTypeIds.clear();

          // Expand first type by default
          if (types.length > 0) {
            this.expandedTypeIds.add(types[0].id);
          }

          // Load descriptions for all types
          this.lookupsApi.getDeficiencyDescriptions()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: (descriptions) => {
                this.deficiencyDescriptions = descriptions;

                // Group descriptions by type
                this.deficienciesByType.clear();
                descriptions.forEach(desc => {
                  if (!this.deficienciesByType.has(desc.deficiencyTypeId)) {
                    this.deficienciesByType.set(desc.deficiencyTypeId, []);
                  }
                  this.deficienciesByType.get(desc.deficiencyTypeId)!.push(desc);
                });

                this.loading = false;
              },
              error: (error) => {
                console.error('Error loading deficiency descriptions:', error);
                this.loading = false;
                this.snackBar.open('فشل في تحميل النواقص', 'إغلاق', { duration: 3000 });
              }
            });
        },
        error: (error) => {
          console.error('Error loading deficiency types:', error);
          this.loading = false;
          this.snackBar.open('فشل في تحميل أنواع النواقص', 'إغلاق', { duration: 3000 });
        }
      });
  }

  toggleType(typeId: number) {
    if (this.expandedTypeIds.has(typeId)) {
      this.expandedTypeIds.delete(typeId);
    } else {
      this.expandedTypeIds.add(typeId);
    }
  }

  isTypeExpanded(typeId: number): boolean {
    return this.expandedTypeIds.has(typeId);
  }

  toggleDescription(descriptionId: number) {
    const index = this.selectedDescriptionIds.indexOf(descriptionId);
    if (index > -1) {
      this.selectedDescriptionIds.splice(index, 1);
    } else {
      this.selectedDescriptionIds.push(descriptionId);
    }
  }

  isDescriptionSelected(descriptionId: number): boolean {
    return this.selectedDescriptionIds.includes(descriptionId);
  }

  getDescriptionsForType(typeId: number): DeficiencyDescriptionVM[] {
    return this.deficienciesByType.get(typeId) || [];
  }

  getCountForType(typeId: number): number {
    const descriptions = this.getDescriptionsForType(typeId);
    return descriptions.filter(d => this.isDescriptionSelected(d.id)).length;
  }

  cancel() {
    this.dialogRef.close(null);
  }

  confirm() {
    // Convert selected description IDs to RequestDeficiencyDTO format
    const deficiencies = this.selectedDescriptionIds.map(id => ({
      deficiencyDescriptionId: id
    }));

    this.dialogRef.close(deficiencies);
  }

  get hasSelections(): boolean {
    return this.selectedDescriptionIds.length > 0;
  }

  getTypeName(typeId: number): string {
    return this.deficiencyTypes.find(t => t.id === typeId)?.nameAr || '';
  }

  getDescriptionText(description: DeficiencyDescriptionVM): string {
    return description.descriptionAr || description.descriptionEn || '';
  }
}
