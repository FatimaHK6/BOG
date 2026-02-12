using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// DTO for creating/updating a single claim.
/// </summary>
public class ClaimDTO
{
    [Required(ErrorMessage = "نص الطلب مطلوب")]
    [StringLength(2000, ErrorMessage = "لا يمكن أن يتجاوز نص الطلب 2000 حرف")]
    public string ClaimText { get; set; } = null!;
}

/// <summary>
/// DTO for batch updating all claims for a request.
/// Replaces all existing claims with the provided list.
/// </summary>
public class ClaimsBatchUpdateDTO
{
    public List<ClaimDTO> Claims { get; set; } = new();
}
