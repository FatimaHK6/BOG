# المطور ب: وحدة المدعى عليهم وإدارة الطلبات
# Developer B: Defendants & Request Management Module

**النطاق:** حالات الاستخدام 6.5.1.1.11 - 6.5.1.1.24 (14 حالة استخدام)
**الاعتماد:** يجب إكمال المرحلة 1 (قاعدة البيانات) أولاً

---

## جدول المحتويات

1. [حالات الاستخدام المخصصة](#حالات-الاستخدام-المخصصة)
2. [الملفات المطلوبة](#الملفات-المطلوبة)
3. [نقاط النهاية API](#نقاط-النهاية-api)
4. [انتقالات الحالة](#انتقالات-الحالة)
5. [قواعد الأعمال](#قواعد-الأعمال)
6. [التكاملات](#التكاملات)
   - [خدمة الرسائل القصيرة](#خدمة-الرسائل-القصيرة-sms)
   - [خدمة البريد الإلكتروني](#خدمة-البريد-الإلكتروني)
   - [نظام إدارة القضايا](#نظام-إدارة-القضايا)
7. [الإشعارات](#الإشعارات)
8. [سيناريوهات الاختبار](#سيناريوهات-الاختبار)
9. [واجهة المستخدم (Frontend)](#9-واجهة-المستخدم-frontend)

---

## حالات الاستخدام المخصصة

| رقم UC | الاسم (عربي) | الاسم (إنجليزي) | الأولوية |
|--------|-------------|-----------------|----------|
| 6.5.1.1.11 | المدعى عليهم | Defendants List | عالية |
| 6.5.1.1.12 | إضافة مدّعى عليه | Add Defendant | عالية |
| 6.5.1.1.13 | تفاصيل مدّعى عليه | View Defendant | متوسطة |
| 6.5.1.1.14 | تعديل مدّعى عليه | Edit Defendant | متوسطة |
| 6.5.1.1.15 | معلومات إضافية | Additional Info | متوسطة |
| 6.5.1.1.16 | استعراض معلومات | View Additional Info | منخفضة |
| 6.5.1.1.17 | مرفقات الطلب | Request Attachments | متوسطة |
| 6.5.1.1.18 | استكمال نواقص | Add Deficiencies | عالية |
| 6.5.1.1.19 | استعراض نواقص | View Deficiencies | منخفضة |
| 6.5.1.1.20 | البحث | Search Requests | عالية |
| 6.5.1.1.21 | تعديل الطلب | Edit Request | متوسطة |
| 6.5.1.1.22 | تفاصيل الطلب | View Request Details | متوسطة |
| 6.5.1.1.23 | اتخاذ إجراء | Take Action | عالية |
| 6.5.1.1.24 | قيد دعوى | Register New Case | عالية |

---

## الملفات المطلوبة

### 1. طبقة الوصول للبيانات (BOG.DAL)

#### الواجهات (`BOG.DAL/Interfaces/`)

```csharp
// IDefendantRepository.cs
public interface IDefendantRepository : IRepository<Defendant>
{
    Task<IEnumerable<Defendant>> GetByRequestIdAsync(int requestId);
    Task<Defendant?> GetByIdentityAsync(int requestId, string identityNumber, int defendantTypeId);
    Task<bool> ExistsByIdentityAsync(int requestId, string identityNumber, int defendantTypeId);
}

// ICaseRegistrationRequestRepository.cs
public interface ICaseRegistrationRequestRepository : IRepository<CaseRegistrationRequest>
{
    Task<CaseRegistrationRequest?> GetWithDetailsAsync(int requestId);
    Task<IEnumerable<CaseRegistrationRequest>> SearchAsync(SearchRequestDTO criteria);
    Task<IEnumerable<CaseRegistrationRequest>> GetPendingCompletionExpiredAsync();
}

// IRequestAttachmentRepository.cs
public interface IRequestAttachmentRepository : IRepository<RequestAttachment>
{
    Task<IEnumerable<RequestAttachment>> GetByRequestIdAsync(int requestId);
}

// IRequestDeficiencyRepository.cs
public interface IRequestDeficiencyRepository : IRepository<RequestDeficiency>
{
    Task<IEnumerable<RequestDeficiency>> GetByRequestIdAsync(int requestId);
}

// IAdditionalInfoRepository.cs
public interface IAdditionalInfoRepository : IRepository<AdditionalInfo>
{
    Task<AdditionalInfo?> GetByRequestIdAsync(int requestId);
}
```

#### التنفيذات (`BOG.DAL/Repositories/`)

```
BOG.DAL/Repositories/
├── DefendantRepository.cs
├── CaseRegistrationRequestRepository.cs
├── RequestAttachmentRepository.cs
├── RequestDeficiencyRepository.cs
└── AdditionalInfoRepository.cs
```

---

### 2. طبقة نقل البيانات (BOG.DTO)

#### DTOs المدعى عليهم (`BOG.DTO/Defendant/`)

```csharp
// DefendantCreateDTO.cs
public class DefendantCreateDTO
{
    public int DefendantTypeId { get; set; }           // مطلوب: 1-6
    public string FullName { get; set; }               // مطلوب: الحد الأقصى 200 حرف

    // هوية اختيارية (للأفراد)
    public int? IdentityTypeId { get; set; }
    public string? IdentityNumber { get; set; }

    // عنوان اختياري
    public string? AddressText { get; set; }
    public AddressDTO? Address { get; set; }

    // للشركات
    public string? CommercialRegNumber { get; set; }

    // للجهات الحكومية
    public int? GovernmentAgencyId { get; set; }
    public string? AdditionalStatement { get; set; }
}

// DefendantUpdateDTO.cs
public class DefendantUpdateDTO
{
    public string? FullName { get; set; }
    public string? AddressText { get; set; }
    public string? AdditionalStatement { get; set; }
    // لا يمكن تغيير: IdentityNumber, DefendantTypeId
}
```

#### DTOs الطلبات (`BOG.DTO/CaseRegistration/`)

```csharp
// CaseRegistrationCreateDTO.cs
public class CaseRegistrationCreateDTO
{
    public int CourtId { get; set; }
    public int DisputeTypeId { get; set; }
    public string? Subject { get; set; }               // الموضوع - الحد الأقصى 4000
    public string? Claims { get; set; }                // الطلبات - الحد الأقصى 4000
    public string? Evidence { get; set; }              // البيّنات - الحد الأقصى 4000
    public string? Notes { get; set; }
}

// TakeActionDTO.cs
public class TakeActionDTO
{
    public string Action { get; set; }                 // Register/SendToJudge/Reject/RequestCompletion
    public string? Notes { get; set; }                 // مطلوب للرفض
    public List<RequestDeficiencyDTO>? Deficiencies { get; set; } // لطلب الاستكمال
}

// SearchRequestDTO.cs
public class SearchRequestDTO
{
    public int? CourtId { get; set; }
    public int? DisputeTypeId { get; set; }
    public string? CaseNumber { get; set; }
    public DateTime? CaseDate { get; set; }
    public int? CaseYear { get; set; }
    public string? IdentityNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? PartyName { get; set; }             // مطابقة جزئية
    public int? StatusId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// RequestDeficiencyDTO.cs
public class RequestDeficiencyDTO
{
    public string DeficiencyLocation { get; set; }     // الموضوع/الطلبات/البيّنات/المعلومات/المرفقات
    public string DeficiencyDescription { get; set; } // الحد الأقصى 2000 حرف
}
```

---

### 3. طبقة نماذج العرض (BOG.VM)

#### VMs المدعى عليهم (`BOG.VM/Defendant/`)

```csharp
// DefendantVM.cs
public class DefendantVM
{
    public int Id { get; set; }
    public int DefendantTypeId { get; set; }
    public string DefendantTypeName { get; set; }
    public string FullName { get; set; }
    public string? IdentityNumber { get; set; }
    public string? AddressText { get; set; }
    public int? DataSourceId { get; set; }             // مصدر البيانات (1=أبشر، 2=المستخدم)
    public string? DataSourceName { get; set; }        // اسم مصدر البيانات
    public string? CommercialRegNumber { get; set; }
    public string? GovernmentAgencyName { get; set; }
    public DateTime CreatedDate { get; set; }
}

// DefendantListVM.cs
public class DefendantListVM
{
    public int Id { get; set; }
    public string DefendantTypeName { get; set; }
    public string FullName { get; set; }
    public string? IdentityNumber { get; set; }
}
```

#### VMs الطلبات (`BOG.VM/CaseRegistration/`)

```csharp
// CaseRegistrationRequestVM.cs
public class CaseRegistrationRequestVM
{
    public int Id { get; set; }
    public int RequestStatusId { get; set; }
    public string RequestStatusName { get; set; }
    public string CourtName { get; set; }
    public string DisputeTypeName { get; set; }
    public string? Subject { get; set; }
    public string? Claims { get; set; }
    public string? Evidence { get; set; }
    public string? Notes { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? CompletionDeadline { get; set; }
    public string? RejectionReason { get; set; }

    // معلومات مقدم الطلب
    public string? ApplicantName { get; set; }
    public string? ApplicantType { get; set; }

    // الإحصائيات
    public int PlaintiffsCount { get; set; }
    public int DefendantsCount { get; set; }
    public int AttachmentsCount { get; set; }
    public int DeficienciesCount { get; set; }

    // القوائم (اختياري، لعرض التفاصيل)
    public List<PlaintiffListVM>? Plaintiffs { get; set; }
    public List<DefendantListVM>? Defendants { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

---

### 4. طبقة منطق الأعمال (BOG.BL)

#### الواجهات (`BOG.BL/Interfaces/`)

```csharp
// IDefendantBL.cs
public interface IDefendantBL
{
    Task<DefendantVM> CreateDefendantAsync(int requestId, DefendantCreateDTO dto);
    Task<DefendantVM?> GetDefendantByIdAsync(int defendantId);
    Task<IEnumerable<DefendantListVM>> GetDefendantsByRequestIdAsync(int requestId);
    Task<DefendantVM> UpdateDefendantAsync(int defendantId, DefendantUpdateDTO dto);
    Task DeleteDefendantAsync(int defendantId);
}

// ICaseRegistrationBL.cs
public interface ICaseRegistrationBL
{
    Task<CaseRegistrationRequestVM> CreateRequestAsync(CaseRegistrationCreateDTO dto);
    Task<CaseRegistrationRequestVM?> GetRequestByIdAsync(int requestId);
    Task<CaseRegistrationRequestVM> UpdateRequestAsync(int requestId, CaseRegistrationUpdateDTO dto);
    Task<CaseRegistrationRequestVM> SubmitRequestAsync(int requestId);
    Task<PagedResult<CaseRegistrationListVM>> SearchRequestsAsync(SearchRequestDTO criteria);
    Task<bool> ValidateForSubmissionAsync(int requestId);
}

// IRequestActionBL.cs
public interface IRequestActionBL
{
    Task<CaseRegistrationRequestVM> TakeActionAsync(int requestId, TakeActionDTO dto);
    Task<CaseRegistrationRequestVM> RegisterCaseAsync(int requestId);
    Task<CaseRegistrationRequestVM> SendToJudgeDeskAsync(int requestId);
    Task<CaseRegistrationRequestVM> RejectRequestAsync(int requestId, string reason);
    Task<CaseRegistrationRequestVM> RequestCompletionAsync(int requestId, List<RequestDeficiencyDTO> deficiencies);
    Task ProcessExpiredCompletionRequestsAsync(); // مهمة خلفية
}
```

#### ملاحظات التنفيذ

**DefendantBL.cs:**
```csharp
public class DefendantBL : IDefendantBL
{
    // الفروقات الرئيسية عن المدعي:
    // - حقول مطلوبة أقل (فقط FullName مطلوب)
    // - العنوان اختياري
    // - تكامل أبشر اختياري
    // - لا يوجد ممثلين
    // - لا يوجد مرفقات

    public async Task<DefendantVM> CreateDefendantAsync(int requestId, DefendantCreateDTO dto)
    {
        // 1. التحقق من وجود الطلب والحالة تسمح بالإضافة
        //    (None, Draft, PendingCompletion)
        // 2. التحقق من التكرار (ERR013) - نفس الهوية + النوع
        // 3. التحقق من أن FullName مطلوب
        // 4. إنشاء الكيان والحفظ
        // 5. إرجاع VM
    }
}
```

**RequestActionBL.cs:**
```csharp
public class RequestActionBL : IRequestActionBL
{
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ICaseManagementService _caseManagementService;

    public async Task<CaseRegistrationRequestVM> TakeActionAsync(int requestId, TakeActionDTO dto)
    {
        return dto.Action switch
        {
            "Register" => await RegisterCaseAsync(requestId),
            "SendToJudge" => await SendToJudgeDeskAsync(requestId),
            "Reject" => await RejectRequestAsync(requestId, dto.Notes!),
            "RequestCompletion" => await RequestCompletionAsync(requestId, dto.Deficiencies!),
            _ => throw new InvalidOperationException("إجراء غير صالح")
        };
    }

    public async Task<CaseRegistrationRequestVM> RegisterCaseAsync(int requestId)
    {
        // 1. التحقق من الحالة الحالية (طلب جديد أو على مكتب القاضي)
        // 2. التحقق من اكتمال البيانات
        // 3. تسجيل القضية في نظام إدارة القضايا
        var caseResult = await _caseManagementService.RegisterCaseAsync(caseData);
        // 4. تغيير الحالة إلى مسجلة (6)
        // 5. إرسال الإشعارات (SMS + Email)
        await _smsService.SendSmsAsync(mobile, SmsTemplates.RequestRegistered, parameters);
        await _emailService.SendEmailAsync(email, EmailTemplates.RequestRegistered, parameters);
        // 6. إرجاع الطلب المحدث
    }

    public async Task ProcessExpiredCompletionRequestsAsync()
    {
        // مهمة خلفية - تعمل يومياً
        // 1. جلب كل PendingCompletion حيث CompletionDeadline < الآن
        // 2. تغيير الحالة إلى مرفوض (10)
        // 3. تعيين RejectionReason = "انتهت مهلة الاستكمال"
        // 4. إرسال الإشعارات
        await _smsService.SendSmsAsync(mobile, SmsTemplates.AutoRejected, parameters);
    }
}
```

---

### 5. وحدات التحكم (BOG.API/Controllers/)

```csharp
// DefendantsController.cs
[ApiController]
[Route("api")]
public class DefendantsController : ControllerBase
{
    // GET /api/case-requests/{requestId}/defendants
    [HttpGet("case-requests/{requestId}/defendants")]
    public async Task<ActionResult<IEnumerable<DefendantListVM>>> GetDefendants(int requestId)

    // POST /api/case-requests/{requestId}/defendants
    [HttpPost("case-requests/{requestId}/defendants")]
    public async Task<ActionResult<DefendantVM>> CreateDefendant(int requestId, DefendantCreateDTO dto)

    // GET /api/defendants/{id}
    [HttpGet("defendants/{id}")]
    public async Task<ActionResult<DefendantVM>> GetDefendant(int id)

    // PUT /api/defendants/{id}
    [HttpPut("defendants/{id}")]
    public async Task<ActionResult<DefendantVM>> UpdateDefendant(int id, DefendantUpdateDTO dto)

    // DELETE /api/defendants/{id}
    [HttpDelete("defendants/{id}")]
    public async Task<IActionResult> DeleteDefendant(int id)
}

// CaseRegistrationController.cs
[ApiController]
[Route("api/case-requests")]
public class CaseRegistrationController : ControllerBase
{
    // POST /api/case-requests
    [HttpPost]
    public async Task<ActionResult<CaseRegistrationRequestVM>> CreateRequest(CaseRegistrationCreateDTO dto)

    // GET /api/case-requests/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CaseRegistrationRequestVM>> GetRequest(int id)

    // PUT /api/case-requests/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<CaseRegistrationRequestVM>> UpdateRequest(int id, CaseRegistrationUpdateDTO dto)

    // POST /api/case-requests/{id}/submit
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<CaseRegistrationRequestVM>> SubmitRequest(int id)

    // GET /api/case-requests/search
    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<CaseRegistrationListVM>>> SearchRequests([FromQuery] SearchRequestDTO criteria)

    // POST /api/case-requests/{id}/action
    [HttpPost("{id}/action")]
    public async Task<ActionResult<CaseRegistrationRequestVM>> TakeAction(int id, TakeActionDTO dto)

    // GET/POST/DELETE للمرفقات والنواقص والمعلومات الإضافية...
}
```

---

## نقاط النهاية API

### Defendants Controller - المدعى عليهم

| الطريقة | النقطة | الوصف |
|---------|--------|-------|
| GET | `/api/case-requests/{requestId}/defendants` | قائمة المدعى عليهم للطلب |
| POST | `/api/case-requests/{requestId}/defendants` | إضافة مدعى عليه |
| GET | `/api/defendants/{id}` | تفاصيل مدعى عليه |
| PUT | `/api/defendants/{id}` | تعديل مدعى عليه |
| DELETE | `/api/defendants/{id}` | حذف مدعى عليه |

### CaseRegistration Controller - الطلبات

| الطريقة | النقطة | الوصف |
|---------|--------|-------|
| POST | `/api/case-requests` | إنشاء طلب جديد (مسودة) |
| GET | `/api/case-requests/{id}` | تفاصيل الطلب |
| PUT | `/api/case-requests/{id}` | تعديل الطلب |
| POST | `/api/case-requests/{id}/submit` | تقديم الطلب |
| GET | `/api/case-requests/search` | البحث في الطلبات |
| POST | `/api/case-requests/{id}/action` | اتخاذ إجراء |
| GET | `/api/case-requests/{id}/attachments` | قائمة المرفقات |
| POST | `/api/case-requests/{id}/attachments` | إضافة مرفق |
| DELETE | `/api/case-requests/{id}/attachments/{attachmentId}` | حذف مرفق |
| GET | `/api/case-requests/{id}/deficiencies` | قائمة النواقص |
| POST | `/api/case-requests/{id}/deficiencies` | إضافة نقص |
| GET | `/api/case-requests/{id}/additional-info` | المعلومات الإضافية |
| PUT | `/api/case-requests/{id}/additional-info` | تعديل المعلومات الإضافية |

---

## انتقالات الحالة

| من | الإجراء | إلى | التحقق |
|----|--------|-----|--------|
| None (0) | حفظ | مسودة (1) | - |
| مسودة (1) | تقديم | طلب جديد (3) | ERR001-007 |
| طلب جديد (3) | قيد | مسجلة (6) | البيانات مكتملة |
| طلب جديد (3) | إرسال للقاضي | على مكتب القاضي (5) | - |
| طلب جديد (3) | رفض | مرفوض (10) | ملاحظات مطلوبة |
| طلب جديد (3) | طلب استكمال | انتظار الاستكمال (8) | نواقص مطلوبة |
| على مكتب القاضي (5) | قيد | مسجلة (6) | موافقة القاضي |
| على مكتب القاضي (5) | رفض | مرفوض (10) | ملاحظات مطلوبة |
| على مكتب القاضي (5) | طلب استكمال | انتظار الاستكمال (8) | نواقص مطلوبة |
| انتظار الاستكمال (8) | استكمال | قيد التحقق (9) | المستندات مقدمة |
| انتظار الاستكمال (8) | انتهاء المهلة (30 يوم) | مرفوض (10) | تلقائي |
| قيد التحقق (9) | قبول | طلب جديد (3) | النواقص مكتملة |
| قيد التحقق (9) | رفض | انتظار الاستكمال (8) | النواقص غير مكتملة |

---

## قواعد الأعمال

| الرمز | القاعدة | التنفيذ |
|-------|--------|---------|
| ERR001 | مدعي واحد على الأقل مطلوب | ValidateForSubmissionAsync |
| ERR002 | مدعى عليه واحد على الأقل مطلوب | ValidateForSubmissionAsync |
| ERR003 | المرفقات الإلزامية غير مكتملة | ValidateForSubmissionAsync |
| ERR004 | مقدم الطلب غير محدد | ValidateForSubmissionAsync |
| ERR005 | التصنيفات غير محددة | ValidateForSubmissionAsync |
| ERR006 | الموضوع فارغ | ValidateForSubmissionAsync |
| ERR007 | البيّنات فارغة | ValidateForSubmissionAsync |
| ERR013 | المدعى عليه موجود مسبقاً | DefendantBL.Create |
| BR04 | PDF فقط، الحد الأقصى 4MB | RequestAttachmentBL |
| BR05 | رفض تلقائي بعد 30 يوم | ProcessExpiredCompletionRequestsAsync |

---

## التكاملات

### جدول مصدر البيانات (DataSource)

| Id | Name | NameAr | الوصف |
|----|------|--------|-------|
| 1 | FromAbsher | من أبشر | البيانات مجلوبة من نظام أبشر |
| 2 | FromUser | من المستخدم | البيانات مدخلة يدوياً |

**ملاحظة:** استخدم `DataSourceId` بدلاً من `IsAbsherVerified` لتتبع مصدر بيانات المدعى عليه.

### خدمة الرسائل القصيرة (SMS)

يستخدم هذا الموديول خدمة الرسائل القصيرة لإرسال الإشعارات عند تغيير الحالات والأحداث المهمة.

#### الواجهة

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

| الرمز | القالب (عربي) |
|-------|--------------|
| REQUEST_SUBMITTED | تم استلام طلب قيد الدعوى رقم {RequestNumber} بنجاح |
| REQUEST_REGISTERED | تم قيد دعواكم رقم {CaseNumber} في المحكمة الإدارية |
| REQUEST_REJECTED | تم رفض طلب قيد الدعوى رقم {RequestNumber}. السبب: {Reason} |
| COMPLETION_REQUIRED | مطلوب استكمال نواقص الطلب رقم {RequestNumber} خلال 30 يوم |
| DEFICIENCY_REMINDER | تذكير: يرجى استكمال نواقص الطلب رقم {RequestNumber}. متبقي {DaysRemaining} أيام |
| AUTO_REJECTED | تم رفض الطلب رقم {RequestNumber} تلقائياً لعدم استكمال النواقص |

#### الاستخدام في RequestActionBL

```csharp
public async Task<CaseRegistrationRequestVM> RegisterCaseAsync(int requestId)
{
    // ... منطق القيد ...

    // إرسال SMS للإشعار
    await _smsService.SendSmsAsync(
        applicant.MobileNumber,
        SmsTemplates.RequestRegistered,
        new Dictionary<string, string>
        {
            ["CaseNumber"] = caseResult.CaseNumber,
            ["CourtName"] = request.Court.NameAr
        });

    // ...
}
```

#### إعداد الخدمة الوهمية

```json
// appsettings.json
{
  "Integration": {
    "Sms": {
      "UseMock": true,
      "Provider": "Unifonic",
      "ApiKey": "",
      "SenderId": "BOG"
    }
  }
}
```

---

### خدمة البريد الإلكتروني

#### الواجهة

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

#### قوالب البريد

| الرمز | الموضوع | المحتوى |
|-------|---------|---------|
| REQUEST_SUBMITTED | تم استلام طلب قيد الدعوى - {RequestNumber} | تأكيد استلام الطلب مع رقم الطلب |
| REQUEST_REGISTERED | تم قيد الدعوى - {CaseNumber} | بيانات القضية والمحكمة والدائرة |
| COMPLETION_REQUIRED | مطلوب استكمال نواقص - طلب رقم {RequestNumber} | قائمة النواقص والمهلة |

#### الاستخدام في RequestActionBL

```csharp
public async Task<CaseRegistrationRequestVM> RequestCompletionAsync(
    int requestId,
    List<RequestDeficiencyDTO> deficiencies)
{
    // ... إضافة النواقص ...

    // إرسال Email للإشعار
    var deficienciesList = string.Join("<li>",
        deficiencies.Select(d => $"{d.DeficiencyLocation}: {d.DeficiencyDescription}"));

    await _emailService.SendEmailAsync(
        applicant.Email,
        EmailTemplates.CompletionRequired,
        new Dictionary<string, string>
        {
            ["RequestNumber"] = request.Id.ToString(),
            ["DeficienciesList"] = deficienciesList
        });

    // ...
}
```

#### إعداد الخدمة الوهمية

```json
// appsettings.json
{
  "Integration": {
    "Email": {
      "UseMock": true,
      "SmtpHost": "smtp.example.sa",
      "SmtpPort": 587,
      "FromEmail": "noreply@bog.gov.sa",
      "FromName": "ديوان المظالم"
    }
  }
}
```

---

### نظام إدارة القضايا

يستخدم هذا الموديول نظام إدارة القضايا لتسجيل القضايا المعتمدة.

#### الواجهة

```csharp
// BOG.Integration/Interfaces/ICaseManagementService.cs
public interface ICaseManagementService
{
    Task<CaseRegistrationResult> RegisterCaseAsync(CaseRegistrationData caseData);
    Task<CaseStatusResult> GetCaseStatusAsync(string caseNumber);
    Task<bool> UpdateCaseAsync(string caseNumber, CaseUpdateData updateData);
}
```

#### البيانات المرسلة للتسجيل

```csharp
// BOG.Integration/DTOs/CaseManagement/CaseRegistrationData.cs
public class CaseRegistrationData
{
    public int RequestId { get; set; }
    public int CourtId { get; set; }
    public int DepartmentId { get; set; }
    public int DisputeTypeId { get; set; }
    public string Subject { get; set; }
    public string Claims { get; set; }
    public List<CasePlaintiffData> Plaintiffs { get; set; }
    public List<CaseDefendantData> Defendants { get; set; }
}
```

#### النتيجة

```csharp
// BOG.Integration/DTOs/CaseManagement/CaseRegistrationResult.cs
public class CaseRegistrationResult
{
    public bool IsSuccess { get; set; }
    public string? CaseNumber { get; set; }          // رقم القضية
    public string? RegistrationNumber { get; set; } // رقم القيد
    public DateTime? RegistrationDate { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
```

#### الاستخدام في RequestActionBL

```csharp
public async Task<CaseRegistrationRequestVM> RegisterCaseAsync(int requestId)
{
    var request = await _requestRepository.GetWithDetailsAsync(requestId);

    // تحضير بيانات القضية
    var caseData = new CaseRegistrationData
    {
        RequestId = request.Id,
        CourtId = request.CourtId,
        DepartmentId = request.DepartmentId,
        DisputeTypeId = request.DisputeTypeId,
        Subject = request.Subject,
        Claims = request.Claims,
        Plaintiffs = request.Plaintiffs.Select(p => new CasePlaintiffData
        {
            FullName = p.FullName,
            IdentityNumber = p.IdentityNumber,
            // ...
        }).ToList(),
        Defendants = request.Defendants.Select(d => new CaseDefendantData
        {
            FullName = d.FullName,
            // ...
        }).ToList()
    };

    // تسجيل القضية في النظام
    var result = await _caseManagementService.RegisterCaseAsync(caseData);

    if (!result.IsSuccess)
    {
        throw new BusinessException($"فشل تسجيل القضية: {result.ErrorMessageAr}");
    }

    // تحديث الطلب برقم القضية
    request.CaseNumber = result.CaseNumber;
    request.RegistrationNumber = result.RegistrationNumber;
    request.RegistrationDate = result.RegistrationDate;
    request.RequestStatusId = (int)RequestStatus.Registered;

    await _unitOfWork.SaveChangesAsync();

    // إرسال الإشعارات...
}
```

#### إعداد الخدمة الوهمية

```json
// appsettings.json
{
  "Integration": {
    "CaseManagement": {
      "UseMock": true,
      "BaseUrl": "https://cms.bog.gov.sa/api",
      "Version": "v4"
    }
  }
}
```

---

## الإشعارات

| الحدث | النوع | المستلم |
|-------|------|---------|
| الطلب → مسجلة | SMS + Email | مقدم الطلب |
| الطلب → مرفوض | SMS + Email | مقدم الطلب |
| الطلب → انتظار الاستكمال | SMS + Email | مقدم الطلب |
| انتظار الاستكمال → مرفوض (انتهاء المهلة) | SMS + Email | مقدم الطلب |

---

## سيناريوهات الاختبار

1. **إنشاء طلب جديد**
   - POST /api/case-requests (إنشاء مسودة)
   - التحقق من الحالة = مسودة (1)

2. **إضافة مدعى عليه**
   - POST /api/case-requests/1/defendants
   - التحقق أن FullName فقط مطلوب

3. **تقديم الطلب**
   - POST /api/case-requests/1/submit
   - التحقق من جميع أخطاء التحقق

4. **اتخاذ إجراء - قيد**
   - POST /api/case-requests/1/action مع Action="Register"
   - التحقق من تغيير الحالة إلى مسجلة (6)
   - التحقق من إرسال SMS و Email

5. **اتخاذ إجراء - طلب استكمال**
   - POST /api/case-requests/1/action مع Action="RequestCompletion"
   - التحقق من إضافة النواقص
   - التحقق من تعيين CompletionDeadline

6. **البحث في الطلبات**
   - GET /api/case-requests/search مع معايير مختلفة
   - التحقق من عمل الترقيم

7. **فحص التكرار**
   - إضافة نفس المدعى عليه مرتين
   - التحقق من إرجاع ERR013

8. **التحقق من تكامل نظام القضايا**
   - قيد الطلب
   - التحقق من استلام رقم القضية من نظام القضايا

---

## تسجيل الخدمات (DI)

أضف إلى `ServiceCollectionExtensions.cs`:

```csharp
// Repositories
services.AddScoped<IDefendantRepository, DefendantRepository>();
services.AddScoped<ICaseRegistrationRequestRepository, CaseRegistrationRequestRepository>();
services.AddScoped<IRequestAttachmentRepository, RequestAttachmentRepository>();
services.AddScoped<IRequestDeficiencyRepository, RequestDeficiencyRepository>();
services.AddScoped<IAdditionalInfoRepository, AdditionalInfoRepository>();

// Business Logic
services.AddScoped<IDefendantBL, DefendantBL>();
services.AddScoped<ICaseRegistrationBL, CaseRegistrationBL>();
services.AddScoped<IRequestActionBL, RequestActionBL>();
services.AddScoped<IRequestAttachmentBL, RequestAttachmentBL>();
```

---

## أوامر التحقق

```bash
# البناء
dotnet build src/Backend/BOG.sln

# التشغيل
dotnet run --project src/Backend/BOG.API

# الاختبار عبر Swagger
# https://localhost:5001/swagger
```

---

## 9. واجهة المستخدم (Frontend)

### 9.1 هيكل المجلدات

```
src/Frontend/bog-app/src/app/
├── features/
│   └── case-registration/
│       ├── defendants/
│       │   ├── components/
│       │   │   ├── defendant-list/
│       │   │   │   ├── defendant-list.component.ts
│       │   │   │   ├── defendant-list.component.html
│       │   │   │   └── defendant-list.component.scss
│       │   │   ├── defendant-form/
│       │   │   │   ├── defendant-form.component.ts
│       │   │   │   ├── defendant-form.component.html
│       │   │   │   └── defendant-form.component.scss
│       │   │   └── defendant-details/
│       │   │       ├── defendant-details.component.ts
│       │   │       └── defendant-details.component.html
│       │   ├── models/
│       │   │   ├── defendant.model.ts
│       │   │   └── defendant-type.model.ts
│       │   ├── services/
│       │   │   └── defendant.service.ts
│       │   └── defendants.module.ts
│       ├── case-data/
│       │   ├── components/
│       │   │   ├── case-data-form/
│       │   │   │   ├── case-data-form.component.ts
│       │   │   │   ├── case-data-form.component.html
│       │   │   │   └── case-data-form.component.scss
│       │   │   ├── additional-info/
│       │   │   │   ├── additional-info.component.ts
│       │   │   │   └── additional-info.component.html
│       │   │   └── request-attachments/
│       │   │       ├── request-attachments.component.ts
│       │   │       └── request-attachments.component.html
│       │   ├── models/
│       │   │   ├── case-data.model.ts
│       │   │   └── additional-info.model.ts
│       │   ├── services/
│       │   │   └── case-data.service.ts
│       │   └── case-data.module.ts
│       ├── deficiencies/
│       │   ├── components/
│       │   │   ├── deficiency-list/
│       │   │   │   ├── deficiency-list.component.ts
│       │   │   │   ├── deficiency-list.component.html
│       │   │   │   └── deficiency-list.component.scss
│       │   │   ├── deficiency-form/
│       │   │   │   ├── deficiency-form.component.ts
│       │   │   │   └── deficiency-form.component.html
│       │   │   └── complete-deficiency/
│       │   │       ├── complete-deficiency.component.ts
│       │   │       └── complete-deficiency.component.html
│       │   ├── models/
│       │   │   └── deficiency.model.ts
│       │   ├── services/
│       │   │   └── deficiency.service.ts
│       │   └── deficiencies.module.ts
│       ├── request-management/
│       │   ├── components/
│       │   │   ├── request-search/
│       │   │   │   ├── request-search.component.ts
│       │   │   │   ├── request-search.component.html
│       │   │   │   └── request-search.component.scss
│       │   │   ├── request-details/
│       │   │   │   ├── request-details.component.ts
│       │   │   │   └── request-details.component.html
│       │   │   ├── submit-request/
│       │   │   │   ├── submit-request.component.ts
│       │   │   │   └── submit-request.component.html
│       │   │   ├── take-action-dialog/
│       │   │   │   ├── take-action-dialog.component.ts
│       │   │   │   └── take-action-dialog.component.html
│       │   │   └── rejection-dialog/
│       │   │       ├── rejection-dialog.component.ts
│       │   │       └── rejection-dialog.component.html
│       │   ├── models/
│       │   │   ├── case-registration-request.model.ts
│       │   │   ├── search-criteria.model.ts
│       │   │   └── take-action.model.ts
│       │   ├── services/
│       │   │   ├── case-registration.service.ts
│       │   │   └── request-action.service.ts
│       │   └── request-management.module.ts
│       └── shared/
│           └── components/
│               └── status-badge/
│                   ├── status-badge.component.ts
│                   └── status-badge.component.html
```

### 9.2 المكونات الرئيسية

#### قائمة المدعى عليهم (defendant-list)

```typescript
// defendant-list.component.ts
@Component({
  selector: 'app-defendant-list',
  templateUrl: './defendant-list.component.html',
  styleUrls: ['./defendant-list.component.scss']
})
export class DefendantListComponent implements OnInit {
  defendants: DefendantListVM[] = [];
  displayedColumns = ['type', 'fullName', 'identityNumber', 'actions'];

  constructor(
    private defendantService: DefendantService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadDefendants();
  }

  onAddDefendant(): void {
    const dialogRef = this.dialog.open(DefendantFormComponent, {
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadDefendants();
    });
  }

  onDeleteDefendant(id: number): void {
    // Confirm and delete
  }
}
```

#### نموذج المدعى عليه (defendant-form)

```typescript
// defendant-form.component.ts
@Component({
  selector: 'app-defendant-form',
  templateUrl: './defendant-form.component.html'
})
export class DefendantFormComponent implements OnInit {
  @Input() requestId: number;
  @Output() saved = new EventEmitter<Defendant>();

  defendantForm: FormGroup;
  defendantTypes: DefendantType[] = [];

  constructor(
    private fb: FormBuilder,
    private defendantService: DefendantService
  ) {}

  ngOnInit(): void {
    this.defendantForm = this.fb.group({
      defendantTypeId: ['', Validators.required],
      fullName: ['', [Validators.required, Validators.maxLength(200)]],
      identityTypeId: [null],
      identityNumber: [null],
      addressText: [''],
      commercialRegNumber: [''],
      governmentAgencyId: [null],
      additionalStatement: ['']
    });
  }

  onDefendantTypeChange(): void {
    // Show/hide fields based on type
  }

  onSubmit(): void {
    if (this.defendantForm.valid) {
      this.defendantService.createDefendant(
        this.requestId,
        this.defendantForm.value
      ).subscribe(defendant => this.saved.emit(defendant));
    }
  }
}
```

#### بحث الطلبات (request-search)

```typescript
// request-search.component.ts
@Component({
  selector: 'app-request-search',
  templateUrl: './request-search.component.html'
})
export class RequestSearchComponent implements OnInit {
  searchForm: FormGroup;
  requests: CaseRegistrationListVM[] = [];
  totalCount = 0;
  pageSize = 20;
  currentPage = 1;

  courts: Court[] = [];
  disputeTypes: DisputeType[] = [];
  statuses: RequestStatus[] = [];

  displayedColumns = ['id', 'court', 'disputeType', 'applicant',
                      'status', 'createdDate', 'actions'];

  constructor(
    private fb: FormBuilder,
    private caseRegistrationService: CaseRegistrationService
  ) {}

  ngOnInit(): void {
    this.searchForm = this.fb.group({
      courtId: [null],
      disputeTypeId: [null],
      caseNumber: [''],
      caseYear: [null],
      identityNumber: [''],
      mobileNumber: [''],
      partyName: [''],
      statusId: [null]
    });
  }

  onSearch(): void {
    const criteria = {
      ...this.searchForm.value,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    this.caseRegistrationService.searchRequests(criteria)
      .subscribe(result => {
        this.requests = result.items;
        this.totalCount = result.totalCount;
      });
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.onSearch();
  }
}
```

#### تقديم الطلب (submit-request)

```typescript
// submit-request.component.ts
@Component({
  selector: 'app-submit-request',
  templateUrl: './submit-request.component.html'
})
export class SubmitRequestComponent implements OnInit {
  @Input() requestId: number;
  @Output() submitted = new EventEmitter<void>();

  validationErrors: string[] = [];
  isValid = false;

  constructor(private caseRegistrationService: CaseRegistrationService) {}

  ngOnInit(): void {
    this.validateRequest();
  }

  validateRequest(): void {
    this.caseRegistrationService.validateForSubmission(this.requestId)
      .subscribe({
        next: (result) => {
          this.isValid = result.isValid;
          this.validationErrors = result.errors;
        }
      });
  }

  onSubmit(): void {
    this.caseRegistrationService.submitRequest(this.requestId)
      .subscribe(() => this.submitted.emit());
  }
}
```

#### حوار اتخاذ الإجراء (take-action-dialog)

```typescript
// take-action-dialog.component.ts
@Component({
  selector: 'app-take-action-dialog',
  templateUrl: './take-action-dialog.component.html'
})
export class TakeActionDialogComponent implements OnInit {
  actionForm: FormGroup;
  actions = [
    { value: 'Register', label: 'قيد الدعوى' },
    { value: 'SendToJudge', label: 'إرسال لمكتب القاضي' },
    { value: 'RequestCompletion', label: 'طلب استكمال' },
    { value: 'Reject', label: 'رفض' }
  ];

  deficiencyLocations = [
    'الموضوع', 'الطلبات', 'البيّنات', 'المعلومات الإضافية', 'المرفقات'
  ];

  deficiencies: FormArray;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<TakeActionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { requestId: number },
    private requestActionService: RequestActionService
  ) {}

  ngOnInit(): void {
    this.actionForm = this.fb.group({
      action: ['', Validators.required],
      notes: [''],
      deficiencies: this.fb.array([])
    });

    this.deficiencies = this.actionForm.get('deficiencies') as FormArray;
  }

  onActionChange(): void {
    const action = this.actionForm.get('action')?.value;
    if (action === 'Reject') {
      this.actionForm.get('notes')?.setValidators([Validators.required]);
    } else {
      this.actionForm.get('notes')?.clearValidators();
    }
    this.actionForm.get('notes')?.updateValueAndValidity();
  }

  addDeficiency(): void {
    this.deficiencies.push(this.fb.group({
      deficiencyLocation: ['', Validators.required],
      deficiencyDescription: ['', [Validators.required, Validators.maxLength(2000)]]
    }));
  }

  onSubmit(): void {
    if (this.actionForm.valid) {
      this.requestActionService.takeAction(
        this.data.requestId,
        this.actionForm.value
      ).subscribe(() => this.dialogRef.close(true));
    }
  }
}
```

### 9.3 النماذج (Models)

```typescript
// defendant.model.ts
export interface Defendant {
  id: number;
  defendantTypeId: number;
  defendantTypeName: string;
  fullName: string;
  identityNumber?: string;
  addressText?: string;
  dataSourceId?: number;
  dataSourceName?: string;
  commercialRegNumber?: string;
  governmentAgencyName?: string;
  createdDate: Date;
}

// case-registration-request.model.ts
export interface CaseRegistrationRequest {
  id: number;
  requestStatusId: number;
  requestStatusName: string;
  courtId: number;
  courtName: string;
  disputeTypeId: number;
  disputeTypeName: string;
  subject?: string;
  claims?: string;
  evidence?: string;
  notes?: string;
  submissionDate?: Date;
  completionDeadline?: Date;
  rejectionReason?: string;
  applicantName?: string;
  applicantType?: string;
  plaintiffsCount: number;
  defendantsCount: number;
  attachmentsCount: number;
  deficienciesCount: number;
  createdDate: Date;
  modifiedDate: Date;
}

// search-criteria.model.ts
export interface SearchCriteria {
  courtId?: number;
  disputeTypeId?: number;
  caseNumber?: string;
  caseDate?: Date;
  caseYear?: number;
  identityNumber?: string;
  mobileNumber?: string;
  partyName?: string;
  statusId?: number;
  pageNumber: number;
  pageSize: number;
}

// take-action.model.ts
export interface TakeActionDTO {
  action: 'Register' | 'SendToJudge' | 'Reject' | 'RequestCompletion';
  notes?: string;
  deficiencies?: RequestDeficiencyDTO[];
}

export interface RequestDeficiencyDTO {
  deficiencyLocation: string;
  deficiencyDescription: string;
}

// deficiency.model.ts
export interface RequestDeficiency {
  id: number;
  requestId: number;
  deficiencyLocation: string;
  deficiencyDescription: string;
  isResolved: boolean;
  resolvedDate?: Date;
  createdDate: Date;
}
```

### 9.4 الخدمات (Services)

```typescript
// defendant.service.ts
@Injectable({ providedIn: 'root' })
export class DefendantService {
  private apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  getDefendants(requestId: number): Observable<DefendantListVM[]> {
    return this.http.get<DefendantListVM[]>(
      `${this.apiUrl}/case-requests/${requestId}/defendants`
    );
  }

  getDefendant(id: number): Observable<Defendant> {
    return this.http.get<Defendant>(`${this.apiUrl}/defendants/${id}`);
  }

  createDefendant(requestId: number, defendant: DefendantCreateDTO): Observable<Defendant> {
    return this.http.post<Defendant>(
      `${this.apiUrl}/case-requests/${requestId}/defendants`, defendant
    );
  }

  updateDefendant(id: number, defendant: DefendantUpdateDTO): Observable<Defendant> {
    return this.http.put<Defendant>(`${this.apiUrl}/defendants/${id}`, defendant);
  }

  deleteDefendant(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/defendants/${id}`);
  }
}

// case-registration.service.ts
@Injectable({ providedIn: 'root' })
export class CaseRegistrationService {
  private apiUrl = `${environment.apiUrl}/api/case-requests`;

  constructor(private http: HttpClient) {}

  createRequest(dto: CaseRegistrationCreateDTO): Observable<CaseRegistrationRequest> {
    return this.http.post<CaseRegistrationRequest>(this.apiUrl, dto);
  }

  getRequest(id: number): Observable<CaseRegistrationRequest> {
    return this.http.get<CaseRegistrationRequest>(`${this.apiUrl}/${id}`);
  }

  updateRequest(id: number, dto: CaseRegistrationUpdateDTO): Observable<CaseRegistrationRequest> {
    return this.http.put<CaseRegistrationRequest>(`${this.apiUrl}/${id}`, dto);
  }

  submitRequest(id: number): Observable<CaseRegistrationRequest> {
    return this.http.post<CaseRegistrationRequest>(`${this.apiUrl}/${id}/submit`, {});
  }

  searchRequests(criteria: SearchCriteria): Observable<PagedResult<CaseRegistrationListVM>> {
    return this.http.get<PagedResult<CaseRegistrationListVM>>(
      `${this.apiUrl}/search`, { params: criteria as any }
    );
  }

  validateForSubmission(id: number): Observable<ValidationResult> {
    return this.http.get<ValidationResult>(`${this.apiUrl}/${id}/validate`);
  }
}

// request-action.service.ts
@Injectable({ providedIn: 'root' })
export class RequestActionService {
  private apiUrl = `${environment.apiUrl}/api/case-requests`;

  constructor(private http: HttpClient) {}

  takeAction(requestId: number, dto: TakeActionDTO): Observable<CaseRegistrationRequest> {
    return this.http.post<CaseRegistrationRequest>(
      `${this.apiUrl}/${requestId}/action`, dto
    );
  }

  getDeficiencies(requestId: number): Observable<RequestDeficiency[]> {
    return this.http.get<RequestDeficiency[]>(
      `${this.apiUrl}/${requestId}/deficiencies`
    );
  }

  addDeficiency(requestId: number, dto: RequestDeficiencyDTO): Observable<RequestDeficiency> {
    return this.http.post<RequestDeficiency>(
      `${this.apiUrl}/${requestId}/deficiencies`, dto
    );
  }

  completeDeficiencies(requestId: number): Observable<CaseRegistrationRequest> {
    return this.http.post<CaseRegistrationRequest>(
      `${this.apiUrl}/${requestId}/complete-deficiencies`, {}
    );
  }
}
```

### 9.5 رسائل التحقق

| الرمز | الرسالة العربية | الاستخدام |
|-------|----------------|-----------|
| ERR001 | يجب إضافة مدعي واحد على الأقل | عند التقديم |
| ERR002 | يجب إضافة مدعى عليه واحد على الأقل | عند التقديم |
| ERR003 | المرفقات الإلزامية غير مكتملة | عند التقديم |
| ERR004 | يجب تحديد مقدم الطلب | عند التقديم |
| ERR005 | يجب تحديد التصنيفات | عند التقديم |
| ERR006 | يجب إدخال موضوع الدعوى | عند التقديم |
| ERR007 | يجب إدخال البيّنات | عند التقديم |
| ERR013 | المدعى عليه موجود مسبقاً | عند إضافة مدعى عليه |
| REJECT_NOTES | يجب إدخال سبب الرفض | عند الرفض |

### 9.5.1 قواعد التحقق لنموذج فرد (DefendantType = 1)

#### التبويب 1: بيانات شخصية

| الحقل | الاسم العربي | إجباري | قواعد التحقق | رسالة الخطأ |
|-------|-------------|--------|--------------|-------------|
| identityTypeId | نوع الهوية | لا | - | - |
| identityNumber | رقم الهوية | لا | 10 أرقام فقط | رقم الهوية يجب أن يكون 10 أرقام |
| firstName | الاسم الأول | نعم | الحد الأقصى 100 حرف | الاسم الأول مطلوب |
| fatherName | اسم الأب | شرطي* | الحد الأقصى 100 حرف | اسم الأب مطلوب عند اختيار هوية وطنية |
| grandfatherName | اسم الجد | لا | الحد الأقصى 100 حرف | - |
| tribeName | اسم الفخذ | لا | الحد الأقصى 100 حرف | - |
| familyName | اسم العائلة | نعم | الحد الأقصى 100 حرف | اسم العائلة مطلوب |
| birthDate | تاريخ الميلاد | لا | لا يتجاوز اليوم | - |
| genderId | الجنس | نعم | 1=ذكر، 2=أنثى | الجنس مطلوب |
| nationalityId | الجنسية | نعم | - | الجنسية مطلوبة |
| mobileNumber | رقم الجوال | نعم | يبدأ بـ 05 + 8 أرقام | رقم الجوال مطلوب / رقم الجوال يجب أن يكون 10 أرقام ويبدأ بـ 05 |
| email | البريد الإلكتروني | لا | صيغة بريد صحيحة | صيغة البريد الإلكتروني غير صحيحة |

**\* شرطي:** اسم الأب مطلوب فقط عند اختيار نوع الهوية = هوية وطنية (1)

#### التبويب 2: بيانات مكان الإقامة

| الحقل | الاسم العربي | إجباري | قواعد التحقق | رسالة الخطأ |
|-------|-------------|--------|--------------|-------------|
| indRegionId | المنطقة | نعم | - | المنطقة مطلوبة |
| indCityId | المدينة | نعم | - | المدينة مطلوبة |
| indDistrict | الحي | لا | الحد الأقصى 100 حرف | - |
| indStreet | الشارع | لا | الحد الأقصى 200 حرف | - |
| indBuildingNumber | رقم المبنى | لا | 4 أرقام فقط | يجب إدخال 4 أرقام فقط |
| indUnitNumber | رقم الوحدة | لا | 4 أرقام فقط | يجب إدخال 4 أرقام فقط |
| indPostalCode | الرمز البريدي | لا | 5 أرقام فقط | يجب إدخال 5 أرقام فقط |
| indAdditionalCode | الرمز الإضافي | لا | 4 أرقام فقط | يجب إدخال 4 أرقام فقط |

#### التبويب 3: بيانات جهة العمل

| الحقل | الاسم العربي | إجباري | قواعد التحقق | رسالة الخطأ |
|-------|-------------|--------|--------------|-------------|
| employmentStatusId | حالة العمل | لا | 1=حكومي، 2=خاص، 3=بدون عمل | - |
| employer | جهة العمل | شرطي** | الحد الأقصى 200 حرف | جهة العمل مطلوبة |
| occupation | المهنة | شرطي** | الحد الأقصى 200 حرف | المهنة مطلوبة |

**\*\* شرطي:** جهة العمل والمهنة مطلوبة فقط عند اختيار حالة العمل = حكومي (1) أو خاص (2)

### 9.6 أوامر إنشاء المكونات

```bash
# الانتقال للمشروع
cd src/Frontend/bog-app

# إنشاء الوحدات
ng generate module features/case-registration/defendants --routing
ng generate module features/case-registration/case-data --routing
ng generate module features/case-registration/deficiencies --routing
ng generate module features/case-registration/request-management --routing

# مكونات المدعى عليهم
ng generate component features/case-registration/defendants/components/defendant-list
ng generate component features/case-registration/defendants/components/defendant-form
ng generate component features/case-registration/defendants/components/defendant-details

# مكونات بيانات الدعوى
ng generate component features/case-registration/case-data/components/case-data-form
ng generate component features/case-registration/case-data/components/additional-info
ng generate component features/case-registration/case-data/components/request-attachments

# مكونات النواقص
ng generate component features/case-registration/deficiencies/components/deficiency-list
ng generate component features/case-registration/deficiencies/components/deficiency-form
ng generate component features/case-registration/deficiencies/components/complete-deficiency

# مكونات إدارة الطلبات
ng generate component features/case-registration/request-management/components/request-search
ng generate component features/case-registration/request-management/components/request-details
ng generate component features/case-registration/request-management/components/submit-request
ng generate component features/case-registration/request-management/components/take-action-dialog
ng generate component features/case-registration/request-management/components/rejection-dialog

# المكونات المشتركة
ng generate component features/case-registration/shared/components/status-badge

# الخدمات
ng generate service features/case-registration/defendants/services/defendant
ng generate service features/case-registration/case-data/services/case-data
ng generate service features/case-registration/deficiencies/services/deficiency
ng generate service features/case-registration/request-management/services/case-registration
ng generate service features/case-registration/request-management/services/request-action
```

---

## المراجع

- **SRS:** `docs/srs/use-cases/6.5.1-case-registration/SRS_UC_6.5.1_Full_AR.md`
- **الخطة الرئيسية:** `docs/implementation/UC-6.5.1-Implementation-Plan.md`
- **مواصفات واجهة المستخدم:** `Ux/UC-6.5.1-UX-Specification.md`
