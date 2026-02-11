using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DTO.Plaintiff;
using BOG.VM.Plaintiff;

namespace BOG.BL.Services;

/// <summary>
/// Plaintiff Attachment business logic service.
/// Handles attachment CRUD operations and business rules.
/// </summary>
public class PlaintiffAttachmentBL : IPlaintiffAttachmentBL
{
    private readonly IPlaintiffAttachmentRepository _attachmentRepository;
    private readonly IPlaintiffRepository _plaintiffRepository;
    private readonly IUnitOfWork _unitOfWork;

    // Max file size: 4MB
    private const long MaxFileSizeBytes = 4 * 1024 * 1024;
    private const string AllowedContentType = "application/pdf";

    // Required attachment types per plaintiff type
    private static readonly Dictionary<int, int[]> RequiredAttachmentsByType = new()
    {
        // Individual (1): Identity copy (attachment type 1)
        [1] = new[] { 1 },
        // RegisteredCompany (4): Commercial registration (attachment type 2)
        [4] = new[] { 2 },
        // GovernmentAgency (6): Representation decision (attachment type 3)
        [6] = new[] { 3 },
        // Waqf (8): Waqf deed (attachment type 4)
        [8] = new[] { 4 }
    };

    public PlaintiffAttachmentBL(
        IPlaintiffAttachmentRepository attachmentRepository,
        IPlaintiffRepository plaintiffRepository,
        IUnitOfWork unitOfWork)
    {
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _plaintiffRepository = plaintiffRepository ?? throw new ArgumentNullException(nameof(plaintiffRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<PlaintiffAttachmentVM>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default)
    {
        var attachments = await _attachmentRepository.GetByPlaintiffIdAsync(plaintiffId, cancellationToken);
        return attachments.Select(MapToVM).ToList();
    }

    public async Task<PlaintiffAttachmentVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id, cancellationToken) as PlaintiffAttachment;
        if (attachment == null || attachment.IsDeleted)
            return null;

        return MapToVM(attachment);
    }

    public async Task<PlaintiffAttachmentVM> CreateAsync(int plaintiffId, PlaintiffAttachmentCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Verify plaintiff exists
        var plaintiff = await _plaintiffRepository.GetByIdAsync(plaintiffId, cancellationToken) as Plaintiff;
        if (plaintiff == null || plaintiff.IsDeleted)
            throw new InvalidOperationException($"المدعي رقم {plaintiffId} غير موجود");

        // BR04: Validate content type (PDF only)
        if (!string.Equals(dto.ContentType, AllowedContentType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("BR04: يجب أن يكون الملف بصيغة PDF فقط");

        // Decode Base64 content and validate size
        byte[] fileContent;
        try
        {
            fileContent = Convert.FromBase64String(dto.FileContent);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("محتوى الملف غير صالح");
        }

        // BR04: Validate file size (max 4MB)
        if (fileContent.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"BR04: حجم الملف يتجاوز الحد الأقصى ({MaxFileSizeBytes / (1024 * 1024)} ميجابايت)");

        // Generate stored file name
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.FileName)}";

        // TODO: Save file to storage (file system, Azure Blob, etc.)
        // For now, we'll store the path only

        var attachment = new PlaintiffAttachment
        {
            PlaintiffId = plaintiffId,
            AttachmentTypeId = dto.AttachmentTypeId,
            FileName = dto.FileName,
            StoredFileName = storedFileName,
            ContentType = dto.ContentType,
            FileSizeBytes = fileContent.Length,
            Description = dto.Description,
            UploadDate = DateTime.UtcNow,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _attachmentRepository.AddAsync(attachment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(attachment.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create attachment");
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id, cancellationToken) as PlaintiffAttachment;
        if (attachment == null || attachment.IsDeleted)
            throw new InvalidOperationException($"المرفق رقم {id} غير موجود");

        // Soft delete
        attachment.IsDeleted = true;
        attachment.IsActive = false;
        attachment.ModifiedDate = DateTime.UtcNow;

        await _attachmentRepository.UpdateAsync(attachment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Optionally delete file from storage
    }

    public async Task<(byte[] Content, string FileName, string ContentType)?> GetFileAsync(int id, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id, cancellationToken) as PlaintiffAttachment;
        if (attachment == null || attachment.IsDeleted)
            return null;

        // TODO: Read file from storage
        // For now, return empty byte array
        return (Array.Empty<byte>(), attachment.FileName, attachment.ContentType);
    }

    public async Task<bool> HasRequiredAttachmentsAsync(int plaintiffId, CancellationToken cancellationToken = default)
    {
        var plaintiff = await _plaintiffRepository.GetByIdAsync(plaintiffId, cancellationToken) as Plaintiff;
        if (plaintiff == null)
            return false;

        var requiredTypes = GetRequiredAttachmentTypes(plaintiff.PlaintiffTypeId);
        if (!requiredTypes.Any())
            return true;

        return await _attachmentRepository.HasRequiredAttachmentsAsync(plaintiffId, requiredTypes, cancellationToken);
    }

    public IEnumerable<int> GetRequiredAttachmentTypes(int plaintiffTypeId)
    {
        return RequiredAttachmentsByType.TryGetValue(plaintiffTypeId, out var types)
            ? types
            : Array.Empty<int>();
    }

    private static PlaintiffAttachmentVM MapToVM(PlaintiffAttachment attachment)
    {
        return new PlaintiffAttachmentVM
        {
            Id = attachment.Id,
            PlaintiffId = attachment.PlaintiffId,
            AttachmentTypeId = attachment.AttachmentTypeId,
            AttachmentTypeNameAr = attachment.AttachmentType?.NameAr ?? "",
            AttachmentTypeName = attachment.AttachmentType?.Name ?? "",
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSizeBytes = attachment.FileSizeBytes,
            Description = attachment.Description,
            UploadDate = attachment.UploadDate
        };
    }
}
