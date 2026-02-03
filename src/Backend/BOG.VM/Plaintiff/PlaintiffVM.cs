using BOG.VM.Common;
using BOG.VM.Representative;

namespace BOG.VM.Plaintiff;

/// <summary>
/// Plaintiff ViewModel for full details presentation.
/// </summary>
public class PlaintiffVM
{
    /// <summary>
    /// Plaintiff ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Plaintiff type ID.
    /// </summary>
    public int PlaintiffTypeId { get; set; }

    /// <summary>
    /// Plaintiff type name (English).
    /// </summary>
    public string PlaintiffTypeName { get; set; } = null!;

    /// <summary>
    /// Plaintiff type name (Arabic).
    /// </summary>
    public string PlaintiffTypeNameAr { get; set; } = null!;

    #region Personal Data

    /// <summary>
    /// Identity type ID.
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity type name.
    /// </summary>
    public string? IdentityTypeName { get; set; }

    /// <summary>
    /// Identity number.
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// First name (الاسم الأول).
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Father's name (اسم الأب).
    /// </summary>
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather's name (اسم الجد).
    /// </summary>
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Clan name (اسم الفخذ).
    /// </summary>
    public string? ClanName { get; set; }

    /// <summary>
    /// Family name (اسم العائلة).
    /// </summary>
    public string? FamilyName { get; set; }

    /// <summary>
    /// Full name (computed).
    /// </summary>
    public string FullName
    {
        get
        {
            var nameParts = new[] { FirstName, FatherName, GrandfatherName, ClanName, FamilyName }
                .Where(n => !string.IsNullOrWhiteSpace(n));
            return string.Join(" ", nameParts);
        }
    }

    /// <summary>
    /// Nationality ID.
    /// </summary>
    public int? NationalityId { get; set; }

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Gender.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Nationality name.
    /// </summary>
    public string? NationalityName { get; set; }

    /// <summary>
    /// Identity issue date.
    /// </summary>
    public DateTime? IdentityIssueDate { get; set; }

    /// <summary>
    /// Identity expiry date.
    /// </summary>
    public DateTime? IdentityExpiryDate { get; set; }

    /// <summary>
    /// Document number for individual without ID (رقم الوثيقة).
    /// </summary>
    public string? DocumentNumber { get; set; }

    #endregion

    #region Employment Data

    /// <summary>
    /// Employment status ID (1=Government, 2=Private, 3=Unemployed).
    /// </summary>
    public int? EmploymentStatusId { get; set; }

    /// <summary>
    /// Employer name (جهة العمل).
    /// </summary>
    public string? Employer { get; set; }

    /// <summary>
    /// Profession/occupation (المهنة).
    /// </summary>
    public string? Profession { get; set; }

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

    #region Business/Company

    /// <summary>
    /// Commercial registration number.
    /// </summary>
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// Company name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Commercial registration start date.
    /// </summary>
    public DateOnly? CRStartDate { get; set; }

    /// <summary>
    /// Commercial registration end date.
    /// </summary>
    public DateOnly? CREndDate { get; set; }

    #endregion

    #region Unregistered Company (Type 5)

    /// <summary>
    /// Company address for unregistered company.
    /// </summary>
    public string? UnregisteredCompanyAddress { get; set; }

    /// <summary>
    /// Country ID for unregistered company.
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Country name for unregistered company.
    /// </summary>
    public string? CountryName { get; set; }

    /// <summary>
    /// City for unregistered company.
    /// </summary>
    public string? UnregisteredCompanyCity { get; set; }

    /// <summary>
    /// Description for unregistered company.
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Government Agency (Type 6)

    /// <summary>
    /// Government agency ID.
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Government agency name.
    /// </summary>
    public string? GovernmentAgencyName { get; set; }

    /// <summary>
    /// Headquarters (المقر).
    /// </summary>
    public string? Headquarters { get; set; }

    /// <summary>
    /// Additional statement.
    /// </summary>
    public string? AdditionalStatement { get; set; }

    #endregion

    #region NGO Data (Type 7)

    /// <summary>
    /// License number.
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// License source ID.
    /// </summary>
    public int? LicenseSourceId { get; set; }

    /// <summary>
    /// License source name.
    /// </summary>
    public string? LicenseSourceName { get; set; }

    /// <summary>
    /// NGO name (اسم الجمعية).
    /// </summary>
    public string? NGOName { get; set; }

    /// <summary>
    /// License date.
    /// </summary>
    public DateTime? LicenseDate { get; set; }

    #endregion

    #region Waqf Data (Type 8)

    /// <summary>
    /// Court deed number.
    /// </summary>
    public string? CourtDeedNumber { get; set; }

    /// <summary>
    /// Waqf name (اسم الوقف).
    /// </summary>
    public string? WaqfName { get; set; }

    /// <summary>
    /// Deed date.
    /// </summary>
    public DateTime? DeedDate { get; set; }

    /// <summary>
    /// Deed source.
    /// </summary>
    public string? DeedSource { get; set; }

    /// <summary>
    /// Waqf oversight type (خاصة/حكومية).
    /// </summary>
    public string? WaqfOversightType { get; set; }

    /// <summary>
    /// Waqf agency name (for government oversight).
    /// </summary>
    public string? WaqfAgencyName { get; set; }

    /// <summary>
    /// Waqf description.
    /// </summary>
    public string? WaqfDescription { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Whether this plaintiff is the applicant.
    /// </summary>
    public bool IsApplicant { get; set; }

    /// <summary>
    /// Creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    #endregion

    #region Addresses

    /// <summary>
    /// Residence address.
    /// </summary>
    public AddressVM? ResidenceAddress { get; set; }

    /// <summary>
    /// Work address.
    /// </summary>
    public AddressVM? WorkAddress { get; set; }

    /// <summary>
    /// Business address (for Type 3 - Business Owner).
    /// </summary>
    public AddressVM? BusinessAddress { get; set; }

    /// <summary>
    /// Company address (for Type 4 - Registered Company).
    /// </summary>
    public AddressVM? CompanyAddress { get; set; }

    /// <summary>
    /// NGO address (for Type 7 - NGO).
    /// </summary>
    public AddressVM? NGOAddress { get; set; }

    /// <summary>
    /// Waqf address (for Type 8 - Waqf).
    /// </summary>
    public AddressVM? WaqfAddress { get; set; }

    /// <summary>
    /// Selected address for correspondence.
    /// </summary>
    public AddressVM? SelectedAddress { get; set; }

    #endregion

    #region Related Data

    /// <summary>
    /// List of representatives.
    /// </summary>
    public List<RepresentativeVM> Representatives { get; set; } = new();

    /// <summary>
    /// List of attachments.
    /// </summary>
    public List<PlaintiffAttachmentVM> Attachments { get; set; } = new();

    /// <summary>
    /// Representatives count.
    /// </summary>
    public int RepresentativesCount => Representatives.Count;

    /// <summary>
    /// Attachments count.
    /// </summary>
    public int AttachmentsCount => Attachments.Count;

    #endregion
}
