namespace BOG.VM.Representative;

/// <summary>
/// Representative ViewModel for presentation layer.
/// </summary>
public class RepresentativeVM
{
    /// <summary>
    /// Representative ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Plaintiff ID.
    /// </summary>
    public int PlaintiffId { get; set; }

    /// <summary>
    /// Representative type ID.
    /// </summary>
    public int RepresentativeTypeId { get; set; }

    /// <summary>
    /// Representative type name (Arabic).
    /// </summary>
    public string RepresentativeTypeNameAr { get; set; } = null!;

    /// <summary>
    /// Representative type name (English).
    /// </summary>
    public string RepresentativeTypeName { get; set; } = null!;

    #region Personal Data

    /// <summary>
    /// Identity type ID.
    /// </summary>
    public int IdentityTypeId { get; set; }

    /// <summary>
    /// Identity type name.
    /// </summary>
    public string IdentityTypeName { get; set; } = null!;

    /// <summary>
    /// Identity number.
    /// </summary>
    public string IdentityNumber { get; set; } = null!;

    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Father's name.
    /// </summary>
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather's name.
    /// </summary>
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Family name.
    /// </summary>
    public string FamilyName { get; set; } = null!;

    /// <summary>
    /// Full name (computed).
    /// </summary>
    public string FullName
    {
        get
        {
            var nameParts = new[] { FirstName, FatherName, GrandfatherName, FamilyName }
                .Where(n => !string.IsNullOrWhiteSpace(n));
            return string.Join(" ", nameParts);
        }
    }

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Clan name (اسم الفخذ).
    /// </summary>
    public string? ClanName { get; set; }

    /// <summary>
    /// Gender (الجنس).
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Nationality ID (الجنسية).
    /// </summary>
    public int? NationalityId { get; set; }

    /// <summary>
    /// Nationality name.
    /// </summary>
    public string? NationalityName { get; set; }

    /// <summary>
    /// Identity issue date (تاريخ إصدار الهوية).
    /// </summary>
    public DateTime? IdentityIssueDate { get; set; }

    /// <summary>
    /// Identity expiry date (تاريخ انتهاء الهوية).
    /// </summary>
    public DateTime? IdentityExpiryDate { get; set; }

    #endregion

    #region Data Source

    /// <summary>
    /// Data source ID (1=FromAbsher, 2=FromUser).
    /// </summary>
    public int? DataSourceId { get; set; }

    /// <summary>
    /// Data source name.
    /// </summary>
    public string? DataSourceName { get; set; }

    /// <summary>
    /// Whether data is from Absher (computed).
    /// </summary>
    public bool IsFromAbsher => DataSourceId == 1;

    #endregion

    #region Residence Address (عنوان السكن - 6.3.2)

    /// <summary>
    /// Region ID for residence address.
    /// </summary>
    public int? ResidenceRegionId { get; set; }

    /// <summary>
    /// Region name for residence address.
    /// </summary>
    public string? ResidenceRegionName { get; set; }

    /// <summary>
    /// City ID for residence address.
    /// </summary>
    public int? ResidenceCityId { get; set; }

    /// <summary>
    /// City name for residence address.
    /// </summary>
    public string? ResidenceCityName { get; set; }

    /// <summary>
    /// District for residence address.
    /// </summary>
    public string? ResidenceDistrict { get; set; }

    /// <summary>
    /// Street for residence address.
    /// </summary>
    public string? ResidenceStreet { get; set; }

    /// <summary>
    /// Building number for residence address.
    /// </summary>
    public string? ResidenceBuildingNumber { get; set; }

    /// <summary>
    /// Unit number for residence address.
    /// </summary>
    public string? ResidenceUnitNumber { get; set; }

    /// <summary>
    /// Postal code for residence address.
    /// </summary>
    public string? ResidencePostalCode { get; set; }

    /// <summary>
    /// Additional code for residence address.
    /// </summary>
    public string? ResidenceAdditionalCode { get; set; }

    #endregion

    #region Employment Data (بيانات العمل)

    /// <summary>
    /// Employment status (حالة العمل): government/private/unemployed.
    /// </summary>
    public string? EmploymentStatus { get; set; }

    /// <summary>
    /// Employer name (جهة العمل).
    /// </summary>
    public string? Employer { get; set; }

    /// <summary>
    /// Profession (المهنة).
    /// </summary>
    public string? Profession { get; set; }

    #endregion

    #region Work Address (عنوان العمل - 6.3.2)

    /// <summary>
    /// Region ID for work address.
    /// </summary>
    public int? WorkRegionId { get; set; }

    /// <summary>
    /// Region name for work address.
    /// </summary>
    public string? WorkRegionName { get; set; }

    /// <summary>
    /// City ID for work address.
    /// </summary>
    public int? WorkCityId { get; set; }

    /// <summary>
    /// City name for work address.
    /// </summary>
    public string? WorkCityName { get; set; }

    /// <summary>
    /// District for work address.
    /// </summary>
    public string? WorkDistrict { get; set; }

    /// <summary>
    /// Street for work address.
    /// </summary>
    public string? WorkStreet { get; set; }

    /// <summary>
    /// Building number for work address.
    /// </summary>
    public string? WorkBuildingNumber { get; set; }

    /// <summary>
    /// Unit number for work address.
    /// </summary>
    public string? WorkUnitNumber { get; set; }

    /// <summary>
    /// Postal code for work address.
    /// </summary>
    public string? WorkPostalCode { get; set; }

    /// <summary>
    /// Additional code for work address.
    /// </summary>
    public string? WorkAdditionalCode { get; set; }

    #endregion

    #region Contact Info

    /// <summary>
    /// Mobile number.
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    #endregion

    #region Authorization Document

    /// <summary>
    /// Authorization/Power of Attorney number.
    /// </summary>
    public string? AuthorizationNumber { get; set; }

    /// <summary>
    /// Authorization date.
    /// </summary>
    public DateTime? AuthorizationDate { get; set; }

    /// <summary>
    /// Authorization source/issuer.
    /// </summary>
    public string? AuthorizationSource { get; set; }

    /// <summary>
    /// Authorization source type.
    /// </summary>
    public string? AuthorizationSourceType { get; set; }

    #endregion

    #region Liquidator Data (مصفي - 6.3.12)

    /// <summary>
    /// Decision number (رقم القرار) for Liquidator.
    /// </summary>
    public string? DecisionNumber { get; set; }

    /// <summary>
    /// Decision date (تاريخ القرار) for Liquidator.
    /// </summary>
    public DateTime? DecisionDate { get; set; }

    /// <summary>
    /// Decision source (مصدر القرار) for Liquidator.
    /// </summary>
    public string? DecisionSource { get; set; }

    #endregion

    #region Guardian Data (ولي - 6.3.16)

    /// <summary>
    /// Deed number (رقم الصك) for Guardian.
    /// </summary>
    public string? DeedNumber { get; set; }

    /// <summary>
    /// Deed date (تاريخ الصك) for Guardian.
    /// </summary>
    public DateTime? DeedDate { get; set; }

    /// <summary>
    /// Deed source (مصدر الصك) for Guardian.
    /// </summary>
    public string? DeedSource { get; set; }

    /// <summary>
    /// Guardianship type for Guardian (ولي).
    /// </summary>
    public string? GuardianshipType { get; set; }

    #endregion

    /// <summary>
    /// Creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}
