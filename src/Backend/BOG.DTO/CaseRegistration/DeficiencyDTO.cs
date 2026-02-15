using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// DTO for creating/updating a single deficiency.
/// Contains only the deficiency description ID - موظف القيد selects from predefined templates.
/// </summary>
public class RequestDeficiencyDTO
{
    [Required(ErrorMessage = "معرف وصف النقص مطلوب")]
    [Range(1, int.MaxValue, ErrorMessage = "معرف وصف النقص يجب أن يكون قيمة موجبة")]
    public int DeficiencyDescriptionId { get; set; }
}

/// <summary>
/// DTO for batch updating all deficiencies for a request.
/// Replaces all existing deficiencies with the provided list.
/// </summary>
public class DeficienciesBatchUpdateDTO
{
    public List<RequestDeficiencyDTO> Deficiencies { get; set; } = new();
}
