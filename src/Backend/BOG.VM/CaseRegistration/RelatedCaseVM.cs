namespace BOG.VM.CaseRegistration;

/// <summary>
/// View Model for RelatedCase entity (الدعوى المرتبطة).
/// Used for API responses.
/// </summary>
public class RelatedCaseVM
{
    public int Id { get; set; }
    public int CaseRegistrationRequestId { get; set; }
    public int? CourtId { get; set; }
    public string? CourtName { get; set; } // For display purposes (from Court.NameAr)
    public long CaseNumber { get; set; }
    public int CaseYear { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
