-- Check if data already exists
IF NOT EXISTS (SELECT 1 FROM [BOG].[dbo].[AttachmentTypes] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [dbo].[AttachmentTypes] ON;

    INSERT INTO [dbo].[AttachmentTypes] ([Id], [AllowedExtensions], [CreatedDate], [Description], [IsActive], [IsMandatory], [MaxFileSizeBytes], [ModifiedDate], [Name], [NameAr], [IsDeleted])
    VALUES
        (1, '.pdf', '2024-01-01 00:00:00', NULL, 1, 1, 4194304, '2024-01-01 00:00:00', 'IdentityCopy', N'صورة الهوية', 0),
        (2, '.pdf', '2024-01-01 00:00:00', NULL, 1, 0, 4194304, '2024-01-01 00:00:00', 'PowerOfAttorney', N'صك الوكالة', 0),
        (3, '.pdf', '2024-01-01 00:00:00', NULL, 1, 0, 4194304, '2024-01-01 00:00:00', 'CommercialRegistration', N'السجل التجاري', 0),
        (4, '.pdf', '2024-01-01 00:00:00', NULL, 1, 0, 4194304, '2024-01-01 00:00:00', 'License', N'الترخيص', 0),
        (5, '.pdf', '2024-01-01 00:00:00', NULL, 1, 0, 4194304, '2024-01-01 00:00:00', 'Deed', N'الصك', 0),
        (6, '.pdf', '2024-01-01 00:00:00', NULL, 1, 0, 4194304, '2024-01-01 00:00:00', 'SupportingDocument', N'مستند داعم', 0);

    SET IDENTITY_INSERT [dbo].[AttachmentTypes] OFF;

    PRINT 'Successfully inserted 6 attachment types';
END
ELSE
BEGIN
    PRINT 'Attachment types already exist in database';
    SELECT [Id], [Name], [NameAr], [IsMandatory], [IsActive] FROM [dbo].[AttachmentTypes] WHERE [IsDeleted] = 0 AND [IsActive] = 1;
END
