# Deficiencies Tab - Developer Quick Reference

## Quick Navigation

### Critical Files by Layer

#### Backend
- **API**: `src/Backend/BOG.API/Controllers/DeficienciesController.cs`
- **API Lookups**: `src/Backend/BOG.API/Controllers/LookupsController.cs`
- **Business Logic**: `src/Backend/BOG.BL/Services/CaseRegistration/DeficiencyBL.cs`
- **Interface**: `src/Backend/BOG.BL/Interfaces/CaseRegistration/IDeficiencyBL.cs`
- **Repository**: `src/Backend/BOG.DAL/Repositories/DeficiencyRepository.cs`
- **Interface**: `src/Backend/BOG.DAL/Repositories/Interfaces/IDeficiencyRepository.cs`
- **Entities**: `src/Backend/BOG.DbModel/Entities/CaseRegistration/`
  - `DeficiencyType.cs`
  - `DeficiencyDescription.cs`
  - `RequestDeficiency.cs`
- **DTOs**: `src/Backend/BOG.DTO/CaseRegistration/DeficiencyDTO.cs`
- **VMs**: `src/Backend/BOG.VM/CaseRegistration/DeficiencyVM.cs`

#### Frontend
- **List Component**: `src/Frontend/bog-app/src/app/features/case-registration/components/deficiencies/`
- **Selection Dialog**: `src/Frontend/bog-app/src/app/features/case-registration/components/request-completion/`
- **Models**: `src/Frontend/bog-app/src/app/features/case-registration/models/deficiency.model.ts`
- **API Service**: `src/Frontend/bog-app/src/app/features/case-registration/services/deficiencies-api.service.ts`
- **Lookups Service**: `src/Frontend/bog-app/src/app/features/case-registration/services/lookups-api.service.ts`

---

## API Endpoints Reference

### Get Deficiencies
```
GET /api/case-requests/{requestId}/deficiencies
```
**Returns**: `IEnumerable<DeficiencyVM>`
**Status Codes**: 200 OK, 500 Internal Server Error

### Update Deficiencies
```
PUT /api/case-requests/{requestId}/deficiencies
Content-Type: application/json
Body: DeficienciesBatchUpdateDTO
```
**Returns**: `IEnumerable<DeficiencyVM>`
**Status Codes**: 200 OK, 400 Bad Request, 404 Not Found, 500 Internal Server Error

### Get Deficiency Types
```
GET /api/lookups/deficiency-types
```
**Returns**: `DeficiencyTypeVM[]`
**Status Codes**: 200 OK

### Get Deficiency Descriptions
```
GET /api/lookups/deficiency-descriptions?typeId={typeId}
```
**Query Parameters**:
- `typeId` (optional) - Filter by type

**Returns**: `DeficiencyDescriptionVM[]`
**Status Codes**: 200 OK

---

## Key Code Snippets

### Backend: Service Layer

```csharp
// Get deficiencies
var deficiencies = await _deficiencyBL.GetDeficienciesAsync(requestId);

// Update deficiencies
var dto = new DeficienciesBatchUpdateDTO
{
    Deficiencies = new List<RequestDeficiencyDTO>
    {
        new RequestDeficiencyDTO { DeficiencyDescriptionId = 1 },
        new RequestDeficiencyDTO { DeficiencyDescriptionId = 3 }
    }
};
var updated = await _deficiencyBL.UpdateDeficienciesAsync(requestId, dto);
```

### Backend: Validation

```csharp
// Status validation happens in BL
if (request.RequestStatusId != 1 && request.RequestStatusId != 6)
    throw new InvalidOperationException("Request cannot be edited in current status");

// Status 1 = Draft
// Status 6 = PendingCompletion
```

### Frontend: Component Usage

```typescript
// List component
<app-deficiencies-list
  [requestId]="requestId"
  [canEdit]="canEdit"
  (countChanged)="deficienciesCount = $event">
</app-deficiencies-list>

// Dialog trigger
if (decisionType === 'RequestCompletion') {
  this.openDeficienciesDialog();
}
```

### Frontend: API Call

```typescript
// Load deficiencies
this.deficienciesApi.getDeficiencies(requestId).subscribe({
  next: (deficiencies) => {
    this.deficiencies = deficiencies;
  },
  error: (error) => {
    // Handle error
  }
});

// Load lookup types
this.lookupsApi.getDeficiencyTypes().subscribe({
  next: (types) => {
    this.deficiencyTypes = types;
  }
});

// Update deficiencies
this.deficienciesApi.updateDeficiencies(requestId, dto).subscribe({
  next: (updated) => {
    // Handle success
  }
});
```

---

## Status Codes Reference

| Status | Meaning | When |
|--------|---------|------|
| 1 | Draft | Initial request creation |
| 2 | Submitted | Waiting for processing |
| 3 | New | Received/being processed |
| 4 | InReview | Under review |
| 5 | Registered | Case registered |
| 6 | PendingCompletion | Deficiencies identified, awaiting update |
| 7 | Rejected | Request rejected |
| 8 | Completed | All actions completed |

**Editable Statuses for Deficiencies**: 1 (Draft) and 6 (PendingCompletion)

---

## Database Queries

### Get Deficiencies with Joined Data
```sql
SELECT * FROM RequestDeficiencies rd
  INNER JOIN DeficiencyDescriptions dd ON rd.DeficiencyDescriptionId = dd.Id
  INNER JOIN DeficiencyTypes dt ON dd.DeficiencyTypeId = dt.Id
WHERE rd.CaseRegistrationRequestId = @requestId
  AND rd.IsDeleted = 0
ORDER BY rd.DisplayOrder
```

### Soft Delete Deficiencies
```sql
UPDATE RequestDeficiencies
SET IsDeleted = 1, ModifiedDate = GETUTCDATE()
WHERE CaseRegistrationRequestId = @requestId
  AND IsDeleted = 0
```

### Get All Deficiency Types
```sql
SELECT * FROM DeficiencyTypes
WHERE IsDeleted = 0
ORDER BY DisplayOrder
```

### Get Descriptions by Type
```sql
SELECT * FROM DeficiencyDescriptions
WHERE DeficiencyTypeId = @typeId
  AND IsDeleted = 0
ORDER BY DisplayOrder
```

---

## Common Issues & Solutions

### Issue: Deficiencies not appearing in list
**Solution**:
- Verify `IsDeleted = 0` in database
- Check eager loading is working (Include/ThenInclude)
- Confirm status is editable (1 or 6)

### Issue: Dialog not opening
**Solution**:
- Verify `DecisionType.RequestCompletion` value matches
- Check `openDeficienciesDialog()` is called
- Ensure dialog component is registered in module

### Issue: Save fails with "Request cannot be edited"
**Solution**:
- Check request status in database
- Only Draft (1) and PendingCompletion (6) are editable
- Verify in BL layer: `if (request.RequestStatusId != 1 && request.RequestStatusId != 6)`

### Issue: Old deficiencies not removed
**Solution**:
- Verify `DeleteByRequestIdAsync` is called before adding new ones
- Check soft delete: `IsDeleted` should be `true` in database
- List filtering should exclude deleted records

### Issue: Dialog loses selections on cancel
**Solution**:
- Current behavior: Selections not persisted unless confirmed
- This is by design (form state not saved automatically)
- Use dialog data property to restore previous selections

---

## Type Color Mapping

```typescript
getTypeColor(typeId: number): string {
  const colors: { [key: number]: string } = {
    1: '#FF5722',  // Documents - Orange-Red
    2: '#2196F3',  // Medical - Blue
    3: '#4CAF50',  // Legal - Green
    4: '#FF9800',  // Financial - Orange
    5: '#9C27B0',  // ID - Purple
    6: '#00BCD4'   // Other - Cyan
  };
  return colors[typeId] || '#757575';
}
```

---

## Testing Quick Reference

### Unit Test Structure
```typescript
// Controller Test
const mockDeficiencyBL = new Mock<IDeficiencyBL>();
const mockLogger = new Mock<ILogger<DeficienciesController>>();
const controller = new DeficienciesController(
  mockDeficiencyBL.Object,
  mockLogger.Object
);

// Act
var result = await controller.GetDeficiencies(requestId);

// Assert
var okResult = Assert.IsType<OkObjectResult>(result);
```

### Manual Test Checklist
- [ ] Dialog opens on RequestCompletion action
- [ ] All 6 types displayed
- [ ] Can select/deselect items
- [ ] Count badges update
- [ ] Form includes deficiencies on submit
- [ ] Deficiencies persist after save
- [ ] Can edit deficiencies again
- [ ] Delete buttons work (when canEdit=true)

---

## Debugging Tips

### Backend Debugging
1. Check `_logger` output for BL exceptions
2. Add breakpoints in `DeficiencyBL.UpdateDeficienciesAsync`
3. Verify database state after SaveChangesAsync
4. Check soft delete filtering in repository

### Frontend Debugging
1. Open browser DevTools → Network tab
2. Check API request payload format
3. Inspect component state in DevTools
4. Verify dialog data binding
5. Check RxJS subscription cleanup

### Database Debugging
1. Query `RequestDeficiencies` for test request
2. Verify `IsDeleted` flag status
3. Check `DisplayOrder` sequencing (0, 1, 2...)
4. Confirm navigation properties loaded

---

## Performance Optimization Points

### Query Optimization
- ✅ Eager loading prevents N+1 queries
- ✅ Single Include chain used
- ✅ Soft delete filtering at DB level
- ✅ OrderBy applied in database

### Memory Optimization
- ✅ takeUntil unsubscribes on component destroy
- ✅ Dialog properly disposed
- ✅ No circular references
- ✅ Lists properly cleared

### Network Optimization
- ✅ Minimal payload size
- ✅ No redundant API calls
- ✅ Batch operations (update all in one call)

---

## Deployment Considerations

### Database
- [ ] Migration applied to database
- [ ] Seed data loaded (6 types, 16 descriptions)
- [ ] Constraints verified
- [ ] Indexes optimal

### Backend Configuration
- [ ] Connection string correct
- [ ] DI registration active
- [ ] API endpoints accessible
- [ ] Error logging configured

### Frontend Configuration
- [ ] API URL environment variable set
- [ ] Assets properly bundled
- [ ] Lazy loading configured
- [ ] RTL styling applied

---

## Version Compatibility

- **.NET**: 8.0 or later
- **Angular**: 13+ (tested with latest)
- **TypeScript**: 4.8+
- **SQL Server**: 2016 or later
- **Browsers**: Chrome, Firefox, Safari, Edge (latest versions)

---

## Support Resources

### Documentation Files
- `DEFICIENCIES_INTEGRATION_TEST_PLAN.md` - Complete test plan
- `DEFICIENCIES_FRONTEND_TEST_GUIDE.md` - Manual testing guide
- `DEFICIENCIES_VERIFICATION_CHECKLIST.md` - Implementation checklist
- `PHASE_8_COMPLETION_SUMMARY.md` - Project overview

### Code Comments
- XML documentation on public methods
- Inline comments for complex logic
- TODO comments for future enhancements

### Test Files
- `DeficienciesControllerTests.cs` - Controller unit tests
- `DeficiencyRepositoryTests.cs` - Repository integration tests

---

## Quick Commands

### Build Backend
```bash
dotnet build src/Backend/BOG.sln
```

### Build Frontend
```bash
cd src/Frontend/bog-app && ng build
```

### Run Backend
```bash
dotnet run --project src/Backend/BOG.API
```

### Run Frontend
```bash
cd src/Frontend/bog-app && ng serve
```

### Run Tests
```bash
dotnet test src/Backend/BOG.sln
```

---

## Contact & Escalation

For issues, questions, or enhancements:
1. Check this reference guide first
2. Review test documentation for expected behavior
3. Check code comments for implementation details
4. Review error logs for specific issues

---

**Last Updated**: 2026-02-14
**Version**: 1.0
**Status**: Production Ready ✅
