import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { PlaintiffListVM, RepresentativeVM } from '../../../../../core/models/plaintiff.model';
import { PlaintiffService } from '../../../../../core/services/plaintiff.service';
import { RepresentativeService } from '../../../../../core/services/representative.service';
import { CaseRegistrationRequestService } from '../../../../../core/services/case-registration-request.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { SetApplicantDialogComponent } from '../set-applicant-dialog/set-applicant-dialog.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

interface PlaintiffType {
  id: number;
  nameAr: string;
}

@Component({
  selector: 'app-plaintiff-list',
  templateUrl: './plaintiff-list.component.html',
  styleUrls: ['./plaintiff-list.component.scss']
})
export class PlaintiffListComponent implements OnInit, AfterViewInit {
  // Get requestId from route query params
  requestId: number = 0;
  isSaving = false;

  plaintiffs: PlaintiffListVM[] = [];
  dataSource = new MatTableDataSource<PlaintiffListVM>([]);
  isLoading = false;

  // Plaintiff types matching database PlaintiffTypes table (SRS Section 1.3 - 8 types)
  plaintiffTypes: PlaintiffType[] = [
    { id: 1, nameAr: 'فرد' },
    { id: 2, nameAr: 'فرد بدون هوية' },
    { id: 3, nameAr: 'صاحب مؤسسة' },
    { id: 4, nameAr: 'شركة مسجلة' },
    { id: 5, nameAr: 'شركة غير مسجلة' },
    { id: 6, nameAr: 'جهة حكومية' },
    { id: 7, nameAr: 'جمعية/مؤسسة أهلية' },
    { id: 8, nameAr: 'وقف' }
  ];

  // All representatives for all plaintiffs
  allRepresentatives: (RepresentativeVM & { plaintiffName?: string })[] = [];
  representativesDataSource = new MatTableDataSource<RepresentativeVM & { plaintiffName?: string }>([]);
  isLoadingRepresentatives = false;

  @ViewChild('plaintiffPaginator') paginator!: MatPaginator;
  @ViewChild('representativePaginator') representativePaginator!: MatPaginator;

  displayedColumns = [
    'plaintiffType',
    'identityNumber',
    'fullName',
    'attachmentStatus',
    'isApplicant',
    'actions'
  ];

  constructor(
    private plaintiffService: PlaintiffService,
    private representativeService: RepresentativeService,
    private requestService: CaseRegistrationRequestService,
    private dialog: MatDialog,
    private notification: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    // Get requestId from query params
    this.route.queryParams.subscribe(params => {
      const newRequestId = params['requestId'] ? +params['requestId'] : 0;
      if (newRequestId !== this.requestId) {
        this.requestId = newRequestId;
        if (this.requestId > 0) {
          this.loadPlaintiffs();
        } else {
          // No request selected, show empty state or redirect
          this.plaintiffs = [];
          this.dataSource.data = [];
          this.allRepresentatives = [];
          this.representativesDataSource.data = [];
        }
      }
    });
  }

  onSaveAsDraft(): void {
    if (this.requestId <= 0) {
      this.notification.validation('لا يوجد طلب محدد');
      return;
    }

    this.isSaving = true;
    // Call API to save as draft (sets status to مسودة)
    this.requestService.update(this.requestId, { saveAsDraft: true }).subscribe({
      next: () => {
        this.router.navigate(['/case-registration/requests']).then(() => {
          this.isSaving = false;
          this.notification.success('تم حفظ الطلب كمسودة');
        });
      },
      error: (error) => {
        console.error('Error saving draft:', error);
        this.isSaving = false;
        this.notification.error('حدث خطأ أثناء حفظ المسودة');
      }
    });
  }

  ngAfterViewInit(): void {
    // Paginator is set after data loads due to *ngIf
  }

  private setPaginator(): void {
    // Use setTimeout to ensure paginator is rendered after *ngIf condition is met
    setTimeout(() => {
      if (this.paginator) {
        this.dataSource.paginator = this.paginator;
      }
    });
  }

  private setRepresentativesPaginator(): void {
    setTimeout(() => {
      if (this.representativePaginator) {
        this.representativesDataSource.paginator = this.representativePaginator;
      }
    });
  }

  loadPlaintiffs(): void {
    this.isLoading = true;
    this.plaintiffService.getPlaintiffs(this.requestId).subscribe({
      next: (data) => {
        // Map API response to expected format
        this.plaintiffs = data.map((p: any) => ({
          ...p,
          plaintiffTypeName: p.plaintiffTypeNameAr || p.plaintiffTypeName,
          fullName: p.displayName || p.fullName,
          attachmentStatus: p.attachmentsCount > 0 ? 'complete' : 'incomplete'
        }));
        this.dataSource.data = this.plaintiffs;
        this.isLoading = false;
        this.setPaginator();
        // Load all representatives
        this.loadAllRepresentatives();
      },
      error: (error) => {
        console.error('Error loading plaintiffs:', error);
        this.isLoading = false;
        // Load demo data when API is not available
        this.loadDemoData();
      }
    });
  }

  private loadDemoData(): void {
    // Demo data for UI testing
    this.plaintiffs = [
      {
        id: 1,
        plaintiffTypeId: 1,
        plaintiffTypeName: 'فرد',
        plaintiffTypeNameAr: 'فرد',
        identityNumber: '1234567890',
        fullName: 'محمد أحمد السعيد',
        displayName: 'محمد أحمد السعيد',
        representativesCount: 1,
        attachmentStatus: 'complete',
        isApplicant: true
      },
      {
        id: 2,
        plaintiffTypeId: 1,
        plaintiffTypeName: 'فرد',
        plaintiffTypeNameAr: 'فرد',
        identityNumber: '0987654321',
        fullName: 'فاطمة خالد المطيري',
        displayName: 'فاطمة خالد المطيري',
        representativesCount: 0,
        attachmentStatus: 'incomplete',
        isApplicant: false
      },
      {
        id: 3,
        plaintiffTypeId: 4,
        plaintiffTypeName: 'شركة مسجلة',
        plaintiffTypeNameAr: 'شركة مسجلة',
        identityNumber: '1010101010',
        fullName: 'شركة الرياض للتجارة',
        displayName: 'شركة الرياض للتجارة',
        representativesCount: 2,
        attachmentStatus: 'complete',
        isApplicant: false
      }
    ];
    this.dataSource.data = this.plaintiffs;
    this.setPaginator();
    // Load demo representatives
    this.loadDemoRepresentatives();
  }

  private loadDemoRepresentatives(): void {
    // Demo representatives data for UI testing
    this.allRepresentatives = [
      {
        id: 1,
        plaintiffId: 1,
        plaintiffName: 'محمد أحمد السعيد',
        representativeTypeId: 1,
        representativeTypeName: 'وكيل',
        representativeTypeNameAr: 'وكيل',
        identityTypeId: 1,
        identityTypeName: 'هوية وطنية',
        identityNumber: '1122334455',
        firstName: 'خالد',
        fatherName: 'عبدالله',
        grandfatherName: 'محمد',
        familyName: 'العتيبي',
        fullName: 'خالد عبدالله محمد العتيبي',
        mobileNumber: '0551234567',
        email: 'khaled@example.com',
        isApplicant: false,
        createdDate: new Date()
      },
      {
        id: 2,
        plaintiffId: 3,
        plaintiffName: 'شركة الرياض للتجارة',
        representativeTypeId: 2,
        representativeTypeName: 'ممثل قانوني',
        representativeTypeNameAr: 'ممثل قانوني',
        identityTypeId: 1,
        identityTypeName: 'هوية وطنية',
        identityNumber: '5566778899',
        firstName: 'سعد',
        fatherName: 'فهد',
        grandfatherName: 'سالم',
        familyName: 'القحطاني',
        fullName: 'سعد فهد سالم القحطاني',
        mobileNumber: '0559876543',
        email: 'saad@company.com',
        isApplicant: false,
        createdDate: new Date()
      },
      {
        id: 3,
        plaintiffId: 3,
        plaintiffName: 'شركة الرياض للتجارة',
        representativeTypeId: 1,
        representativeTypeName: 'وكيل',
        representativeTypeNameAr: 'وكيل',
        identityTypeId: 1,
        identityTypeName: 'هوية وطنية',
        identityNumber: '6677889900',
        firstName: 'عمر',
        fatherName: 'أحمد',
        grandfatherName: 'علي',
        familyName: 'الشمري',
        fullName: 'عمر أحمد علي الشمري',
        mobileNumber: '0554567890',
        isApplicant: false,
        createdDate: new Date()
      }
    ];
    this.representativesDataSource.data = this.allRepresentatives;
    this.setRepresentativesPaginator();
  }

  /**
   * Called when user selects a plaintiff type from the dropdown menu.
   * Navigates to the add form with the selected type pre-set.
   */
  onSelectPlaintiffType(type: PlaintiffType): void {
    this.router.navigate(['/case-registration/plaintiffs/add'], {
      queryParams: { requestId: this.requestId, type: type.id }
    });
  }

  /**
   * @deprecated Use onSelectPlaintiffType instead. Kept for backwards compatibility.
   */
  onAddPlaintiff(): void {
    this.router.navigate(['/case-registration/plaintiffs/add'], {
      queryParams: { requestId: this.requestId }
    });
  }

  onEditPlaintiff(id: number): void {
    this.router.navigate(['/case-registration/plaintiffs', id, 'edit'], {
      queryParams: { requestId: this.requestId }
    });
  }

  onViewPlaintiff(id: number): void {
    this.router.navigate(['/case-registration/plaintiffs', id, 'view'], {
      queryParams: { requestId: this.requestId }
    });
  }

  onDeletePlaintiff(plaintiff: PlaintiffListVM): void {
    const dialogData: ConfirmDialogData = {
      title: 'تأكيد الحذف',
      message: `هل أنت متأكد من حذف المدعي "${plaintiff.fullName}"؟`,
      confirmText: 'حذف',
      cancelText: 'إلغاء',
      confirmColor: 'warn',
      icon: 'delete'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.plaintiffService.deletePlaintiff(plaintiff.id).subscribe({
          next: () => {
            this.notification.success('تم حذف المدعي بنجاح');
            this.loadPlaintiffs();
          },
          error: (error) => {
            console.error('Error deleting plaintiff:', error);
            this.notification.error('حدث خطأ أثناء حذف المدعي');
          }
        });
      }
    });
  }

  onSetApplicant(plaintiff: PlaintiffListVM): void {
    const dialogRef = this.dialog.open(SetApplicantDialogComponent, {
      width: '400px',
      data: { plaintiff }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.plaintiffService.setAsApplicant(plaintiff.id).subscribe({
          next: () => {
            this.notification.success('تم تعيين مقدم الطلب بنجاح');
            this.loadPlaintiffs();
          },
          error: (error) => {
            console.error('Error setting applicant:', error);
            this.notification.handleError(error, 'حدث خطأ أثناء تعيين مقدم الطلب');
          }
        });
      }
    });
  }

  getAttachmentStatusClass(status: string): string {
    return status === 'complete' ? 'status-complete' : 'status-incomplete';
  }

  getAttachmentStatusText(status: string): string {
    return status === 'complete' ? 'مكتملة' : 'ناقصة';
  }

  // Load all representatives for all plaintiffs
  private loadAllRepresentatives(): void {
    this.isLoadingRepresentatives = true;
    this.allRepresentatives = [];
    this.representativesDataSource.data = [];

    // Load representatives for each plaintiff
    const plaintiffIds = this.plaintiffs.map(p => p.id);
    let completedRequests = 0;

    if (plaintiffIds.length === 0) {
      this.isLoadingRepresentatives = false;
      return;
    }

    plaintiffIds.forEach(plaintiffId => {
      const plaintiff = this.plaintiffs.find(p => p.id === plaintiffId);
      this.representativeService.getRepresentatives(plaintiffId).subscribe({
        next: (data) => {
          // Add plaintiff name to each representative
          const repsWithPlaintiff = data.map(rep => ({
            ...rep,
            plaintiffName: plaintiff?.displayName || plaintiff?.fullName
          }));
          this.allRepresentatives = [...this.allRepresentatives, ...repsWithPlaintiff];
          completedRequests++;
          if (completedRequests === plaintiffIds.length) {
            this.representativesDataSource.data = this.allRepresentatives;
            this.isLoadingRepresentatives = false;
            this.setRepresentativesPaginator();
          }
        },
        error: (error) => {
          console.error('Error loading representatives for plaintiff:', plaintiffId, error);
          completedRequests++;
          if (completedRequests === plaintiffIds.length) {
            this.representativesDataSource.data = this.allRepresentatives;
            this.isLoadingRepresentatives = false;
            this.setRepresentativesPaginator();
          }
        }
      });
    });
  }

  // Set representative as applicant
  onSetRepresentativeAsApplicant(representative: RepresentativeVM & { plaintiffName?: string }): void {
    const dialogData: ConfirmDialogData = {
      title: 'تعيين مقدم الطلب',
      message: `هل تريد تعيين "${representative.fullName}" كمقدم للطلب؟ سيتم أيضاً تعيين أي مدعي بنفس رقم الهوية كمقدم للطلب.`,
      confirmText: 'تعيين',
      cancelText: 'إلغاء',
      confirmColor: 'primary',
      icon: 'person_pin'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '450px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.representativeService.setAsApplicant(representative.id).subscribe({
          next: () => {
            this.notification.success('تم تعيين مقدم الطلب بنجاح');
            // Reload both plaintiffs and representatives to reflect changes
            this.loadPlaintiffs();
          },
          error: (error) => {
            console.error('Error setting representative as applicant:', error);
            this.notification.handleError(error, 'حدث خطأ أثناء تعيين مقدم الطلب');
          }
        });
      }
    });
  }
}
