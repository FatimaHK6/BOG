# Save Failed Fix - Quick Test Guide

## Quick Start

### 1. Start Backend API
```bash
cd C:\Users\Lenovo\Desktop\Claude\BOG
dotnet run --project src/Backend/BOG.API
```
API will be available at: `https://localhost:5001`

### 2. Start Frontend
```bash
cd src/Frontend/bog-app
ng serve
```
Frontend will be available at: `http://localhost:4200`

### 3. Navigate to Case Request
1. Go to http://localhost:4200
2. Click on a case registration or create a new one
3. Go to the "Request Details" page

## Manual Testing via API

### Setup: Get a Request ID
First, create or find a request ID to test with:
```bash
# Create a new request
curl -X POST https://localhost:5001/api/case-requests \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "courtId": 1,
    "subject": "Test Case",
    "evidence": "Test Evidence"
  }' 2>&1 | jq '.id'
```

Save the returned ID as `{REQUEST_ID}` for testing.

### Test 1: Update Subject Field
```bash
curl -X PUT https://localhost:5001/api/case-requests/{REQUEST_ID} \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "subject": "Updated Subject - Fix Verified"
  }' 2>&1 | jq '.'
```

**Expected Response:**
- Status: 200 OK
- Response contains: `"subject": "Updated Subject - Fix Verified"`

**Database Verification:**
```sql
SELECT Subject FROM CaseRegistrationRequest WHERE Id = {REQUEST_ID}
-- Should show: "Updated Subject - Fix Verified"
```

### Test 2: Update Contact Information
```bash
curl -X PUT https://localhost:5001/api/case-requests/{REQUEST_ID} \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "primaryMobile": "0512345678",
    "secondaryMobile": "0587654321",
    "email": "test@example.com"
  }' 2>&1 | jq '.'
```

**Expected Response:**
- Status: 200 OK
- Response contains all three fields

**Database Verification:**
```sql
SELECT PrimaryMobile, SecondaryMobile, Email
FROM CaseRegistrationRequest WHERE Id = {REQUEST_ID}
```

### Test 3: Update Classifications
```bash
curl -X PUT https://localhost:5001/api/case-requests/{REQUEST_ID} \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "classificationIds": [1, 2, 3]
  }' 2>&1 | jq '.'
```

**Expected Response:**
- Status: 200 OK
- Response contains: `"classificationIds": [1, 2, 3]`

**Database Verification:**
```sql
SELECT * FROM RequestClassification
WHERE CaseRegistrationRequestId = {REQUEST_ID} AND IsDeleted = 0
-- Should show 3 records
```

### Test 4: Clear Classifications
```bash
curl -X PUT https://localhost:5001/api/case-requests/{REQUEST_ID} \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "classificationIds": []
  }' 2>&1 | jq '.'
```

**Expected Response:**
- Status: 200 OK
- Response contains: `"classificationIds": []`

**Database Verification:**
```sql
SELECT * FROM RequestClassification
WHERE CaseRegistrationRequestId = {REQUEST_ID} AND IsDeleted = 0
-- Should show 0 records
```

### Test 5: Invalid Mobile Number
```bash
curl -X PUT https://localhost:5001/api/case-requests/{REQUEST_ID} \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "primaryMobile": "123"
  }' 2>&1 | jq '.'
```

**Expected Response:**
- Status: 400 Bad Request
- Message: "رقم الجوال الأساسي يجب أن يكون 10 أرقام ويبدأ بـ 05"

### Test 6: Invalid Email
```bash
curl -X PUT https://localhost:5001/api/case-requests/{REQUEST_ID} \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "email": "not-an-email"
  }' 2>&1 | jq '.'
```

**Expected Response:**
- Status: 400 Bad Request
- Message: "صيغة البريد الإلكتروني غير صحيحة"

### Test 7: Subject Too Long
```bash
curl -X PUT https://localhost:5001/api/case-requests/{REQUEST_ID} \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "subject": "'$(python3 -c "print('A' * 5000))''"
  }' 2>&1 | jq '.'
```

**Expected Response:**
- Status: 400 Bad Request
- Message: "الموضوع لا يمكن أن يتجاوز 4000 حرف"

### Test 8: Update All Fields at Once
```bash
curl -X PUT https://localhost:5001/api/case-requests/{REQUEST_ID} \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "subject": "Complete Update Test",
    "evidence": "Updated evidence description",
    "courtId": 2,
    "caseTypeId": 1,
    "notes": "Test notes",
    "classificationIds": [1, 2],
    "primaryMobile": "0555555555",
    "secondaryMobile": "0544444444",
    "email": "update@test.com"
  }' 2>&1 | jq '.'
```

**Expected Response:**
- Status: 200 OK
- All fields present in response

**Database Verification:**
```sql
SELECT Subject, Evidence, CourtId, CaseTypeId, Notes,
       PrimaryMobile, SecondaryMobile, Email
FROM CaseRegistrationRequest WHERE Id = {REQUEST_ID}
-- Should show all updated values
```

## UI Testing

### Test in Angular UI

1. **Navigate to Case Registration Page**
   - Open http://localhost:4200/case-registration/{REQUEST_ID}

2. **Test Subject Update**
   - Fill in Subject field with new text
   - Click Save
   - Expected: Success message, no error
   - Refresh page: Changes persisted

3. **Test Contact Info**
   - Fill in Email: test@example.com
   - Fill in Primary Mobile: 0512345678
   - Click Save
   - Expected: Success message, data saved

4. **Test Classifications**
   - Click on Classifications tab
   - Select 2-3 classifications
   - Click Save
   - Expected: Success message
   - Refresh page: Classifications visible

5. **Test Error Handling**
   - Enter invalid email: "invalid"
   - Click Save
   - Expected: Error message in Arabic about email format
   - Data not saved

6. **Test Invalid Mobile**
   - Enter invalid mobile: "123"
   - Click Save
   - Expected: Error about mobile format (10 digits, starts with 05)

## Verification Checklist

After running tests, verify:

- [ ] Subject field saves correctly
- [ ] Classifications save and can be cleared
- [ ] Contact info saves (email, both mobiles)
- [ ] Validation error messages display correctly
- [ ] No "Save failed" error on successful saves
- [ ] Database shows all saved values
- [ ] Refresh page shows persisted changes
- [ ] Invalid data is rejected with specific error
- [ ] Empty optional fields can be cleared
- [ ] CourtId and CaseTypeId can be updated
- [ ] Notes field saves correctly

## Debugging

### Check Backend Logs
```bash
# Watch API logs in real-time
dotnet run --project src/Backend/BOG.API 2>&1 | grep -E "(Error|Warning|DEBUG)"
```

### Check Database
```bash
# SQL Server query
SELECT * FROM CaseRegistrationRequest WHERE Id = {REQUEST_ID}

# Check classifications
SELECT * FROM RequestClassification
WHERE CaseRegistrationRequestId = {REQUEST_ID}
```

### Browser Console
1. Open DevTools (F12)
2. Check Console tab for errors
3. Check Network tab for API responses
4. Look for error messages in responses

### API Response Structure
Expected successful update response:
```json
{
  "id": {REQUEST_ID},
  "subject": "Updated value",
  "evidence": "Updated value",
  "primaryMobile": "0512345678",
  "secondaryMobile": "0587654321",
  "email": "test@example.com",
  "classificationIds": [1, 2],
  "courtId": 2,
  "caseTypeId": 1,
  "notes": "Test notes",
  "requestStatusId": 1,
  "statusName": "Draft"
}
```

### Error Response Structure
Expected error response:
```json
{
  "message": "رقم الجوال الأساسي يجب أن يكون 10 أرقام ويبدأ بـ 05"
}
```

## Troubleshooting

### "Save Failed" Still Shows
1. Check API is running: http://localhost:5001/swagger
2. Check console for actual error message
3. Look at API logs for detailed error
4. Verify database connection

### Data Not Saving
1. Check API response status code (should be 200)
2. Check database directly with SQL query
3. Verify no validation errors in API response
4. Check change tracker is cleared (should be automatic)

### Browser Shows Blank Error
1. Open DevTools Network tab
2. Look at failed request
3. Check Response tab for actual error message
4. Message should be in `message` field of JSON

## Performance Notes

- Validation happens in memory (fast)
- Database write happens once per update
- No additional queries added
- Classifications handled via soft-delete (preserves history)

## Success Indicators

✅ Subject field saves without error
✅ Classifications update and clear correctly
✅ Contact info saves with validation
✅ Specific error messages in Arabic
✅ No partial saves with errors
✅ All tests pass
✅ Database shows correct values
