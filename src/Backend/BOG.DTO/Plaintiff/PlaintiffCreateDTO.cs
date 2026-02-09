using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BOG.DTO.Common;
using BOG.DTO.Representative;

namespace BOG.DTO.Plaintiff;

/// <summary>
/// DTO for creating a new plaintiff.
/// Fields are type-specific based on PlaintiffTypeId.
/// Based on SRS sections 6.3.1 - 6.3.11.
/// </summary>
public class PlaintiffCreateDTO
{
    /// <summary>
    /// Type of plaintiff (required):
    /// 1-Individual, 2-IndividualNoId, 3-BusinessOwner,
    /// 4-RegisteredCompany, 5-UnregisteredCompany, 6-GovernmentAgency, 7-NGO, 8-Waqf.
    /// </summary>
    [Required(ErrorMessage = "نوع المدعي مطلوب")]
    public int PlaintiffTypeId { get; set; }

    #region Personal Data (6.3.1) - for Individual types: 1, 2, 3

    /// <summary>
    /// Identity type ID: 1-National ID, 2-Resident ID, 3-Passport.
    /// Required for type 1 (Individual).
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number (رقم الهوية) - 10 digits for National/Resident, up to 20 for Passport.
    /// </summary>
    [StringLength(20, ErrorMessage = "رقم الهوية يجب ألا يتجاوز 20 حرف")]
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// First name (الاسم الأول).
    /// </summary>
    [StringLength(100, ErrorMessage = "الاسم الأول يجب ألا يتجاوز 100 حرف")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Father's name (اسم الأب).
    /// </summary>
    [StringLength(100, ErrorMessage = "اسم الأب يجب ألا يتجاوز 100 حرف")]
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather's name (اسم الجد).
    /// </summary>
    [StringLength(100, ErrorMessage = "اسم الجد يجب ألا يتجاوز 100 حرف")]
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Clan name (اسم الفخذ) - SRS 6.3.1.
    /// </summary>
    [StringLength(100, ErrorMessage = "اسم الفخذ يجب ألا يتجاوز 100 حرف")]
    public string? ClanName { get; set; }

    /// <summary>
    /// Family name (اسم العائلة).
    /// </summary>
    [StringLength(100, ErrorMessage = "اسم العائلة يجب ألا يتجاوز 100 حرف")]
    public string? FamilyName { get; set; }

    /// <summary>
    /// Date of birth - cannot be in the future.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Gender (ذكر/أنثى).
    /// </summary>
    [StringLength(10)]
    public string? Gender { get; set; }

    /// <summary>
    /// Nationality ID.
    /// </summary>
    public int? NationalityId { get; set; }

    /// <summary>
    /// Identity issue date (تاريخ إصدار الهوية) - SRS 6.3.1.
    /// </summary>
    public DateTime? IdentityIssueDate { get; set; }

    /// <summary>
    /// Identity expiry date (تاريخ انتهاء الهوية) - SRS 6.3.1.
    /// </summary>
    public DateTime? IdentityExpiryDate { get; set; }

    /// <summary>
    /// Document number (رقم الوثيقة) - for Type 2 (Individual without ID).
    /// Optional field, max 20 characters. SRS 6.3.10.
    /// </summary>
    [StringLength(20, ErrorMessage = "رقم الوثيقة يجب ألا يتجاوز 20 حرف")]
    public string? DocumentNumber { get; set; }

    #endregion

    #region Contact Info (بيانات التواصل)

    /// <summary>
    /// Mobile number (10 digits, starts with 05).
    /// </summary>
    [StringLength(10, ErrorMessage = "رقم الجوال يجب أن يكون 10 أرقام")]
    [RegularExpression(@"^05\d{8}$", ErrorMessage = "رقم الجوال يجب أن يبدأ بـ 05 ويتكون من 10 أرقام")]
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address (optional - validated by FluentValidation only when provided).
    /// </summary>
    [StringLength(255)]
    public string? Email { get; set; }

    #endregion

    #region Employment Data (6.3.9) - for Individual types

    /// <summary>
    /// Employment status ID (حالة العمل):
    /// 1-Government (حكومي), 2-Private (خاص), 3-Unemployed (بدون عمل).
    /// Required for type 1 (Individual) and type 3 (Business Owner).
    /// </summary>
    public int? EmploymentStatusId { get; set; }

    /// <summary>
    /// Employer name (جهة العمل) - max 200 characters.
    /// BC01: Required when EmploymentStatusId is 1 or 2, hidden when 3.
    /// </summary>
    [StringLength(200, ErrorMessage = "جهة العمل يجب ألا يتجاوز 200 حرف")]
    public string? Employer { get; set; }

    /// <summary>
    /// Profession/occupation (المهنة) - max 200 characters.
    /// Required for type 1 (Individual) and type 3 (Business Owner).
    /// </summary>
    [StringLength(200, ErrorMessage = "المهنة يجب ألا يتجاوز 200 حرف")]
    public string? Profession { get; set; }

    #endregion

    #region Addresses (6.3.2)

    /// <summary>
    /// Residence address (عنوان السكن) - required for individuals.
    /// </summary>
    public AddressCreateDTO? ResidenceAddress { get; set; }

    /// <summary>
    /// Work address (عنوان العمل) - optional for individuals.
    /// </summary>
    public AddressCreateDTO? WorkAddress { get; set; }

    /// <summary>
    /// Business/Institution address (عنوان المؤسسة) - for type 3 (Business Owner).
    /// </summary>
    public AddressCreateDTO? BusinessAddress { get; set; }

    /// <summary>
    /// Company address (عنوان الشركة) - for type 4 (Registered Company).
    /// </summary>
    public AddressCreateDTO? CompanyAddress { get; set; }

    /// <summary>
    /// NGO address (عنوان الجمعية) - for type 7 (NGO).
    /// </summary>
    public AddressCreateDTO? NGOAddress { get; set; }

    /// <summary>
    /// Waqf address (عنوان الوقف) - for type 8 (Waqf).
    /// </summary>
    public AddressCreateDTO? WaqfAddress { get; set; }

    /// <summary>
    /// Selected/Custom address for correspondence (العنوان المختار).
    /// </summary>
    public AddressCreateDTO? SelectedAddress { get; set; }

    #endregion

    #region Additional Data (بيانات إضافية)

    /// <summary>
    /// Whether this plaintiff is the applicant (تحديد كمقدم طلب).
    /// </summary>
    public bool IsApplicant { get; set; }

    /// <summary>
    /// Whether this plaintiff is saved as a draft (حفظ كمسودة).
    /// When true, validation rules are relaxed - only PlaintiffTypeId is required.
    /// </summary>
    public bool IsDraft { get; set; }

    #endregion

    #region Commercial Registration Data (6.3.3) - for types: 3, 4, 5

    /// <summary>
    /// Commercial registration number.
    /// 10 digits for registered companies (types 3, 4).
    /// Up to 20 characters for unregistered companies (type 5).
    /// </summary>
    [StringLength(20, ErrorMessage = "رقم السجل التجاري يجب ألا يتجاوز 20 حرف")]
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// Company/Institution name (اسم المؤسسة/الشركة) - max 200 Arabic characters.
    /// </summary>
    [StringLength(200, ErrorMessage = "اسم الشركة يجب ألا يتجاوز 200 حرف")]
    public string? CompanyName { get; set; }

    /// <summary>
    /// Commercial registration start date (تاريخ بداية السجل) - SRS 6.3.3.
    /// Must be <= today.
    /// </summary>
    [JsonPropertyName("crStartDate")]
    public DateOnly? CRStartDate { get; set; }

    /// <summary>
    /// Commercial registration end date (تاريخ نهاية السجل) - SRS 6.3.3.
    /// Must be > CRStartDate.
    /// </summary>
    [JsonPropertyName("crEndDate")]
    public DateOnly? CREndDate { get; set; }

    #endregion

    #region Unregistered Company Data (6.3.7) - for type 5

    /// <summary>
    /// Company address text (عنوان الشركة) - for unregistered company.
    /// </summary>
    [StringLength(500, ErrorMessage = "عنوان الشركة يجب ألا يتجاوز 500 حرف")]
    public string? UnregisteredCompanyAddress { get; set; }

    /// <summary>
    /// Country ID (الدولة) - from database.
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// City name text (المدينة) - max 100 characters.
    /// </summary>
    [StringLength(100, ErrorMessage = "المدينة يجب ألا يتجاوز 100 حرف")]
    public string? UnregisteredCompanyCity { get; set; }

    /// <summary>
    /// Approximate description (وصف تقريبي) - max 1000 characters.
    /// </summary>
    [StringLength(1000, ErrorMessage = "الوصف يجب ألا يتجاوز 1000 حرف")]
    public string? Description { get; set; }

    #endregion

    #region Government Agency Data (6.3.5) - for type 6

    /// <summary>
    /// Government agency ID (الجهة) - from database.
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Headquarters (المقر) - auto-filled based on agency.
    /// </summary>
    [StringLength(200, ErrorMessage = "المقر يجب ألا يتجاوز 200 حرف")]
    public string? Headquarters { get; set; }

    /// <summary>
    /// Additional statement (بيان إضافي) - max 4000 characters.
    /// </summary>
    [StringLength(4000, ErrorMessage = "البيان الإضافي يجب ألا يتجاوز 4000 حرف")]
    public string? AdditionalStatement { get; set; }

    #endregion

    #region Society/NGO Data (6.3.4) - for type 7

    /// <summary>
    /// License number (رقم الترخيص) - exactly 10 digits.
    /// </summary>
    [StringLength(10, ErrorMessage = "رقم الترخيص يجب أن يكون 10 أرقام")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "رقم الترخيص يجب أن يكون 10 أرقام فقط")]
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// License source ID (مصدر الترخيص) - from database lookup.
    /// </summary>
    public int? LicenseSourceId { get; set; }

    /// <summary>
    /// NGO name (اسم الجمعية/المؤسسة) - max 200 Arabic characters.
    /// </summary>
    [StringLength(200, ErrorMessage = "اسم الجمعية يجب ألا يتجاوز 200 حرف")]
    public string? NGOName { get; set; }

    /// <summary>
    /// License date (تاريخ الترخيص).
    /// </summary>
    public DateTime? LicenseDate { get; set; }

    #endregion

    #region Waqf Data (6.3.11) - for type 8

    /// <summary>
    /// Court deed number (رقم صك المحكمة) - exactly 10 digits.
    /// </summary>
    [StringLength(10, ErrorMessage = "رقم الصك يجب أن يكون 10 أرقام")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "رقم صك المحكمة يجب أن يكون 10 أرقام فقط")]
    public string? CourtDeedNumber { get; set; }

    /// <summary>
    /// Waqf name (اسم الوقف) - max 200 Arabic characters.
    /// </summary>
    [StringLength(200, ErrorMessage = "اسم الوقف يجب ألا يتجاوز 200 حرف")]
    public string? WaqfName { get; set; }

    /// <summary>
    /// Deed date (تاريخ صك المحكمة) - must be <= today.
    /// </summary>
    public DateTime? DeedDate { get; set; }

    /// <summary>
    /// Deed source/issuer (مصدر الصك) - max 100 characters.
    /// </summary>
    [StringLength(100, ErrorMessage = "مصدر الصك يجب ألا يتجاوز 100 حرف")]
    public string? DeedSource { get; set; }

    /// <summary>
    /// Waqf oversight type (نظارة الوقف) - خاصة/حكومية.
    /// </summary>
    [StringLength(20)]
    public string? WaqfOversightType { get; set; }

    /// <summary>
    /// Agency name (اسم الجهة) - required if WaqfOversightType is حكومية.
    /// Max 200 characters.
    /// </summary>
    [StringLength(200, ErrorMessage = "اسم الجهة يجب ألا يتجاوز 200 حرف")]
    public string? WaqfAgencyName { get; set; }

    /// <summary>
    /// Waqf description (وصف تقريبي) - max 200 characters.
    /// </summary>
    [StringLength(200, ErrorMessage = "وصف الوقف يجب ألا يتجاوز 200 حرف")]
    public string? WaqfDescription { get; set; }

    #endregion

    #region Representatives

    /// <summary>
    /// List of representatives to create with the plaintiff.
    /// </summary>
    public List<RepresentativeCreateDTO>? Representatives { get; set; }

    #endregion
}
