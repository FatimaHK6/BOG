-- Update RequestStatus Arabic names to match frontend labels
UPDATE RequestStatuses SET NameAr = N'مقيد حديثًا' WHERE Id = 4;
UPDATE RequestStatuses SET NameAr = N'تم حفظ الطلب' WHERE Id = 5;
UPDATE RequestStatuses SET NameAr = N'عرض على رئيس المحكمة' WHERE Id = 7;
UPDATE RequestStatuses SET NameAr = N'استكمال النواقص' WHERE Id = 6;

-- Verify the changes
SELECT Id, Name, NameAr FROM RequestStatuses WHERE Id IN (4, 5, 6, 7);
