namespace BOG.VM.AdditionalInfo;

/// <summary>
/// View model for additional information related to case registration requests.
/// Supports three types of additional information based on case type.
/// Represents complete additional info details for API responses.
/// </summary>
public class AdditionalInfoVM
{
    /// <summary>
    /// Additional information record ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Associated case registration request ID.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    #region Type 1: إلغاء قرار إداري (Management Decision Cancellation) - 5 Fields

    /// <summary>
    /// رقم القرار - Decision Number
    /// </summary>
    public string? DecisionNumber { get; set; }

    /// <summary>
    /// تاريخ القرار - Decision Date
    /// </summary>
    public DateTime? DecisionDate { get; set; }

    /// <summary>
    /// تاريخ العلم بالقرار - Date of Notification of Decision
    /// </summary>
    public DateTime? NotificationDate { get; set; }

    /// <summary>
    /// طريقة العلم بالقرار - Method of Notification of Decision (Display Name)
    /// </summary>
    public string? NotificationMethod { get; set; }

    /// <summary>
    /// طريقة العلم بالقرار - Method of Notification of Decision (Lookup ID)
    /// </summary>
    public int? NotificationMethodId { get; set; }

    /// <summary>
    /// جهة إصدار القرار - Decision Issuing Authority (Display Name)
    /// </summary>
    public string? IssuingAuthority { get; set; }

    /// <summary>
    /// جهة إصدار القرار - Decision Issuing Authority (Lookup ID)
    /// </summary>
    public int? IssuingAuthorityId { get; set; }

    #endregion

    #region Type 2: حقوق خدمة/تقاعدية (Service/Retirement Rights) - 6 Fields

    /// <summary>
    /// يوجد تظلم - Has Complaint (Yes/No)
    /// </summary>
    public bool? HasComplaint { get; set; }

    /// <summary>
    /// رقم التظلم - Complaint Number
    /// </summary>
    public string? ComplaintNumber { get; set; }

    /// <summary>
    /// تاريخ التظلم - Complaint Date
    /// </summary>
    public DateTime? ComplaintDate { get; set; }

    /// <summary>
    /// الجهة المتظلم لها - Authority Complained To (Display Name)
    /// </summary>
    public string? ComplaintAuthority { get; set; }

    /// <summary>
    /// الجهة المتظلم لها - Authority Complained To (Lookup ID)
    /// </summary>
    public int? ComplaintAuthorityId { get; set; }

    /// <summary>
    /// تاريخ البت في التظلم - Date of Decision on Complaint
    /// </summary>
    public DateTime? ComplaintDecisionDate { get; set; }

    /// <summary>
    /// نتيجة النظام - System Result
    /// </summary>
    public string? SystemResult { get; set; }

    #endregion

    #region Type 3: نزاع علامة تجارية (Trademark Dispute) - 2 Fields

    /// <summary>
    /// رقم الطلب - Request Number
    /// </summary>
    public string? RequestNumber { get; set; }

    /// <summary>
    /// تاريخه - Request Date (Its Date)
    /// </summary>
    public DateTime? RequestDate { get; set; }

    #endregion

    /// <summary>
    /// Timestamp when the record was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Timestamp when the record was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
