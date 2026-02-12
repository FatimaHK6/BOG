SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

-- Delete all CaseRegistrationRequest records
-- Cascade delete rules will handle related tables
DELETE FROM [dbo].[CaseRegistrationRequests];

COMMIT TRANSACTION;

-- Show confirmation
SELECT 'Test data deletion completed.' AS [Result];
SELECT COUNT(*) AS [Remaining Case Requests] FROM [dbo].[CaseRegistrationRequests];
