# Attachment Upload End-to-End Testing Guide

## Prerequisites
- Both backend API and frontend Angular app are running
- Backend: http://localhost:5001 (or check launchSettings.json)
- Frontend: http://localhost:4200
- Database has 6 AttachmentType records (verified ✓)

## Backend API Verification

### 1. Verify API is Running
```bash
curl http://localhost:5001/api/lookups/attachment-types
```

Expected response:
```json
[
  {
    "id": 1,
    "name": "IdentityCopy",
    "nameAr": "صورة الهوية",
    "isMandatory": true,
    "description": null
  },
  // ... 5 more attachment types
]
```

### 2. Check Swagger Documentation
- Navigate to: http://localhost:5001/swagger
- Look for `/api/lookups/attachment-types` endpoint
- Should show GET method with 200 response

---

## Frontend Testing Steps

### Step 1: Start Frontend (if not already running)
```bash
cd src/Frontend/bog-app
ng serve
```

Wait until you see: `Application bundle generation complete. 12 files in 45 KB`

### Step 2: Navigate to Case Registration
1. Open browser: http://localhost:4200
2. Login with test credentials (if required)
3. Navigate to **Case Registration** → **Create Request** or **Request Details**

### Step 3: Test Attachment Type Dropdown

**Expected Behavior:**
- The "المرفقات" (Attachments) section should be visible
- The "نوع المرفق" (Attachment Type) dropdown should load with 6 options:
  1. صورة الهوية (Identity Copy) - shows "(إلزامي)" badge
  2. صك الوكالة (Power of Attorney)
  3. السجل التجاري (Commercial Registration)
  4. الترخيص (License)
  5. الصك (Deed)
  6. مستند داعم (Supporting Document)

**Troubleshooting if dropdown is empty:**
- Open Browser DevTools (F12)
- Go to Console tab
- Check for errors like:
  ```
  GET http://localhost:5001/api/lookups/attachment-types 404 (Not Found)
  ```
- If you see 404: Backend API endpoint is not properly registered
- If you see CORS error: Need to enable CORS in backend

### Step 4: Test File Upload Workflow

#### Test Case 1: Select Attachment Type
1. Click the "نوع المرفق" dropdown
2. Select first option: "صورة الهوية (إلزامي)"
3. (Optional) Add description in "الوصف (اختياري)" field
4. Expected: Dropdown shows selection, no errors in console

#### Test Case 2: Create Test PDF File
1. Create a test PDF file locally:
   ```bash
   # On Windows, you can create a simple text file and rename it
   echo "Test PDF" > C:\Users\Lenovo\Desktop\test.pdf
   ```
2. Or use an existing PDF file

#### Test Case 3: Upload Valid File
1. Click "رفع ملف" (Upload File) button
2. Select the test PDF file
3. Expected behavior:
   - Button shows "جاري الرفع..." (Uploading...) with spinner
   - After upload: "تم رفع الملف بنجاح" (File uploaded successfully) message
   - File appears in the attachments list below with:
     - File icon (📄)
     - File name
     - File size
     - Upload date
     - Delete button

#### Test Case 4: Upload Invalid File
1. Try to upload a non-PDF file (.txt, .doc, etc.)
2. Expected: Error message "BR04: يقبل فقط ملفات PDF" (Only PDF files accepted)

#### Test Case 5: Upload Oversized File
1. Create a file larger than 4MB
2. Try to upload it
3. Expected: Error message "BR04: حجم الملف لا يزيد عن 4 MB" (File size cannot exceed 4 MB)

#### Test Case 6: Delete Attachment
1. After uploading a file, click the delete button (trash icon) on the attachment
2. Confirm deletion in dialog: "هل تريد حذف هذا المرفق؟" (Are you sure?)
3. Expected: File is removed from list

#### Test Case 7: Upload Multiple Attachments
1. Upload different attachment types:
   - صورة الهوية (Identity Copy)
   - صك الوكالة (Power of Attorney)
   - Any others
2. Expected: All appear in the attachments list

### Step 5: Test State Persistence
1. After uploading files, refresh the page (F5)
2. Expected: Attachments list still shows uploaded files (if they were saved to database)

### Step 6: Test Request Submission
1. Complete all required fields in the case registration form
2. Upload the mandatory attachment: "صورة الهوية" (Identity Copy)
3. Click "Submit" button
4. Expected:
   - No error "ERR003: المرفقات الإلزامية مفقودة" (Required attachments missing)
   - Request submission succeeds

---

## Console Checks (Browser DevTools F12)

### Check Network Tab
1. Click Network tab
2. Upload a file
3. You should see:
   - POST request to: `http://localhost:5001/api/case-requests/{id}/attachments`
   - Status: 200 OK
   - Response body contains the uploaded attachment data

### Check Console for Errors
1. Click Console tab
2. Look for errors with red color
3. Common issues:
   - `ERR003` - Missing required attachments
   - `CORS` error - Backend CORS not configured
   - `404` error - Endpoint not found
   - Network timeouts - API not responding

---

## Success Checklist

- [ ] **Dropdown loads** with 6 attachment types
- [ ] **صورة الهوية** shows "(إلزامي)" badge
- [ ] Can **select attachment type** from dropdown
- [ ] Can **upload PDF file** successfully
- [ ] **Success message** appears after upload
- [ ] Uploaded file **appears in list** with correct details
- [ ] Can **delete** uploaded file
- [ ] Can upload **multiple attachments** of different types
- [ ] **Invalid file** upload shows error
- [ ] **Oversized file** upload shows error
- [ ] **Page refresh** preserves uploaded files
- [ ] **Request submission** succeeds without ERR003 error
- [ ] Browser **console has no errors**
- [ ] Network shows **200 OK** for attachment uploads

---

## Troubleshooting

### Issue: Dropdown shows no options
**Solution:**
```bash
# Check API is running and returns data
curl http://localhost:5001/api/lookups/attachment-types

# Clear browser cache
# DevTools > Storage > Clear All
# Then refresh page (Ctrl+Shift+R)
```

### Issue: Upload button disabled
**Solution:**
- Must select attachment type first
- Check if you have edit permissions (canEdit = true)

### Issue: 404 error on attachment upload
**Solution:**
```bash
# Verify endpoint exists in LookupsController
# Verify DI registration in ServiceCollectionExtensions
dotnet build src/Backend/BOG.sln
dotnet run --project src/Backend/BOG.API
```

### Issue: CORS error
**Solution:**
- Check that CORS is enabled for http://localhost:4200 in Program.cs
- Frontend should be able to call backend API endpoints

---

## Database Verification

If files are uploaded but don't persist after refresh:

```sql
-- Check RequestAttachments table
SELECT * FROM [dbo].[RequestAttachments]
WHERE [IsDeleted] = 0
ORDER BY [Id] DESC;

-- Check file content was saved
SELECT [Id], [FileName], [ContentType], [FileSize]
FROM [dbo].[RequestAttachments]
WHERE [IsDeleted] = 0
LIMIT 10;
```

---

## Additional Resources

- **API Endpoint Documentation**: http://localhost:5001/swagger
- **Frontend Component**: `src/Frontend/bog-app/src/app/features/case-registration/components/attachments/attachments-list.component.ts`
- **API Service**: `src/Frontend/bog-app/src/app/features/case-registration/services/attachment-api.service.ts`
- **Backend Controller**: `src/Backend/BOG.API/Controllers/LookupsController.cs`
- **Database**: `BOG` database in LocalDB

---

## Notes

- The API and frontend must both be running for this test to work
- Clear browser cache if you don't see changes after code updates
- Check browser console (F12) for detailed error messages
- Network tab shows actual HTTP requests/responses
