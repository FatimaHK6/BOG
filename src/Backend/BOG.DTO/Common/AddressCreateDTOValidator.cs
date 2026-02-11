using FluentValidation;

namespace BOG.DTO.Common;

/// <summary>
/// FluentValidation validator for AddressCreateDTO (العنوان الوطني).
/// Implements all address field validations as per SRS 6.3.2.
/// </summary>
public class AddressCreateDTOValidator : AbstractValidator<AddressCreateDTO>
{
    public AddressCreateDTOValidator()
    {
        RuleFor(x => x.RegionId)
            .NotNull().WithMessage("المنطقة مطلوبة")
            .GreaterThan(0).WithMessage("المنطقة غير صالحة");

        RuleFor(x => x.CityId)
            .NotNull().WithMessage("المدينة مطلوبة")
            .GreaterThan(0).WithMessage("المدينة غير صالحة");

        RuleFor(x => x.DistrictId)
            .NotNull().WithMessage("الحي مطلوب")
            .GreaterThan(0).WithMessage("الحي غير صالح");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("الشارع مطلوب")
            .MaximumLength(200).WithMessage("الشارع يجب ألا يتجاوز 200 حرف");

        RuleFor(x => x.BuildingNumber)
            .NotEmpty().WithMessage("رقم المبنى مطلوب")
            .Matches(@"^\d{4}$").WithMessage("رقم المبنى يجب أن يكون 4 أرقام فقط");

        RuleFor(x => x.UnitNumber)
            .NotEmpty().WithMessage("رقم الوحدة مطلوب")
            .Matches(@"^\d+$").WithMessage("رقم الوحدة يجب أن يكون أرقام فقط");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("الرمز البريدي مطلوب")
            .Matches(@"^\d{5}$").WithMessage("الرمز البريدي يجب أن يكون 5 أرقام فقط");

        RuleFor(x => x.AdditionalCode)
            .NotEmpty().WithMessage("الرمز الإضافي مطلوب")
            .Matches(@"^\d{4}$").WithMessage("الرمز الإضافي يجب أن يكون 4 أرقام فقط");
    }
}
