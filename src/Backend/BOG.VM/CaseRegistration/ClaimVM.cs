namespace BOG.VM.CaseRegistration;

/// <summary>
/// View Model for Claim entity (طلب الدعوى).
/// Used for API responses.
/// </summary>
public class ClaimVM
{
    public int Id { get; set; }
    public int CaseRegistrationRequestId { get; set; }
    public string ClaimText { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
