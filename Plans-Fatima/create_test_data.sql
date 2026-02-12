SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @RequestId INT;
DECLARE @DefendantId INT;
DECLARE @PlaintiffId INT;
DECLARE @Now DATETIME2 = GETUTCDATE();

BEGIN TRANSACTION;

-- 1. Create case request with required data
INSERT INTO [dbo].[CaseRegistrationRequests] 
(CourtId, Subject, Evidence, RequestStatusId, CreatedByUserId, CreatedDate, ModifiedDate, IsDeleted)
VALUES 
(1, 
 'اختبار شامل لنظام إدارة الدعاوى - قضية اختبار للتحقق من جميع المتطلبات',
 'الأدلة والمستندات المرفقة تشمل: صور الهوية، العقود، والمخاطبات الرسمية. هذا الطلب يهدف للتحقق من اكتمال النظام وجاهزيته للعمل الفعلي.',
 1,
 9999,
 @Now,
 @Now,
 0);

SET @RequestId = SCOPE_IDENTITY();

-- 2. Add defendant
INSERT INTO [dbo].[Defendants] 
(DefendantTypeId, FullName, IdentityTypeId, IdentityNumber, AddressText, AdditionalStatement, IsActive, IsDeleted, CreatedDate, ModifiedDate)
VALUES 
(1, 'محمد أحمد علي الشمري', 1, '1234567890', 'الرياض - حي النخيل - شارع الملك فهد', 'مدعى عليه في قضية تجارية', 1, 0, @Now, @Now);

SET @DefendantId = SCOPE_IDENTITY();

-- 3. Link defendant to request
INSERT INTO [dbo].[CaseRequestDefendants] 
(CaseRegistrationRequestId, DefendantId, IsDeleted, CreatedDate, ModifiedDate)
VALUES 
(@RequestId, @DefendantId, 0, @Now, @Now);

-- 4. Add plaintiff (required for submission)
INSERT INTO [dbo].[Plaintiffs] 
(PlaintiffTypeId, FullName, IdentityTypeId, IdentityNumber, IsApplicant, IsActive, IsDeleted, CreatedDate, ModifiedDate)
VALUES 
(1, 'فاطمة محمود عبد الرحمن السعيد', 1, '0987654321', 1, 1, 0, @Now, @Now);

SET @PlaintiffId = SCOPE_IDENTITY();

-- 5. Link plaintiff to request
INSERT INTO [dbo].[CaseRequestPlaintiffs] 
(CaseRegistrationRequestId, PlaintiffId, IsDeleted, CreatedDate, ModifiedDate)
VALUES 
(@RequestId, @PlaintiffId, 0, @Now, @Now);

-- 6. Add case classification
INSERT INTO [dbo].[RequestClassifications] 
(CaseRegistrationRequestId, ClassificationText, IsDeleted, CreatedDate, ModifiedDate)
VALUES 
(@RequestId, 'دعوى تجارية - نزاع عقدي', 0, @Now, @Now);

-- 7. Add mandatory attachment (Identity Copy)
INSERT INTO [dbo].[RequestAttachments] 
(CaseRegistrationRequestId, AttachmentTypeId, FileName, StoredFileName, ContentType, IsActive, IsDeleted, CreatedDate, ModifiedDate)
VALUES 
(@RequestId, 1, 'الهوية_الوطنية.pdf', 'test_identity_' + CAST(@RequestId AS VARCHAR) + '.pdf', 'application/pdf', 1, 0, @Now, @Now);

COMMIT TRANSACTION;

SELECT 'Test request created successfully!' AS [Result],
       @RequestId AS [RequestId],
       'http://localhost:4200/case-registration/' + CAST(@RequestId AS VARCHAR) + '/edit' AS [URL];
