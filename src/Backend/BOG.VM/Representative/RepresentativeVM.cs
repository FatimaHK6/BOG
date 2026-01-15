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

    #region Guardian Data

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
