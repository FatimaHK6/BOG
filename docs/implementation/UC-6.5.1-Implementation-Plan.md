# خطة تنفيذ حالة الاستخدام 6.5.1 - إدارة طلبات قيد دعوى إدارية
# Implementation Plan: UC 6.5.1 - Case Registration Management

**الإصدار:** 1.0
**التاريخ:** 2026-01-14
**المشروع:** نظام إدارة الدعاوى - ديوان المظالم

---

## جدول المحتويات

1. [نظرة عامة](#1-نظرة-عامة)
2. [المرحلة 0: الأدوار والصلاحيات](#2-المرحلة-0-الأدوار-والصلاحيات)
3. [المرحلة 1: طبقة قاعدة البيانات](#3-المرحلة-1-طبقة-قاعدة-البيانات)
4. [المرحلة 2: توزيع المهام على المطورين](#4-المرحلة-2-توزيع-المهام-على-المطورين)
5. [المرحلة 3: الدمج والاختبار](#5-المرحلة-3-الدمج-والاختبار)
6. [المرحلة 4: خدمات التكامل](#6-المرحلة-4-خدمات-التكامل)
7. [ملخص الملفات](#7-ملخص-الملفات)
8. [التحقق](#8-التحقق)

---

## 1. نظرة عامة

تنفذ هذه الخطة حالة الاستخدام 6.5.1 (إدارة طلبات قيد دعوى إدارية) باتباع أنماط معمارية SOLID المعتمدة في المشروع.

### هيكل التنفيذ

| المرحلة | الوصف | الاعتماديات |
|---------|-------|-------------|
| المرحلة 0 | الأدوار والصلاحيات (الأساس) | لا يوجد - يجب إكمالها أولاً |
| المرحلة 1 | طبقة قاعدة البيانات (الكيانات، DbContext، الهجرات) | المرحلة 0 |
| المرحلة 2أ | وحدة المدعين والممثلين | المرحلة 1 |
| المرحلة 2ب | وحدة المدعى عليهم وإدارة الطلبات | المرحلة 1 |
| المرحلة 3 | الدمج والاختبار | المرحلة 2أ + 2ب |
| المرحلة 4 | خدمات التكامل والمحاكاة | المرحلة 3 |

### ملخص حالات الاستخدام

| العنصر | العدد |
|--------|-------|
| حالات الاستخدام الفرعية | 24 |
| أنواع المدعين | 8 |
| أنواع المدعى عليهم | 6 |
| أنواع الممثلين | 9 |
| حالات الطلب | 10 |

---

## 2. المرحلة 0: الأدوار والصلاحيات

**الأولوية:** يجب إكمالها قبل المرحلة 1

### 2.1 أدوار المستخدمين

| الدور | الاسم بالعربية | الصلاحيات |
|-------|----------------|-----------|
| Clerk | كاتب | إنشاء الطلبات، إضافة المدعين/المدعى عليهم، التقديم |
| Reviewer | مدقق | عرض الطلبات، اتخاذ الإجراءات (قيد، رفض، طلب استكمال) |
| Judge | قاضي | عرض على المكتب، الموافقة/الرفض من مكتب القاضي |
| RegistrationEmployee | موظف قيد | إنشاء الطلبات، تعديل، تقديم، قيد الدعاوى |
| StatusEmployee | موظف حالة | عرض الطلبات، تحديث الحالة |
| Admin | مدير | صلاحيات كاملة |

### 2.2 الكيانات المطلوبة

```
BOG.DbModel/Entities/Identity/
├── Role.cs                    # أدوار النظام
├── UserRole.cs                # ربط المستخدم-الدور (متعدد-لمتعدد)
├── Court.cs                   # المحاكم
├── Department.cs              # الدوائر داخل المحكمة
└── UserDepartment.cs          # تعيين المستخدم للدائرة
```

### 2.3 كيان الدور (Role)

```csharp
public class Role : BaseEntity
{
    public string Name { get; set; }           // Clerk, Reviewer, Judge, Admin
    public string NameAr { get; set; }         // كاتب, مدقق, قاضي, مدير
    public string Description { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; }
}
```

### 2.4 كيان ربط المستخدم بالدور (UserRole)

```csharp
public class UserRole : BaseEntity
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public virtual User User { get; set; }
    public virtual Role Role { get; set; }
}
```

### 2.5 كيان المحكمة (Court)

```csharp
public class Court : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public int RegionId { get; set; }
    public int CityId { get; set; }

    public virtual ICollection<Department> Departments { get; set; }
}
```

### 2.6 كيان الدائرة (Department)

```csharp
public class Department : BaseEntity
{
    public int CourtId { get; set; }
    public string Name { get; set; }
    public string NameAr { get; set; }

    public virtual Court Court { get; set; }
    public virtual ICollection<UserDepartment> UserDepartments { get; set; }
}
```

### 2.7 ثوابت الصلاحيات

```csharp
public static class Permissions
{
    public const string CreateRequest = "CaseRegistration.Create";      // إنشاء طلب
    public const string ViewRequest = "CaseRegistration.View";          // عرض طلب
    public const string EditRequest = "CaseRegistration.Edit";          // تعديل طلب
    public const string SubmitRequest = "CaseRegistration.Submit";      // تقديم طلب
    public const string RegisterCase = "CaseRegistration.Register";     // قيد دعوى
    public const string RejectRequest = "CaseRegistration.Reject";      // رفض طلب
    public const string SendToJudge = "CaseRegistration.SendToJudge";   // إرسال للقاضي
    public const string RequestCompletion = "CaseRegistration.RequestCompletion"; // طلب استكمال
    public const string UpdateStatus = "CaseRegistration.UpdateStatus"; // تحديث الحالة
}
```

### 2.8 الملفات المطلوبة للمرحلة 0

```
BOG.DbModel/Entities/Identity/
├── Role.cs
├── UserRole.cs
├── Court.cs
├── Department.cs
└── UserDepartment.cs

BOG.DAL/Interfaces/
├── IRoleRepository.cs
└── IUserDepartmentRepository.cs

BOG.DAL/Repositories/
├── RoleRepository.cs
└── UserDepartmentRepository.cs

BOG.BL/Interfaces/
├── IRoleBL.cs
└── IAuthorizationBL.cs

BOG.BL/Services/
├── RoleBL.cs
└── AuthorizationBL.cs

BOG.API/Authorization/
├── PermissionAttribute.cs
└── PermissionHandler.cs

BOG.API/Controllers/
└── RolesController.cs
```

### 2.9 بيانات البذر (Seed Data)

```csharp
// الأدوار
new Role { Id = 1, Name = "Clerk", NameAr = "كاتب" },
new Role { Id = 2, Name = "Reviewer", NameAr = "مدقق" },
new Role { Id = 3, Name = "Judge", NameAr = "قاضي" },
new Role { Id = 4, Name = "RegistrationEmployee", NameAr = "موظف قيد" },
new Role { Id = 5, Name = "StatusEmployee", NameAr = "موظف حالة" },
new Role { Id = 6, Name = "Admin", NameAr = "مدير" }
```

---

## 3. المرحلة 1: طبقة قاعدة البيانات

**الأولوية:** يجب إكمالها قبل توزيع المهام على المطورين

### 3.1 هيكل الكيانات

```
BOG.DbModel/Entities/
├── Lookups/                          # جداول البحث
│   ├── PlaintiffType.cs              # أنواع المدعين (8)
│   ├── DefendantType.cs              # أنواع المدعى عليهم (6)
│   ├── RepresentativeType.cs         # أنواع الممثلين (9)
│   ├── RequestStatus.cs              # حالات الطلب (10)
│   ├── IdentityType.cs               # أنواع الهوية (3)
│   ├── AttachmentType.cs             # أنواع المرفقات
│   ├── DataSource.cs                 # مصدر البيانات (أبشر/المستخدم)
│   ├── Region.cs                     # المناطق (13)
│   ├── City.cs                       # المدن
│   └── GovernmentAgency.cs           # الجهات الحكومية
│
├── Common/                           # كيانات مشتركة
│   ├── Address.cs                    # العنوان الوطني (6.3.2)
│   ├── ContactInfo.cs                # معلومات الاتصال
│   └── PersonalData.cs               # البيانات الشخصية (6.3.1)
│
└── CaseRegistration/                 # كيانات قيد الدعوى
    ├── CaseRegistrationRequest.cs    # طلب قيد الدعوى
    ├── Claim.cs                      # الطلبات (علاقة واحد لمتعدد)
    ├── RelatedCase.cs                # الدعاوى المرتبطة (علاقة واحد لمتعدد)
    ├── RequestClassification.cs      # تصنيف الدعوى (علاقة واحد لمتعدد)
    ├── Plaintiff.cs                  # المدعي
    ├── CaseRequestPlaintiff.cs       # جدول الربط: طلب-مدعي (متعدد لمتعدد)
    ├── PlaintiffAttachment.cs        # مرفقات المدعي
    ├── Defendant.cs                  # المدعى عليه
    ├── CaseRequestDefendant.cs       # جدول الربط: طلب-مدعى عليه (متعدد لمتعدد)
    ├── Representative.cs             # الممثل
    └── RequestAttachment.cs          # مرفقات الطلب
```

### 3.2 بيانات البذر للجداول المرجعية

#### أنواع المدعين (PlaintiffType)

| المعرف | الاسم بالعربية | الاسم بالإنجليزية |
|--------|----------------|-------------------|
| 1 | فرد | Individual |
| 2 | فرد بدون هوية | Individual without ID |
| 3 | صاحب مؤسسة | Business Owner |
| 4 | شركة مسجلة | Registered Company |
| 5 | شركة غير مسجلة | Unregistered Company |
| 6 | جهة حكومية | Government Agency |
| 7 | جمعية/مؤسسة أهلية | Society/NGO |
| 8 | وقف | Waqf/Endowment |

#### أنواع المدعى عليهم (DefendantType)

| المعرف | الاسم بالعربية | الاسم بالإنجليزية |
|--------|----------------|-------------------|
| 1 | فرد | Individual |
| 2 | شركة مسجلة | Registered Company |
| 3 | شركة غير مسجلة | Unregistered Company |
| 4 | جهة حكومية | Government Agency |
| 5 | صاحب مؤسسة | Business Owner |
| 6 | وقف | Waqf/Endowment |

#### أنواع الممثلين (RepresentativeType)

| المعرف | الاسم بالعربية | الاسم بالإنجليزية |
|--------|----------------|-------------------|
| 1 | محامي/وكيل | Attorney/Lawyer |
| 2 | مصفي | Liquidator |
| 3 | أمين تفليسة | Bankruptcy Trustee |
| 4 | حارس قضائي | Judicial Custodian |
| 5 | ممثل نظامي | Company Representative |
| 6 | ولي | Guardian |
| 7 | ممثل جهة حكومية | Government Rep |
| 8 | وصي | Conservator |
| 9 | ناظر | Waqf Inspector |

#### حالات الطلب (RequestStatus)

| المعرف | الاسم بالعربية | الاسم بالإنجليزية |
|--------|----------------|-------------------|
| 0 | لا يوجد حالة | None |
| 1 | مسوّدة | Draft |
| 2 | يحتاج توثيق | Needs Authentication |
| 3 | طلب جديد | New Request |
| 4 | انتظار فحص الاكتمال | Awaiting Check |
| 5 | معروض على مكتب القاضي | On Judge Desk |
| 6 | دعوى مقيدة | Registered Case |
| 7 | دعوى محفوظة | Case Saved |
| 8 | استكمال النواقص | Pending Completion |
| 9 | التحقق من الاستكمال | Verifying Completion |
| 10 | مرفوضة | Rejected |

#### مصدر البيانات (DataSource)

| المعرف | الاسم بالعربية | الاسم بالإنجليزية |
|--------|----------------|-------------------|
| 1 | من أبشر | FromAbsher |
| 2 | من المستخدم | FromUser |

### 3.3 مواصفات الكيانات الأساسية

#### طلب قيد الدعوى (CaseRegistrationRequest)

```csharp
public class CaseRegistrationRequest : BaseEntity
{
    public int RequestStatusId { get; set; }           // حالة الطلب
    public string Subject { get; set; }                // الموضوع (4000 حرف)
    public string Evidence { get; set; }               // البيّنات (4000 حرف)
    public string Notes { get; set; }                  // ملاحظات
    public int CourtId { get; set; }                   // المحكمة
    public DateTime? SubmissionDate { get; set; }      // تاريخ التقديم
    public DateTime? CompletionDeadline { get; set; }  // مهلة الاستكمال (30 يوم)
    public string RejectionReason { get; set; }        // سبب الرفض

    // تتبع المستخدم
    public int CreatedByUserId { get; set; }           // المستخدم المنشئ
    public int? LastModifiedByUserId { get; set; }     // آخر معدل

    // علاقات التنقل
    public virtual RequestStatus Status { get; set; }
    public virtual Court Court { get; set; }
    public virtual User CreatedByUser { get; set; }

    // علاقات متعدد لمتعدد (Many-to-Many)
    public virtual ICollection<CaseRequestPlaintiff> CaseRequestPlaintiffs { get; set; }
    public virtual ICollection<CaseRequestDefendant> CaseRequestDefendants { get; set; }

    // علاقات واحد لمتعدد (One-to-Many)
    public virtual ICollection<Claim> Claims { get; set; }
    public virtual ICollection<RelatedCase> RelatedCases { get; set; }
    public virtual ICollection<RequestClassification> Classifications { get; set; }
    public virtual ICollection<RequestAttachment> Attachments { get; set; }
}
```

#### الطلبات (Claim) - علاقة واحد لمتعدد

```csharp
public class Claim : BaseEntity
{
    public int CaseRegistrationRequestId { get; set; }  // مفتاح أجنبي
    public string ClaimText { get; set; }               // نص الطلب (2000 حرف)

    // علاقات التنقل
    public virtual CaseRegistrationRequest Request { get; set; }
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE Claim (
    Id                          INT PRIMARY KEY IDENTITY(1,1),
    CaseRegistrationRequestId   INT NOT NULL,
    ClaimText                   NVARCHAR(2000) NOT NULL,
    CreatedDate                 DATETIME2 NOT NULL,
    ModifiedDate                DATETIME2 NULL,
    IsDeleted                   BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Claim_Request
        FOREIGN KEY (CaseRegistrationRequestId)
        REFERENCES CaseRegistrationRequest(Id)
);
```

**العلاقة:**
```
CaseRegistrationRequest (1) ←────→ (N) Claim
        طلب قيد دعوى                    الطلبات
```

#### الدعاوى المرتبطة (RelatedCase) - علاقة واحد لمتعدد

```csharp
public class RelatedCase : BaseEntity
{
    public int CaseRegistrationRequestId { get; set; }  // مفتاح أجنبي
    public string CaseNumber { get; set; }              // رقم القضية المرتبطة (50 حرف)

    // علاقات التنقل
    public virtual CaseRegistrationRequest Request { get; set; }
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE RelatedCase (
    Id                          INT PRIMARY KEY IDENTITY(1,1),
    CaseRegistrationRequestId   INT NOT NULL,
    CaseNumber                  NVARCHAR(50) NOT NULL,
    CreatedDate                 DATETIME2 NOT NULL,
    ModifiedDate                DATETIME2 NULL,
    IsDeleted                   BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_RelatedCase_Request
        FOREIGN KEY (CaseRegistrationRequestId)
        REFERENCES CaseRegistrationRequest(Id)
);
```

**العلاقة:**
```
CaseRegistrationRequest (1) ←────→ (N) RelatedCase
        طلب قيد دعوى                    الدعاوى المرتبطة
```

#### تصنيف الدعوى (RequestClassification) - علاقة واحد لمتعدد

```csharp
public class RequestClassification : BaseEntity
{
    public int CaseRegistrationRequestId { get; set; }  // مفتاح أجنبي
    public string ClassificationText { get; set; }      // نص التصنيف (500 حرف)

    // علاقات التنقل
    public virtual CaseRegistrationRequest Request { get; set; }
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE RequestClassification (
    Id                          INT PRIMARY KEY IDENTITY(1,1),
    CaseRegistrationRequestId   INT NOT NULL,
    ClassificationText          NVARCHAR(500) NOT NULL,
    CreatedDate                 DATETIME2 NOT NULL,
    ModifiedDate                DATETIME2 NULL,
    IsDeleted                   BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_RequestClassification_Request
        FOREIGN KEY (CaseRegistrationRequestId)
        REFERENCES CaseRegistrationRequest(Id)
);
```

**العلاقة:**
```
CaseRegistrationRequest (1) ←────→ (N) RequestClassification
        طلب قيد دعوى                    تصنيفات الدعوى
```

---

### 3.4 جداول الربط (Junction Tables) - علاقات متعدد لمتعدد

#### جدول ربط الطلب بالمدعي (CaseRequestPlaintiff)

```csharp
public class CaseRequestPlaintiff : BaseEntity
{
    public int CaseRegistrationRequestId { get; set; }
    public int PlaintiffId { get; set; }

    // علاقات التنقل
    public virtual CaseRegistrationRequest Request { get; set; }
    public virtual Plaintiff Plaintiff { get; set; }
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE CaseRequestPlaintiff (
    Id                          INT PRIMARY KEY IDENTITY(1,1),
    CaseRegistrationRequestId   INT NOT NULL,
    PlaintiffId                 INT NOT NULL,
    CreatedDate                 DATETIME2 NOT NULL,
    ModifiedDate                DATETIME2 NULL,
    IsDeleted                   BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_CaseRequestPlaintiff_Request
        FOREIGN KEY (CaseRegistrationRequestId)
        REFERENCES CaseRegistrationRequest(Id),

    CONSTRAINT FK_CaseRequestPlaintiff_Plaintiff
        FOREIGN KEY (PlaintiffId)
        REFERENCES Plaintiff(Id),

    CONSTRAINT UQ_CaseRequestPlaintiff
        UNIQUE (CaseRegistrationRequestId, PlaintiffId)
);

CREATE INDEX IX_CaseRequestPlaintiff_RequestId ON CaseRequestPlaintiff(CaseRegistrationRequestId);
CREATE INDEX IX_CaseRequestPlaintiff_PlaintiffId ON CaseRequestPlaintiff(PlaintiffId);
```

**العلاقة:**
```
CaseRegistrationRequest (N) ←────→ (N) Plaintiff
        طلب قيد دعوى          ↑         المدعي
                              |
                    CaseRequestPlaintiff
                        جدول الربط
```

---

#### جدول ربط الطلب بالمدعى عليه (CaseRequestDefendant)

```csharp
public class CaseRequestDefendant : BaseEntity
{
    public int CaseRegistrationRequestId { get; set; }
    public int DefendantId { get; set; }

    // علاقات التنقل
    public virtual CaseRegistrationRequest Request { get; set; }
    public virtual Defendant Defendant { get; set; }
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE CaseRequestDefendant (
    Id                          INT PRIMARY KEY IDENTITY(1,1),
    CaseRegistrationRequestId   INT NOT NULL,
    DefendantId                 INT NOT NULL,
    CreatedDate                 DATETIME2 NOT NULL,
    ModifiedDate                DATETIME2 NULL,
    IsDeleted                   BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_CaseRequestDefendant_Request
        FOREIGN KEY (CaseRegistrationRequestId)
        REFERENCES CaseRegistrationRequest(Id),

    CONSTRAINT FK_CaseRequestDefendant_Defendant
        FOREIGN KEY (DefendantId)
        REFERENCES Defendant(Id),

    CONSTRAINT UQ_CaseRequestDefendant
        UNIQUE (CaseRegistrationRequestId, DefendantId)
);

CREATE INDEX IX_CaseRequestDefendant_RequestId ON CaseRequestDefendant(CaseRegistrationRequestId);
CREATE INDEX IX_CaseRequestDefendant_DefendantId ON CaseRequestDefendant(DefendantId);
```

**العلاقة:**
```
CaseRegistrationRequest (N) ←────→ (N) Defendant
        طلب قيد دعوى          ↑         المدعى عليه
                              |
                    CaseRequestDefendant
                        جدول الربط
```

---

### 3.5 كيانات الأطراف

#### المدعي (Plaintiff)

```csharp
public class Plaintiff : BaseEntity
{
    public int PlaintiffTypeId { get; set; }           // نوع المدعي

    // البيانات الشخصية (للأفراد)
    public int? IdentityTypeId { get; set; }           // نوع الهوية
    public string IdentityNumber { get; set; }         // رقم الهوية (10 أو 20 رقم)
    public string FirstName { get; set; }              // الاسم الأول
    public string FatherName { get; set; }             // اسم الأب
    public string GrandfatherName { get; set; }        // اسم الجد
    public string FamilyName { get; set; }             // اسم العائلة
    public string ClanName { get; set; }               // اسم القبيلة
    public DateTime? BirthDate { get; set; }           // تاريخ الميلاد
    public string Gender { get; set; }                 // الجنس
    public int? NationalityId { get; set; }            // الجنسية
    public DateTime? IdentityIssueDate { get; set; }   // تاريخ إصدار الهوية
    public DateTime? IdentityExpiryDate { get; set; }  // تاريخ انتهاء الهوية
    public int? DataSourceId { get; set; }             // مصدر البيانات (أبشر/المستخدم)

    // معلومات الاتصال
    public string MobileNumber { get; set; }           // رقم الجوال (10 أرقام، يبدأ 05)
    public string Email { get; set; }                  // البريد الإلكتروني

    // العناوين
    public int? ResidenceAddressId { get; set; }       // عنوان السكن
    public int? WorkAddressId { get; set; }            // عنوان العمل
    public int? SelectedAddressId { get; set; }        // العنوان المختار

    // للمؤسسات والشركات
    public string CommercialRegNumber { get; set; }    // رقم السجل التجاري (10 أرقام)
    public string CompanyName { get; set; }            // اسم الشركة
    public DateTime? CRStartDate { get; set; }         // تاريخ بداية السجل
    public DateTime? CREndDate { get; set; }           // تاريخ انتهاء السجل

    // للجهات الحكومية
    public int? GovernmentAgencyId { get; set; }       // الجهة الحكومية
    public string AdditionalStatement { get; set; }    // بيان إضافي (4000 حرف)

    // للجمعيات/المؤسسات الأهلية
    public string LicenseNumber { get; set; }          // رقم الترخيص
    public string LicenseSource { get; set; }          // مصدر الترخيص
    public DateTime? LicenseDate { get; set; }         // تاريخ الترخيص

    // للوقف
    public string CourtDeedNumber { get; set; }        // رقم صك المحكمة
    public DateTime? DeedDate { get; set; }            // تاريخ الصك
    public string DeedSource { get; set; }             // مصدر الصك
    public string WaqfOversightType { get; set; }      // نوع الرقابة: خاصة/حكومية

    // بيانات العمل (للأفراد)
    public string Employer { get; set; }               // جهة العمل
    public string Profession { get; set; }             // المهنة

    // علاقات التنقل
    public virtual PlaintiffType PlaintiffType { get; set; }
    public virtual DataSource DataSource { get; set; }
    public virtual ICollection<CaseRequestPlaintiff> CaseRequestPlaintiffs { get; set; }  // متعدد لمتعدد
    public virtual ICollection<Representative> Representatives { get; set; }
    public virtual ICollection<PlaintiffAttachment> Attachments { get; set; }
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE Plaintiff (
    Id                      INT PRIMARY KEY IDENTITY(1,1),
    PlaintiffTypeId         INT NOT NULL,

    -- البيانات الشخصية
    IdentityTypeId          INT NULL,
    IdentityNumber          NVARCHAR(20) NULL,
    FirstName               NVARCHAR(100) NULL,
    FatherName              NVARCHAR(100) NULL,
    GrandfatherName         NVARCHAR(100) NULL,
    FamilyName              NVARCHAR(100) NULL,
    ClanName                NVARCHAR(100) NULL,
    BirthDate               DATE NULL,
    Gender                  NVARCHAR(10) NULL,
    NationalityId           INT NULL,
    IdentityIssueDate       DATE NULL,
    IdentityExpiryDate      DATE NULL,
    DataSourceId            INT NULL,

    -- معلومات الاتصال
    MobileNumber            NVARCHAR(10) NULL,
    Email                   NVARCHAR(255) NULL,

    -- العناوين
    ResidenceAddressId      INT NULL,
    WorkAddressId           INT NULL,
    SelectedAddressId       INT NULL,

    -- للمؤسسات والشركات
    CommercialRegNumber     NVARCHAR(10) NULL,
    CompanyName             NVARCHAR(200) NULL,
    CRStartDate             DATE NULL,
    CREndDate               DATE NULL,

    -- للجهات الحكومية
    GovernmentAgencyId      INT NULL,
    AdditionalStatement     NVARCHAR(4000) NULL,

    -- للجمعيات/المؤسسات الأهلية
    LicenseNumber           NVARCHAR(50) NULL,
    LicenseSource           NVARCHAR(200) NULL,
    LicenseDate             DATE NULL,

    -- للوقف
    CourtDeedNumber         NVARCHAR(50) NULL,
    DeedDate                DATE NULL,
    DeedSource              NVARCHAR(200) NULL,
    WaqfOversightType       NVARCHAR(20) NULL,

    -- بيانات العمل
    Employer                NVARCHAR(200) NULL,
    Profession              NVARCHAR(100) NULL,

    -- حقول BaseEntity
    CreatedDate             DATETIME2 NOT NULL,
    ModifiedDate            DATETIME2 NULL,
    IsDeleted               BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Plaintiff_PlaintiffType
        FOREIGN KEY (PlaintiffTypeId)
        REFERENCES PlaintiffType(Id),

    CONSTRAINT FK_Plaintiff_DataSource
        FOREIGN KEY (DataSourceId)
        REFERENCES DataSource(Id)
);

CREATE INDEX IX_Plaintiff_IdentityNumber ON Plaintiff(IdentityNumber);
CREATE INDEX IX_Plaintiff_PlaintiffTypeId ON Plaintiff(PlaintiffTypeId);
CREATE INDEX IX_Plaintiff_DataSourceId ON Plaintiff(DataSourceId);
```

---

#### المدعى عليه (Defendant)

```csharp
public class Defendant : BaseEntity
{
    public int DefendantTypeId { get; set; }           // نوع المدعى عليه
    public string FullName { get; set; }               // الاسم الكامل (مطلوب - 200 حرف)

    // الهوية (اختياري)
    public int? IdentityTypeId { get; set; }
    public string IdentityNumber { get; set; }
    public int? DataSourceId { get; set; }             // مصدر البيانات (أبشر/المستخدم)

    // العنوان (نصي أو مهيكل)
    public string AddressText { get; set; }
    public int? AddressId { get; set; }

    // للشركات
    public string CommercialRegNumber { get; set; }

    // للجهات الحكومية
    public int? GovernmentAgencyId { get; set; }
    public string Headquarters { get; set; }           // المقر الرئيسي
    public string AdditionalStatement { get; set; }

    // علاقات التنقل
    public virtual DefendantType DefendantType { get; set; }
    public virtual DataSource DataSource { get; set; }
    public virtual ICollection<CaseRequestDefendant> CaseRequestDefendants { get; set; }  // متعدد لمتعدد
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE Defendant (
    Id                      INT PRIMARY KEY IDENTITY(1,1),
    DefendantTypeId         INT NOT NULL,
    FullName                NVARCHAR(200) NOT NULL,

    -- الهوية (اختياري)
    IdentityTypeId          INT NULL,
    IdentityNumber          NVARCHAR(20) NULL,
    DataSourceId            INT NULL,

    -- العنوان
    AddressText             NVARCHAR(500) NULL,
    AddressId               INT NULL,

    -- للشركات
    CommercialRegNumber     NVARCHAR(10) NULL,

    -- للجهات الحكومية
    GovernmentAgencyId      INT NULL,
    Headquarters            NVARCHAR(200) NULL,
    AdditionalStatement     NVARCHAR(4000) NULL,

    -- حقول BaseEntity
    CreatedDate             DATETIME2 NOT NULL,
    ModifiedDate            DATETIME2 NULL,
    IsDeleted               BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Defendant_DefendantType
        FOREIGN KEY (DefendantTypeId)
        REFERENCES DefendantType(Id),

    CONSTRAINT FK_Defendant_DataSource
        FOREIGN KEY (DataSourceId)
        REFERENCES DataSource(Id)
);

CREATE INDEX IX_Defendant_IdentityNumber ON Defendant(IdentityNumber);
CREATE INDEX IX_Defendant_DefendantTypeId ON Defendant(DefendantTypeId);
CREATE INDEX IX_Defendant_DataSourceId ON Defendant(DataSourceId);
```

---

### 3.6 الكيانات الفرعية

#### الممثل (Representative)

```csharp
public class Representative : BaseEntity
{
    public int PlaintiffId { get; set; }
    public int RepresentativeTypeId { get; set; }      // نوع الممثل

    // البيانات الشخصية
    public int IdentityTypeId { get; set; }
    public string IdentityNumber { get; set; }
    public string FirstName { get; set; }
    public string FatherName { get; set; }
    public string GrandfatherName { get; set; }
    public string FamilyName { get; set; }
    public string ClanName { get; set; }               // اسم القبيلة
    public DateTime? BirthDate { get; set; }
    public int? DataSourceId { get; set; }             // مصدر البيانات (أبشر/المستخدم)

    // معلومات الاتصال
    public string MobileNumber { get; set; }
    public string Email { get; set; }

    // وثيقة التفويض
    public string AuthorizationNumber { get; set; }    // رقم الوكالة/القرار/الصك
    public DateTime? AuthorizationDate { get; set; }   // تاريخ الوكالة
    public string AuthorizationSource { get; set; }    // مصدر الوكالة
    public string AuthorizationSourceType { get; set; } // نوع المصدر: كتابة عدل/محكمة/وزارة خارجية

    // للولي
    public string GuardianshipType { get; set; }       // نوع الولاية: طبيعية/مكتسبة

    // علاقات التنقل
    public virtual Plaintiff Plaintiff { get; set; }
    public virtual RepresentativeType RepresentativeType { get; set; }
    public virtual DataSource DataSource { get; set; }
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE Representative (
    Id                          INT PRIMARY KEY IDENTITY(1,1),
    PlaintiffId                 INT NOT NULL,
    RepresentativeTypeId        INT NOT NULL,

    -- البيانات الشخصية
    IdentityTypeId              INT NOT NULL,
    IdentityNumber              NVARCHAR(20) NOT NULL,
    FirstName                   NVARCHAR(100) NOT NULL,
    FatherName                  NVARCHAR(100) NULL,
    GrandfatherName             NVARCHAR(100) NULL,
    FamilyName                  NVARCHAR(100) NOT NULL,
    ClanName                    NVARCHAR(100) NULL,
    BirthDate                   DATE NULL,
    DataSourceId                INT NULL,

    -- معلومات الاتصال
    MobileNumber                NVARCHAR(10) NOT NULL,
    Email                       NVARCHAR(255) NULL,

    -- وثيقة التفويض
    AuthorizationNumber         NVARCHAR(50) NULL,
    AuthorizationDate           DATE NULL,
    AuthorizationSource         NVARCHAR(200) NULL,
    AuthorizationSourceType     NVARCHAR(50) NULL,

    -- للولي
    GuardianshipType            NVARCHAR(20) NULL,

    -- حقول BaseEntity
    CreatedDate                 DATETIME2 NOT NULL,
    ModifiedDate                DATETIME2 NULL,
    IsDeleted                   BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Representative_Plaintiff
        FOREIGN KEY (PlaintiffId)
        REFERENCES Plaintiff(Id),

    CONSTRAINT FK_Representative_RepresentativeType
        FOREIGN KEY (RepresentativeTypeId)
        REFERENCES RepresentativeType(Id),

    CONSTRAINT FK_Representative_DataSource
        FOREIGN KEY (DataSourceId)
        REFERENCES DataSource(Id)
);

CREATE INDEX IX_Representative_PlaintiffId ON Representative(PlaintiffId);
CREATE INDEX IX_Representative_IdentityNumber ON Representative(IdentityNumber);
CREATE INDEX IX_Representative_DataSourceId ON Representative(DataSourceId);
```

---

#### مرفقات المدعي (PlaintiffAttachment)

```csharp
public class PlaintiffAttachment : BaseEntity
{
    public int PlaintiffId { get; set; }
    public int AttachmentTypeId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }

    // علاقات التنقل
    public virtual Plaintiff Plaintiff { get; set; }
    public virtual AttachmentType AttachmentType { get; set; }
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE PlaintiffAttachment (
    Id                  INT PRIMARY KEY IDENTITY(1,1),
    PlaintiffId         INT NOT NULL,
    AttachmentTypeId    INT NOT NULL,
    FileName            NVARCHAR(255) NOT NULL,
    FilePath            NVARCHAR(500) NOT NULL,
    ContentType         NVARCHAR(100) NOT NULL,
    FileSize            BIGINT NOT NULL,

    -- حقول BaseEntity
    CreatedDate         DATETIME2 NOT NULL,
    ModifiedDate        DATETIME2 NULL,
    IsDeleted           BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_PlaintiffAttachment_Plaintiff
        FOREIGN KEY (PlaintiffId)
        REFERENCES Plaintiff(Id),

    CONSTRAINT FK_PlaintiffAttachment_AttachmentType
        FOREIGN KEY (AttachmentTypeId)
        REFERENCES AttachmentType(Id),

    CONSTRAINT CHK_PlaintiffAttachment_FileSize
        CHECK (FileSize <= 4194304)  -- 4MB max
);

CREATE INDEX IX_PlaintiffAttachment_PlaintiffId ON PlaintiffAttachment(PlaintiffId);
```

---

#### مرفقات الطلب (RequestAttachment)

```csharp
public class RequestAttachment : BaseEntity
{
    public int CaseRegistrationRequestId { get; set; }
    public int AttachmentTypeId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }

    // علاقات التنقل
    public virtual CaseRegistrationRequest Request { get; set; }
    public virtual AttachmentType AttachmentType { get; set; }
}
```

**مخطط قاعدة البيانات:**

```sql
CREATE TABLE RequestAttachment (
    Id                          INT PRIMARY KEY IDENTITY(1,1),
    CaseRegistrationRequestId   INT NOT NULL,
    AttachmentTypeId            INT NOT NULL,
    FileName                    NVARCHAR(255) NOT NULL,
    FilePath                    NVARCHAR(500) NOT NULL,
    ContentType                 NVARCHAR(100) NOT NULL,
    FileSize                    BIGINT NOT NULL,

    -- حقول BaseEntity
    CreatedDate                 DATETIME2 NOT NULL,
    ModifiedDate                DATETIME2 NULL,
    IsDeleted                   BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_RequestAttachment_Request
        FOREIGN KEY (CaseRegistrationRequestId)
        REFERENCES CaseRegistrationRequest(Id),

    CONSTRAINT FK_RequestAttachment_AttachmentType
        FOREIGN KEY (AttachmentTypeId)
        REFERENCES AttachmentType(Id),

    CONSTRAINT CHK_RequestAttachment_FileSize
        CHECK (FileSize <= 4194304)  -- 4MB max
);

CREATE INDEX IX_RequestAttachment_RequestId ON RequestAttachment(CaseRegistrationRequestId);
```

---

### 3.7 جداول البحث (Lookup Tables)

#### نوع المدعي (PlaintiffType)

```csharp
public class PlaintiffType : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<Plaintiff> Plaintiffs { get; set; }
}
```

```sql
CREATE TABLE PlaintiffType (
    Id          INT PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(100) NOT NULL,
    NameAr      NVARCHAR(100) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    ModifiedDate DATETIME2 NULL,
    IsDeleted   BIT NOT NULL DEFAULT 0
);
```

---

#### نوع المدعى عليه (DefendantType)

```csharp
public class DefendantType : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<Defendant> Defendants { get; set; }
}
```

```sql
CREATE TABLE DefendantType (
    Id          INT PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(100) NOT NULL,
    NameAr      NVARCHAR(100) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    ModifiedDate DATETIME2 NULL,
    IsDeleted   BIT NOT NULL DEFAULT 0
);
```

---

#### نوع الممثل (RepresentativeType)

```csharp
public class RepresentativeType : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<Representative> Representatives { get; set; }
}
```

```sql
CREATE TABLE RepresentativeType (
    Id          INT PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(100) NOT NULL,
    NameAr      NVARCHAR(100) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    ModifiedDate DATETIME2 NULL,
    IsDeleted   BIT NOT NULL DEFAULT 0
);
```

---

#### حالة الطلب (RequestStatus)

```csharp
public class RequestStatus : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<CaseRegistrationRequest> Requests { get; set; }
}
```

```sql
CREATE TABLE RequestStatus (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(100) NOT NULL,
    NameAr          NVARCHAR(100) NOT NULL,
    DisplayOrder    INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedDate     DATETIME2 NOT NULL,
    ModifiedDate    DATETIME2 NULL,
    IsDeleted       BIT NOT NULL DEFAULT 0
);
```

---

#### نوع الهوية (IdentityType)

```csharp
public class IdentityType : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public int IdentityLength { get; set; }  // طول رقم الهوية
    public bool IsActive { get; set; }
}
```

```sql
CREATE TABLE IdentityType (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(100) NOT NULL,
    NameAr          NVARCHAR(100) NOT NULL,
    IdentityLength  INT NOT NULL DEFAULT 10,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedDate     DATETIME2 NOT NULL,
    ModifiedDate    DATETIME2 NULL,
    IsDeleted       BIT NOT NULL DEFAULT 0
);

-- بيانات البذر
INSERT INTO IdentityType (Id, Name, NameAr, IdentityLength, IsActive, CreatedDate) VALUES
(1, 'National ID', 'هوية وطنية', 10, 1, GETDATE()),
(2, 'Resident ID', 'هوية مقيم', 10, 1, GETDATE()),
(3, 'Passport', 'جواز سفر', 20, 1, GETDATE());
```

---

#### نوع المرفق (AttachmentType)

```csharp
public class AttachmentType : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public bool IsRequired { get; set; }          // إجباري
    public string ApplicableFor { get; set; }     // يُطبق على: Plaintiff/Request/Both
    public bool IsActive { get; set; }
}
```

```sql
CREATE TABLE AttachmentType (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(100) NOT NULL,
    NameAr          NVARCHAR(100) NOT NULL,
    IsRequired      BIT NOT NULL DEFAULT 0,
    ApplicableFor   NVARCHAR(20) NOT NULL DEFAULT 'Both',
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedDate     DATETIME2 NOT NULL,
    ModifiedDate    DATETIME2 NULL,
    IsDeleted       BIT NOT NULL DEFAULT 0
);
```

---

#### مصدر البيانات (DataSource)

```csharp
public class DataSource : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public bool IsActive { get; set; }
}
```

```sql
CREATE TABLE DataSource (
    Id          INT PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(100) NOT NULL,
    NameAr      NVARCHAR(100) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    ModifiedDate DATETIME2 NULL,
    IsDeleted   BIT NOT NULL DEFAULT 0
);

-- بيانات البذر
INSERT INTO DataSource (Id, Name, NameAr, IsActive, CreatedDate) VALUES
(1, 'FromAbsher', 'من أبشر', 1, GETDATE()),
(2, 'FromUser', 'من المستخدم', 1, GETDATE());
```

---

#### المنطقة (Region)

```csharp
public class Region : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public string Code { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<City> Cities { get; set; }
}
```

```sql
CREATE TABLE Region (
    Id          INT PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(100) NOT NULL,
    NameAr      NVARCHAR(100) NOT NULL,
    Code        NVARCHAR(10) NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    ModifiedDate DATETIME2 NULL,
    IsDeleted   BIT NOT NULL DEFAULT 0
);
```

---

#### المدينة (City)

```csharp
public class City : BaseEntity
{
    public int RegionId { get; set; }
    public string Name { get; set; }
    public string NameAr { get; set; }
    public bool IsActive { get; set; }

    public virtual Region Region { get; set; }
}
```

```sql
CREATE TABLE City (
    Id          INT PRIMARY KEY IDENTITY(1,1),
    RegionId    INT NOT NULL,
    Name        NVARCHAR(100) NOT NULL,
    NameAr      NVARCHAR(100) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    ModifiedDate DATETIME2 NULL,
    IsDeleted   BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_City_Region
        FOREIGN KEY (RegionId)
        REFERENCES Region(Id)
);

CREATE INDEX IX_City_RegionId ON City(RegionId);
```

---

#### الجهة الحكومية (GovernmentAgency)

```csharp
public class GovernmentAgency : BaseEntity
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public string Code { get; set; }
    public bool IsActive { get; set; }
}
```

```sql
CREATE TABLE GovernmentAgency (
    Id          INT PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(200) NOT NULL,
    NameAr      NVARCHAR(200) NOT NULL,
    Code        NVARCHAR(20) NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    ModifiedDate DATETIME2 NULL,
    IsDeleted   BIT NOT NULL DEFAULT 0
);
```

---

### 3.8 الكيانات المشتركة (Common Entities)

#### العنوان الوطني (Address)

```csharp
public class Address : BaseEntity
{
    public string BuildingNumber { get; set; }      // رقم المبنى (4 أرقام)
    public string StreetName { get; set; }          // اسم الشارع
    public string District { get; set; }            // الحي
    public int CityId { get; set; }                 // المدينة
    public string PostalCode { get; set; }          // الرمز البريدي (5 أرقام)
    public string AdditionalNumber { get; set; }    // الرقم الإضافي (4 أرقام)
    public string UnitNumber { get; set; }          // رقم الوحدة

    public virtual City City { get; set; }
}
```

```sql
CREATE TABLE Address (
    Id                  INT PRIMARY KEY IDENTITY(1,1),
    BuildingNumber      NVARCHAR(4) NOT NULL,
    StreetName          NVARCHAR(200) NOT NULL,
    District            NVARCHAR(100) NOT NULL,
    CityId              INT NOT NULL,
    PostalCode          NVARCHAR(5) NOT NULL,
    AdditionalNumber    NVARCHAR(4) NOT NULL,
    UnitNumber          NVARCHAR(10) NULL,

    -- حقول BaseEntity
    CreatedDate         DATETIME2 NOT NULL,
    ModifiedDate        DATETIME2 NULL,
    IsDeleted           BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Address_City
        FOREIGN KEY (CityId)
        REFERENCES City(Id)
);

CREATE INDEX IX_Address_CityId ON Address(CityId);
CREATE INDEX IX_Address_PostalCode ON Address(PostalCode);
```

---

### 3.9 أوامر الهجرة

```bash
# إضافة الهجرة
dotnet ef migrations add AddCaseRegistrationEntities \
    --project src/Backend/BOG.DbModel \
    --startup-project src/Backend/BOG.API

# تحديث قاعدة البيانات
dotnet ef database update \
    --project src/Backend/BOG.DbModel \
    --startup-project src/Backend/BOG.API
```

---

## 4. المرحلة 2: توزيع المهام على المطورين

### 4.1 المطور أ: وحدة المدعين والممثلين

**النطاق:** حالات الاستخدام 6.5.1.1.1 - 6.5.1.1.10 (10 حالات)
**التركيز:** إدارة المدعين، الممثلين، المرفقات، العنوان المختار

#### حالات الاستخدام

| الرمز | الاسم | الوصف |
|-------|-------|-------|
| 6.5.1.1.1 | المدعين | قائمة المدعين |
| 6.5.1.1.2 | إضافة مدعي | إضافة مدعي جديد |
| 6.5.1.1.3 | استعراض تفاصيل مدّعي | عرض تفاصيل المدعي |
| 6.5.1.1.4 | تعديل مدّعي | تعديل بيانات المدعي |
| 6.5.1.1.5 | مرفقات مدّعي | إدارة المرفقات |
| 6.5.1.1.6 | العنوان المختار | تحديد العنوان |
| 6.5.1.1.7 | إضافة ممثل | إضافة ممثل للمدعي |
| 6.5.1.1.8 | تفاصيل ممثّل | عرض تفاصيل الممثل |
| 6.5.1.1.9 | تعديل ممثّل | تعديل بيانات الممثل |
| 6.5.1.1.10 | مقدّم الطلب | تعيين كمقدم طلب |

#### نقاط النهاية (API Endpoints)

```
GET    /api/case-requests/{requestId}/plaintiffs      # قائمة المدعين
POST   /api/case-requests/{requestId}/plaintiffs      # إضافة مدعي
GET    /api/plaintiffs/{id}                           # تفاصيل مدعي
PUT    /api/plaintiffs/{id}                           # تعديل مدعي
DELETE /api/plaintiffs/{id}                           # حذف مدعي

GET    /api/plaintiffs/{id}/attachments               # قائمة المرفقات
POST   /api/plaintiffs/{id}/attachments               # إضافة مرفق
DELETE /api/plaintiffs/{id}/attachments/{attachmentId} # حذف مرفق

GET    /api/plaintiffs/{id}/selected-address          # العنوان المختار
PUT    /api/plaintiffs/{id}/selected-address          # تحديث العنوان

GET    /api/plaintiffs/{plaintiffId}/representatives  # قائمة الممثلين
POST   /api/plaintiffs/{plaintiffId}/representatives  # إضافة ممثل
GET    /api/representatives/{id}                      # تفاصيل ممثل
PUT    /api/representatives/{id}                      # تعديل ممثل
DELETE /api/representatives/{id}                      # حذف ممثل
```

#### قواعد الأعمال المطلوبة

| الرمز | القاعدة |
|-------|---------|
| ERR008 | الممثّل موجود مسبقاً للمدعي |
| ERR011 | المدّعي موجود مسبقاً بنفس رقم الهوية |
| ERR012 | رقم هوية الممثّل نفس رقم هوية المدّعي |
| BC01 | عرض النموذج حسب نوع المدعي |
| BC02 | التكامل مع أبشر للأفراد |
| BC03 | العنوان المختار عند اختلاف المدينة عن المحكمة |

---

### 4.2 المطور ب: وحدة المدعى عليهم وإدارة الطلبات

**النطاق:** حالات الاستخدام 6.5.1.1.11 - 6.5.1.1.24 (14 حالة)
**التركيز:** إدارة المدعى عليهم، دورة حياة الطلب، الإجراءات، البحث

#### حالات الاستخدام

| الرمز | الاسم | الوصف |
|-------|-------|-------|
| 6.5.1.1.11 | المدعى عليهم | قائمة المدعى عليهم |
| 6.5.1.1.12 | إضافة مدّعى عليه | إضافة مدعى عليه جديد |
| 6.5.1.1.13 | تفاصيل مدّعى عليه | عرض التفاصيل |
| 6.5.1.1.14 | تعديل مدّعى عليه | تعديل البيانات |
| 6.5.1.1.15 | معلومات إضافية | إضافة معلومات |
| 6.5.1.1.16 | استعراض معلومات | عرض المعلومات الإضافية |
| 6.5.1.1.17 | مرفقات الطلب | إدارة مرفقات الطلب |
| 6.5.1.1.18 | استكمال نواقص | إضافة النواقص |
| 6.5.1.1.19 | استعراض نواقص | عرض النواقص |
| 6.5.1.1.20 | البحث | البحث في الطلبات |
| 6.5.1.1.21 | تعديل الطلب | تعديل بيانات الطلب |
| 6.5.1.1.22 | تفاصيل الطلب | عرض تفاصيل الطلب |
| 6.5.1.1.23 | اتخاذ إجراء | اتخاذ إجراء على الطلب |
| 6.5.1.1.24 | قيد دعوى | قيد الدعوى في النظام |

#### نقاط النهاية (API Endpoints)

```
GET    /api/case-requests/{requestId}/defendants      # قائمة المدعى عليهم
POST   /api/case-requests/{requestId}/defendants      # إضافة مدعى عليه
GET    /api/defendants/{id}                           # تفاصيل مدعى عليه
PUT    /api/defendants/{id}                           # تعديل مدعى عليه
DELETE /api/defendants/{id}                           # حذف مدعى عليه

POST   /api/case-requests                             # إنشاء طلب (مسودة)
GET    /api/case-requests/{id}                        # تفاصيل طلب
PUT    /api/case-requests/{id}                        # تعديل طلب
POST   /api/case-requests/{id}/submit                 # تقديم طلب
GET    /api/case-requests/search                      # البحث

POST   /api/case-requests/{id}/action                 # اتخاذ إجراء
GET    /api/case-requests/{id}/attachments            # قائمة المرفقات
POST   /api/case-requests/{id}/attachments            # إضافة مرفق
DELETE /api/case-requests/{id}/attachments/{attachmentId} # حذف مرفق

GET    /api/case-requests/{id}/claims                 # قائمة الطلبات
POST   /api/case-requests/{id}/claims                 # إضافة طلب
PUT    /api/claims/{claimId}                          # تعديل طلب
DELETE /api/claims/{claimId}                          # حذف طلب

GET    /api/case-requests/{id}/related-cases          # قائمة الدعاوى المرتبطة
POST   /api/case-requests/{id}/related-cases          # إضافة دعوى مرتبطة
DELETE /api/related-cases/{id}                        # حذف دعوى مرتبطة

GET    /api/case-requests/{id}/classifications        # قائمة التصنيفات
POST   /api/case-requests/{id}/classifications        # إضافة تصنيف
PUT    /api/classifications/{id}                      # تعديل تصنيف
DELETE /api/classifications/{id}                      # حذف تصنيف
```

#### قواعد الأعمال المطلوبة

| الرمز | القاعدة |
|-------|---------|
| ERR001 | يجب إضافة مدّعي واحد على الأقل |
| ERR002 | يجب إضافة مدّعى عليه واحد على الأقل |
| ERR003 | المرفقات الإجبارية غير مكتملة |
| ERR013 | المدّعى عليه موجود مسبقاً |
| BR04 | PDF فقط، الحد الأقصى 4 ميجابايت |
| BR05 | الرفض التلقائي بعد 30 يوم |

#### انتقالات الحالة

| من | الإجراء | إلى |
|----|---------|-----|
| لا يوجد | حفظ | مسودة |
| مسودة | تقديم | طلب جديد |
| طلب جديد | قيد | دعوى مقيدة |
| طلب جديد | إرسال للقاضي | معروض على مكتب القاضي |
| طلب جديد | طلب استكمال | استكمال النواقص |
| طلب جديد | رفض | مرفوضة |
| استكمال النواقص | استكمال | التحقق من الاستكمال |
| استكمال النواقص | انتهاء المهلة (30 يوم) | مرفوضة |

---

## 5. المرحلة 3: الدمج والاختبار

### 5.1 مهام الدمج

1. **خدمة التحقق من انتقال الحالة**
   - تنفيذ `IStatusTransitionValidator`
   - فرض الانتقالات الصحيحة فقط

2. **تكامل أبشر**
   - إنشاء `IAbsherService` في BOG.Integration
   - تنفيذ المحاكاة للتطوير

3. **خدمة الإشعارات**
   - إنشاء `INotificationService`
   - إرسال رسائل SMS/بريد إلكتروني عند تغيير الحالة

4. **مهمة الخلفية للرفض التلقائي**
   - تنفيذ فحص مهلة 30 يوم
   - رفض تلقائي للطلبات المعلقة

### 5.2 ملخص رسائل التحقق

| الرمز | الرسالة |
|-------|---------|
| ERR001 | يجب إضافة مدّعي واحد على الأقل |
| ERR002 | يجب إضافة مدّعى عليه واحد على الأقل |
| ERR003 | المرفقات الإجبارية غير مكتملة |
| ERR008 | الممثّل موجود مسبقاً |
| ERR011 | المدّعي موجود مسبقاً |
| ERR012 | رقم هوية الممثّل نفس رقم هوية المدّعي |
| ERR013 | المدّعى عليه موجود مسبقاً |

---

## 6. المرحلة 4: خدمات التكامل

جميع خدمات التكامل الخارجية ستُنفذ بواجهات وتنفيذات محاكاة للتطوير/الاختبار.

### 6.1 تكامل أبشر (التحقق من الهوية)

**الغرض:** التحقق من هوية المواطن/المقيم وجلب البيانات الشخصية من أبشر (مركز المعلومات الوطني).

#### تعريف الواجهة

```csharp
// BOG.Integration/Interfaces/IAbsherService.cs
public interface IAbsherService
{
    Task<AbsherVerificationResult> VerifyIdentityAsync(string identityNumber, int identityType);
    Task<AbsherPersonData?> GetPersonDataAsync(string identityNumber, int identityType);
    Task<AbsherAddressData?> GetNationalAddressAsync(string identityNumber);
}
```

#### كائنات نقل البيانات (DTOs)

```csharp
// نتيجة التحقق
public class AbsherVerificationResult
{
    public bool IsVerified { get; set; }           // تم التحقق
    public string? ErrorCode { get; set; }         // رمز الخطأ
    public string? ErrorMessage { get; set; }      // رسالة الخطأ (إنجليزي)
    public string? ErrorMessageAr { get; set; }    // رسالة الخطأ (عربي)
}

// بيانات الشخص
public class AbsherPersonData
{
    public string FirstName { get; set; }           // الاسم الأول
    public string FatherName { get; set; }          // اسم الأب
    public string GrandfatherName { get; set; }     // اسم الجد
    public string FamilyName { get; set; }          // اسم العائلة
    public string FirstNameEn { get; set; }         // الاسم الأول (إنجليزي)
    public string FatherNameEn { get; set; }
    public string GrandfatherNameEn { get; set; }
    public string FamilyNameEn { get; set; }
    public DateTime BirthDate { get; set; }         // تاريخ الميلاد
    public string Gender { get; set; }              // الجنس: ذكر/أنثى
    public int NationalityId { get; set; }          // الجنسية
    public DateTime? IdentityIssueDate { get; set; } // تاريخ إصدار الهوية
    public DateTime? IdentityExpiryDate { get; set; } // تاريخ انتهاء الهوية
    public string MobileNumber { get; set; }        // رقم الجوال
    public string Email { get; set; }               // البريد الإلكتروني
}

// بيانات العنوان
public class AbsherAddressData
{
    // عنوان السكن
    public string ResidenceBuildingNumber { get; set; }    // رقم المبنى
    public string ResidenceStreetName { get; set; }        // اسم الشارع
    public string ResidenceDistrict { get; set; }          // الحي
    public string ResidenceCity { get; set; }              // المدينة
    public string ResidencePostalCode { get; set; }        // الرمز البريدي
    public string ResidenceAdditionalNumber { get; set; }  // الرقم الإضافي

    // عنوان العمل
    public string WorkBuildingNumber { get; set; }
    public string WorkStreetName { get; set; }
    public string WorkDistrict { get; set; }
    public string WorkCity { get; set; }
    public string WorkPostalCode { get; set; }
    public string WorkAdditionalNumber { get; set; }
}
```

#### بيانات المحاكاة

```csharp
// مواطن سعودي - ذكر
["1000000001"] = new AbsherPersonData
{
    FirstName = "محمد",
    FatherName = "أحمد",
    GrandfatherName = "عبدالله",
    FamilyName = "السعيد",
    BirthDate = new DateTime(1985, 5, 15),
    Gender = "ذكر",
    MobileNumber = "0501234567",
    Email = "mohammed@example.com"
}

// مواطنة سعودية - أنثى
["2000000001"] = new AbsherPersonData
{
    FirstName = "فاطمة",
    FatherName = "خالد",
    GrandfatherName = "سعد",
    FamilyName = "المطيري",
    BirthDate = new DateTime(1990, 8, 20),
    Gender = "أنثى",
    MobileNumber = "0559876543",
    Email = "fatima@example.com"
}
```

---

### 6.2 خدمة الرسائل القصيرة (SMS)

**الغرض:** إرسال إشعارات SMS عند تغيير الحالة والأحداث المهمة.

#### تعريف الواجهة

```csharp
// BOG.Integration/Interfaces/ISmsService.cs
public interface ISmsService
{
    Task<SmsResult> SendSmsAsync(string mobileNumber, string message);
    Task<SmsResult> SendSmsAsync(string mobileNumber, string templateCode, Dictionary<string, string> parameters);
    Task<List<SmsResult>> SendBulkSmsAsync(List<string> mobileNumbers, string message);
}
```

#### قوالب الرسائل

| رمز القالب | الرسالة |
|------------|---------|
| REQUEST_SUBMITTED | تم استلام طلب قيد الدعوى رقم {RequestNumber} بنجاح |
| REQUEST_REGISTERED | تم قيد دعواكم رقم {CaseNumber} في المحكمة الإدارية |
| REQUEST_REJECTED | تم رفض طلب قيد الدعوى رقم {RequestNumber}. السبب: {Reason} |
| COMPLETION_REQUIRED | مطلوب استكمال نواقص الطلب رقم {RequestNumber} خلال 30 يوم |
| DEFICIENCY_REMINDER | تذكير: يرجى استكمال نواقص الطلب رقم {RequestNumber}. متبقي {DaysRemaining} أيام |
| AUTO_REJECTED | تم رفض الطلب رقم {RequestNumber} تلقائياً لعدم استكمال النواقص |

---

### 6.3 خدمة البريد الإلكتروني

**الغرض:** إرسال إشعارات البريد الإلكتروني للتحديثات والمراسلات الرسمية.

#### تعريف الواجهة

```csharp
// BOG.Integration/Interfaces/IEmailService.cs
public interface IEmailService
{
    Task<EmailResult> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    Task<EmailResult> SendEmailAsync(string toEmail, string templateCode, Dictionary<string, string> parameters);
    Task<EmailResult> SendEmailWithAttachmentAsync(string toEmail, string subject, string body,
        List<EmailAttachment> attachments);
}
```

#### قوالب البريد الإلكتروني

**قالب استلام الطلب:**
```html
<div dir='rtl'>
    <h2>تم استلام طلبكم بنجاح</h2>
    <p>رقم الطلب: <strong>{RequestNumber}</strong></p>
    <p>تاريخ التقديم: {SubmissionDate}</p>
    <p>سيتم مراجعة طلبكم من قبل المختصين.</p>
</div>
```

**قالب قيد الدعوى:**
```html
<div dir='rtl'>
    <h2>تم قيد دعواكم</h2>
    <p>رقم القضية: <strong>{CaseNumber}</strong></p>
    <p>المحكمة: {CourtName}</p>
    <p>الدائرة: {DepartmentName}</p>
</div>
```

**قالب طلب الاستكمال:**
```html
<div dir='rtl'>
    <h2>مطلوب استكمال النواقص التالية</h2>
    <p>رقم الطلب: {RequestNumber}</p>
    <p>النواقص:</p>
    <ul>{DeficienciesList}</ul>
    <p>المهلة: 30 يوم من تاريخ الإشعار</p>
</div>
```

---

### 6.4 تكامل نظام إدارة القضايا

**الغرض:** قيد الدعاوى المعتمدة في نظام إدارة القضايا الأساسي (الإصدار 3/4).

#### تعريف الواجهة

```csharp
// BOG.Integration/Interfaces/ICaseManagementService.cs
public interface ICaseManagementService
{
    Task<CaseRegistrationResult> RegisterCaseAsync(CaseRegistrationData caseData);
    Task<CaseStatusResult> GetCaseStatusAsync(string caseNumber);
    Task<bool> UpdateCaseAsync(string caseNumber, CaseUpdateData updateData);
}
```

#### كائنات نقل البيانات

```csharp
// بيانات قيد الدعوى
public class CaseRegistrationData
{
    public int RequestId { get; set; }              // معرف الطلب
    public int CourtId { get; set; }                // المحكمة
    public string Subject { get; set; }             // الموضوع
    public List<string> Claims { get; set; }        // الطلبات (قائمة نصوص)
    public List<CasePlaintiffData> Plaintiffs { get; set; }   // المدعون
    public List<CaseDefendantData> Defendants { get; set; }   // المدعى عليهم
}

// نتيجة قيد الدعوى
public class CaseRegistrationResult
{
    public bool IsSuccess { get; set; }             // نجح القيد
    public string? CaseNumber { get; set; }         // رقم القضية
    public string? RegistrationNumber { get; set; } // رقم القيد
    public DateTime? RegistrationDate { get; set; } // تاريخ القيد
    public string? ErrorCode { get; set; }          // رمز الخطأ
    public string? ErrorMessage { get; set; }       // رسالة الخطأ
}
```

---

### 6.5 إعدادات التكامل

#### ملف الإعدادات (appsettings.json)

```json
{
  "Integration": {
    "Absher": {
      "UseMock": true,
      "BaseUrl": "https://absher.api.example.sa",
      "ApiKey": "",
      "TimeoutSeconds": 30
    },
    "Sms": {
      "UseMock": true,
      "Provider": "Unifonic",
      "ApiKey": "",
      "SenderId": "BOG"
    },
    "Email": {
      "UseMock": true,
      "SmtpHost": "smtp.example.sa",
      "SmtpPort": 587,
      "FromEmail": "noreply@bog.gov.sa",
      "FromName": "ديوان المظالم"
    },
    "CaseManagement": {
      "UseMock": true,
      "BaseUrl": "https://cms.bog.gov.sa/api",
      "Version": "v4"
    }
  }
}
```

#### تسجيل الخدمات (DI)

```csharp
// BOG.Integration/Extensions/IntegrationServiceExtensions.cs
public static IServiceCollection AddIntegrationServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var settings = configuration.GetSection("Integration").Get<IntegrationSettings>();

    // أبشر
    if (settings.Absher.UseMock)
        services.AddScoped<IAbsherService, AbsherMockService>();
    else
        services.AddScoped<IAbsherService, AbsherService>();

    // الرسائل القصيرة
    if (settings.Sms.UseMock)
        services.AddScoped<ISmsService, SmsMockService>();
    else
        services.AddScoped<ISmsService, SmsService>();

    // البريد الإلكتروني
    if (settings.Email.UseMock)
        services.AddScoped<IEmailService, EmailMockService>();
    else
        services.AddScoped<IEmailService, EmailService>();

    // نظام إدارة القضايا
    if (settings.CaseManagement.UseMock)
        services.AddScoped<ICaseManagementService, CaseManagementMockService>();
    else
        services.AddScoped<ICaseManagementService, CaseManagementService>();

    return services;
}
```

---

### 6.6 هيكل ملفات التكامل

```
BOG.Integration/
├── Configuration/
│   └── IntegrationSettings.cs          # إعدادات التكامل
├── Interfaces/
│   ├── IAbsherService.cs               # واجهة أبشر
│   ├── ISmsService.cs                  # واجهة الرسائل القصيرة
│   ├── IEmailService.cs                # واجهة البريد الإلكتروني
│   └── ICaseManagementService.cs       # واجهة نظام القضايا
├── Services/
│   ├── AbsherMockService.cs            # محاكاة أبشر
│   ├── AbsherService.cs                # التنفيذ الحقيقي
│   ├── SmsMockService.cs               # محاكاة الرسائل
│   ├── SmsService.cs                   # التنفيذ الحقيقي
│   ├── EmailMockService.cs             # محاكاة البريد
│   ├── EmailService.cs                 # التنفيذ الحقيقي
│   ├── CaseManagementMockService.cs    # محاكاة نظام القضايا
│   └── CaseManagementService.cs        # التنفيذ الحقيقي
├── DTOs/
│   ├── Absher/
│   │   ├── AbsherVerificationResult.cs
│   │   ├── AbsherPersonData.cs
│   │   └── AbsherAddressData.cs
│   ├── Sms/
│   │   ├── SmsResult.cs
│   │   └── SmsTemplate.cs
│   ├── Email/
│   │   ├── EmailResult.cs
│   │   ├── EmailAttachment.cs
│   │   └── EmailTemplate.cs
│   └── CaseManagement/
│       ├── CaseRegistrationData.cs
│       ├── CaseRegistrationResult.cs
│       ├── CasePlaintiffData.cs
│       ├── CaseDefendantData.cs
│       ├── CaseStatusResult.cs
│       └── CaseUpdateData.cs
└── Extensions/
    └── IntegrationServiceExtensions.cs  # تسجيل الخدمات
```

---

## 7. ملخص الملفات

### المرحلة 0 (الأدوار والصلاحيات)
- 5 كيانات للهوية
- 2 مستودعات
- 2 خدمات منطق الأعمال
- 2 ملفات للصلاحيات
- 1 متحكم API

### المرحلة 1 (قاعدة البيانات)
- 10 كيانات للجداول المرجعية (Lookup Tables)
- 1 كيان مشترك (Address)
- 11 كيان لقيد الدعوى:
  - CaseRegistrationRequest (الطلب الرئيسي)
  - Claim (الطلبات)
  - RelatedCase (الدعاوى المرتبطة)
  - RequestClassification (تصنيف الدعوى)
  - Plaintiff (المدعي)
  - CaseRequestPlaintiff (جدول ربط - متعدد لمتعدد)
  - Defendant (المدعى عليه)
  - CaseRequestDefendant (جدول ربط - متعدد لمتعدد)
  - Representative (الممثل)
  - PlaintiffAttachment (مرفقات المدعي)
  - RequestAttachment (مرفقات الطلب)
- 1 تحديث DbContext
- 1 هجرة

### المطور أ (المدعين والممثلين)
- 5 واجهات مستودعات + تنفيذات:
  - IPlaintiffRepository
  - ICaseRequestPlaintiffRepository (جدول الربط)
  - IRepresentativeRepository
  - IPlaintiffAttachmentRepository
  - IAddressRepository
- 8 كائنات نقل بيانات (DTO)
- 6 نماذج عرض (ViewModel)
- 4 خدمات منطق أعمال
- 2 متحكم API

### المطور ب (المدعى عليهم والطلبات)
- 8 واجهات مستودعات + تنفيذات:
  - IDefendantRepository
  - ICaseRegistrationRequestRepository
  - ICaseRequestDefendantRepository (جدول الربط)
  - IClaimRepository
  - IRelatedCaseRepository
  - IRequestClassificationRepository
  - IRequestAttachmentRepository
  - + تنفيذات
- 12 كائن نقل بيانات (DTO)
- 9 نماذج عرض (ViewModel)
- 7 خدمات منطق أعمال
- 5 متحكمات API

### المرحلة 4 (التكامل)
- 4 واجهات خدمات
- 8 تنفيذات (4 محاكاة + 4 حقيقية)
- 15+ كائنات نقل بيانات
- 1 ملف إعدادات
- 1 ملف تسجيل خدمات

---

## 8. التحقق

### البناء
```bash
dotnet build src/Backend/BOG.sln
```

### قاعدة البيانات
```bash
dotnet ef database update \
    --project src/Backend/BOG.DbModel \
    --startup-project src/Backend/BOG.API
```

### تشغيل API
```bash
dotnet run --project src/Backend/BOG.API
```

### الاختبار باستخدام Swagger
افتح: https://localhost:5001/swagger

### سيناريوهات الاختبار

1. **مسار إنشاء الطلب**
   - إنشاء طلب مسودة
   - إضافة مدعي (فرد)
   - إضافة مدعى عليه
   - تعيين مقدم الطلب
   - تقديم الطلب
   - قيد كدعوى

2. **مسار النواقص**
   - تقديم طلب
   - طلب استكمال (إضافة نواقص)
   - استكمال النواقص
   - التحقق والقيد

3. **اختبارات التحقق**
   - تقديم بدون مدعي (ERR001)
   - تقديم بدون مدعى عليه (ERR002)
   - تقديم بدون مقدم طلب (ERR004)
   - إضافة مدعي مكرر (ERR011)

---

## المراجع

- **مستند المتطلبات:** `docs/srs/use-cases/6.5.1-case-registration/SRS_UC_6.5.1_Full_AR.md`
- **المعمارية:** اتباع الأنماط في تنفيذ كيان `User`
- **CLAUDE.md:** اتفاقيات المشروع وأوامر البناء
