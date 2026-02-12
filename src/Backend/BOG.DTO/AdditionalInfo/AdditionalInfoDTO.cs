using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.AdditionalInfo;

/// <summary>
/// Data transfer object for adding/updating additional information for a case registration request.
/// Supports three types of additional information based on case type.
/// Used in POST/PUT /api/case-requests/{requestId}/additional-info
/// </summary>
public class AdditionalInfoDTO
{
    #region Type 1: إلغاء قرار إداري (Management Decision Cancellation) - 5 Fields

    /// <summary>
    /// رقم القرار - Decision Number
    /// </summary>
    [StringLength(50, ErrorMessage = "Decision number cannot exceed 50 characters")]
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
    /// طريقة العلم بالقرار - Method of Notification of Decision (Lookup ID)
    /// </summary>
    public int? NotificationMethodId { get; set; }

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
    [StringLength(50, ErrorMessage = "Complaint number cannot exceed 50 characters")]
    public string? ComplaintNumber { get; set; }

    /// <summary>
    /// تاريخ التظلم - Complaint Date
    /// </summary>
    public DateTime? ComplaintDate { get; set; }

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
    [StringLength(500, ErrorMessage = "System result cannot exceed 500 characters")]
    public string? SystemResult { get; set; }

    #endregion

    #region Type 3: نزاع علامة تجارية (Trademark Dispute) - 2 Fields

    /// <summary>
    /// رقم الطلب - Request Number
    /// </summary>
    [StringLength(50, ErrorMessage = "Request number cannot exceed 50 characters")]
    public string? RequestNumber { get; set; }

    /// <summary>
    /// تاريخه - Request Date (Its Date)
    /// </summary>
    public DateTime? RequestDate { get; set; }

    #endregion
}
