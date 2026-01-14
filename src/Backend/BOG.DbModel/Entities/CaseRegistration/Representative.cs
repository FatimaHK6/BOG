using BOG.DbModel.Entities.Lookups;

namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for representative (ممثل) of a plaintiff.
/// </summary>
public class Representative : BaseEntity
{
    /// <summary>
    /// Foreign key to Plaintiff.
    /// </summary>
    public int PlaintiffId { get; set; }

    /// <summary>
    /// Foreign key to RepresentativeType.
    /// </summary>
    public int RepresentativeTypeId { get; set; }

    #region Personal Data

    /// <summary>
    /// Foreign key to IdentityType.
    /// </summary>
    public int IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number.
    /// </summary>
    public string IdentityNumber { get; set; } = null!;

    /// <summary>
    /// First name (الاسم الأول).
    /// </summary>
    public string FirstName { get; set; } = null!;

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
    public string FamilyName { get; set; } = null!;

    /// <summary>
    /// Clan name (اسم القبيلة).
    /// </summary>
    public string? ClanName { get; set; }

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Foreign key to DataSource (مصدر البيانات - أبشر/المستخدم).
    /// </summary>
    public int? DataSourceId { get; set; }

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
    /// Authorization/Power of Attorney number (رقم الوكالة/القرار/الصك).
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
    /// Authorization source type (كتابة عدل/محكمة/وزارة خارجية).
    /// </summary>
    public string? AuthorizationSourceType { get; set; }

    #endregion

    #region Guardian Data

    /// <summary>
    /// Guardianship type for Guardian (ولي): طبيعية/مكتسبة.
    /// </summary>
    public string? GuardianshipType { get; set; }

    #endregion

    /// <summary>
    /// Whether the representative is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region Navigation Properties

    /// <summary>
    /// Navigation property for plaintiff.
    /// </summary>
    public virtual Plaintiff Plaintiff { get; set; } = null!;

    /// <summary>
    /// Navigation property for representative type.
    /// </summary>
    public virtual RepresentativeType RepresentativeType { get; set; } = null!;

    /// <summary>
    /// Navigation property for identity type.
    /// </summary>
    public virtual IdentityType IdentityType { get; set; } = null!;

    /// <summary>
    /// Navigation property for data source.
    /// </summary>
    public virtual DataSource? DataSource { get; set; }

    #endregion
}
