/**
 * Classification View Model (4-level hierarchy)
 * Used to represent classifications from backend API response
 */
export interface ClassificationVM {
  id: number;
  level1: string;
  level2: string;
  level3: string;
  level4: string;
  name: string;      // Legacy field
  nameAr: string;    // Legacy field
  description?: string;
  isActive: boolean;
}

/**
 * Classification Filter Model
 * Used for advanced search/filtering in classification selection dialog
 */
export interface ClassificationFilter {
  level1?: string;
  level2?: string;
  level3?: string;
  level4?: string;
  searchText?: string;
}

/**
 * Selected Classification
 * Represents a selected classification for a case request
 */
export interface SelectedClassification {
  id: number;
  level1: string;
  level2: string;
  level3: string;
  level4: string;
  fullDisplay: string; // e.g., "عقود > عقود مدنية > عقود البيع > عقد بيع عقار"
}

/**
 * Classification Hierarchy Level
 * Used for filtering/display in multi-level classification selection
 */
export interface ClassificationLevel {
  name: string;
  value: string;
  children?: ClassificationLevel[];
}

/**
 * Classification Statistics
 * For tracking selection counts
 */
export interface ClassificationStats {
  totalSelected: number;
  byLevel: {
    level1Count: number;
    level2Count: number;
    level3Count: number;
    level4Count: number;
  };
}
