using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DTO.Representative;
using BOG.Integration.Interfaces;
using BOG.VM.Representative;

namespace BOG.BL.Services;

/// <summary>
/// Representative business logic service.
/// Handles representative CRUD operations and business rules.
/// </summary>
public class RepresentativeBL : IRepresentativeBL
{
    private readonly IRepresentativeRepository _representativeRepository;
    private readonly IPlaintiffRepository _plaintiffRepository;
    private readonly IAbsherService _absherService;
    private readonly IUnitOfWork _unitOfWork;

    // Representative type to plaintiff type mapping per SRS table 2.16
    // Types: 1-Lawyer, 2-Liquidator, 3-BankruptcyTrustee, 4-JudicialCustodian,
    //        5-CompanyRep, 6-Guardian, 7-GovRep, 8-Conservator, 9-WaqfInspector
    private static readonly Dictionary<int, int[]> AllowedRepresentativeTypes = new()
    {
        // فرد Individual (1): محامي(1)، ولي(6)، وصي(8)
        [1] = new[] { 1, 6, 8 },
        // فرد بدون هوية IndividualNoId (2): محامي(1)، ولي(6)، وصي(8)
        [2] = new[] { 1, 6, 8 },
        // صاحب مؤسسة BusinessOwner (3): محامي(1) only
        [3] = new[] { 1 },
        // شركة مسجلة RegisteredCompany (4): محامي(1)، مصفي(2)، أمين تفليسة(3)، حارس قضائي(4)، ممثل نظامي(5)
        [4] = new[] { 1, 2, 3, 4, 5 },
        // شركة غير مسجلة UnregisteredCompany (5): محامي(1)، مصفي(2)، أمين تفليسة(3)، حارس قضائي(4)، ممثل نظامي(5)
        [5] = new[] { 1, 2, 3, 4, 5 },
        // جهة حكومية GovernmentAgency (6): ممثل جهة حكومية(7) only
        [6] = new[] { 7 },
        // جمعية/مؤسسة أهلية NGO (7): محامي(1)، ممثل نظامي(5)
        [7] = new[] { 1, 5 },
        // وقف Waqf (8): محامي(1)، ناظر(9)
        [8] = new[] { 1, 9 }
    };

    public RepresentativeBL(
        IRepresentativeRepository representativeRepository,
        IPlaintiffRepository plaintiffRepository,
        IAbsherService absherService,
        IUnitOfWork unitOfWork)
    {
        _representativeRepository = representativeRepository ?? throw new ArgumentNullException(nameof(representativeRepository));
        _plaintiffRepository = plaintiffRepository ?? throw new ArgumentNullException(nameof(plaintiffRepository));
        _absherService = absherService ?? throw new ArgumentNullException(nameof(absherService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<RepresentativeVM>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default)
    {
        var representatives = await _representativeRepository.GetByPlaintiffIdAsync(plaintiffId, cancellationToken);
        return representatives.Select(MapToVM).ToList();
    }

    public async Task<RepresentativeVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var representative = await _representativeRepository.GetWithDetailsAsync(id, cancellationToken);
        return representative == null ? null : MapToVM(representative);
    }

    public async Task<RepresentativeVM> CreateAsync(int plaintiffId, RepresentativeCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Get plaintiff to validate
        var plaintiff = await _plaintiffRepository.GetByIdAsync(plaintiffId, cancellationToken) as Plaintiff;
        if (plaintiff == null || plaintiff.IsDeleted)
            throw new InvalidOperationException($"المدعي رقم {plaintiffId} غير موجود");

        // Validate representative type allowed for plaintiff type
        if (!IsRepresentativeTypeAllowed(plaintiff.PlaintiffTypeId, dto.RepresentativeTypeId))
            throw new InvalidOperationException("نوع الممثل غير مسموح لهذا النوع من المدعين");

        // ERR012: Representative identity cannot be same as plaintiff identity
        if (!string.IsNullOrEmpty(dto.IdentityNumber) && dto.IdentityNumber == plaintiff.IdentityNumber)
            throw new InvalidOperationException("ERR012: رقم هوية الممثّل نفس رقم هوية المدّعي");

        // ERR008: Check for duplicate representative
        var exists = await _representativeRepository.ExistsByIdentityAsync(plaintiffId, dto.IdentityNumber, cancellationToken);
        if (exists)
            throw new InvalidOperationException("ERR008: الممثّل موجود مسبقاً للمدعي");

        // Verify with Absher and get data
        int dataSourceId = 2; // Default: FromUser
        var verificationResult = await _absherService.VerifyIdentityAsync(dto.IdentityNumber, dto.IdentityTypeId, cancellationToken);

        if (verificationResult.IsVerified)
        {
            var personData = await _absherService.GetPersonDataAsync(dto.IdentityNumber, dto.IdentityTypeId, cancellationToken);
            if (personData != null)
            {
                dto.FirstName = personData.FirstName;
                dto.FatherName = personData.FatherName;
                dto.GrandfatherName = personData.GrandfatherName;
                dto.FamilyName = personData.FamilyName;
                dto.BirthDate = personData.BirthDate;
                dto.MobileNumber ??= personData.MobileNumber;
                dto.Email ??= personData.Email;
                dataSourceId = 1; // FromAbsher
            }
        }

        // Create representative entity
        var representative = new Representative
        {
            PlaintiffId = plaintiffId,
            RepresentativeTypeId = dto.RepresentativeTypeId,
            IdentityTypeId = dto.IdentityTypeId,
            IdentityNumber = dto.IdentityNumber,
            FirstName = dto.FirstName,
            FatherName = dto.FatherName,
            GrandfatherName = dto.GrandfatherName,
            FamilyName = dto.FamilyName,
            ClanName = dto.ClanName,
            BirthDate = dto.BirthDate,
            Gender = dto.Gender,
            NationalityId = dto.NationalityId,
            IdentityIssueDate = dto.IdentityIssueDate,
            IdentityExpiryDate = dto.IdentityExpiryDate,
            DataSourceId = dataSourceId,
            MobileNumber = dto.MobileNumber,
            Email = dto.Email,
            AuthorizationNumber = dto.AuthorizationNumber,
            AuthorizationDate = dto.AuthorizationDate,
            AuthorizationSource = dto.AuthorizationSource,
            AuthorizationSourceType = dto.AuthorizationSourceType,
            // Liquidator fields
            DecisionNumber = dto.DecisionNumber,
            DecisionDate = dto.DecisionDate,
            DecisionSource = dto.DecisionSource,
            // Guardian fields
            DeedNumber = dto.DeedNumber,
            DeedDate = dto.DeedDate,
            DeedSource = dto.DeedSource,
            GuardianshipType = dto.GuardianshipType,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _representativeRepository.AddAsync(representative, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(representative.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create representative");
    }

    public async Task<RepresentativeVM> UpdateAsync(int id, RepresentativeUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var representative = await _representativeRepository.GetByIdAsync(id, cancellationToken) as Representative;
        if (representative == null || representative.IsDeleted)
            throw new InvalidOperationException($"الممثل رقم {id} غير موجود");

        // Update fields (only non-Absher fields can be updated if data is from Absher)
        if (representative.DataSourceId != 1) // Not from Absher
        {
            if (!string.IsNullOrEmpty(dto.FirstName)) representative.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.FatherName)) representative.FatherName = dto.FatherName;
            if (!string.IsNullOrEmpty(dto.GrandfatherName)) representative.GrandfatherName = dto.GrandfatherName;
            if (!string.IsNullOrEmpty(dto.FamilyName)) representative.FamilyName = dto.FamilyName;
            if (!string.IsNullOrEmpty(dto.ClanName)) representative.ClanName = dto.ClanName;
            if (dto.BirthDate.HasValue) representative.BirthDate = dto.BirthDate;
            if (!string.IsNullOrEmpty(dto.Gender)) representative.Gender = dto.Gender;
            if (dto.NationalityId.HasValue) representative.NationalityId = dto.NationalityId;
            if (dto.IdentityIssueDate.HasValue) representative.IdentityIssueDate = dto.IdentityIssueDate;
            if (dto.IdentityExpiryDate.HasValue) representative.IdentityExpiryDate = dto.IdentityExpiryDate;
        }

        // Contact info can always be updated
        if (dto.MobileNumber != null) representative.MobileNumber = dto.MobileNumber;
        if (dto.Email != null) representative.Email = dto.Email;

        // Authorization info
        if (dto.AuthorizationNumber != null) representative.AuthorizationNumber = dto.AuthorizationNumber;
        if (dto.AuthorizationDate.HasValue) representative.AuthorizationDate = dto.AuthorizationDate;
        if (dto.AuthorizationSource != null) representative.AuthorizationSource = dto.AuthorizationSource;
        if (dto.AuthorizationSourceType != null) representative.AuthorizationSourceType = dto.AuthorizationSourceType;

        // Liquidator fields
        if (dto.DecisionNumber != null) representative.DecisionNumber = dto.DecisionNumber;
        if (dto.DecisionDate.HasValue) representative.DecisionDate = dto.DecisionDate;
        if (dto.DecisionSource != null) representative.DecisionSource = dto.DecisionSource;

        // Guardian fields
        if (dto.DeedNumber != null) representative.DeedNumber = dto.DeedNumber;
        if (dto.DeedDate.HasValue) representative.DeedDate = dto.DeedDate;
        if (dto.DeedSource != null) representative.DeedSource = dto.DeedSource;
        if (dto.GuardianshipType != null) representative.GuardianshipType = dto.GuardianshipType;

        representative.ModifiedDate = DateTime.UtcNow;

        await _representativeRepository.UpdateAsync(representative, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to update representative");
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var representative = await _representativeRepository.GetByIdAsync(id, cancellationToken) as Representative;
        if (representative == null || representative.IsDeleted)
            throw new InvalidOperationException($"الممثل رقم {id} غير موجود");

        // Soft delete
        representative.IsDeleted = true;
        representative.IsActive = false;
        representative.ModifiedDate = DateTime.UtcNow;

        await _representativeRepository.UpdateAsync(representative, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public IEnumerable<int> GetAllowedRepresentativeTypes(int plaintiffTypeId)
    {
        return AllowedRepresentativeTypes.TryGetValue(plaintiffTypeId, out var types)
            ? types
            : Array.Empty<int>();
    }

    private bool IsRepresentativeTypeAllowed(int plaintiffTypeId, int representativeTypeId)
    {
        return AllowedRepresentativeTypes.TryGetValue(plaintiffTypeId, out var allowedTypes)
            && allowedTypes.Contains(representativeTypeId);
    }

    private static RepresentativeVM MapToVM(Representative rep)
    {
        return new RepresentativeVM
        {
            Id = rep.Id,
            PlaintiffId = rep.PlaintiffId,
            RepresentativeTypeId = rep.RepresentativeTypeId,
            RepresentativeTypeNameAr = rep.RepresentativeType?.NameAr ?? "",
            RepresentativeTypeName = rep.RepresentativeType?.Name ?? "",
            IdentityTypeId = rep.IdentityTypeId,
            IdentityTypeName = rep.IdentityType?.NameAr ?? "",
            IdentityNumber = rep.IdentityNumber,
            FirstName = rep.FirstName,
            FatherName = rep.FatherName,
            GrandfatherName = rep.GrandfatherName,
            FamilyName = rep.FamilyName,
            ClanName = rep.ClanName,
            BirthDate = rep.BirthDate,
            Gender = rep.Gender,
            NationalityId = rep.NationalityId,
            IdentityIssueDate = rep.IdentityIssueDate,
            IdentityExpiryDate = rep.IdentityExpiryDate,
            DataSourceId = rep.DataSourceId,
            DataSourceName = rep.DataSource?.NameAr,
            MobileNumber = rep.MobileNumber,
            Email = rep.Email,
            AuthorizationNumber = rep.AuthorizationNumber,
            AuthorizationDate = rep.AuthorizationDate,
            AuthorizationSource = rep.AuthorizationSource,
            AuthorizationSourceType = rep.AuthorizationSourceType,
            // Liquidator fields
            DecisionNumber = rep.DecisionNumber,
            DecisionDate = rep.DecisionDate,
            DecisionSource = rep.DecisionSource,
            // Guardian fields
            DeedNumber = rep.DeedNumber,
            DeedDate = rep.DeedDate,
            DeedSource = rep.DeedSource,
            GuardianshipType = rep.GuardianshipType,
            CreatedDate = rep.CreatedDate
        };
    }
}
