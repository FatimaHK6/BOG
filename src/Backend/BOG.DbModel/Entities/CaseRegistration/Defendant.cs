using BOG.DbModel.Entities.Common;
using BOG.DbModel.Entities.Lookups;

namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for defendant (مدعى عليه) - many-to-many with CaseRegistrationRequest.
/// </summary>
public class Defendant : BaseEntity
{
    /// <summary>
    /// Foreign key to DefendantType.
    /// </summary>
    public int DefendantTypeId { get; set; }

    /// <summary>
    /// Full name (required) - max 200 characters.
    /// </summary>
    public string FullName { get; set; } = null!;

    #region Optional Identity Data

    /// <summary>
    /// Foreign key to IdentityType.
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number.
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Foreign key to DataSource (مصدر البيانات - أبشر/المستخدم).
    /// </summary>
    public int? DataSourceId { get; set; }

    #endregion

    #region Address Data

    /// <summary>
    /// Address text (if not structured).
    /// </summary>
    public string? AddressText { get; set; }

    /// <summary>
    /// Foreign key to Address (if structured).
    /// </summary>
    public int? AddressId { get; set; }

    #endregion

    #region Company Data

    /// <summary>
    /// Commercial registration number.
    /// </summary>
    public string? CommercialRegNumber { get; set; }

    #endregion

    #region Government Agency Data

    /// <summary>
    /// Foreign key to GovernmentAgency.
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Headquarters location.
    /// </summary>
    public string? Headquarters { get; set; }

    /// <summary>
    /// Additional statement (بيان إضافي) - max 4000 characters.
    /// </summary>
    public string? AdditionalStatement { get; set; }

    #endregion

    /// <summary>
    /// Whether the defendant is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region Navigation Properties

    /// <summary>
    /// Navigation property for defendant type.
    /// </summary>
    public virtual DefendantType DefendantType { get; set; } = null!;

    /// <summary>
    /// Navigation property for identity type.
    /// </summary>
    public virtual IdentityType? IdentityType { get; set; }

    /// <summary>
    /// Navigation property for data source.
    /// </summary>
    public virtual DataSource? DataSource { get; set; }

    /// <summary>
    /// Navigation property for address.
    /// </summary>
    public virtual Address? Address { get; set; }

    /// <summary>
    /// Navigation property for government agency.
    /// </summary>
    public virtual GovernmentAgency? GovernmentAgency { get; set; }

    /// <summary>
    /// Collection of case request associations (many-to-many).
    /// </summary>
    public virtual ICollection<CaseRequestDefendant> CaseRequestDefendants { get; set; } = new List<CaseRequestDefendant>();

    #endregion
}
