namespace BOG.VM.Defendant;

/// <summary>
/// Defendant ViewModel for full details presentation.
/// </summary>
public class DefendantVM
{
    /// <summary>
    /// Defendant ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Defendant type ID.
    /// </summary>
    public int DefendantTypeId { get; set; }

    /// <summary>
    /// Defendant type name (Arabic).
    /// </summary>
    public string DefendantTypeNameAr { get; set; } = null!;

    /// <summary>
    /// Display name (full name or agency name).
    /// </summary>
    public string DisplayName { get; set; } = null!;

    #region Individual (Type 1)

    /// <summary>
    /// Identity type ID.
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity type name.
    /// </summary>
    public string? IdentityTypeName { get; set; }

    /// <summary>
    /// Identity number.
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// First name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Father name.
    /// </summary>
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather name.
    /// </summary>
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Tribe name.
    /// </summary>
    public string? TribeName { get; set; }

    /// <summary>
    /// Family name.
    /// </summary>
    public string? FamilyName { get; set; }

    /// <summary>
    /// Birth date.
    /// </summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// Identity issue date.
    /// </summary>
    public DateOnly? IdentityIssueDate { get; set; }

    /// <summary>
    /// Identity expiry date.
    /// </summary>
    public DateOnly? IdentityExpiryDate { get; set; }

    /// <summary>
    /// Gender ID.
    /// </summary>
    public int? GenderId { get; set; }

    /// <summary>
    /// Gender name.
    /// </summary>
    public string? GenderName { get; set; }

    /// <summary>
    /// Nationality ID.
    /// </summary>
    public int? NationalityId { get; set; }

    /// <summary>
    /// Nationality name.
    /// </summary>
    public string? NationalityName { get; set; }

    /// <summary>
    /// Mobile number.
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Individual residence address - Region ID.
    /// </summary>
    public int? IndRegionId { get; set; }

    /// <summary>
    /// Individual residence address - Region name.
    /// </summary>
    public string? IndRegionName { get; set; }

    /// <summary>
    /// Individual residence address - City ID.
    /// </summary>
    public int? IndCityId { get; set; }

    /// <summary>
    /// Individual residence address - City name.
    /// </summary>
    public string? IndCityName { get; set; }

    /// <summary>
    /// Individual residence address - District.
    /// </summary>
    public string? IndDistrict { get; set; }

    /// <summary>
    /// Individual residence address - Street.
    /// </summary>
    public string? IndStreet { get; set; }

    /// <summary>
    /// Individual residence address - Building number.
    /// </summary>
    public string? IndBuildingNumber { get; set; }

    /// <summary>
    /// Individual residence address - Unit number.
    /// </summary>
    public string? IndUnitNumber { get; set; }

    /// <summary>
    /// Individual residence address - Postal code.
    /// </summary>
    public string? IndPostalCode { get; set; }

    /// <summary>
    /// Individual residence address - Additional code.
    /// </summary>
    public string? IndAdditionalCode { get; set; }

    /// <summary>
    /// Employment status ID.
    /// </summary>
    public int? EmploymentStatusId { get; set; }

    /// <summary>
    /// Employment status name.
    /// </summary>
    public string? EmploymentStatusName { get; set; }

    /// <summary>
    /// Employer name.
    /// </summary>
    public string? Employer { get; set; }

    /// <summary>
    /// Occupation.
    /// </summary>
    public string? Occupation { get; set; }

    /// <summary>
    /// Work address - Region ID.
    /// </summary>
    public int? WorkRegionId { get; set; }

    /// <summary>
    /// Work address - Region name.
    /// </summary>
    public string? WorkRegionName { get; set; }

    /// <summary>
    /// Work address - City ID.
    /// </summary>
    public int? WorkCityId { get; set; }

    /// <summary>
    /// Work address - City name.
    /// </summary>
    public string? WorkCityName { get; set; }

    /// <summary>
    /// Work address - District.
    /// </summary>
    public string? WorkDistrict { get; set; }

    /// <summary>
    /// Work address - Street.
    /// </summary>
    public string? WorkStreet { get; set; }

    /// <summary>
    /// Work address - Building number.
    /// </summary>
    public string? WorkBuildingNumber { get; set; }

    /// <summary>
    /// Work address - Unit number.
    /// </summary>
    public string? WorkUnitNumber { get; set; }

    /// <summary>
    /// Work address - Postal code.
    /// </summary>
    public string? WorkPostalCode { get; set; }

    /// <summary>
    /// Work address - Additional code.
    /// </summary>
    public string? WorkAdditionalCode { get; set; }

    #endregion

    #region Government Agency (Type 3)

    /// <summary>
    /// Government agency ID.
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Government agency name.
    /// </summary>
    public string? GovernmentAgencyName { get; set; }

    /// <summary>
    /// Headquarters location.
    /// </summary>
    public string? Headquarters { get; set; }

    /// <summary>
    /// Additional statement.
    /// </summary>
    public string? AdditionalStatement { get; set; }

    #endregion

    #region Registered Company (Type 2)

    /// <summary>
    /// Registration start date.
    /// </summary>
    public DateOnly? RegistrationStartDate { get; set; }

    /// <summary>
    /// Registration end date.
    /// </summary>
    public DateOnly? RegistrationEndDate { get; set; }

    /// <summary>
    /// Registered company address - Region ID.
    /// </summary>
    public int? RegCompanyRegionId { get; set; }

    /// <summary>
    /// Registered company address - Region name.
    /// </summary>
    public string? RegCompanyRegionName { get; set; }

    /// <summary>
    /// Registered company address - City ID.
    /// </summary>
    public int? RegCompanyCityId { get; set; }

    /// <summary>
    /// Registered company address - City name.
    /// </summary>
    public string? RegCompanyCityName { get; set; }

    /// <summary>
    /// Registered company address - District.
    /// </summary>
    public string? RegCompanyDistrict { get; set; }

    /// <summary>
    /// Registered company address - Street.
    /// </summary>
    public string? RegCompanyStreet { get; set; }

    /// <summary>
    /// Registered company address - Building number.
    /// </summary>
    public string? RegCompanyBuildingNumber { get; set; }

    /// <summary>
    /// Registered company address - Unit number.
    /// </summary>
    public string? RegCompanyUnitNumber { get; set; }

    /// <summary>
    /// Registered company address - Postal code.
    /// </summary>
    public string? RegCompanyPostalCode { get; set; }

    /// <summary>
    /// Registered company address - Additional code.
    /// </summary>
    public string? RegCompanyAdditionalCode { get; set; }

    #endregion

    #region Unregistered Company (Type 4)

    /// <summary>
    /// Commercial registration number.
    /// </summary>
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// Company name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Country ID.
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Country name.
    /// </summary>
    public string? CountryName { get; set; }

    /// <summary>
    /// City name.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Waqf (Type 7)

    /// <summary>
    /// Waqf name.
    /// </summary>
    public string? WaqfName { get; set; }

    /// <summary>
    /// Court deed number.
    /// </summary>
    public string? CourtDeedNumber { get; set; }

    /// <summary>
    /// Court deed date.
    /// </summary>
    public DateOnly? CourtDeedDate { get; set; }

    /// <summary>
    /// Deed source.
    /// </summary>
    public string? DeedSource { get; set; }

    /// <summary>
    /// Waqf supervisory type ID (1=أهلية, 2=حكومية).
    /// </summary>
    public int? WaqfSupervisoryTypeId { get; set; }

    /// <summary>
    /// Waqf supervisory type name.
    /// </summary>
    public string? WaqfSupervisoryTypeName { get; set; }

    /// <summary>
    /// Agency name (if supervisory = حكومية).
    /// </summary>
    public string? WaqfAgencyName { get; set; }

    /// <summary>
    /// Waqf address - Region ID.
    /// </summary>
    public int? WaqfRegionId { get; set; }

    /// <summary>
    /// Waqf address - Region name.
    /// </summary>
    public string? WaqfRegionName { get; set; }

    /// <summary>
    /// Waqf address - City ID.
    /// </summary>
    public int? WaqfCityId { get; set; }

    /// <summary>
    /// Waqf address - City name.
    /// </summary>
    public string? WaqfCityName { get; set; }

    /// <summary>
    /// Waqf address - District.
    /// </summary>
    public string? WaqfDistrict { get; set; }

    /// <summary>
    /// Waqf address - Street.
    /// </summary>
    public string? WaqfStreet { get; set; }

    /// <summary>
    /// Waqf address - Building number.
    /// </summary>
    public string? WaqfBuildingNumber { get; set; }

    /// <summary>
    /// Waqf address - Unit number.
    /// </summary>
    public string? WaqfUnitNumber { get; set; }

    /// <summary>
    /// Waqf address - Postal code.
    /// </summary>
    public string? WaqfPostalCode { get; set; }

    /// <summary>
    /// Waqf address - Additional code.
    /// </summary>
    public string? WaqfAdditionalCode { get; set; }

    /// <summary>
    /// Waqf address - Description.
    /// </summary>
    public string? WaqfAddressDescription { get; set; }

    #endregion

    #region NGO/Society (Type 6)

    /// <summary>
    /// License number.
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// License source ID.
    /// </summary>
    public int? LicenseSourceId { get; set; }

    /// <summary>
    /// License source name.
    /// </summary>
    public string? LicenseSourceName { get; set; }

    /// <summary>
    /// NGO name.
    /// </summary>
    public string? NGOName { get; set; }

    /// <summary>
    /// License date.
    /// </summary>
    public DateOnly? LicenseDate { get; set; }

    /// <summary>
    /// NGO address - Region ID.
    /// </summary>
    public int? NGORegionId { get; set; }

    /// <summary>
    /// NGO address - Region name.
    /// </summary>
    public string? NGORegionName { get; set; }

    /// <summary>
    /// NGO address - City ID.
    /// </summary>
    public int? NGOCityId { get; set; }

    /// <summary>
    /// NGO address - City name.
    /// </summary>
    public string? NGOCityName { get; set; }

    /// <summary>
    /// NGO address - District.
    /// </summary>
    public string? NGODistrict { get; set; }

    /// <summary>
    /// NGO address - Street.
    /// </summary>
    public string? NGOStreet { get; set; }

    /// <summary>
    /// NGO address - Building number.
    /// </summary>
    public string? NGOBuildingNumber { get; set; }

    /// <summary>
    /// NGO address - Unit number.
    /// </summary>
    public string? NGOUnitNumber { get; set; }

    /// <summary>
    /// NGO address - Postal code.
    /// </summary>
    public string? NGOPostalCode { get; set; }

    /// <summary>
    /// NGO address - Additional code.
    /// </summary>
    public string? NGOAdditionalCode { get; set; }

    #endregion

    /// <summary>
    /// Creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}
