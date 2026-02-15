using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// DTO for creating/updating a single related case.
/// </summary>
public class RelatedCaseDTO
{
    public int? CourtId { get; set; }

    [Required(ErrorMessage = "رقم الدعوى مطلوب")]
    [Range(1, int.MaxValue, ErrorMessage = "رقم الدعوى يجب أن يكون رقماً صحيحاً صالحاً")]
    public int CaseNumber { get; set; }

    [Required(ErrorMessage = "عام الدعوى مطلوب")]
    [Range(1900, 2100, ErrorMessage = "يجب أن يكون عام الدعوى بين 1900 و 2100")]
    public int CaseYear { get; set; }
}

/// <summary>
/// DTO for batch updating all related cases for a request.
/// Replaces all existing related cases with the provided list.
/// </summary>
public class RelatedCasesBatchUpdateDTO
{
    public List<RelatedCaseDTO> RelatedCases { get; set; } = new();
}
