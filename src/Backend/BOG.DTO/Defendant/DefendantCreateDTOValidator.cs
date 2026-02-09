using FluentValidation;

namespace BOG.DTO.Defendant;

/// <summary>
/// Validator for DefendantCreateDTO.
/// </summary>
public class DefendantCreateDTOValidator : AbstractValidator<DefendantCreateDTO>
{
    // Defendant type constants
    private const int TypeIndividual = 1;
    private const int TypeRegisteredCompany = 2;
    private const int TypeGovernmentAgency = 3;
    private const int TypeUnregisteredCompany = 4;
    private const int TypeBusinessOwner = 5;
    private const int TypeNGO = 6;
    private const int TypeWaqf = 7;

    // Identity type constants
    private const int IdentityTypeNationalId = 1;
    private const int IdentityTypeResidentId = 2;

    public DefendantCreateDTOValidator()
    {
        // DefendantTypeId is always required
        RuleFor(x => x.DefendantTypeId)
            .GreaterThan(0)
            .WithMessage("نوع المدعى عليه مطلوب");

        // Individual (Type 1) validation
        When(x => x.DefendantTypeId == TypeIndividual, () =>
        {
            // Identity number (optional, but if provided must be 10 digits)
            RuleFor(x => x.IdentityNumber)
                .Matches(@"^[0-9]{10}$")
                .When(x => !string.IsNullOrEmpty(x.IdentityNumber))
                .WithMessage("رقم الهوية يجب أن يكون 10 أرقام");

            // First name (required)
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("الاسم الأول مطلوب")
                .MaximumLength(100)
                .WithMessage("الاسم الأول يجب ألا يتجاوز 100 حرف");

            // Father name (conditional - required if IdentityType = National ID)
            When(x => x.IdentityTypeId == IdentityTypeNationalId, () =>
            {
                RuleFor(x => x.FatherName)
                    .NotEmpty()
                    .WithMessage("اسم الأب مطلوب عند اختيار هوية وطنية")
                    .MaximumLength(100)
                    .WithMessage("اسم الأب يجب ألا يتجاوز 100 حرف");
            });

            // Father name (optional max length)
            RuleFor(x => x.FatherName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.FatherName))
                .WithMessage("اسم الأب يجب ألا يتجاوز 100 حرف");

            // Grandfather name (optional)
            RuleFor(x => x.GrandfatherName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.GrandfatherName))
                .WithMessage("اسم الجد يجب ألا يتجاوز 100 حرف");

            // Tribe name (optional)
            RuleFor(x => x.TribeName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.TribeName))
                .WithMessage("اسم الفخذ يجب ألا يتجاوز 100 حرف");

            // Family name (required)
            RuleFor(x => x.FamilyName)
                .NotEmpty()
                .WithMessage("اسم العائلة مطلوب")
                .MaximumLength(100)
                .WithMessage("اسم العائلة يجب ألا يتجاوز 100 حرف");

            // Birth date (optional, but cannot be in future)
            RuleFor(x => x.BirthDate)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .When(x => x.BirthDate.HasValue)
                .WithMessage("تاريخ الميلاد يجب ألا يكون في المستقبل");

            // Gender (required)
            RuleFor(x => x.GenderId)
                .NotNull()
                .WithMessage("الجنس مطلوب")
                .InclusiveBetween(1, 2)
                .WithMessage("الجنس يجب أن يكون ذكر أو أنثى");

            // Nationality (required)
            RuleFor(x => x.NationalityId)
                .NotNull()
                .WithMessage("الجنسية مطلوبة")
                .GreaterThan(0)
                .WithMessage("الجنسية مطلوبة");

            // Mobile number (required, 10 digits starting with 05)
            RuleFor(x => x.MobileNumber)
                .NotEmpty()
                .WithMessage("رقم الجوال مطلوب")
                .Matches(@"^05[0-9]{8}$")
                .WithMessage("رقم الجوال يجب أن يكون 10 أرقام ويبدأ بـ 05");

            // Email (optional, valid format)
            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("صيغة البريد الإلكتروني غير صحيحة");

            // Address fields (all optional, but if provided must match pattern)
            RuleFor(x => x.IndBuildingNumber)
                .Matches(@"^[0-9]{4}$")
                .When(x => !string.IsNullOrEmpty(x.IndBuildingNumber))
                .WithMessage("رقم المبنى يجب أن يكون 4 أرقام");

            RuleFor(x => x.IndUnitNumber)
                .Matches(@"^[0-9]{4}$")
                .When(x => !string.IsNullOrEmpty(x.IndUnitNumber))
                .WithMessage("رقم الوحدة يجب أن يكون 4 أرقام");

            RuleFor(x => x.IndPostalCode)
                .Matches(@"^[0-9]{5}$")
                .When(x => !string.IsNullOrEmpty(x.IndPostalCode))
                .WithMessage("الرمز البريدي يجب أن يكون 5 أرقام");

            RuleFor(x => x.IndAdditionalCode)
                .Matches(@"^[0-9]{4}$")
                .When(x => !string.IsNullOrEmpty(x.IndAdditionalCode))
                .WithMessage("الرمز الإضافي يجب أن يكون 4 أرقام");

            // Employer (conditional - shown only if EmploymentStatus = Government or Private)
            When(x => x.EmploymentStatusId == 1 || x.EmploymentStatusId == 2, () =>
            {
                RuleFor(x => x.Employer)
                    .MaximumLength(200)
                    .When(x => !string.IsNullOrEmpty(x.Employer))
                    .WithMessage("جهة العمل يجب ألا تتجاوز 200 حرف");

                RuleFor(x => x.Occupation)
                    .MaximumLength(200)
                    .When(x => !string.IsNullOrEmpty(x.Occupation))
                    .WithMessage("المهنة يجب ألا تتجاوز 200 حرف");
            });
        });

        // Registered Company (Type 2) validation
        When(x => x.DefendantTypeId == TypeRegisteredCompany, () =>
        {
            RuleFor(x => x.CommercialRegNumber)
                .NotEmpty()
                .WithMessage("رقم السجل التجاري مطلوب")
                .Matches(@"^[0-9]{10}$")
                .WithMessage("رقم السجل التجاري يجب أن يكون 10 أرقام");

            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .WithMessage("اسم الشركة مطلوب")
                .MaximumLength(200)
                .WithMessage("اسم الشركة يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.RegistrationStartDate)
                .NotNull()
                .WithMessage("تاريخ بداية السجل مطلوب")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("تاريخ بداية السجل يجب ألا يكون في المستقبل");

            RuleFor(x => x.RegistrationEndDate)
                .NotNull()
                .WithMessage("تاريخ نهاية السجل مطلوب")
                .GreaterThan(x => x.RegistrationStartDate ?? DateOnly.MinValue)
                .WithMessage("تاريخ نهاية السجل يجب أن يكون أكبر من تاريخ البداية");

            RuleFor(x => x.RegCompanyRegionId)
                .NotNull()
                .WithMessage("المنطقة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المنطقة مطلوبة");

            RuleFor(x => x.RegCompanyCityId)
                .NotNull()
                .WithMessage("المدينة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المدينة مطلوبة");

            RuleFor(x => x.RegCompanyDistrict)
                .NotEmpty()
                .WithMessage("الحي مطلوب")
                .MaximumLength(100)
                .WithMessage("الحي يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.RegCompanyStreet)
                .NotEmpty()
                .WithMessage("الشارع مطلوب")
                .MaximumLength(200)
                .WithMessage("الشارع يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.RegCompanyBuildingNumber)
                .NotEmpty()
                .WithMessage("رقم المبنى مطلوب")
                .Matches(@"^[0-9]{4}$")
                .WithMessage("رقم المبنى يجب أن يكون 4 أرقام");

            RuleFor(x => x.RegCompanyUnitNumber)
                .NotEmpty()
                .WithMessage("رقم الوحدة مطلوب")
                .Matches(@"^[0-9]+$")
                .WithMessage("رقم الوحدة يجب أن يكون أرقام فقط");

            RuleFor(x => x.RegCompanyPostalCode)
                .NotEmpty()
                .WithMessage("الرمز البريدي مطلوب")
                .Matches(@"^[0-9]{5}$")
                .WithMessage("الرمز البريدي يجب أن يكون 5 أرقام");

            RuleFor(x => x.RegCompanyAdditionalCode)
                .NotEmpty()
                .WithMessage("الرمز الإضافي مطلوب")
                .Matches(@"^[0-9]{4}$")
                .WithMessage("الرمز الإضافي يجب أن يكون 4 أرقام");
        });

        // Government Agency (Type 3) validation
        When(x => x.DefendantTypeId == TypeGovernmentAgency, () =>
        {
            RuleFor(x => x.GovernmentAgencyId)
                .NotNull()
                .WithMessage("الجهة الحكومية مطلوبة")
                .GreaterThan(0)
                .WithMessage("الجهة الحكومية مطلوبة");

            RuleFor(x => x.Headquarters)
                .NotEmpty()
                .WithMessage("المقر مطلوب")
                .MaximumLength(200)
                .WithMessage("المقر يجب ألا يتجاوز 200 حرف");
        });

        // Unregistered Company (Type 4) validation
        When(x => x.DefendantTypeId == TypeUnregisteredCompany, () =>
        {
            RuleFor(x => x.CommercialRegNumber)
                .NotEmpty()
                .WithMessage("رقم السجل التجاري مطلوب")
                .MaximumLength(20)
                .WithMessage("رقم السجل التجاري يجب ألا يتجاوز 20 حرف");

            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .WithMessage("اسم الشركة مطلوب")
                .MaximumLength(200)
                .WithMessage("اسم الشركة يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.CountryId)
                .NotNull()
                .WithMessage("الدولة مطلوبة")
                .GreaterThan(0)
                .WithMessage("الدولة مطلوبة");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("المدينة مطلوبة")
                .MaximumLength(100)
                .WithMessage("المدينة يجب ألا تتجاوز 100 حرف");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("الوصف التقريبي مطلوب")
                .MaximumLength(1000)
                .WithMessage("الوصف التقريبي يجب ألا يتجاوز 1000 حرف");
        });

        // Business Owner (Type 5) validation
        When(x => x.DefendantTypeId == TypeBusinessOwner, () =>
        {
            // Identity type (required)
            RuleFor(x => x.IdentityTypeId)
                .NotNull()
                .WithMessage("نوع الهوية مطلوب")
                .GreaterThan(0)
                .WithMessage("نوع الهوية مطلوب");

            // Identity number (required, 10 digits)
            RuleFor(x => x.IdentityNumber)
                .NotEmpty()
                .WithMessage("رقم الهوية مطلوب")
                .Matches(@"^[0-9]{10}$")
                .WithMessage("رقم الهوية يجب أن يكون 10 أرقام");

            // First name (required)
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("الاسم الأول مطلوب")
                .MaximumLength(100)
                .WithMessage("الاسم الأول يجب ألا يتجاوز 100 حرف");

            // Father name (required)
            RuleFor(x => x.FatherName)
                .NotEmpty()
                .WithMessage("اسم الأب مطلوب")
                .MaximumLength(100)
                .WithMessage("اسم الأب يجب ألا يتجاوز 100 حرف");

            // Family name (required)
            RuleFor(x => x.FamilyName)
                .NotEmpty()
                .WithMessage("اسم العائلة مطلوب")
                .MaximumLength(100)
                .WithMessage("اسم العائلة يجب ألا يتجاوز 100 حرف");

            // Birth date (required)
            RuleFor(x => x.BirthDate)
                .NotNull()
                .WithMessage("تاريخ الميلاد مطلوب")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("تاريخ الميلاد يجب ألا يكون في المستقبل");

            // Identity issue date (required)
            RuleFor(x => x.IdentityIssueDate)
                .NotNull()
                .WithMessage("تاريخ إصدار الهوية مطلوب")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("تاريخ إصدار الهوية يجب ألا يكون في المستقبل");

            // Identity expiry date (required)
            RuleFor(x => x.IdentityExpiryDate)
                .NotNull()
                .WithMessage("تاريخ انتهاء الهوية مطلوب")
                .GreaterThan(x => x.IdentityIssueDate ?? DateOnly.MinValue)
                .WithMessage("تاريخ انتهاء الهوية يجب أن يكون أكبر من تاريخ الإصدار");

            // Gender (required)
            RuleFor(x => x.GenderId)
                .NotNull()
                .WithMessage("الجنس مطلوب")
                .InclusiveBetween(1, 2)
                .WithMessage("الجنس يجب أن يكون ذكر أو أنثى");

            // Nationality (required)
            RuleFor(x => x.NationalityId)
                .NotNull()
                .WithMessage("الجنسية مطلوبة")
                .GreaterThan(0)
                .WithMessage("الجنسية مطلوبة");

            // Mobile number (required)
            RuleFor(x => x.MobileNumber)
                .NotEmpty()
                .WithMessage("رقم الجوال مطلوب")
                .Matches(@"^05[0-9]{8}$")
                .WithMessage("رقم الجوال يجب أن يكون 10 أرقام ويبدأ بـ 05");

            // Employer (required if employed - Government or Private)
            When(x => x.EmploymentStatusId == 1 || x.EmploymentStatusId == 2, () =>
            {
                RuleFor(x => x.Employer)
                    .NotEmpty()
                    .WithMessage("جهة العمل مطلوبة")
                    .MaximumLength(200)
                    .WithMessage("جهة العمل يجب ألا تتجاوز 200 حرف");

                RuleFor(x => x.Occupation)
                    .NotEmpty()
                    .WithMessage("المهنة مطلوبة")
                    .MaximumLength(200)
                    .WithMessage("المهنة يجب ألا تتجاوز 200 حرف");
            });

            // Email (optional, valid format)
            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("صيغة البريد الإلكتروني غير صحيحة");

            // Residence address fields (all required for Business Owner)
            RuleFor(x => x.IndRegionId)
                .NotNull()
                .WithMessage("المنطقة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المنطقة مطلوبة");

            RuleFor(x => x.IndCityId)
                .NotNull()
                .WithMessage("المدينة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المدينة مطلوبة");

            RuleFor(x => x.IndDistrict)
                .NotEmpty()
                .WithMessage("الحي مطلوب")
                .MaximumLength(100)
                .WithMessage("الحي يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.IndStreet)
                .NotEmpty()
                .WithMessage("الشارع مطلوب")
                .MaximumLength(200)
                .WithMessage("الشارع يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.IndBuildingNumber)
                .NotEmpty()
                .WithMessage("رقم المبنى مطلوب")
                .Matches(@"^[0-9]{4}$")
                .WithMessage("رقم المبنى يجب أن يكون 4 أرقام");

            RuleFor(x => x.IndUnitNumber)
                .NotEmpty()
                .WithMessage("رقم الوحدة مطلوب")
                .Matches(@"^[0-9]{4}$")
                .WithMessage("رقم الوحدة يجب أن يكون 4 أرقام");

            RuleFor(x => x.IndPostalCode)
                .NotEmpty()
                .WithMessage("الرمز البريدي مطلوب")
                .Matches(@"^[0-9]{5}$")
                .WithMessage("الرمز البريدي يجب أن يكون 5 أرقام");

            RuleFor(x => x.IndAdditionalCode)
                .NotEmpty()
                .WithMessage("الرمز الإضافي مطلوب")
                .Matches(@"^[0-9]{4}$")
                .WithMessage("الرمز الإضافي يجب أن يكون 4 أرقام");

            // Work address fields (optional, pattern validation)
            RuleFor(x => x.WorkBuildingNumber)
                .Matches(@"^[0-9]{4}$")
                .When(x => !string.IsNullOrEmpty(x.WorkBuildingNumber))
                .WithMessage("رقم المبنى يجب أن يكون 4 أرقام");

            RuleFor(x => x.WorkUnitNumber)
                .Matches(@"^[0-9]{4}$")
                .When(x => !string.IsNullOrEmpty(x.WorkUnitNumber))
                .WithMessage("رقم الوحدة يجب أن يكون 4 أرقام");

            RuleFor(x => x.WorkPostalCode)
                .Matches(@"^[0-9]{5}$")
                .When(x => !string.IsNullOrEmpty(x.WorkPostalCode))
                .WithMessage("الرمز البريدي يجب أن يكون 5 أرقام");

            RuleFor(x => x.WorkAdditionalCode)
                .Matches(@"^[0-9]{4}$")
                .When(x => !string.IsNullOrEmpty(x.WorkAdditionalCode))
                .WithMessage("الرمز الإضافي يجب أن يكون 4 أرقام");

            // Commercial registration (required for Business Owner)
            RuleFor(x => x.CommercialRegNumber)
                .NotEmpty()
                .WithMessage("رقم السجل التجاري مطلوب")
                .Matches(@"^[0-9]{10}$")
                .WithMessage("رقم السجل التجاري يجب أن يكون 10 أرقام");

            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .WithMessage("اسم المنشأة مطلوب")
                .MaximumLength(200)
                .WithMessage("اسم المنشأة يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.RegistrationStartDate)
                .NotNull()
                .WithMessage("تاريخ بداية السجل مطلوب")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("تاريخ بداية السجل يجب ألا يكون في المستقبل");

            RuleFor(x => x.RegistrationEndDate)
                .NotNull()
                .WithMessage("تاريخ نهاية السجل مطلوب")
                .GreaterThan(x => x.RegistrationStartDate ?? DateOnly.MinValue)
                .WithMessage("تاريخ نهاية السجل يجب أن يكون أكبر من تاريخ البداية");

            // Business address (required for Business Owner)
            RuleFor(x => x.RegCompanyRegionId)
                .NotNull()
                .WithMessage("المنطقة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المنطقة مطلوبة");

            RuleFor(x => x.RegCompanyCityId)
                .NotNull()
                .WithMessage("المدينة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المدينة مطلوبة");

            RuleFor(x => x.RegCompanyDistrict)
                .NotEmpty()
                .WithMessage("الحي مطلوب")
                .MaximumLength(100)
                .WithMessage("الحي يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.RegCompanyStreet)
                .NotEmpty()
                .WithMessage("الشارع مطلوب")
                .MaximumLength(200)
                .WithMessage("الشارع يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.RegCompanyBuildingNumber)
                .NotEmpty()
                .WithMessage("رقم المبنى مطلوب")
                .Matches(@"^[0-9]{4}$")
                .WithMessage("رقم المبنى يجب أن يكون 4 أرقام");

            RuleFor(x => x.RegCompanyUnitNumber)
                .NotEmpty()
                .WithMessage("رقم الوحدة مطلوب")
                .Matches(@"^[0-9]+$")
                .WithMessage("رقم الوحدة يجب أن يكون أرقام فقط");

            RuleFor(x => x.RegCompanyPostalCode)
                .NotEmpty()
                .WithMessage("الرمز البريدي مطلوب")
                .Matches(@"^[0-9]{5}$")
                .WithMessage("الرمز البريدي يجب أن يكون 5 أرقام");

            RuleFor(x => x.RegCompanyAdditionalCode)
                .NotEmpty()
                .WithMessage("الرمز الإضافي مطلوب")
                .Matches(@"^[0-9]{4}$")
                .WithMessage("الرمز الإضافي يجب أن يكون 4 أرقام");
        });

        // Waqf (Type 7) validation
        When(x => x.DefendantTypeId == TypeWaqf, () =>
        {
            RuleFor(x => x.WaqfName)
                .NotEmpty()
                .WithMessage("اسم الوقف مطلوب")
                .MaximumLength(200)
                .WithMessage("اسم الوقف يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.CourtDeedNumber)
                .NotEmpty()
                .WithMessage("رقم صك المحكمة مطلوب")
                .MaximumLength(10)
                .WithMessage("رقم صك المحكمة يجب ألا يتجاوز 10 أحرف");

            RuleFor(x => x.CourtDeedDate)
                .NotNull()
                .WithMessage("تاريخ صك المحكمة مطلوب")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("تاريخ صك المحكمة يجب ألا يكون في المستقبل");

            RuleFor(x => x.DeedSource)
                .NotEmpty()
                .WithMessage("مصدر الصك مطلوب")
                .MaximumLength(100)
                .WithMessage("مصدر الصك يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.WaqfSupervisoryTypeId)
                .NotNull()
                .WithMessage("نظارة الوقف مطلوبة")
                .InclusiveBetween(1, 2)
                .WithMessage("نظارة الوقف يجب أن تكون أهلية أو حكومية");

            RuleFor(x => x.WaqfRegionId)
                .NotNull()
                .WithMessage("المنطقة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المنطقة مطلوبة");

            RuleFor(x => x.WaqfCityId)
                .NotNull()
                .WithMessage("المدينة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المدينة مطلوبة");

            RuleFor(x => x.WaqfAddressDescription)
                .NotEmpty()
                .WithMessage("وصف تقريبي مطلوب");

            // Conditional: Agency name required if supervisory = حكومية (2)
            When(x => x.WaqfSupervisoryTypeId == 2, () =>
            {
                RuleFor(x => x.WaqfAgencyName)
                    .NotEmpty()
                    .WithMessage("اسم الجهة مطلوب عند اختيار نظارة حكومية")
                    .MaximumLength(200)
                    .WithMessage("اسم الجهة يجب ألا يتجاوز 200 حرف");
            });

            // Optional field validations
            RuleFor(x => x.WaqfBuildingNumber)
                .Matches(@"^[0-9]{4}$")
                .When(x => !string.IsNullOrEmpty(x.WaqfBuildingNumber))
                .WithMessage("رقم المبنى يجب أن يكون 4 أرقام");

            RuleFor(x => x.WaqfPostalCode)
                .Matches(@"^[0-9]{5}$")
                .When(x => !string.IsNullOrEmpty(x.WaqfPostalCode))
                .WithMessage("الرمز البريدي يجب أن يكون 5 أرقام");

            RuleFor(x => x.WaqfAdditionalCode)
                .Matches(@"^[0-9]{4}$")
                .When(x => !string.IsNullOrEmpty(x.WaqfAdditionalCode))
                .WithMessage("الرمز الإضافي يجب أن يكون 4 أرقام");
        });

        // NGO/Society (Type 6) validation
        When(x => x.DefendantTypeId == TypeNGO, () =>
        {
            RuleFor(x => x.LicenseNumber)
                .NotEmpty()
                .WithMessage("رقم الترخيص مطلوب")
                .Matches(@"^[0-9]{10}$")
                .WithMessage("رقم الترخيص يجب أن يكون 10 أرقام");

            RuleFor(x => x.LicenseSourceId)
                .NotNull()
                .WithMessage("مصدر الترخيص مطلوب")
                .GreaterThan(0)
                .WithMessage("مصدر الترخيص مطلوب");

            RuleFor(x => x.NGOName)
                .NotEmpty()
                .WithMessage("اسم الجمعية/المؤسسة مطلوب")
                .MaximumLength(200)
                .WithMessage("اسم الجمعية/المؤسسة يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.LicenseDate)
                .NotNull()
                .WithMessage("تاريخ الترخيص مطلوب");

            RuleFor(x => x.NGORegionId)
                .NotNull()
                .WithMessage("المنطقة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المنطقة مطلوبة");

            RuleFor(x => x.NGOCityId)
                .NotNull()
                .WithMessage("المدينة مطلوبة")
                .GreaterThan(0)
                .WithMessage("المدينة مطلوبة");

            RuleFor(x => x.NGODistrict)
                .NotEmpty()
                .WithMessage("الحي مطلوب")
                .MaximumLength(100)
                .WithMessage("الحي يجب ألا يتجاوز 100 حرف");

            RuleFor(x => x.NGOStreet)
                .NotEmpty()
                .WithMessage("الشارع مطلوب")
                .MaximumLength(200)
                .WithMessage("الشارع يجب ألا يتجاوز 200 حرف");

            RuleFor(x => x.NGOBuildingNumber)
                .NotEmpty()
                .WithMessage("رقم المبنى مطلوب")
                .Matches(@"^[0-9]{4}$")
                .WithMessage("رقم المبنى يجب أن يكون 4 أرقام");

            RuleFor(x => x.NGOUnitNumber)
                .NotEmpty()
                .WithMessage("رقم الوحدة مطلوب")
                .Matches(@"^[0-9]+$")
                .WithMessage("رقم الوحدة يجب أن يكون أرقام فقط");

            RuleFor(x => x.NGOPostalCode)
                .NotEmpty()
                .WithMessage("الرمز البريدي مطلوب")
                .Matches(@"^[0-9]{5}$")
                .WithMessage("الرمز البريدي يجب أن يكون 5 أرقام");

            RuleFor(x => x.NGOAdditionalCode)
                .NotEmpty()
                .WithMessage("الرمز الإضافي مطلوب")
                .Matches(@"^[0-9]{4}$")
                .WithMessage("الرمز الإضافي يجب أن يكون 4 أرقام");
        });

        // AdditionalStatement validation (optional but max 4000)
        RuleFor(x => x.AdditionalStatement)
            .MaximumLength(4000)
            .WithMessage("البيان الإضافي يجب ألا يتجاوز 4000 حرف");
    }
}
