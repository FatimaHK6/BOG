# خطة المطور أ: وحدة المدعين والممثلين
# Developer A Plan: Plaintiffs & Representatives Module

**النطاق:** حالات الاستخدام 6.5.1.1.1 - 6.5.1.1.10 (10 حالات)
**التركيز:** إدارة المدعين، الممثلين، المرفقات، العنوان المختار
**الاعتمادية:** يجب إكمال المرحلة 1 (قاعدة البيانات) أولاً

---

## جدول المحتويات

1. [حالات الاستخدام](#1-حالات-الاستخدام)
2. [الملفات المطلوبة](#2-الملفات-المطلوبة)
3. [نقاط النهاية (API)](#3-نقاط-النهاية-api)
4. [قواعد الأعمال](#4-قواعد-الأعمال)
5. [تكامل أبشر](#5-تكامل-أبشر)
6. [التحقق](#6-التحقق)
7. [واجهة المستخدم (Frontend)](#7-واجهة-المستخدم-frontend)

---

## 1. حالات الاستخدام

| الرمز | الاسم | الوصف | الأولوية |
|-------|-------|-------|----------|
| 6.5.1.1.1 | المدعين | عرض قائمة المدعين في الطلب | عالية |
| 6.5.1.1.2 | إضافة مدعي | إضافة مدعي جديد (8 أنواع) | عالية |
| 6.5.1.1.3 | استعراض تفاصيل مدّعي | عرض بيانات المدعي الكاملة | عالية |
| 6.5.1.1.4 | تعديل مدّعي | تعديل بيانات مدعي موجود | عالية |
| 6.5.1.1.5 | مرفقات مدّعي | إضافة/حذف مرفقات المدعي | متوسطة |
| 6.5.1.1.6 | العنوان المختار | تحديد العنوان للإشعارات | متوسطة |
| 6.5.1.1.7 | إضافة ممثل | إضافة ممثل للمدعي (9 أنواع) | عالية |
| 6.5.1.1.8 | تفاصيل ممثّل | عرض بيانات الممثل | عالية |
| 6.5.1.1.9 | تعديل ممثّل | تعديل بيانات الممثل | عالية |
| 6.5.1.1.10 | مقدّم الطلب | تعيين مدعي كمقدم طلب | عالية |

---

## 2. الملفات المطلوبة

### 2.1 طبقة الوصول للبيانات (BOG.DAL)

```
BOG.DAL/
├── Interfaces/
│   ├── IPlaintiffRepository.cs
│   ├── IRepresentativeRepository.cs
│   ├── IPlaintiffAttachmentRepository.cs
│   └── IAddressRepository.cs
└── Repositories/
    ├── PlaintiffRepository.cs
    ├── RepresentativeRepository.cs
    ├── PlaintiffAttachmentRepository.cs
    └── AddressRepository.cs
```

#### واجهة مستودع المدعي

```csharp
// IPlaintiffRepository.cs
public interface IPlaintiffRepository : IRepository<Plaintiff>
{
    Task<IEnumerable<Plaintiff>> GetByRequestIdAsync(int requestId);
    Task<Plaintiff?> GetByIdentityAsync(int requestId, string identityNumber, int plaintiffTypeId);
    Task<bool> ExistsByIdentityAsync(int requestId, string identityNumber, int plaintiffTypeId);
}
```

#### واجهة مستودع الممثل

```csharp
// IRepresentativeRepository.cs
public interface IRepresentativeRepository : IRepository<Representative>
{
    Task<IEnumerable<Representative>> GetByPlaintiffIdAsync(int plaintiffId);
    Task<Representative?> GetByIdentityAsync(int plaintiffId, string identityNumber);
    Task<bool> ExistsByIdentityAsync(int plaintiffId, string identityNumber);
}
```

### 2.2 كائنات نقل البيانات (BOG.DTO)

```
BOG.DTO/
├── Plaintiff/
│   ├── PlaintiffCreateDTO.cs        # إنشاء مدعي جديد
│   ├── PlaintiffUpdateDTO.cs        # تحديث بيانات مدعي
│   ├── PlaintiffAttachmentDTO.cs    # مرفقات المدعي
│   └── SelectedAddressDTO.cs        # العنوان المختار
└── Representative/
    ├── RepresentativeCreateDTO.cs   # إنشاء ممثل جديد
    └── RepresentativeUpdateDTO.cs   # تحديث بيانات ممثل
```

#### كائن إنشاء المدعي

```csharp
// PlaintiffCreateDTO.cs
public class PlaintiffCreateDTO
{
    [Required]
    public int PlaintiffTypeId { get; set; }           // نوع المدعي

    // البيانات الشخصية (للأفراد)
    public int? IdentityTypeId { get; set; }           // نوع الهوية
    public string? IdentityNumber { get; set; }        // رقم الهوية

    // الأسماء (يمكن ملؤها من أبشر)
    public string? FirstName { get; set; }             // الاسم الأول
    public string? FatherName { get; set; }            // اسم الأب
    public string? GrandfatherName { get; set; }       // اسم الجد
    public string? FamilyName { get; set; }            // اسم العائلة
    public DateTime? BirthDate { get; set; }           // تاريخ الميلاد
    public string? Gender { get; set; }                // الجنس

    // معلومات الاتصال
    public string? MobileNumber { get; set; }          // رقم الجوال
    public string? Email { get; set; }                 // البريد الإلكتروني

    // للمؤسسات والشركات
    public string? CommercialRegNumber { get; set; }   // رقم السجل التجاري
    public string? CompanyName { get; set; }           // اسم الشركة

    // للجهات الحكومية
    public int? GovernmentAgencyId { get; set; }       // الجهة الحكومية
    public string? AdditionalStatement { get; set; }   // بيان إضافي
}
```

### 2.3 نماذج العرض (BOG.VM)

```
BOG.VM/
├── Plaintiff/
│   ├── PlaintiffVM.cs               # تفاصيل المدعي الكاملة
│   ├── PlaintiffListVM.cs           # ملخص للقائمة
│   └── PlaintiffAttachmentVM.cs     # عرض المرفقات
└── Representative/
    └── RepresentativeVM.cs          # تفاصيل الممثل
```

#### نموذج عرض المدعي

```csharp
// PlaintiffVM.cs
public class PlaintiffVM
{
    public int Id { get; set; }
    public int PlaintiffTypeId { get; set; }
    public string PlaintiffTypeName { get; set; }      // اسم النوع
    public string PlaintiffTypeNameAr { get; set; }    // اسم النوع بالعربية

    // الاسم الكامل (محسوب)
    public string FullName => $"{FirstName} {FatherName} {GrandfatherName} {FamilyName}".Trim();

    public string? FirstName { get; set; }
    public string? FatherName { get; set; }
    public string? GrandfatherName { get; set; }
    public string? FamilyName { get; set; }

    public string? IdentityNumber { get; set; }
    public int? DataSourceId { get; set; }             // مصدر البيانات (1=أبشر، 2=المستخدم)
    public string? DataSourceName { get; set; }        // اسم مصدر البيانات

    public string? MobileNumber { get; set; }
    public string? Email { get; set; }

    // العناوين
    public AddressVM? ResidenceAddress { get; set; }
    public AddressVM? WorkAddress { get; set; }
    public AddressVM? SelectedAddress { get; set; }

    // الممثلين
    public List<RepresentativeVM> Representatives { get; set; }

    // المرفقات
    public List<PlaintiffAttachmentVM> Attachments { get; set; }
}
```

### 2.4 طبقة منطق الأعمال (BOG.BL)

```
BOG.BL/
├── Interfaces/
│   ├── IPlaintiffBL.cs
│   ├── IRepresentativeBL.cs
│   └── IPlaintiffAttachmentBL.cs
└── Services/
    ├── PlaintiffBL.cs
    ├── RepresentativeBL.cs
    └── PlaintiffAttachmentBL.cs
```

#### واجهة منطق الأعمال للمدعي

```csharp
// IPlaintiffBL.cs
public interface IPlaintiffBL
{
    Task<IEnumerable<PlaintiffListVM>> GetByRequestIdAsync(int requestId);
    Task<PlaintiffVM> GetByIdAsync(int id);
    Task<PlaintiffVM> CreateAsync(int requestId, PlaintiffCreateDTO dto);
    Task<PlaintiffVM> UpdateAsync(int id, PlaintiffUpdateDTO dto);
    Task DeleteAsync(int id);
    Task SetAsApplicantAsync(int plaintiffId);
    Task<AddressVM> GetSelectedAddressAsync(int plaintiffId);
    Task<AddressVM> SetSelectedAddressAsync(int plaintiffId, SelectedAddressDTO dto);
}
```

### 2.5 متحكمات API (BOG.API)

```
BOG.API/Controllers/
├── PlaintiffsController.cs
└── RepresentativesController.cs
```

---

## 3. نقاط النهاية (API)

### 3.1 متحكم المدعين (PlaintiffsController)

| الطريقة | المسار | الوصف |
|---------|--------|-------|
| GET | `/api/case-requests/{requestId}/plaintiffs` | قائمة المدعين في طلب |
| POST | `/api/case-requests/{requestId}/plaintiffs` | إضافة مدعي جديد |
| GET | `/api/plaintiffs/{id}` | تفاصيل مدعي |
| PUT | `/api/plaintiffs/{id}` | تعديل مدعي |
| DELETE | `/api/plaintiffs/{id}` | حذف مدعي |
| POST | `/api/plaintiffs/{id}/set-applicant` | تعيين كمقدم طلب |

### 3.2 مرفقات المدعي

| الطريقة | المسار | الوصف |
|---------|--------|-------|
| GET | `/api/plaintiffs/{id}/attachments` | قائمة المرفقات |
| POST | `/api/plaintiffs/{id}/attachments` | إضافة مرفق |
| DELETE | `/api/plaintiffs/{id}/attachments/{attachmentId}` | حذف مرفق |

### 3.3 العنوان المختار

| الطريقة | المسار | الوصف |
|---------|--------|-------|
| GET | `/api/plaintiffs/{id}/selected-address` | العنوان الحالي |
| PUT | `/api/plaintiffs/{id}/selected-address` | تحديث العنوان |

### 3.4 متحكم الممثلين (RepresentativesController)

| الطريقة | المسار | الوصف |
|---------|--------|-------|
| GET | `/api/plaintiffs/{plaintiffId}/representatives` | قائمة ممثلي المدعي |
| POST | `/api/plaintiffs/{plaintiffId}/representatives` | إضافة ممثل |
| GET | `/api/representatives/{id}` | تفاصيل ممثل |
| PUT | `/api/representatives/{id}` | تعديل ممثل |
| DELETE | `/api/representatives/{id}` | حذف ممثل |

---

## 4. قواعد الأعمال

### 4.1 أنواع المدعين وأنواع الممثلين المسموحة

| نوع المدعي | أنواع الممثلين المسموحة |
|------------|------------------------|
| فرد | محامي/وكيل، مصفي، أمين تفليسة، حارس قضائي، ولي، وصي |
| فرد بدون هوية | محامي/وكيل، مصفي، أمين تفليسة، حارس قضائي، ولي، وصي |
| صاحب مؤسسة | محامي/وكيل، مصفي، أمين تفليسة، حارس قضائي |
| شركة مسجلة | محامي/وكيل، مصفي، أمين تفليسة، حارس قضائي، ممثل نظامي |
| شركة غير مسجلة | محامي/وكيل، مصفي، أمين تفليسة، حارس قضائي |
| جهة حكومية | محامي/وكيل، ممثل جهة حكومية |
| جمعية/مؤسسة أهلية | محامي/وكيل، ممثل نظامي |
| وقف | محامي/وكيل، ناظر |

### 4.2 رسائل التحقق

| الرمز | الرسالة | الوصف |
|-------|---------|-------|
| ERR008 | الممثّل موجود مسبقاً للمدعي | لا يمكن إضافة نفس الممثل مرتين |
| ERR011 | المدّعي موجود مسبقاً بنفس رقم الهوية | تحقق من التكرار |
| ERR012 | رقم هوية الممثّل نفس رقم هوية المدّعي | الممثل لا يمكن أن يكون المدعي نفسه |

### 4.3 قواعد مقدم الطلب (BC07)

- فقط المدعي من نوع **"فرد"** أو **"فرد بدون هوية"** يمكنه أن يكون مقدم الطلب
- يجب أن يكون مقدم الطلب إما:
  - أصيل (المدعي نفسه)
  - وكيل (ممثل عن المدعي)
  - أصيل ووكيل (كلاهما)

### 4.4 قواعد العنوان المختار (BC03)

- إذا كانت مدينة المدعي مختلفة عن مدينة المحكمة
- يجب على المدعي تحديد عنوان مختار للإشعارات
- العنوان يمكن أن يكون عنوان السكن أو عنوان العمل

### 4.5 قواعد المرفقات (BR04)

- نوع الملف: PDF فقط
- الحجم الأقصى: 4 ميجابايت
- المرفقات حسب نوع المدعي:
  - فرد: صورة الهوية
  - شركة مسجلة: السجل التجاري
  - جهة حكومية: قرار التمثيل
  - وقف: صك الوقف

---

## 5. تكامل أبشر

### 5.0 جدول مصدر البيانات (DataSource)

| Id | Name | NameAr | الوصف |
|----|------|--------|-------|
| 1 | FromAbsher | من أبشر | البيانات مجلوبة من نظام أبشر |
| 2 | FromUser | من المستخدم | البيانات مدخلة يدوياً |

**ملاحظة:** استخدم `DataSourceId` بدلاً من `IsAbsherVerified` لتتبع مصدر بيانات المدعي أو الممثل.

### 5.1 استخدام خدمة أبشر

المدعي من نوع **"فرد"** يجب التحقق منه عبر أبشر:

```csharp
// في PlaintiffBL.cs
public async Task<PlaintiffVM> CreateAsync(int requestId, PlaintiffCreateDTO dto)
{
    // التحقق من التكرار
    if (await _repository.ExistsByIdentityAsync(requestId, dto.IdentityNumber, dto.PlaintiffTypeId))
    {
        throw new BusinessException("ERR011", "المدّعي موجود مسبقاً بنفس رقم الهوية");
    }

    // التحقق من الهوية للأفراد
    if (dto.PlaintiffTypeId == PlaintiffTypes.Individual)
    {
        var verificationResult = await _absherService.VerifyIdentityAsync(
            dto.IdentityNumber,
            dto.IdentityTypeId ?? 1
        );

        if (!verificationResult.IsVerified)
        {
            throw new BusinessException("ERR_ABSHER", verificationResult.ErrorMessageAr);
        }

        // جلب البيانات الشخصية تلقائياً
        var personData = await _absherService.GetPersonDataAsync(
            dto.IdentityNumber,
            dto.IdentityTypeId ?? 1
        );

        if (personData != null)
        {
            dto.FirstName = personData.FirstName;
            dto.FatherName = personData.FatherName;
            dto.GrandfatherName = personData.GrandfatherName;
            dto.FamilyName = personData.FamilyName;
            dto.BirthDate = personData.BirthDate;
            dto.Gender = personData.Gender;
            dto.MobileNumber = personData.MobileNumber;
            dto.Email = personData.Email;
        }
    }

    // إنشاء المدعي
    var plaintiff = _mapper.Map<Plaintiff>(dto);
    plaintiff.CaseRegistrationRequestId = requestId;
    // DataSourceId: 1 = FromAbsher (من أبشر), 2 = FromUser (من المستخدم)
    plaintiff.DataSourceId = dto.PlaintiffTypeId == PlaintiffTypes.Individual ? 1 : 2;

    await _repository.AddAsync(plaintiff);
    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<PlaintiffVM>(plaintiff);
}
```

### 5.2 التحقق من الممثل

```csharp
// في RepresentativeBL.cs
public async Task<RepresentativeVM> CreateAsync(int plaintiffId, RepresentativeCreateDTO dto)
{
    var plaintiff = await _plaintiffRepository.GetByIdAsync(plaintiffId);
    if (plaintiff == null)
    {
        throw new NotFoundException("المدعي غير موجود");
    }

    // التحقق من نوع الممثل المسموح
    if (!IsRepresentativeTypeAllowed(plaintiff.PlaintiffTypeId, dto.RepresentativeTypeId))
    {
        throw new BusinessException("ERR_REP_TYPE", "نوع الممثل غير مسموح لهذا النوع من المدعين");
    }

    // التحقق من أن رقم هوية الممثل ليس نفس رقم هوية المدعي
    if (dto.IdentityNumber == plaintiff.IdentityNumber)
    {
        throw new BusinessException("ERR012", "رقم هوية الممثّل نفس رقم هوية المدّعي");
    }

    // التحقق من التكرار
    if (await _repository.ExistsByIdentityAsync(plaintiffId, dto.IdentityNumber))
    {
        throw new BusinessException("ERR008", "الممثّل موجود مسبقاً للمدعي");
    }

    // التحقق من أبشر
    var verificationResult = await _absherService.VerifyIdentityAsync(
        dto.IdentityNumber,
        dto.IdentityTypeId
    );

    // إنشاء الممثل
    var representative = _mapper.Map<Representative>(dto);
    representative.PlaintiffId = plaintiffId;
    // DataSourceId: 1 = FromAbsher (من أبشر), 2 = FromUser (من المستخدم)
    representative.DataSourceId = verificationResult.IsVerified ? 1 : 2;

    await _repository.AddAsync(representative);
    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<RepresentativeVM>(representative);
}
```

---

## 6. التحقق

### 6.1 سيناريوهات الاختبار

1. **إضافة مدعي فرد:**
   - إدخال رقم هوية صحيح
   - التحقق من أبشر
   - جلب البيانات تلقائياً
   - حفظ المدعي

2. **إضافة مدعي شركة:**
   - إدخال رقم السجل التجاري
   - إضافة ممثل نظامي
   - حفظ المدعي

3. **إضافة ممثل:**
   - التحقق من نوع الممثل المسموح
   - التحقق من عدم التكرار
   - التحقق من أبشر
   - حفظ الممثل

4. **تعيين مقدم طلب:**
   - التحقق من أن المدعي فرد
   - تعيين كمقدم طلب

5. **اختيار العنوان:**
   - عند اختلاف مدينة المدعي عن مدينة المحكمة
   - تحديد العنوان المختار

### 6.2 أوامر الاختبار

```bash
# بناء الحل
dotnet build src/Backend/BOG.sln

# تشغيل API
dotnet run --project src/Backend/BOG.API

# اختبار مع Swagger
# https://localhost:5001/swagger
```

---

## 7. واجهة المستخدم (Frontend)

### 7.1 هيكل المجلدات

```
src/Frontend/bog-app/src/app/
├── features/
│   └── case-registration/
│       ├── plaintiffs/
│       │   ├── components/
│       │   │   ├── plaintiff-list/
│       │   │   │   ├── plaintiff-list.component.ts
│       │   │   │   ├── plaintiff-list.component.html
│       │   │   │   └── plaintiff-list.component.scss
│       │   │   ├── plaintiff-wizard/
│       │   │   │   ├── plaintiff-wizard.component.ts
│       │   │   │   ├── plaintiff-wizard.component.html
│       │   │   │   ├── plaintiff-wizard.component.scss
│       │   │   │   ├── step-personal-data/
│       │   │   │   ├── step-representatives/
│       │   │   │   ├── step-attachments/
│       │   │   │   └── step-additional-data/
│       │   │   ├── plaintiff-details/
│       │   │   │   ├── plaintiff-details.component.ts
│       │   │   │   ├── plaintiff-details.component.html
│       │   │   │   └── plaintiff-details.component.scss
│       │   │   └── set-applicant-dialog/
│       │   │       ├── set-applicant-dialog.component.ts
│       │   │       └── set-applicant-dialog.component.html
│       │   ├── models/
│       │   │   ├── plaintiff.model.ts
│       │   │   ├── plaintiff-type.model.ts
│       │   │   └── address.model.ts
│       │   ├── services/
│       │   │   └── plaintiff.service.ts
│       │   └── plaintiffs.module.ts
│       ├── representatives/
│       │   ├── components/
│       │   │   ├── representative-list/
│       │   │   │   ├── representative-list.component.ts
│       │   │   │   ├── representative-list.component.html
│       │   │   │   └── representative-list.component.scss
│       │   │   ├── representative-form/
│       │   │   │   ├── representative-form.component.ts
│       │   │   │   ├── representative-form.component.html
│       │   │   │   └── representative-form.component.scss
│       │   │   └── representative-details/
│       │   │       ├── representative-details.component.ts
│       │   │       └── representative-details.component.html
│       │   ├── models/
│       │   │   ├── representative.model.ts
│       │   │   └── representative-type.model.ts
│       │   ├── services/
│       │   │   └── representative.service.ts
│       │   └── representatives.module.ts
│       ├── attachments/
│       │   ├── components/
│       │   │   ├── attachment-list/
│       │   │   │   ├── attachment-list.component.ts
│       │   │   │   ├── attachment-list.component.html
│       │   │   │   └── attachment-list.component.scss
│       │   │   └── attachment-upload/
│       │   │       ├── attachment-upload.component.ts
│       │   │       ├── attachment-upload.component.html
│       │   │       └── attachment-upload.component.scss
│       │   ├── models/
│       │   │   └── attachment.model.ts
│       │   ├── services/
│       │   │   └── attachment.service.ts
│       │   └── attachments.module.ts
│       └── shared/
│           ├── services/
│           │   └── absher.service.ts
│           └── components/
│               ├── identity-lookup/
│               │   ├── identity-lookup.component.ts
│               │   └── identity-lookup.component.html
│               └── address-form/
│                   ├── address-form.component.ts
│                   └── address-form.component.html
```

### 7.2 المكونات الرئيسية

#### قائمة المدعين (plaintiff-list)

```typescript
// plaintiff-list.component.ts
@Component({
  selector: 'app-plaintiff-list',
  templateUrl: './plaintiff-list.component.html',
  styleUrls: ['./plaintiff-list.component.scss']
})
export class PlaintiffListComponent implements OnInit {
  plaintiffs: PlaintiffListVM[] = [];
  displayedColumns = ['type', 'identityNumber', 'fullName',
                      'representativesCount', 'attachmentStatus', 'isApplicant', 'actions'];

  constructor(
    private plaintiffService: PlaintiffService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadPlaintiffs();
  }

  onAddPlaintiff(): void {
    // Navigate to wizard
  }

  onSetApplicant(plaintiff: PlaintiffListVM): void {
    const dialogRef = this.dialog.open(SetApplicantDialogComponent, {
      data: { plaintiff }
    });
  }
}
```

#### معالج إضافة المدعي (plaintiff-wizard)

```typescript
// plaintiff-wizard.component.ts
@Component({
  selector: 'app-plaintiff-wizard',
  templateUrl: './plaintiff-wizard.component.html'
})
export class PlaintiffWizardComponent implements OnInit {
  currentStep = 0;
  steps = [
    { label: 'بيانات الشخصي', completed: false },
    { label: 'بيانات الممثلين', completed: false },
    { label: 'المرفقات', completed: false },
    { label: 'بيانات إضافية', completed: false }
  ];

  plaintiffForm: FormGroup;
  plaintiffTypes: PlaintiffType[] = [];

  constructor(
    private fb: FormBuilder,
    private plaintiffService: PlaintiffService,
    private absherService: AbsherService
  ) {}

  onIdentityLookup(): void {
    // Call Absher service to fetch data
  }

  onNext(): void {
    if (this.currentStep < 3) {
      this.steps[this.currentStep].completed = true;
      this.currentStep++;
    }
  }

  onPrevious(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  onSave(): void {
    // Save plaintiff
  }
}
```

#### نموذج الممثل (representative-form)

```typescript
// representative-form.component.ts
@Component({
  selector: 'app-representative-form',
  templateUrl: './representative-form.component.html'
})
export class RepresentativeFormComponent implements OnInit {
  @Input() plaintiffId: number;
  @Output() saved = new EventEmitter<Representative>();

  representativeForm: FormGroup;
  representativeTypes: RepresentativeType[] = [];
  allowedRepresentativeTypes: RepresentativeType[] = [];

  constructor(
    private fb: FormBuilder,
    private representativeService: RepresentativeService
  ) {}

  filterAllowedTypes(plaintiffTypeId: number): void {
    // Filter based on plaintiff type
  }
}
```

#### رفع المرفقات (attachment-upload)

```typescript
// attachment-upload.component.ts
@Component({
  selector: 'app-attachment-upload',
  templateUrl: './attachment-upload.component.html'
})
export class AttachmentUploadComponent {
  @Input() plaintiffId: number;
  @Input() requiredAttachments: string[] = [];
  @Output() uploaded = new EventEmitter<Attachment>();

  maxFileSize = 4 * 1024 * 1024; // 4MB
  allowedTypes = ['application/pdf'];

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.validateAndUpload(file);
    }
  }

  private validateAndUpload(file: File): void {
    if (file.size > this.maxFileSize) {
      this.showError('حجم الملف يتجاوز الحد الأقصى (4 ميجابايت)');
      return;
    }
    if (!this.allowedTypes.includes(file.type)) {
      this.showError('يجب أن يكون الملف بصيغة PDF فقط');
      return;
    }
    // Upload file
  }
}
```

### 7.3 النماذج (Models)

```typescript
// plaintiff.model.ts
export interface Plaintiff {
  id: number;
  plaintiffTypeId: number;
  plaintiffTypeName: string;
  identityTypeId?: number;
  identityNumber?: string;
  firstName?: string;
  fatherName?: string;
  grandfatherName?: string;
  familyName?: string;
  fullName: string;
  birthDate?: Date;
  gender?: string;
  mobileNumber?: string;
  email?: string;
  dataSourceId?: number;
  dataSourceName?: string;
  isApplicant: boolean;
  representativesCount: number;
  attachmentStatus: 'complete' | 'incomplete';
  residenceAddress?: Address;
  workAddress?: Address;
  selectedAddress?: Address;
}

// representative.model.ts
export interface Representative {
  id: number;
  plaintiffId: number;
  representativeTypeId: number;
  representativeTypeName: string;
  identityNumber: string;
  fullName: string;
  mobileNumber?: string;
  email?: string;
  powerOfAttorneyNumber?: string;
  powerOfAttorneyDate?: Date;
  powerOfAttorneySource?: string;
  guardianshipType?: string;
}

// attachment.model.ts
export interface Attachment {
  id: number;
  attachmentTypeId: number;
  attachmentTypeName: string;
  fileName: string;
  fileSize: number;
  uploadDate: Date;
  fileUrl: string;
}
```

### 7.4 الخدمات (Services)

```typescript
// plaintiff.service.ts
@Injectable({ providedIn: 'root' })
export class PlaintiffService {
  private apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  getPlaintiffs(requestId: number): Observable<PlaintiffListVM[]> {
    return this.http.get<PlaintiffListVM[]>(
      `${this.apiUrl}/case-requests/${requestId}/plaintiffs`
    );
  }

  getPlaintiff(id: number): Observable<Plaintiff> {
    return this.http.get<Plaintiff>(`${this.apiUrl}/plaintiffs/${id}`);
  }

  createPlaintiff(requestId: number, plaintiff: PlaintiffCreateDTO): Observable<Plaintiff> {
    return this.http.post<Plaintiff>(
      `${this.apiUrl}/case-requests/${requestId}/plaintiffs`, plaintiff
    );
  }

  updatePlaintiff(id: number, plaintiff: PlaintiffUpdateDTO): Observable<Plaintiff> {
    return this.http.put<Plaintiff>(`${this.apiUrl}/plaintiffs/${id}`, plaintiff);
  }

  deletePlaintiff(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/plaintiffs/${id}`);
  }

  setAsApplicant(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/plaintiffs/${id}/set-applicant`, {});
  }
}

// absher.service.ts
@Injectable({ providedIn: 'root' })
export class AbsherService {
  private apiUrl = `${environment.apiUrl}/api/absher`;

  constructor(private http: HttpClient) {}

  verifyIdentity(identityNumber: string, identityTypeId: number): Observable<VerificationResult> {
    return this.http.post<VerificationResult>(`${this.apiUrl}/verify`, {
      identityNumber, identityTypeId
    });
  }

  getPersonData(identityNumber: string, identityTypeId: number): Observable<PersonData> {
    return this.http.get<PersonData>(`${this.apiUrl}/person-data`, {
      params: { identityNumber, identityTypeId: identityTypeId.toString() }
    });
  }
}
```

### 7.5 رسائل التحقق

| الرمز | الرسالة العربية | الاستخدام |
|-------|----------------|-----------|
| ERR008 | الممثّل موجود مسبقاً للمدعي | عند إضافة ممثل مكرر |
| ERR011 | المدّعي موجود مسبقاً بنفس رقم الهوية | عند إضافة مدعي مكرر |
| ERR012 | رقم هوية الممثّل نفس رقم هوية المدّعي | عند إضافة الممثل |
| BR04_TYPE | يجب أن يكون الملف بصيغة PDF فقط | عند رفع المرفقات |
| BR04_SIZE | حجم الملف يتجاوز الحد الأقصى (4 ميجابايت) | عند رفع المرفقات |
| BC07 | فقط المدعي من نوع 'فرد' يمكنه أن يكون مقدم الطلب | عند تعيين مقدم الطلب |

### 7.6 أوامر إنشاء المكونات

```bash
# الانتقال للمشروع
cd src/Frontend/bog-app

# إنشاء الوحدات
ng generate module features/case-registration/plaintiffs --routing
ng generate module features/case-registration/representatives --routing
ng generate module features/case-registration/attachments --routing

# مكونات المدعين
ng generate component features/case-registration/plaintiffs/components/plaintiff-list
ng generate component features/case-registration/plaintiffs/components/plaintiff-wizard
ng generate component features/case-registration/plaintiffs/components/plaintiff-wizard/step-personal-data
ng generate component features/case-registration/plaintiffs/components/plaintiff-wizard/step-representatives
ng generate component features/case-registration/plaintiffs/components/plaintiff-wizard/step-attachments
ng generate component features/case-registration/plaintiffs/components/plaintiff-wizard/step-additional-data
ng generate component features/case-registration/plaintiffs/components/plaintiff-details
ng generate component features/case-registration/plaintiffs/components/set-applicant-dialog

# مكونات الممثلين
ng generate component features/case-registration/representatives/components/representative-list
ng generate component features/case-registration/representatives/components/representative-form
ng generate component features/case-registration/representatives/components/representative-details

# مكونات المرفقات
ng generate component features/case-registration/attachments/components/attachment-list
ng generate component features/case-registration/attachments/components/attachment-upload

# المكونات المشتركة
ng generate component features/case-registration/shared/components/identity-lookup
ng generate component features/case-registration/shared/components/address-form

# الخدمات
ng generate service features/case-registration/plaintiffs/services/plaintiff
ng generate service features/case-registration/representatives/services/representative
ng generate service features/case-registration/attachments/services/attachment
ng generate service features/case-registration/shared/services/absher
```

---

## المراجع

- **خطة التنفيذ الرئيسية:** `UC-6.5.1-Implementation-Plan.md`
- **مستند المتطلبات:** `docs/srs/use-cases/6.5.1-case-registration/SRS_UC_6.5.1_Full_AR.md`
- **خدمة أبشر:** `BOG.Integration/Services/AbsherMockService.cs`
- **مواصفات واجهة المستخدم:** `Ux/UC-6.5.1-UX-Specification.md`
