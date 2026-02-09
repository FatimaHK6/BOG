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

    // Representative type to plaintiff type mapping per plaintiff-user-stories-plan.html table 5.2
    // DB Representative Types:
    // 1: وكيل (Agent), 2: ولي (Guardian), 3: وصي (Custodian), 4: ناظر (Executor)
    // 5: ممثل الورثة (HeirRepresentative), 6: ممثل الشركة (CompanyRepresentative)
    // 7: ممثل الجهة (AgencyRepresentative), 8: أمين التفليسة (Trustee)
    // 9: ممثل نظامي (LegalRepresentative), 10: مصفي (Liquidator), 11: حارس قضائي (JudicialCustodian)
    private static readonly Dictionary<int, int[]> AllowedRepresentativeTypes = new()
    {
        [1] = new[] { 1, 2, 3, 8 },            // فرد: وكيل، ولي، وصي، أمين تفليسة
        [2] = new[] { 1, 2, 3, 4, 7, 8, 9, 10, 11 }, // فرد بدون هوية: جميع الأنواع
        [3] = new[] { 1, 2, 8 },               // صاحب مؤسسة: وكيل، ولي، أمين تفليسة
        [4] = new[] { 1, 8, 9, 10, 11 },       // شركة مسجلة: وكيل، أمين تفليسة، ممثل نظامي، مصفي، حارس قضائي
        [5] = new[] { 1 },                     // شركة غير مسجلة: وكيل فقط
        [6] = new[] { 1, 7 },                  // جهة حكومية: وكيل، ممثل الجهة
        [7] = new[] { 1 },                     // جمعية/مؤسسة أهلية: وكيل فقط
        [8] = new[] { 1, 4 }                   // وقف: وكيل، ناظر
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

        // CompanyRepresentative (ممثل الشركة - type 6): Document fields required
        if (dto.RepresentativeTypeId == 6)
        {
            if (string.IsNullOrWhiteSpace(dto.RepresentationDocSource))
                throw new InvalidOperationException("مصدر مستند التمثيل مطلوب لممثل الشركة");
            if (string.IsNullOrWhiteSpace(dto.RepresentativeCapacity))
                throw new InvalidOperationException("صفة الممثل مطلوبة لممثل الشركة");
            if (string.IsNullOrWhiteSpace(dto.RepresentationDocType))
                throw new InvalidOperationException("نوع مستند التمثيل مطلوب لممثل الشركة");
            if (string.IsNullOrWhiteSpace(dto.RepresentationDocNumber))
                throw new InvalidOperationException("رقم مستند التمثيل مطلوب لممثل الشركة");
        }

        // LegalRepresentative (ممثل نظامي - type 9): Document fields required (same as type 6)
        if (dto.RepresentativeTypeId == 9)
        {
            if (string.IsNullOrWhiteSpace(dto.RepresentationDocSource))
                throw new InvalidOperationException("مصدر مستند التمثيل مطلوب لممثل نظامي");
            if (string.IsNullOrWhiteSpace(dto.RepresentativeCapacity))
                throw new InvalidOperationException("صفة الممثل مطلوبة لممثل نظامي");
            if (string.IsNullOrWhiteSpace(dto.RepresentationDocType))
                throw new InvalidOperationException("نوع مستند التمثيل مطلوب لممثل نظامي");
            if (string.IsNullOrWhiteSpace(dto.RepresentationDocNumber))
                throw new InvalidOperationException("رقم مستند التمثيل مطلوب لممثل نظامي");
        }

        // AgencyRepresentative (ممثل الجهة - type 7): Letter fields required
        if (dto.RepresentativeTypeId == 7)
        {
            if (string.IsNullOrWhiteSpace(dto.RepresentationLetterNumber))
                throw new InvalidOperationException("رقم خطاب التمثيل مطلوب لممثل الجهة");
            if (!dto.RepresentationLetterDate.HasValue)
                throw new InvalidOperationException("تاريخ خطاب التمثيل مطلوب لممثل الجهة");
            if (string.IsNullOrWhiteSpace(dto.RepresentationLetterSource))
                throw new InvalidOperationException("مصدر خطاب التمثيل مطلوب لممثل الجهة");
        }

        // Liquidator (مصفي - type 10): Decision fields required
        if (dto.RepresentativeTypeId == 10)
        {
            if (string.IsNullOrWhiteSpace(dto.DecisionNumber))
                throw new InvalidOperationException("رقم القرار مطلوب لمصفي");
            if (!dto.DecisionDate.HasValue)
                throw new InvalidOperationException("تاريخ القرار مطلوب لمصفي");
            if (string.IsNullOrWhiteSpace(dto.DecisionSource))
                throw new InvalidOperationException("مصدر القرار مطلوب لمصفي");
        }

        // JudicialCustodian (حارس قضائي - type 11): Decision fields required (same as type 10/4)
        if (dto.RepresentativeTypeId == 11)
        {
            if (string.IsNullOrWhiteSpace(dto.DecisionNumber))
                throw new InvalidOperationException("رقم القرار / الدعوى مطلوب لحارس قضائي");
            if (!dto.DecisionDate.HasValue)
                throw new InvalidOperationException("تاريخ القرار / الدعوى مطلوب لحارس قضائي");
            if (string.IsNullOrWhiteSpace(dto.DecisionSource))
                throw new InvalidOperationException("مصدر القرار / الحكم مطلوب لحارس قضائي");
        }

        // Executor (ناظر - type 4): Decision fields required
        if (dto.RepresentativeTypeId == 4)
        {
            if (string.IsNullOrWhiteSpace(dto.DecisionNumber))
                throw new InvalidOperationException("رقم القرار مطلوب لنوع الممثل ناظر");
            if (!dto.DecisionDate.HasValue)
                throw new InvalidOperationException("تاريخ القرار مطلوب لنوع الممثل ناظر");
            if (string.IsNullOrWhiteSpace(dto.DecisionSource))
                throw new InvalidOperationException("مصدر القرار مطلوب لنوع الممثل ناظر");
        }

        // Custodian (وصي - type 3): Deed fields required
        if (dto.RepresentativeTypeId == 3)
        {
            if (string.IsNullOrWhiteSpace(dto.DeedNumber))
                throw new InvalidOperationException("رقم الصك مطلوب لنوع الممثل وصي");
            if (!dto.DeedDate.HasValue)
                throw new InvalidOperationException("تاريخ الصك مطلوب لنوع الممثل وصي");
            if (string.IsNullOrWhiteSpace(dto.DeedSource))
                throw new InvalidOperationException("مصدر الصك مطلوب لنوع الممثل وصي");
        }

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
            // Residence Address (عنوان السكن)
            ResidenceRegionId = dto.ResidenceRegionId,
            ResidenceCityId = dto.ResidenceCityId,
            ResidenceDistrict = dto.ResidenceDistrict,
            ResidenceStreet = dto.ResidenceStreet,
            ResidenceBuildingNumber = dto.ResidenceBuildingNumber,
            ResidenceUnitNumber = dto.ResidenceUnitNumber,
            ResidencePostalCode = dto.ResidencePostalCode,
            ResidenceAdditionalCode = dto.ResidenceAdditionalCode,
            // Employment Data (بيانات العمل)
            EmploymentStatus = dto.EmploymentStatus,
            Employer = dto.Employer,
            Profession = dto.Profession,
            // Work Address (عنوان العمل)
            WorkRegionId = dto.WorkRegionId,
            WorkCityId = dto.WorkCityId,
            WorkDistrict = dto.WorkDistrict,
            WorkStreet = dto.WorkStreet,
            WorkBuildingNumber = dto.WorkBuildingNumber,
            WorkUnitNumber = dto.WorkUnitNumber,
            WorkPostalCode = dto.WorkPostalCode,
            WorkAdditionalCode = dto.WorkAdditionalCode,
            // Contact Info
            MobileNumber = dto.MobileNumber,
            Email = dto.Email,
            // Lawyer License (بيانات رخصة المحاماة)
            LawyerLicenseNumber = dto.LawyerLicenseNumber,
            LawyerLicenseDate = dto.LawyerLicenseDate,
            LawyerLicenseExpiryDate = dto.LawyerLicenseExpiryDate,
            // Authorization fields
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
            // CompanyRepresentative fields
            RepresentationDocSource = dto.RepresentationDocSource,
            RepresentativeCapacity = dto.RepresentativeCapacity,
            RepresentationDocType = dto.RepresentationDocType,
            RepresentationDocNumber = dto.RepresentationDocNumber,
            // AgencyRepresentative fields
            RepresentationLetterNumber = dto.RepresentationLetterNumber,
            RepresentationLetterDate = dto.RepresentationLetterDate,
            RepresentationLetterSource = dto.RepresentationLetterSource,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            Attachments = new List<RepresentativeAttachment>()
        };

        // Add attachments for representative (صورة التمثيل)
        if (dto.Attachments != null && dto.Attachments.Count > 0)
        {
            foreach (var attachmentDto in dto.Attachments)
            {
                var storedFileName = $"rep_{Guid.NewGuid()}{Path.GetExtension(attachmentDto.FileName)}";
                var attachment = new RepresentativeAttachment
                {
                    AttachmentTypeId = attachmentDto.AttachmentTypeId,
                    FileName = attachmentDto.FileName,
                    StoredFileName = storedFileName,
                    ContentType = attachmentDto.ContentType,
                    FileSizeBytes = attachmentDto.FileSizeBytes,
                    Description = attachmentDto.Description,
                    UploadDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                representative.Attachments.Add(attachment);
            }
        }

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

        // Residence Address (عنوان السكن)
        if (dto.ResidenceRegionId.HasValue) representative.ResidenceRegionId = dto.ResidenceRegionId;
        if (dto.ResidenceCityId.HasValue) representative.ResidenceCityId = dto.ResidenceCityId;
        if (dto.ResidenceDistrict != null) representative.ResidenceDistrict = dto.ResidenceDistrict;
        if (dto.ResidenceStreet != null) representative.ResidenceStreet = dto.ResidenceStreet;
        if (dto.ResidenceBuildingNumber != null) representative.ResidenceBuildingNumber = dto.ResidenceBuildingNumber;
        if (dto.ResidenceUnitNumber != null) representative.ResidenceUnitNumber = dto.ResidenceUnitNumber;
        if (dto.ResidencePostalCode != null) representative.ResidencePostalCode = dto.ResidencePostalCode;
        if (dto.ResidenceAdditionalCode != null) representative.ResidenceAdditionalCode = dto.ResidenceAdditionalCode;

        // Employment Data (بيانات العمل)
        if (dto.EmploymentStatus != null) representative.EmploymentStatus = dto.EmploymentStatus;
        if (dto.Employer != null) representative.Employer = dto.Employer;
        if (dto.Profession != null) representative.Profession = dto.Profession;

        // Work Address (عنوان العمل)
        if (dto.WorkRegionId.HasValue) representative.WorkRegionId = dto.WorkRegionId;
        if (dto.WorkCityId.HasValue) representative.WorkCityId = dto.WorkCityId;
        if (dto.WorkDistrict != null) representative.WorkDistrict = dto.WorkDistrict;
        if (dto.WorkStreet != null) representative.WorkStreet = dto.WorkStreet;
        if (dto.WorkBuildingNumber != null) representative.WorkBuildingNumber = dto.WorkBuildingNumber;
        if (dto.WorkUnitNumber != null) representative.WorkUnitNumber = dto.WorkUnitNumber;
        if (dto.WorkPostalCode != null) representative.WorkPostalCode = dto.WorkPostalCode;
        if (dto.WorkAdditionalCode != null) representative.WorkAdditionalCode = dto.WorkAdditionalCode;

        // Contact info can always be updated
        if (dto.MobileNumber != null) representative.MobileNumber = dto.MobileNumber;
        if (dto.Email != null) representative.Email = dto.Email;

        // Lawyer License (بيانات رخصة المحاماة)
        if (dto.LawyerLicenseNumber != null) representative.LawyerLicenseNumber = dto.LawyerLicenseNumber;
        if (dto.LawyerLicenseDate.HasValue) representative.LawyerLicenseDate = dto.LawyerLicenseDate;
        if (dto.LawyerLicenseExpiryDate.HasValue) representative.LawyerLicenseExpiryDate = dto.LawyerLicenseExpiryDate;

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

        // CompanyRepresentative fields
        if (dto.RepresentationDocSource != null) representative.RepresentationDocSource = dto.RepresentationDocSource;
        if (dto.RepresentativeCapacity != null) representative.RepresentativeCapacity = dto.RepresentativeCapacity;
        if (dto.RepresentationDocType != null) representative.RepresentationDocType = dto.RepresentationDocType;
        if (dto.RepresentationDocNumber != null) representative.RepresentationDocNumber = dto.RepresentationDocNumber;
        // AgencyRepresentative fields
        if (dto.RepresentationLetterNumber != null) representative.RepresentationLetterNumber = dto.RepresentationLetterNumber;
        if (dto.RepresentationLetterDate.HasValue) representative.RepresentationLetterDate = dto.RepresentationLetterDate;
        if (dto.RepresentationLetterSource != null) representative.RepresentationLetterSource = dto.RepresentationLetterSource;

        representative.ModifiedDate = DateTime.UtcNow;

        // Handle new attachments
        if (dto.Attachments != null && dto.Attachments.Count > 0)
        {
            // Initialize collection if null
            representative.Attachments ??= new List<RepresentativeAttachment>();

            foreach (var attachmentDto in dto.Attachments)
            {
                var storedFileName = $"rep_{representative.Id}_{Guid.NewGuid()}{Path.GetExtension(attachmentDto.FileName)}";
                var attachment = new RepresentativeAttachment
                {
                    RepresentativeId = representative.Id,
                    AttachmentTypeId = attachmentDto.AttachmentTypeId,
                    FileName = attachmentDto.FileName,
                    StoredFileName = storedFileName,
                    ContentType = attachmentDto.ContentType,
                    FileSizeBytes = attachmentDto.FileSizeBytes,
                    Description = attachmentDto.Description,
                    UploadDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                representative.Attachments.Add(attachment);
            }
        }

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
            // Residence Address (عنوان السكن)
            ResidenceRegionId = rep.ResidenceRegionId,
            ResidenceCityId = rep.ResidenceCityId,
            ResidenceDistrict = rep.ResidenceDistrict,
            ResidenceStreet = rep.ResidenceStreet,
            ResidenceBuildingNumber = rep.ResidenceBuildingNumber,
            ResidenceUnitNumber = rep.ResidenceUnitNumber,
            ResidencePostalCode = rep.ResidencePostalCode,
            ResidenceAdditionalCode = rep.ResidenceAdditionalCode,
            // Employment Data (بيانات العمل)
            EmploymentStatus = rep.EmploymentStatus,
            Employer = rep.Employer,
            Profession = rep.Profession,
            // Work Address (عنوان العمل)
            WorkRegionId = rep.WorkRegionId,
            WorkCityId = rep.WorkCityId,
            WorkDistrict = rep.WorkDistrict,
            WorkStreet = rep.WorkStreet,
            WorkBuildingNumber = rep.WorkBuildingNumber,
            WorkUnitNumber = rep.WorkUnitNumber,
            WorkPostalCode = rep.WorkPostalCode,
            WorkAdditionalCode = rep.WorkAdditionalCode,
            // Contact Info
            MobileNumber = rep.MobileNumber,
            Email = rep.Email,
            // Lawyer License (بيانات رخصة المحاماة)
            LawyerLicenseNumber = rep.LawyerLicenseNumber,
            LawyerLicenseDate = rep.LawyerLicenseDate,
            LawyerLicenseExpiryDate = rep.LawyerLicenseExpiryDate,
            // Authorization fields
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
            // CompanyRepresentative fields
            RepresentationDocSource = rep.RepresentationDocSource,
            RepresentativeCapacity = rep.RepresentativeCapacity,
            RepresentationDocType = rep.RepresentationDocType,
            RepresentationDocNumber = rep.RepresentationDocNumber,
            // AgencyRepresentative fields
            RepresentationLetterNumber = rep.RepresentationLetterNumber,
            RepresentationLetterDate = rep.RepresentationLetterDate,
            RepresentationLetterSource = rep.RepresentationLetterSource,
            CreatedDate = rep.CreatedDate,
            // Attachments (صورة التمثيل)
            Attachments = rep.Attachments?.Where(a => !a.IsDeleted && a.IsActive)
                .Select(a => new RepresentativeAttachmentVM
                {
                    Id = a.Id,
                    RepresentativeId = a.RepresentativeId,
                    AttachmentTypeId = a.AttachmentTypeId,
                    AttachmentTypeName = a.AttachmentType?.Name ?? "",
                    AttachmentTypeNameAr = a.AttachmentType?.NameAr ?? "",
                    FileName = a.FileName,
                    FileSizeBytes = a.FileSizeBytes,
                    ContentType = a.ContentType,
                    DownloadUrl = $"/api/representatives/{a.RepresentativeId}/attachments/{a.Id}/download",
                    UploadDate = a.UploadDate,
                    Description = a.Description
                }).ToList() ?? new List<RepresentativeAttachmentVM>()
        };
    }
}
