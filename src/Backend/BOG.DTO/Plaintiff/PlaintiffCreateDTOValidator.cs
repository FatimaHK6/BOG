using FluentValidation;

namespace BOG.DTO.Plaintiff;

/// <summary>
/// FluentValidation validator for PlaintiffCreateDTO.
/// Implements conditional validation rules based on plaintiff type as per SRS UC 6.5.1.
/// Implements ERR014/ERR015 for identity validation and all field constraints.
/// </summary>
public class PlaintiffCreateDTOValidator : AbstractValidator<PlaintiffCreateDTO>
{
    // Plaintiff type constants - must match database PlaintiffTypes table (exactly 8 types per SRS Section 1.3)
    private const int TypeIndividual = 1;              // فرد
    private const int TypeIndividualWithoutId = 2;     // فرد بدون هوية
    private const int TypeBusinessOwner = 3;           // صاحب مؤسسة
    private const int TypeRegisteredCompany = 4;       // شركة مسجلة
    private const int TypeUnregisteredCompany = 5;     // شركة غير مسجلة
    private const int TypeGovernmentAgency = 6;        // جهة حكومية
    private const int TypeSociety = 7;                 // جمعية/مؤسسة أهلية
    private const int TypeWaqf = 8;                    // وقف

    // Identity type constants
    private const int IdentityTypeNationalId = 1;
    private const int IdentityTypeResidentId = 2;
    private const int IdentityTypePassport = 3;

    // Employment status constants (BC01/BC02)
    private const int EmploymentStatusGovernment = 1;
    private const int EmploymentStatusPrivate = 2;
    private const int EmploymentStatusUnemployed = 3;

    // Nationality constant for Saudi (for Nationality Rule)
    private const int SaudiNationalityId = 1; // Assuming Saudi = 1 in lookup table

    // Arabic-only regex pattern
    private const string ArabicOnlyPattern = @"^[\u0600-\u06FF\s\d\-\.]+$";

    public PlaintiffCreateDTOValidator()
    {
        // PlaintiffTypeId is always required (8 types only)
        RuleFor(x => x.PlaintiffTypeId)
            .InclusiveBetween(1, 8)
            .WithMessage("نوع المدعي يجب أن يكون بين 1 و 8");

        // Skip all other validation when saving as draft
        // Only PlaintiffTypeId is required for drafts
        When(x => x.IsDraft, () =>
        {
            // No additional validation for drafts - data is saved as-is
        });

        // ===== Type 1: Individual (فرد) - SRS 6.3.9 =====
        When(x => !x.IsDraft && x.PlaintiffTypeId == TypeIndividual, () =>
        {
            RuleFor(x => x.IdentityTypeId)
                .NotNull().WithMessage("نوع الهوية مطلوب للأفراد")
                .InclusiveBetween(1, 3).WithMessage("نوع الهوية غير صالح");

            RuleFor(x => x.IdentityNumber)
                .NotEmpty().WithMessage("رقم الهوية مطلوب")
                .MaximumLength(20).WithMessage("رقم الهوية يجب ألا يتجاوز 20 حرف")
                .Must((dto, identityNumber) => ValidateIdentityNumberFormat(identityNumber, dto.IdentityTypeId))
                .WithMessage((dto, _) => GetIdentityErrorMessage(dto.IdentityTypeId));

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("الاسم الأول مطلوب")
                .MaximumLength(100).WithMessage("الاسم الأول يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.FatherName)
                .NotEmpty().WithMessage("اسم الأب مطلوب")
                .MaximumLength(100).WithMessage("اسم الأب يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.GrandfatherName)
                .MaximumLength(100).WithMessage("اسم الجد يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.ClanName)
                .MaximumLength(100).WithMessage("اسم الفخذ يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.FamilyName)
                .NotEmpty().WithMessage("اسم العائلة مطلوب")
                .MaximumLength(100).WithMessage("اسم العائلة يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.BirthDate)
                .NotNull().WithMessage("تاريخ الميلاد مطلوب");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("الجنس مطلوب")
                .Must(g => g == "ذكر" || g == "أنثى" || g == "male" || g == "female")
                .WithMessage("الجنس يجب أن يكون 'ذكر' أو 'أنثى'");

            // Nationality Rule: Saudi must be set for National ID, excluded for Resident/Passport
            RuleFor(x => x.NationalityId)
                .NotNull().WithMessage("الجنسية مطلوبة")
                .Equal(SaudiNationalityId)
                .When(x => x.IdentityTypeId == IdentityTypeNationalId)
                .WithMessage("الجنسية يجب أن تكون سعودي لهوية وطنية");

            RuleFor(x => x.NationalityId)
                .NotEqual(SaudiNationalityId)
                .When(x => x.IdentityTypeId == IdentityTypeResidentId || x.IdentityTypeId == IdentityTypePassport)
                .WithMessage("لا يمكن اختيار الجنسية السعودية لهوية مقيم أو جواز سفر");

            RuleFor(x => x.IdentityIssueDate)
                .NotNull().WithMessage("تاريخ إصدار الهوية مطلوب");

            RuleFor(x => x.IdentityExpiryDate)
                .NotNull().WithMessage("تاريخ انتهاء الهوية مطلوب");

            RuleFor(x => x.MobileNumber)
                .NotEmpty().WithMessage("رقم الجوال مطلوب")
                .Matches(@"^05\d{8}$").WithMessage("رقم الجوال يجب أن يبدأ بـ 05 ويتكون من 10 أرقام");

            RuleFor(x => x.Email)
                .MaximumLength(255).WithMessage("البريد الإلكتروني يجب ألا يتجاوز 255 حرف")
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("البريد الإلكتروني غير صحيح");

            // Profession only required when employment status is Government(1) or Private(2)
            RuleFor(x => x.Profession)
                .NotEmpty()
                .When(x => x.EmploymentStatusId == EmploymentStatusGovernment || x.EmploymentStatusId == EmploymentStatusPrivate)
                .WithMessage("المهنة مطلوبة")
                .MaximumLength(200).WithMessage("المهنة يجب ألا يتجاوز 200 حرف");

            // Employment Status required for Individual
            RuleFor(x => x.EmploymentStatusId)
                .NotNull().WithMessage("حالة العمل مطلوبة")
                .InclusiveBetween(1, 3).WithMessage("حالة العمل غير صالحة");

            // BC01: Employer required when Government or Private, must be null when Unemployed
            RuleFor(x => x.Employer)
                .NotEmpty()
                .When(x => x.EmploymentStatusId == EmploymentStatusGovernment || x.EmploymentStatusId == EmploymentStatusPrivate)
                .WithMessage("جهة العمل مطلوبة عند اختيار حكومي أو خاص");

            RuleFor(x => x.Employer)
                .MaximumLength(200).WithMessage("جهة العمل يجب ألا يتجاوز 200 حرف");

            // BC02: Work Address optional - only shown for Private sector
            // RuleFor(x => x.WorkAddress)
            //     .NotNull()
            //     .When(x => x.EmploymentStatusId == EmploymentStatusPrivate)
            //     .WithMessage("عنوان العمل مطلوب للقطاع الخاص");

            // Residence Address is now optional (moved to Step 1)
            // RuleFor(x => x.ResidenceAddress)
            //     .NotNull().WithMessage("عنوان السكن مطلوب");
        });

        // ===== Type 2: Individual Without ID (فرد بدون هوية) - SRS Section 1.3 =====
        When(x => !x.IsDraft && x.PlaintiffTypeId == TypeIndividualWithoutId, () =>
        {
            // Name fields required but no identity validation
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("الاسم الأول مطلوب")
                .MaximumLength(100).WithMessage("الاسم الأول يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.FatherName)
                .NotEmpty().WithMessage("اسم الأب مطلوب")
                .MaximumLength(100).WithMessage("اسم الأب يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.FamilyName)
                .NotEmpty().WithMessage("اسم العائلة مطلوب")
                .MaximumLength(100).WithMessage("اسم العائلة يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.MobileNumber)
                .NotEmpty().WithMessage("رقم الجوال مطلوب")
                .Matches(@"^05\d{8}$").WithMessage("رقم الجوال يجب أن يبدأ بـ 05 ويتكون من 10 أرقام");
        });

        // ===== Type 3: Business Owner (صاحب مؤسسة) - SRS Section 1.3 =====
        When(x => !x.IsDraft && x.PlaintiffTypeId == TypeBusinessOwner, () =>
        {
            // Personal data required
            RuleFor(x => x.IdentityTypeId)
                .NotNull().WithMessage("نوع الهوية مطلوب")
                .InclusiveBetween(1, 3).WithMessage("نوع الهوية غير صالح");

            RuleFor(x => x.IdentityNumber)
                .NotEmpty().WithMessage("رقم الهوية مطلوب")
                .MaximumLength(20).WithMessage("رقم الهوية يجب ألا يتجاوز 20 حرف")
                .Must((dto, identityNumber) => ValidateIdentityNumberFormat(identityNumber, dto.IdentityTypeId))
                .WithMessage((dto, _) => GetIdentityErrorMessage(dto.IdentityTypeId));

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("الاسم الأول مطلوب")
                .MaximumLength(100).WithMessage("الاسم الأول يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.FatherName)
                .NotEmpty().WithMessage("اسم الأب مطلوب")
                .MaximumLength(100).WithMessage("اسم الأب يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.FamilyName)
                .NotEmpty().WithMessage("اسم العائلة مطلوب")
                .MaximumLength(100).WithMessage("اسم العائلة يجب ألا يتجاوز 100 حرف");

            // Nationality Rule
            RuleFor(x => x.NationalityId)
                .NotNull().WithMessage("الجنسية مطلوبة")
                .Equal(SaudiNationalityId)
                .When(x => x.IdentityTypeId == IdentityTypeNationalId)
                .WithMessage("الجنسية يجب أن تكون سعودي لهوية وطنية");

            RuleFor(x => x.IdentityIssueDate)
                .NotNull().WithMessage("تاريخ إصدار الهوية مطلوب");

            RuleFor(x => x.IdentityExpiryDate)
                .NotNull().WithMessage("تاريخ انتهاء الهوية مطلوب");

            RuleFor(x => x.MobileNumber)
                .NotEmpty().WithMessage("رقم الجوال مطلوب")
                .Matches(@"^05\d{8}$").WithMessage("رقم الجوال يجب أن يبدأ بـ 05 ويتكون من 10 أرقام");

            // Commercial registration required for Business Owner
            RuleFor(x => x.CommercialRegNumber)
                .NotEmpty().WithMessage("رقم السجل التجاري مطلوب")
                .Matches(@"^\d{10}$").WithMessage("رقم السجل التجاري يجب أن يكون 10 أرقام فقط");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("اسم المؤسسة مطلوب")
                .MaximumLength(200).WithMessage("اسم المؤسسة يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.CRStartDate)
                .NotNull().WithMessage("تاريخ بداية السجل التجاري مطلوب");

            RuleFor(x => x.CREndDate)
                .NotNull().WithMessage("تاريخ نهاية السجل التجاري مطلوب");
        });

        // ===== Type 4: Registered Company (شركة مسجلة) - SRS Section 1.3 =====
        When(x => !x.IsDraft && x.PlaintiffTypeId == TypeRegisteredCompany, () =>
        {
            RuleFor(x => x.CommercialRegNumber)
                .NotEmpty().WithMessage("رقم السجل التجاري مطلوب")
                .Matches(@"^\d{10}$").WithMessage("رقم السجل التجاري يجب أن يكون 10 أرقام فقط");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("اسم الشركة مطلوب")
                .MaximumLength(200).WithMessage("اسم الشركة يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.CRStartDate)
                .NotNull().WithMessage("تاريخ بداية السجل التجاري مطلوب");

            RuleFor(x => x.CREndDate)
                .NotNull().WithMessage("تاريخ نهاية السجل التجاري مطلوب");

            RuleFor(x => x.CompanyAddress)
                .NotNull().WithMessage("عنوان الشركة مطلوب");
        });

        // ===== Type 5: Unregistered Company (شركة غير مسجلة) - SRS Section 1.3 =====
        When(x => !x.IsDraft && x.PlaintiffTypeId == TypeUnregisteredCompany, () =>
        {
            RuleFor(x => x.CommercialRegNumber)
                .NotEmpty().WithMessage("رقم السجل التجاري مطلوب")
                .MaximumLength(20).WithMessage("رقم السجل التجاري يجب ألا يتجاوز 20 حرف");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("اسم الشركة مطلوب")
                .MaximumLength(200).WithMessage("اسم الشركة يجب ألا يتجاوز 200 حرف");

            // No contact information section for Unregistered Company
        });

        // ===== Type 6: Government Agency (جهة حكومية) - SRS Section 1.3 =====
        When(x => !x.IsDraft && x.PlaintiffTypeId == TypeGovernmentAgency, () =>
        {
            RuleFor(x => x.GovernmentAgencyId)
                .NotNull().WithMessage("الجهة الحكومية مطلوبة");

            RuleFor(x => x.Headquarters)
                .NotEmpty().WithMessage("المقر مطلوب")
                .MaximumLength(200).WithMessage("المقر يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.AdditionalStatement)
                .MaximumLength(4000).WithMessage("البيان الإضافي يجب ألا يتجاوز 4000 حرف");
        });

        // ===== Type 7: Society/NGO (جمعية/مؤسسة أهلية) - SRS Section 1.3 =====
        When(x => !x.IsDraft && x.PlaintiffTypeId == TypeSociety, () =>
        {
            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage("رقم الترخيص مطلوب")
                .Matches(@"^\d{10}$").WithMessage("رقم الترخيص يجب أن يكون 10 أرقام فقط");

            RuleFor(x => x.LicenseSourceId)
                .NotNull().WithMessage("مصدر الترخيص مطلوب");

            RuleFor(x => x.NGOName)
                .NotEmpty().WithMessage("اسم الجمعية/المؤسسة مطلوب")
                .MaximumLength(200).WithMessage("اسم الجمعية يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.LicenseDate)
                .NotNull().WithMessage("تاريخ الترخيص مطلوب");

            RuleFor(x => x.NGOAddress)
                .NotNull().WithMessage("عنوان الجمعية/المؤسسة مطلوب");
        });

        // ===== Type 8: Waqf (وقف) - SRS Section 1.3 =====
        When(x => !x.IsDraft && x.PlaintiffTypeId == TypeWaqf, () =>
        {
            RuleFor(x => x.CourtDeedNumber)
                .NotEmpty().WithMessage("رقم صك المحكمة مطلوب")
                .Matches(@"^\d{10}$").WithMessage("رقم صك المحكمة يجب أن يكون 10 أرقام فقط");

            RuleFor(x => x.WaqfName)
                .NotEmpty().WithMessage("اسم الوقف مطلوب")
                .MaximumLength(200).WithMessage("اسم الوقف يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.DeedDate)
                .NotNull().WithMessage("تاريخ صك المحكمة مطلوب");

            RuleFor(x => x.DeedSource)
                .NotEmpty().WithMessage("مصدر الصك مطلوب")
                .MaximumLength(100).WithMessage("مصدر الصك يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.WaqfOversightType)
                .NotEmpty().WithMessage("نظارة الوقف مطلوبة")
                .Must(x => x == "خاصة" || x == "حكومية" || x == "private" || x == "government")
                .WithMessage("نظارة الوقف يجب أن تكون 'خاصة' أو 'حكومية'");

            RuleFor(x => x.WaqfAddress)
                .NotNull().WithMessage("عنوان الوقف مطلوب");

            RuleFor(x => x.WaqfDescription)
                .NotEmpty().WithMessage("وصف الوقف مطلوب")
                .MaximumLength(200).WithMessage("وصف الوقف يجب ألا يتجاوز 200 حرف");

            // Conditional: WaqfAgencyName required when oversight is حكومية
            When(x => x.WaqfOversightType == "حكومية", () =>
            {
                RuleFor(x => x.WaqfAgencyName)
                    .NotEmpty().WithMessage("اسم الجهة مطلوب عندما تكون النظارة حكومية")
                    .MaximumLength(200).WithMessage("اسم الجهة يجب ألا يتجاوز 200 حرف");
            });
        });

        // ===== Global Date Validations (only when not draft) =====
        When(x => !x.IsDraft, () =>
        {
            RuleFor(x => x.BirthDate)
                .Must(date => !date.HasValue || date.Value <= DateTime.Today)
                .WithMessage("تاريخ الميلاد لا يمكن أن يكون في المستقبل");

            RuleFor(x => x.IdentityIssueDate)
                .Must(date => !date.HasValue || date.Value <= DateTime.Today)
                .WithMessage("تاريخ إصدار الهوية لا يمكن أن يكون في المستقبل");

            // Identity Expiry Date must be after Issue Date
            RuleFor(x => x.IdentityExpiryDate)
                .Must((dto, expiryDate) => !expiryDate.HasValue || !dto.IdentityIssueDate.HasValue ||
                      expiryDate.Value > dto.IdentityIssueDate.Value)
                .WithMessage("تاريخ انتهاء الهوية يجب أن يكون أكبر من تاريخ الإصدار");

            RuleFor(x => x.CRStartDate)
                .Must(date => !date.HasValue || date.Value <= DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("تاريخ بداية السجل التجاري لا يمكن أن يكون في المستقبل");

            RuleFor(x => x.CREndDate)
                .Must((dto, endDate) => !endDate.HasValue || !dto.CRStartDate.HasValue || endDate.Value > dto.CRStartDate.Value)
                .WithMessage("تاريخ نهاية السجل يجب أن يكون أكبر من تاريخ البداية");

            RuleFor(x => x.DeedDate)
                .Must(date => !date.HasValue || date.Value <= DateTime.Today)
                .WithMessage("تاريخ صك المحكمة لا يمكن أن يكون في المستقبل");

            RuleFor(x => x.LicenseDate)
                .Must(date => !date.HasValue || date.Value <= DateTime.Today)
                .WithMessage("تاريخ الترخيص لا يمكن أن يكون في المستقبل");
        });
    }

    /// <summary>
    /// Validates identity number format based on identity type.
    /// ERR014: National ID must start with 1 (10 digits total).
    /// ERR015: Resident ID must start with 2 (10 digits total).
    /// Passport: up to 20 characters.
    /// </summary>
    private static bool ValidateIdentityNumberFormat(string? identityNumber, int? identityTypeId)
    {
        if (string.IsNullOrEmpty(identityNumber))
            return true; // Let NotEmpty handle this

        return identityTypeId switch
        {
            IdentityTypeNationalId => System.Text.RegularExpressions.Regex.IsMatch(identityNumber, @"^1\d{9}$"),
            IdentityTypeResidentId => System.Text.RegularExpressions.Regex.IsMatch(identityNumber, @"^2\d{9}$"),
            IdentityTypePassport => identityNumber.Length <= 20,
            _ => true
        };
    }

    /// <summary>
    /// Gets the appropriate error message based on identity type (ERR014/ERR015).
    /// </summary>
    private static string GetIdentityErrorMessage(int? identityTypeId)
    {
        return identityTypeId switch
        {
            IdentityTypeNationalId => "الرقم المدخل ليس هوية مواطن - يجب أن يبدأ بـ 1 ويتكون من 10 أرقام", // ERR014
            IdentityTypeResidentId => "الرقم المدخل ليس هوية مقيم - يجب أن يبدأ بـ 2 ويتكون من 10 أرقام", // ERR015
            IdentityTypePassport => "رقم جواز السفر يجب ألا يتجاوز 20 حرف",
            _ => "رقم الهوية غير صالح"
        };
    }
}
