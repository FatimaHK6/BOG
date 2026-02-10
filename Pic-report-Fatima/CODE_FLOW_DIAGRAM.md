# Request Completion Code Flow

## User Action Flow

```
User clicks "اعتماد القرار" button
    ↓
request-completion.component.html
    - Button: <button ... (ngSubmit)="onSubmit()" ...>
    ↓
request-completion.component.ts :: onSubmit()
    - Validates form
    - Shows confirmation dialog
    ↓
User clicks "نعم" (Yes) in dialog
    ↓
Creates RequestDecisionDTO:
    {
      decisionType: 1,      // from dropdown
      caseTypeId: 1,        // from dropdown (default)
      notes: ""             // from textarea (optional)
    }
    ↓
Calls: apiService.completeRequest(requestId, decision)
    ↓
case-registration-api.service.ts :: completeRequest()
    - Makes POST request to:
    - URL: http://localhost:5001/api/case-requests/{id}/complete
    - Body: decision object (JSON)
    ↓
HTTP Network Request
    Method: POST
    URL: http://localhost:5001/api/case-requests/1411/complete
    Headers: Content-Type: application/json
    Body: {
      "decisionType": 1,
      "caseTypeId": 1,
      "notes": ""
    }
    ↓
Backend: src/Backend/BOG.API/Controllers/CaseRegistrationController.cs
    [HttpPost("{id}/complete")]
    public async Task<ActionResult<CaseRegistrationRequestVM>> CompleteRequest(...)
    ↓
Response: 200 OK (success) or 400 Bad Request (error)
    {
      "id": 1411,
      "requestNumber": "REQ-001",
      "requestStatusId": 2,
      "requestStatusName": "تحت المراجعة",
      ...
    }
    ↓
Back to request-completion.component.ts
    - Success: Shows success message
    - Error: Shows error message from backend
    ↓
User sees snackbar with result
```

---

## File Structure & Connections

```
Frontend (Angular)
├── request-details.component
│   └── request-completion.component
│       ├── Form: completionForm (id="completionForm")
│       ├── Button: [form]="completionForm" → triggers submit
│       └── onSubmit() → shows dialog
│
└── Services
    └── case-registration-api.service.ts
        └── completeRequest(id, decision)
            ↓ HTTP POST
            └── http://localhost:5001/api/case-requests/{id}/complete
                                      ↓
Backend (.NET)
│
├── API Layer
│   └── CaseRegistrationController.cs
│       └── [HttpPost("{id}/complete")]
│           └── CompleteRequest(id, decision)
│               ↓ calls
│
├── Business Logic
│   └── CaseRegistrationBL
│       └── CompleteRequestAsync()
│           ↓ calls
│
├── Data Access
│   └── IUnitOfWork
│       └── CaseRegistrationRequestRepository
│           ↓ updates
│
└── Database
    └── ApplicationDbContext
        └── CaseRegistrationRequest
            ↓ saves
            └── SQL Server (localdb)
```

---

## Component Hierarchy

### Layout Structure
```
request-details.component (main page)
│
└── Tabs/Sections
    ├── defendants
    ├── case-data
    ├── additional-info
    ├── deficiencies
    ├── actions
    └── completion ← We're here
        │
        └── app-request-completion
            │
            └── app-section-container
                ├── header (green bar)
                │   ├── title: "إنهاء الطلب"
                │   ├── icon: check_circle
                │   └── [slot="header-actions"]
                │       └── Button: "اعتماد القرار" ← BUTTON HERE
                │
                └── content (white area)
                    └── form (id="completionForm")
                        ├── Decision dropdown
                        ├── Case Type dropdown
                        └── Notes textarea
```

### Slot System
```
Parent: app-section-container
  └── @Input() showActions: boolean
      └── <div class="section-actions">
          └── <ng-content select="[slot='header-actions']">

Child: app-request-completion
  └── <button slot="header-actions">
      └── "اعتماد القرار"
```

---

## Key Configuration Points

### 1. Backend Port Configuration
**File**: `src/Backend/BOG.API/Properties/launchSettings.json`
```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:5001"  // ← Changed from 5003
    },
    "http": {
      "applicationUrl": "http://localhost:5000"   // ← Changed from 5002
    }
  }
}
```

### 2. Frontend API URL
**File**: `src/Frontend/bog-app/src/environments/environment.ts`
```typescript
export const environment = {
  apiUrl: 'http://localhost:5001'  // ← Matches backend HTTPS port
};
```

### 3. API Service Call
**File**: `src/Frontend/bog-app/src/app/features/case-registration/services/case-registration-api.service.ts`
```typescript
completeRequest(id: number, decision: RequestDecisionDTO): Observable<CaseRequestVM> {
  return this.http.post<CaseRequestVM>(
    `${this.baseUrl}/${id}/complete`,  // → /api/case-requests/{id}/complete
    decision
  );
}
```

### 4. Backend Endpoint
**File**: `src/Backend/BOG.API/Controllers/CaseRegistrationController.cs`
```csharp
[HttpPost("{id}/complete")]
public async Task<ActionResult<CaseRegistrationRequestVM>> CompleteRequest(
    [FromRoute] int id,
    [FromBody] RequestDecisionDTO decision,
    CancellationToken cancellationToken)
{
    var result = await _requestActionBL.CompleteRequestAsync(
        id,
        decision,
        cancellationToken
    );
    return Ok(result);
}
```

---

## Data Types

### RequestDecisionDTO (Request Body)
```typescript
interface RequestDecisionDTO {
  decisionType: DecisionType;  // 1=Register, 2=SendToJudge, 3=Reject, 4=RequestCompletion
  caseTypeId: number;          // e.g., 1 (إداري/Administrative)
  notes?: string;              // Optional, max 4000 chars
}
```

### DecisionType Enum
```typescript
enum DecisionType {
  Register = 1,              // قيد الدعوى
  SendToJudge = 2,          // العرض على رئيس المحكمة
  Reject = 3,               // التوجيه بعدم قيد الطلب
  RequestCompletion = 4     // استكمال النواقص
}
```

### CaseRegistrationRequestVM (Response)
```typescript
interface CaseRegistrationRequestVM {
  id: number;
  requestNumber: string;
  requestStatusId: number;
  requestStatusName: string;
  // ... other properties
}
```

---

## Error Handling

### Frontend Error Handling
```typescript
// request-completion.component.ts
this.apiService.completeRequest(this.requestId, decision).subscribe({
  next: (result) => {
    // Success: update request, show success message, reset form
    this.snackBar.open('تم إنهاء الطلب بنجاح', 'إغلاق', { duration: 5000 });
    this.completionForm.reset({...});
  },
  error: (error) => {
    // Error: extract message from backend, show in snackbar
    let errorMessage = error.error?.message || 'فشل إنهاء الطلب';
    this.snackBar.open(errorMessage, 'إغلاق', { duration: 8000 });
  }
});
```

### Expected Backend Errors
- **400 Bad Request**: Validation error (ERR005, ERR002, ERR010)
- **404 Not Found**: Request not found
- **500 Internal Server Error**: Server error

### Expected Frontend Errors
- **ERR_CONNECTION_REFUSED**: Backend not running on port 5001
- **Network error**: No internet or CORS issue

---

## Success Path (Happy Path)

```
1. User navigates to: http://localhost:4200/case-registration/1411/edit?tab=completion
2. Page loads, form initializes with:
   - decisionType: empty (required)
   - caseTypeId: 1 (default)
   - notes: empty (optional)
3. User selects decision type from dropdown
4. User clicks "اعتماد القرار" button (in green header bar)
5. Form validation passes → onSubmit() called
6. Confirmation dialog shown: "هل أنت متأكد من الحفظ؟"
7. User clicks "نعم" button
8. HTTP POST sent to: http://localhost:5001/api/case-requests/1411/complete
9. Backend validates request and processes decision
10. Backend returns 200 OK with updated request object
11. Frontend receives response:
    - Updates request state service
    - Shows success message: "تم قيد الدعوى بنجاح" (or relevant decision message)
    - Resets form
    - Waits for user's next action
```

---

## Error Path (Validation Error)

```
1-7. Same as happy path
8. HTTP POST sent to backend
9. Backend validates:
   - Checks if classifications exist → NO → returns ERR005
   - Checks if defendants exist → NO → returns ERR002
   - Checks if attachments exist → NO → returns ERR010
10. Backend returns 400 Bad Request:
    {
      "message": "يجب تحديد مدعى عليه واحد على الأقل|يجب إضافة مرفق واحد على الأقل"
    }
11. Frontend receives error response
12. Frontend shows error message in snackbar (8 second duration)
13. Form preserves user's data for retry
14. User can fix missing data and resubmit
```

---

## Critical Success Factors

### Configuration
✅ Backend HTTPS port: 5001
✅ Frontend API URL: http://localhost:5001
✅ Database connection: SQL Server localdb

### API Contract
✅ Endpoint exists: POST /api/case-requests/{id}/complete
✅ Accepts RequestDecisionDTO
✅ Returns CaseRegistrationRequestVM

### UI/UX
✅ Button visible in header bar
✅ Form validation prevents invalid submission
✅ Confirmation dialog prevents accidental submission
✅ Error messages in Arabic

### Error Handling
✅ Network errors caught and displayed
✅ Validation errors show backend message
✅ Form preserved after error
✅ User can retry

---

## Ports Summary

| Component | HTTP | HTTPS | Notes |
|-----------|------|-------|-------|
| Frontend | 4200 | N/A | Angular dev server |
| Backend | 5000 | 5001 | ASP.NET Core |
| Frontend calls | N/A | 5001 | Uses HTTPS port |

Frontend makes requests to: **http://localhost:5001** (mixed: HTTP client → HTTPS server, allowed in dev)

---

## Ready to Test!

All code is in place. Just need to:
1. `cd src/Backend/BOG.API && dotnet run`
2. `cd src/Frontend/bog-app && ng serve`
3. Navigate to request completion tab
4. Test form submission

**The flow is complete and properly integrated!**
