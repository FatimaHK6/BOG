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
            var nameParts = new[] { FirstName, FatherName, GrandfatherName, FamilyName }
                .Where(n => !string.IsNullOrWhiteSpace(n));
            return string.Join(" ", nameParts);
        }
    }

    /// <summary>
    /// Identity type name.
    /// </summary>
    public string? IdentityTypeName { get; set; }

    /// <summary>
    /// Identity number.
    /// </summary>
    public string? IdentityNumber { get; set; }

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

    #endregion

    #region Government Agency

    /// <summary>
    /// Government agency name.
    /// </summary>
    public string? GovernmentAgencyName { get; set; }

    /// <summary>
    /// Additional statement.
    /// </summary>
    public string? AdditionalStatement { get; set; }

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
