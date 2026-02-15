namespace BOG.DbModel.Constants;

/// <summary>
/// Constants for Request Status IDs.
/// Matches the seed data in ApplicationDbContext.cs (lines 1212-1221).
/// </summary>
public static class RequestStatusIds
{
    public const int Draft = 1;              // مسودة
    public const int New = 2;                // طلب جديد
    public const int UnderReview = 3;        // قيد المراجعة
    public const int Registered = 4;         // مقيد حديثًا
    public const int Rejected = 5;           // تم حفظ الطلب
    public const int PendingCompletion = 6;  // استكمال النواقص
    public const int OnJudgeDesk = 7;        // عرض على رئيس المحكمة
    public const int Completed = 8;          // مكتمل
    public const int AutoRejected = 9;       // مرفوض تلقائياً
    public const int Cancelled = 10;         // ملغي
}
