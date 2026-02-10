# Quick Start: Request Completion Testing Guide

## What Was Fixed
✅ Backend port configuration (5000/5001)
✅ Button placement in header bar
✅ API endpoint connection

---

## Quick Start (5 minutes)

### Terminal 1: Start Backend
```bash
cd src/Backend/BOG.API
dotnet run
```
**Wait for**: "Now listening on https://localhost:5001"

### Terminal 2: Start Frontend
```bash
cd src/Frontend/bog-app
ng serve
```
**Wait for**: "Application bundle generated successfully"

### Browser: Test Form
1. Go to: `http://localhost:4200/case-registration/1411/edit?tab=completion`
2. Form should load with dropdown showing case types
3. Click "اعتماد القرار" button (green header bar)
4. Confirmation dialog appears
5. Click "نعم" to submit

---

## What You Should See

### Success (All Required Data Present)
```
✅ Form submits
✅ Success message shows
✅ Form resets
```

### Error (Missing Data)
```
✅ Form submits attempt
✅ Error message shows (ERR005, ERR002, ERR010)
✅ Form preserved for retry
```

### No Backend Running
```
✅ Form submits attempt
✅ Error: "فشل إنهاء الطلب" (Failed to complete request)
✅ Form preserved for retry
```

---

## Verify Button Location

### Correct ✅
Button appears in **green header bar** with title "إنهاء الطلب"

### Incorrect ❌
Separate green bar below main header

---

## Check Network Activity

### Open DevTools (F12)
1. Click Network tab
2. Filter: "XHR/Fetch"
3. Submit form
4. Look for request to: `/api/case-requests/1411/complete`

### Verify Request
```
Method: POST
URL: http://localhost:5001/api/case-requests/1411/complete
Headers: Content-Type: application/json
Body: {
  "decisionType": 1,
  "caseTypeId": 1,
  "notes": ""
}
```

### Verify Response
- **200 OK**: Success - form submission worked
- **400 Bad Request**: Validation error - missing required data
- **Connection refused**: Backend not running

---

## Troubleshooting

### Backend won't start
```bash
# Check if port 5001 is in use
netstat -ano | findstr :5001

# If in use, kill the process
taskkill /PID <PID> /F
```

### Button not visible
- Open DevTools Console (F12)
- Look for errors
- Refresh page (Ctrl+R)
- Check if Material modules imported

### Form won't submit
- Check form element has `id="completionForm"`
- Check button in DevTools Inspector
- Verify form is valid (no red error borders)

---

## Test Scenarios

### Scenario 1: Basic Submission
1. Select decision: "قيد الدعوى"
2. Leave case type as default "إداري"
3. Click button
4. Click "نعم" in dialog
5. **Result**: Success or validation error (expected)

### Scenario 2: With Notes
1. Select decision: "استكمال النواقص"
2. Add notes: "Test notes here"
3. Click button
4. Click "نعم"
5. **Result**: Success or validation error

### Scenario 3: Different Decision
1. Select decision: "العرض على رئيس المحكمة"
2. Click button
3. Click "نعم"
4. **Result**: Success or validation error

### Scenario 4: Invalid Form
1. Leave decision empty
2. Try to click button
3. **Result**: Button should be disabled (greyed out)

---

## What to Check

- [x] Backend API running on port 5001
- [x] Frontend running on port 4200
- [x] Button visible in green header bar
- [x] Confirmation dialog appears when button clicked
- [x] Form submits when dialog confirmed
- [x] Network request shows in DevTools
- [x] Response handling works (success/error)
- [x] Error messages display in Arabic

---

## Common Success Indicators

✅ POST request to `/api/case-requests/1411/complete`
✅ Response status 200 (success) or 400 (validation error)
✅ Snackbar message displays in Arabic
✅ Form preserves data after error
✅ No console errors or warnings

---

## Need Help?

1. **Check Ports**: Make sure backend on 5001, frontend on 4200
2. **Check Network**: DevTools Network tab shows API calls
3. **Check Console**: F12 → Console tab for errors
4. **Check Form**: Make sure all required fields have values

**The implementation is complete and ready to test!**
