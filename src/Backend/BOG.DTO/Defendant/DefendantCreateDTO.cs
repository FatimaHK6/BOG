namespace BOG.DTO.Defendant;

/// <summary>
/// DTO for creating a new defendant.
/// </summary>
public class DefendantCreateDTO
{
    /// <summary>
    /// Defendant type ID (required).
    /// </summary>
    public int DefendantTypeId { get; set; }

    #region Individual (Type 1)

    /// <summary>
    /// Identity type ID (optional for Type 1: 1=National ID, 2=Resident ID).
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number (required for Type 1, 10 digits).
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// First name (required for Type 1).
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Father name (conditional for Type 1, required if IdentityType = National ID).
    /// </summary>
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather name (optional for Type 1).
    /// </summary>
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Tribe name (optional for Type 1).
    /// </summary>
    public string? TribeName { get; set; }

    /// <summary>
    /// Family name (required for Type 1).
    /// </summary>
    public string? FamilyName { get; set; }

    /// <summary>
    /// Birth date (optional for Type 1, no future date).
    /// </summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// Identity issue date (required for Type 5 - Business Owner).
    /// </summary>
    public DateOnly? IdentityIssueDate { get; set; }

    /// <summary>
    /// Identity expiry date (required for Type 5 - Business Owner).
    /// </summary>
    public DateOnly? IdentityExpiryDate { get; set; }

    /// <summary>
    /// Gender ID (required for Type 1: 1=Male, 2=Female).
    /// </summary>
    public int? GenderId { get; set; }

    /// <summary>
    /// Nationality ID (required for Type 1).
    /// </summary>
    public int? NationalityId { get; set; }

    /// <summary>
    /// Mobile number (required for Type 1, 10 digits starting with 05).
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address (optional for Type 1, valid email format).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Individual residence address - Region ID (optional for Type 1).
    /// </summary>
    public int? IndRegionId { get; set; }

    /// <summary>
    /// Individual residence address - City ID (optional for Type 1).
    /// </summary>
    public int? IndCityId { get; set; }

    /// <summary>
    /// Individual residence address - District (optional for Type 1).
    /// </summary>
    public string? IndDistrict { get; set; }

    /// <summary>
    /// Individual residence address - Street (optional for Type 1).
    /// </summary>
    public string? IndStreet { get; set; }

    /// <summary>
    /// Individual residence address - Building number (optional, 4 digits).
    /// </summary>
    public string? IndBuildingNumber { get; set; }

    /// <summary>
    /// Individual residence address - Unit number (optional, 4 digits).
    /// </summary>
    public string? IndUnitNumber { get; set; }

    /// <summary>
    /// Individual residence address - Postal code (optional, 5 digits).
    /// </summary>
    public string? IndPostalCode { get; set; }

    /// <summary>
    /// Individual residence address - Additional code (optional, 4 digits).
    /// </summary>
    public string? IndAdditionalCode { get; set; }

    /// <summary>
    /// Employment status ID (optional for Type 1: 1=Government, 2=Private, 3=Unemployed).
    /// </summary>
    public int? EmploymentStatusId { get; set; }

    /// <summary>
    /// Employer name (conditional, shown if EmploymentStatus = Government or Private).
    /// </summary>
    public string? Employer { get; set; }

    /// <summary>
    /// Occupation (conditional, shown if EmploymentStatus = Government or Private).
    /// </summary>
    public string? Occupation { get; set; }

    /// <summary>
    /// Work address - Region ID (conditional, shown if EmploymentStatus = Private).
    /// </summary>
    public int? WorkRegionId { get; set; }

    /// <summary>
    /// Work address - City ID (conditional, shown if EmploymentStatus = Private).
    /// </summary>
    public int? WorkCityId { get; set; }

    /// <summary>
    /// Work address - District (conditional, shown if EmploymentStatus = Private).
    /// </summary>
    public string? WorkDistrict { get; set; }

    /// <summary>
    /// Work address - Street (conditional, shown if EmploymentStatus = Private).
    /// </summary>
    public string? WorkStreet { get; set; }

    /// <summary>
    /// Work address - Building number (conditional, 4 digits).
    /// </summary>
    public string? WorkBuildingNumber { get; set; }

    /// <summary>
    /// Work address - Unit number (conditional, 4 digits).
    /// </summary>
    public string? WorkUnitNumber { get; set; }

    /// <summary>
    /// Work address - Postal code (conditional, 5 digits).
    /// </summary>
    public string? WorkPostalCode { get; set; }

    /// <summary>
    /// Work address - Additional code (conditional, 4 digits).
    /// </summary>
    public string? WorkAdditionalCode { get; set; }

    #endregion

    #region Government Agency (Type 3)

    /// <summary>
    /// Government agency ID (required for Type 3).
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Headquarters location (required for Type 3, auto-filled).
    /// </summary>
    public string? Headquarters { get; set; }

    /// <summary>
    /// Additional statement (optional, max 4000 chars).
    /// </summary>
    public string? AdditionalStatement { get; set; }

    #endregion

    #region Registered Company (Type 2)

    /// <summary>
    /// Registration start date (required for Type 2).
    /// </summary>
    public DateOnly? RegistrationStartDate { get; set; }

    /// <summary>
    /// Registration end date (required for Type 2, must be > start date).
    /// </summary>
    public DateOnly? RegistrationEndDate { get; set; }

    /// <summary>
    /// Registered company address - Region ID (required for Type 2).
    /// </summary>
    public int? RegCompanyRegionId { get; set; }

    /// <summary>
    /// Registered company address - City ID (required for Type 2).
    /// </summary>
    public int? RegCompanyCityId { get; set; }

    /// <summary>
    /// Registered company address - District (required for Type 2).
    /// </summary>
    public string? RegCompanyDistrict { get; set; }

    /// <summary>
    /// Registered company address - Street (required for Type 2, max 200).
    /// </summary>
    public string? RegCompanyStreet { get; set; }

    /// <summary>
    /// Registered company address - Building number (required for Type 2, 4 digits).
    /// </summary>
    public string? RegCompanyBuildingNumber { get; set; }

    /// <summary>
    /// Registered company address - Unit number (required for Type 2, digits only).
    /// </summary>
    public string? RegCompanyUnitNumber { get; set; }

    /// <summary>
    /// Registered company address - Postal code (required for Type 2, 5 digits).
    /// </summary>
    public string? RegCompanyPostalCode { get; set; }

    /// <summary>
    /// Registered company address - Additional code (required for Type 2, 4 digits).
    /// </summary>
    public string? RegCompanyAdditionalCode { get; set; }

    #endregion

    #region Unregistered Company (Type 4)

    /// <summary>
    /// Commercial registration number (required for Type 4, max 20 chars).
    /// </summary>
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// Company name (required for Type 4, max 200 chars).
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Country ID (required for Type 4).
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// City name (required for Type 4, max 100 chars).
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Description (required for Type 4, max 1000 chars).
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Waqf (Type 7)

    /// <summary>
    /// Waqf name (required for Type 7, max 200 chars).
    /// </summary>
    public string? WaqfName { get; set; }

    /// <summary>
    /// Court deed number (required for Type 7, 10 chars).
    /// </summary>
    public string? CourtDeedNumber { get; set; }

    /// <summary>
    /// Court deed date (required for Type 7, no future date).
    /// </summary>
    public DateOnly? CourtDeedDate { get; set; }

    /// <summary>
    /// Deed source (required for Type 7, max 100 chars).
    /// </summary>
    public string? DeedSource { get; set; }

    /// <summary>
    /// Waqf supervisory type ID (required for Type 7: 1=أهلية, 2=حكومية).
    /// </summary>
    public int? WaqfSupervisoryTypeId { get; set; }

    /// <summary>
    /// Agency name (required if WaqfSupervisoryTypeId = 2).
    /// </summary>
    public string? WaqfAgencyName { get; set; }

    /// <summary>
    /// Waqf address - Region ID (required for Type 7).
    /// </summary>
    public int? WaqfRegionId { get; set; }

    /// <summary>
    /// Waqf address - City ID (required for Type 7).
    /// </summary>
    public int? WaqfCityId { get; set; }

    /// <summary>
    /// Waqf address - District (optional).
    /// </summary>
    public string? WaqfDistrict { get; set; }

    /// <summary>
    /// Waqf address - Street (optional, max 200 chars).
    /// </summary>
    public string? WaqfStreet { get; set; }

    /// <summary>
    /// Waqf address - Building number (optional, 4 digits).
    /// </summary>
    public string? WaqfBuildingNumber { get; set; }

    /// <summary>
    /// Waqf address - Unit number (optional).
    /// </summary>
    public string? WaqfUnitNumber { get; set; }

    /// <summary>
    /// Waqf address - Postal code (optional, 5 digits).
    /// </summary>
    public string? WaqfPostalCode { get; set; }

    /// <summary>
    /// Waqf address - Additional code (optional, 4 digits).
    /// </summary>
    public string? WaqfAdditionalCode { get; set; }

    /// <summary>
    /// Waqf address - Description (required for Type 7).
    /// </summary>
    public string? WaqfAddressDescription { get; set; }

    #endregion

    #region NGO/Society (Type 6)

    /// <summary>
    /// License number (required for Type 6, 10 digits).
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// License source ID (required for Type 6).
    /// </summary>
    public int? LicenseSourceId { get; set; }

    /// <summary>
    /// NGO name (required for Type 6, max 200 chars).
    /// </summary>
    public string? NGOName { get; set; }

    /// <summary>
    /// License date (required for Type 6).
    /// </summary>
    public DateOnly? LicenseDate { get; set; }

    /// <summary>
    /// NGO address - Region ID (required for Type 6).
    /// </summary>
    public int? NGORegionId { get; set; }

    /// <summary>
    /// NGO address - City ID (required for Type 6).
    /// </summary>
    public int? NGOCityId { get; set; }

    /// <summary>
    /// NGO address - District (required for Type 6, max 100 chars).
    /// </summary>
    public string? NGODistrict { get; set; }

    /// <summary>
    /// NGO address - Street (required for Type 6, max 200 chars).
    /// </summary>
    public string? NGOStreet { get; set; }

    /// <summary>
    /// NGO address - Building number (required for Type 6, 4 digits).
    /// </summary>
    public string? NGOBuildingNumber { get; set; }

    /// <summary>
    /// NGO address - Unit number (required for Type 6, digits only).
    /// </summary>
    public string? NGOUnitNumber { get; set; }

    /// <summary>
    /// NGO address - Postal code (required for Type 6, 5 digits).
    /// </summary>
    public string? NGOPostalCode { get; set; }

    /// <summary>
    /// NGO address - Additional code (required for Type 6, 4 digits).
    /// </summary>
    public string? NGOAdditionalCode { get; set; }

    #endregion
}
