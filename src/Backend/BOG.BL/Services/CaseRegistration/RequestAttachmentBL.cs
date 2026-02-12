using BOG.BL.Interfaces;
using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DTO.RequestAttachment;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Entities.Lookups;
using BOG.VM.RequestAttachment;
using Microsoft.Extensions.Logging;

namespace BOG.BL.Services.CaseRegistration;

/// <summary>
/// Business logic service for case registration request attachments.
/// Handles attachment upload, validation, retrieval, and deletion.
/// Enforces business rule BR04: PDF only, max 4MB per file.
/// </summary>
public class RequestAttachmentBL : IRequestAttachmentBL
{
    private const long MaxFileSizeBytes = 4 * 1024 * 1024; // 4MB
    private const string PdfContentType = "application/pdf";

    private readonly IRepository<RequestAttachment> _attachmentRepository;
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IRepository<AttachmentType> _attachmentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<RequestAttachmentBL> _logger;

    public RequestAttachmentBL(
        IRepository<RequestAttachment> attachmentRepository,
        ICaseRegistrationRequestRepository requestRepository,
        IRepository<AttachmentType> attachmentTypeRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<RequestAttachmentBL> logger)
    {
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _attachmentTypeRepository = attachmentTypeRepository ?? throw new ArgumentNullException(nameof(attachmentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Adds a new attachment to a case registration request.
    /// Validates BR04: PDF only, max 4MB.
    /// </summary>
    public async Task<RequestAttachmentVM> AddAttachmentAsync(int requestId, object attachmentData, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        if (attachmentData == null)
            throw new ArgumentNullException(nameof(attachmentData));

        // Get request
        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null || request.IsDeleted)
            throw new InvalidOperationException($"Request {requestId} not found.");

        // Handle both strongly-typed DTO and object types
        RequestAttachmentDTO attachmentDto;
        if (attachmentData is RequestAttachmentDTO typedDto)
        {
            attachmentDto = typedDto;
        }
        else if (attachmentData is IDictionary<string, object> dictData)
        {
            byte[] fileContent = (byte[])dictData["fileContent"];
            attachmentDto = new RequestAttachmentDTO
            {
                AttachmentTypeId = Convert.ToInt32(dictData["attachmentTypeId"]),
                FileName = dictData["fileName"]?.ToString() ?? "document.pdf",
                ContentType = dictData["contentType"]?.ToString() ?? PdfContentType,
                FileContent = fileContent
            };
        }
        else
        {
            throw new ArgumentException("Invalid attachment data format.", nameof(attachmentData));
        }

        // BR04 Validation: PDF only
        if (!attachmentDto.ContentType.Equals(PdfContentType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("BR04: Only PDF files are allowed.");

        // BR04 Validation: Max 4MB
        if (attachmentDto.FileContent.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"BR04: File size cannot exceed 4MB. Current size: {attachmentDto.FileContent.Length / (1024 * 1024)}MB.");

        // Validate attachment type
        var attachmentType = await _attachmentTypeRepository.GetByIdAsync(attachmentDto.AttachmentTypeId, cancellationToken);
        if (attachmentType == null || attachmentType.IsDeleted)
            throw new InvalidOperationException($"Attachment type {attachmentDto.AttachmentTypeId} not found.");

        // Generate unique stored filename
        var storedFileName = $"{Guid.NewGuid():N}_{attachmentDto.FileName}";

        // Save file to disk BEFORE saving to database
        string savedFilePath;
        try
        {
            savedFilePath = await _fileStorageService.SaveFileAsync(
                attachmentDto.FileContent,
                storedFileName,
                cancellationToken);
            _logger.LogInformation("File saved to disk: {FilePath}", savedFilePath);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to save file {FileName} to disk", storedFileName);
            throw new InvalidOperationException("Failed to save attachment file. Please try again.", ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "File storage error for {FileName}", storedFileName);
            throw;
        }

        // Create attachment entity
        var attachment = new RequestAttachment
        {
            CaseRegistrationRequestId = requestId,
            AttachmentTypeId = attachmentDto.AttachmentTypeId,
            FileName = attachmentDto.FileName,
            StoredFileName = storedFileName,
            ContentType = attachmentDto.ContentType,
            FileSizeBytes = attachmentDto.FileContent.Length,
            UploadDate = DateTime.UtcNow,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        // Save to database with rollback on failure
        try
        {
            await _attachmentRepository.AddAsync(attachment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Rollback: delete physical file if database save fails
            _logger.LogError(ex, "Database save failed for attachment {FileName}, rolling back physical file", storedFileName);
            try
            {
                await _fileStorageService.DeleteFileAsync(storedFileName, CancellationToken.None);
                _logger.LogInformation("Rolled back physical file: {FileName}", storedFileName);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogWarning(rollbackEx, "Failed to rollback physical file during error recovery: {FileName}", storedFileName);
            }
            throw;
        }

        // Reload attachment with navigation properties for mapping
        var savedAttachment = await _attachmentRepository.GetByIdAsync(attachment.Id, cancellationToken);
        if (savedAttachment == null)
            throw new InvalidOperationException($"Failed to retrieve created attachment with ID {attachment.Id}");

        return MapToViewModel(savedAttachment);
    }

    /// <summary>
    /// Gets all attachments for a case registration request.
    /// </summary>
    public async Task<IEnumerable<RequestAttachmentVM>> GetAttachmentsByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var attachments = await _attachmentRepository.FindAsync(
            a => a.CaseRegistrationRequestId == requestId && !a.IsDeleted,
            cancellationToken);

        return attachments.Select(MapToViewModel).ToList();
    }

    /// <summary>
    /// Gets mandatory attachments for a case registration request.
    /// </summary>
    public async Task<IEnumerable<RequestAttachmentVM>> GetMandatoryAttachmentsByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var attachments = await _attachmentRepository.FindAsync(
            a => a.CaseRegistrationRequestId == requestId && !a.IsDeleted && a.AttachmentType != null && a.AttachmentType.IsMandatory,
            cancellationToken);

        return attachments.Select(MapToViewModel).ToList();
    }

    /// <summary>
    /// Gets a specific attachment by ID.
    /// </summary>
    public async Task<RequestAttachmentVM?> GetAttachmentByIdAsync(int attachmentId, CancellationToken cancellationToken = default)
    {
        if (attachmentId <= 0)
            throw new ArgumentException("Invalid attachment ID.", nameof(attachmentId));

        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

        if (attachment == null || attachment.IsDeleted)
            return null;

        return MapToViewModel(attachment);
    }

    /// <summary>
    /// Soft-deletes an attachment and deletes the physical file from disk.
    /// </summary>
    public async Task DeleteAttachmentAsync(int attachmentId, CancellationToken cancellationToken = default)
    {
        if (attachmentId <= 0)
            throw new ArgumentException("Invalid attachment ID.", nameof(attachmentId));

        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);
        if (attachment == null || attachment.IsDeleted)
            throw new InvalidOperationException($"Attachment {attachmentId} not found.");

        // Soft delete in database
        attachment.IsDeleted = true;
        attachment.ModifiedDate = DateTime.UtcNow;

        await _attachmentRepository.UpdateAsync(attachment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Delete physical file (best-effort)
        try
        {
            await _fileStorageService.DeleteFileAsync(attachment.StoredFileName, cancellationToken);
            _logger.LogInformation("Physical file deleted: {StoredFileName}", attachment.StoredFileName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete physical file {StoredFileName} for attachment {AttachmentId}",
                attachment.StoredFileName, attachmentId);
            // Don't throw - soft delete in DB is sufficient
        }
    }

    /// <summary>
    /// Gets mandatory attachment types that are still missing.
    /// Used during submission validation (ERR003).
    /// </summary>
    public async Task<IEnumerable<object>> GetMissingMandatoryAttachmentsAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        // Get all mandatory attachment types
        var mandatoryTypes = await _attachmentTypeRepository.FindAsync(
            at => at.IsMandatory && !at.IsDeleted,
            cancellationToken);

        // Get attached types for this request
        var attachedAttachments = await _attachmentRepository.FindAsync(
            a => a.CaseRegistrationRequestId == requestId && !a.IsDeleted,
            cancellationToken);

        var attachedTypeIds = attachedAttachments.Select(a => a.AttachmentTypeId).ToHashSet();

        // Find missing types
        var missingTypes = mandatoryTypes.Where(mt => !attachedTypeIds.Contains(mt.Id)).ToList();

        return missingTypes.Select(mt => new
        {
            id = mt.Id,
            nameAr = mt.NameAr,
            name = mt.Name,
            isMandatory = mt.IsMandatory
        }).Cast<object>().ToList();
    }

    /// <summary>
    /// Downloads attachment file content from disk storage.
    /// </summary>
    public async Task<(string FileName, byte[] FileContent)> DownloadAttachmentAsync(int attachmentId, CancellationToken cancellationToken = default)
    {
        if (attachmentId <= 0)
            throw new ArgumentException("Invalid attachment ID.", nameof(attachmentId));

        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);
        if (attachment == null || attachment.IsDeleted)
            throw new InvalidOperationException($"Attachment {attachmentId} not found.");

        try
        {
            var fileContent = await _fileStorageService.GetFileAsync(attachment.StoredFileName, cancellationToken);
            _logger.LogInformation("File downloaded successfully: {AttachmentId}, StoredFileName: {StoredFileName}",
                attachmentId, attachment.StoredFileName);
            return (attachment.FileName ?? "document.pdf", fileContent);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Physical file not found for attachment {AttachmentId}, StoredFileName: {StoredFileName}",
                attachmentId, attachment.StoredFileName);
            throw new InvalidOperationException("Attachment file not found on server.", ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error reading attachment file {AttachmentId}", attachmentId);
            throw;
        }
    }

    /// <summary>
    /// Maps RequestAttachment entity to view model.
    /// </summary>
    private static RequestAttachmentVM MapToViewModel(RequestAttachment attachment)
    {
        return new RequestAttachmentVM
        {
            Id = attachment.Id,
            RequestId = attachment.CaseRegistrationRequestId,
            AttachmentTypeId = attachment.AttachmentTypeId,
            AttachmentTypeName = attachment.AttachmentType?.NameAr ?? "",
            FileName = attachment.FileName ?? "",
            FileSize = (int)attachment.FileSizeBytes,
            FileSizeKb = Math.Round(attachment.FileSizeBytes / 1024.0, 2),
            IsMandatory = attachment.AttachmentType?.IsMandatory ?? false,
            CreatedDate = attachment.CreatedDate,
            ModifiedDate = attachment.ModifiedDate,
            Description = attachment.Description
        };
    }
}
