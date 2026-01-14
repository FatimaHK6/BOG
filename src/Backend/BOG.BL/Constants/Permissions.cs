namespace BOG.BL.Constants;

/// <summary>
/// Permission constants for Case Registration module.
/// ثوابت الصلاحيات لوحدة قيد الدعوى
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Permission to create a new case registration request.
    /// صلاحية إنشاء طلب قيد دعوى
    /// </summary>
    public const string CreateRequest = "CaseRegistration.Create";

    /// <summary>
    /// Permission to view case registration requests.
    /// صلاحية عرض طلبات قيد الدعوى
    /// </summary>
    public const string ViewRequest = "CaseRegistration.View";

    /// <summary>
    /// Permission to edit case registration requests.
    /// صلاحية تعديل طلبات قيد الدعوى
    /// </summary>
    public const string EditRequest = "CaseRegistration.Edit";

    /// <summary>
    /// Permission to submit case registration requests.
    /// صلاحية تقديم طلبات قيد الدعوى
    /// </summary>
    public const string SubmitRequest = "CaseRegistration.Submit";

    /// <summary>
    /// Permission to register a case.
    /// صلاحية قيد الدعوى
    /// </summary>
    public const string RegisterCase = "CaseRegistration.Register";

    /// <summary>
    /// Permission to reject case registration requests.
    /// صلاحية رفض طلبات قيد الدعوى
    /// </summary>
    public const string RejectRequest = "CaseRegistration.Reject";

    /// <summary>
    /// Permission to send request to judge desk.
    /// صلاحية إرسال الطلب لمكتب القاضي
    /// </summary>
    public const string SendToJudge = "CaseRegistration.SendToJudge";

    /// <summary>
    /// Permission to request completion of deficiencies.
    /// صلاحية طلب استكمال النواقص
    /// </summary>
    public const string RequestCompletion = "CaseRegistration.RequestCompletion";

    /// <summary>
    /// Permission to update request status.
    /// صلاحية تحديث حالة الطلب
    /// </summary>
    public const string UpdateStatus = "CaseRegistration.UpdateStatus";
}
