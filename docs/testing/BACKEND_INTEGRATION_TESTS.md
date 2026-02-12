# Backend Integration Tests - Additional Info API

**File**: `src/Backend/BOG.API/Controllers/AdditionalInfoController.cs`

---

## API Endpoints to Test

### 1. GET `/api/case-requests/{requestId}/additional-info`

**Purpose**: Retrieve existing additional information for a case

**Test Cases**:

#### Test 1.1: Get Type 1 Data
```csharp
[Fact]
public async Task GetAdditionalInfo_WithType1Data_ReturnsManagementDecisionFields()
{
    // Arrange
    var requestId = 1;
    // Pre-seed database with Type 1 data

    // Act
    var response = await client.GetAsync($"/api/case-requests/{requestId}/additional-info");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var vm = await response.Content.ReadAsAsync<AdditionalInfoVM>();
    Assert.NotNull(vm.DecisionNumber);
    Assert.NotNull(vm.DecisionDate);
    Assert.NotNull(vm.NotificationMethod);
    Assert.Null(vm.ComplaintNumber); // Type 2 should be null
    Assert.Null(vm.RequestNumber);   // Type 3 should be null
}
```

#### Test 1.2: Get Type 2 Data
```csharp
[Fact]
public async Task GetAdditionalInfo_WithType2Data_ReturnsServiceRightsFields()
{
    // Arrange
    var requestId = 2;
    // Pre-seed with Type 2 data

    // Act
    var response = await client.GetAsync($"/api/case-requests/{requestId}/additional-info");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var vm = await response.Content.ReadAsAsync<AdditionalInfoVM>();
    Assert.NotNull(vm.HasComplaint);
    Assert.NotNull(vm.ComplaintNumber);
    Assert.Null(vm.DecisionNumber);   // Type 1 should be null
    Assert.Null(vm.RequestNumber);    // Type 3 should be null
}
```

#### Test 1.3: Get Type 3 Data
```csharp
[Fact]
public async Task GetAdditionalInfo_WithType3Data_ReturnsTrademarkFields()
{
    // Arrange
    var requestId = 3;
    // Pre-seed with Type 3 data

    // Act
    var response = await client.GetAsync($"/api/case-requests/{requestId}/additional-info");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var vm = await response.Content.ReadAsAsync<AdditionalInfoVM>();
    Assert.NotNull(vm.RequestNumber);
    Assert.NotNull(vm.RequestDate);
    Assert.Null(vm.DecisionNumber);   // Type 1 should be null
    Assert.Null(vm.ComplaintNumber);  // Type 2 should be null
}
```

#### Test 1.4: Get Multiple Types
```csharp
[Fact]
public async Task GetAdditionalInfo_WithMultipleTypes_ReturnsAllPopulatedFields()
{
    // Arrange
    var requestId = 4;
    // Pre-seed with Type 1, Type 2, and Type 3 data

    // Act
    var response = await client.GetAsync($"/api/case-requests/{requestId}/additional-info");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var vm = await response.Content.ReadAsAsync<AdditionalInfoVM>();
    // Type 1 fields
    Assert.NotNull(vm.DecisionNumber);
    Assert.NotNull(vm.NotificationMethod);
    // Type 2 fields
    Assert.NotNull(vm.ComplaintNumber);
    // Type 3 fields
    Assert.NotNull(vm.RequestNumber);
}
```

#### Test 1.5: Get Non-existent Data (404)
```csharp
[Fact]
public async Task GetAdditionalInfo_WithNoData_Returns404()
{
    // Arrange
    var requestId = 999; // Non-existent

    // Act
    var response = await client.GetAsync($"/api/case-requests/{requestId}/additional-info");

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

#### Test 1.6: Invalid Request ID
```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
public async Task GetAdditionalInfo_WithInvalidRequestId_Returns400(int requestId)
{
    // Act
    var response = await client.GetAsync($"/api/case-requests/{requestId}/additional-info");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

---

### 2. PUT `/api/case-requests/{requestId}/additional-info`

**Purpose**: Save or update additional information

**Test Cases**:

#### Test 2.1: Save New Type 1 Data
```csharp
[Fact]
public async Task SaveAdditionalInfo_WithType1Data_CreatesManagementDecisionRecord()
{
    // Arrange
    var requestId = 1;
    var dto = new AdditionalInfoDTO
    {
        DecisionNumber = "DEC-2025-001",
        DecisionDate = DateTime.Now.AddMonths(-1),
        NotificationDate = DateTime.Now.AddDays(-5),
        NotificationMethodId = 1,
        IssuingAuthorityId = 1,
        // Type 2 and 3 null
        HasComplaint = null,
        RequestNumber = null
    };

    // Act
    var content = new StringContent(
        JsonConvert.SerializeObject(dto),
        Encoding.UTF8,
        "application/json"
    );
    var response = await client.PutAsync(
        $"/api/case-requests/{requestId}/additional-info",
        content
    );

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var vm = await response.Content.ReadAsAsync<AdditionalInfoVM>();
    Assert.Equal("DEC-2025-001", vm.DecisionNumber);

    // Verify in database
    var dbRecord = dbContext.AdditionalInfoManagementDecisions
        .FirstOrDefault(md => md.AdditionalInfoId == vm.Id);
    Assert.NotNull(dbRecord);
    Assert.Equal("DEC-2025-001", dbRecord.DecisionNumber);
}
```

#### Test 2.2: Save Type 2 With Complaint
```csharp
[Fact]
public async Task SaveAdditionalInfo_WithType2Complaint_CreatesServiceRightsRecord()
{
    // Arrange
    var requestId = 2;
    var dto = new AdditionalInfoDTO
    {
        HasComplaint = true,
        ComplaintNumber = "COMP-2025-001",
        ComplaintDate = DateTime.Now.AddMonths(-2),
        ComplaintAuthorityId = 2,
        ComplaintDecisionDate = DateTime.Now.AddDays(-10),
        SystemResult = "Complaint reviewed and accepted"
    };

    // Act
    var content = new StringContent(
        JsonConvert.SerializeObject(dto),
        Encoding.UTF8,
        "application/json"
    );
    var response = await client.PutAsync(
        $"/api/case-requests/{requestId}/additional-info",
        content
    );

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    // Verify in database
    var dbRecord = dbContext.AdditionalInfoServiceRights
        .FirstOrDefault(sr => sr.ComplaintNumber == "COMP-2025-001");
    Assert.NotNull(dbRecord);
    Assert.True(dbRecord.HasComplaint);
    Assert.Equal("Complaint reviewed and accepted", dbRecord.SystemResult);
}
```

#### Test 2.3: Save Type 2 Without Complaint
```csharp
[Fact]
public async Task SaveAdditionalInfo_WithType2NoComplaint_CreatesRecordWithNullFields()
{
    // Arrange
    var requestId = 2;
    var dto = new AdditionalInfoDTO { HasComplaint = false };

    // Act
    var content = new StringContent(
        JsonConvert.SerializeObject(dto),
        Encoding.UTF8,
        "application/json"
    );
    var response = await client.PutAsync(
        $"/api/case-requests/{requestId}/additional-info",
        content
    );

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var dbRecord = dbContext.AdditionalInfoServiceRights
        .FirstOrDefault(sr => sr.HasComplaint == false);
    Assert.NotNull(dbRecord);
    Assert.Null(dbRecord.ComplaintNumber);
    Assert.Null(dbRecord.SystemResult);
}
```

#### Test 2.4: Save Type 3 Trademark Data
```csharp
[Fact]
public async Task SaveAdditionalInfo_WithType3Data_CreatesTrademarkRecord()
{
    // Arrange
    var requestId = 3;
    var dto = new AdditionalInfoDTO
    {
        RequestNumber = "TM-2025-001",
        RequestDate = DateTime.Now.AddMonths(-3)
    };

    // Act
    var content = new StringContent(
        JsonConvert.SerializeObject(dto),
        Encoding.UTF8,
        "application/json"
    );
    var response = await client.PutAsync(
        $"/api/case-requests/{requestId}/additional-info",
        content
    );

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var dbRecord = dbContext.AdditionalInfoTrademarks
        .FirstOrDefault(t => t.RequestNumber == "TM-2025-001");
    Assert.NotNull(dbRecord);
    Assert.NotNull(dbRecord.RequestDate);
}
```

#### Test 2.5: Update Existing Type 1 Data
```csharp
[Fact]
public async Task SaveAdditionalInfo_WithExistingType1_UpdatesRecord()
{
    // Arrange
    var requestId = 1;
    // Pre-seed with Type 1 data
    var newDecisionNumber = "DEC-2025-UPDATED";
    var dto = new AdditionalInfoDTO
    {
        DecisionNumber = newDecisionNumber,
        DecisionDate = DateTime.Now.AddMonths(-1),
        NotificationDate = DateTime.Now.AddDays(-5),
        NotificationMethodId = 1,
        IssuingAuthorityId = 1
    };

    // Act
    var content = new StringContent(
        JsonConvert.SerializeObject(dto),
        Encoding.UTF8,
        "application/json"
    );
    var response = await client.PutAsync(
        $"/api/case-requests/{requestId}/additional-info",
        content
    );

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var vm = await response.Content.ReadAsAsync<AdditionalInfoVM>();
    Assert.Equal(newDecisionNumber, vm.DecisionNumber);

    // Verify only one record exists (not duplicated)
    var recordCount = dbContext.AdditionalInfoManagementDecisions
        .Count(md => md.AdditionalInfo.CaseRegistrationRequestId == requestId);
    Assert.Equal(1, recordCount);
}
```

#### Test 2.6: Save All Three Types Simultaneously
```csharp
[Fact]
public async Task SaveAdditionalInfo_WithAllThreeTypes_CreatesAllRecords()
{
    // Arrange
    var requestId = 4;
    var dto = new AdditionalInfoDTO
    {
        // Type 1
        DecisionNumber = "DEC-2025-001",
        DecisionDate = DateTime.Now.AddMonths(-1),
        NotificationDate = DateTime.Now.AddDays(-5),
        NotificationMethodId = 1,
        IssuingAuthorityId = 1,
        // Type 2
        HasComplaint = true,
        ComplaintNumber = "COMP-2025-001",
        ComplaintDate = DateTime.Now.AddMonths(-2),
        ComplaintAuthorityId = 2,
        ComplaintDecisionDate = DateTime.Now.AddDays(-10),
        SystemResult = "Under review",
        // Type 3
        RequestNumber = "TM-2025-001",
        RequestDate = DateTime.Now.AddMonths(-3)
    };

    // Act
    var content = new StringContent(
        JsonConvert.SerializeObject(dto),
        Encoding.UTF8,
        "application/json"
    );
    var response = await client.PutAsync(
        $"/api/case-requests/{requestId}/additional-info",
        content
    );

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    // Verify all three records exist
    var baseRecord = dbContext.AdditionalInfos
        .FirstOrDefault(ai => ai.CaseRegistrationRequestId == requestId);
    Assert.NotNull(baseRecord);

    var type1 = dbContext.AdditionalInfoManagementDecisions
        .FirstOrDefault(md => md.AdditionalInfoId == baseRecord.Id);
    Assert.NotNull(type1);

    var type2 = dbContext.AdditionalInfoServiceRights
        .FirstOrDefault(sr => sr.AdditionalInfoId == baseRecord.Id);
    Assert.NotNull(type2);

    var type3 = dbContext.AdditionalInfoTrademarks
        .FirstOrDefault(t => t.AdditionalInfoId == baseRecord.Id);
    Assert.NotNull(type3);
}
```

#### Test 2.7: Clear Type 1, Keep Type 2
```csharp
[Fact]
public async Task SaveAdditionalInfo_WithClearedType1_RemovesType1Record()
{
    // Arrange
    var requestId = 4; // Has Type 1 and Type 2
    var dto = new AdditionalInfoDTO
    {
        // Type 1 fields all null
        DecisionNumber = null,
        DecisionDate = null,
        NotificationDate = null,
        NotificationMethodId = null,
        IssuingAuthorityId = null,
        // Type 2 remains
        HasComplaint = true,
        ComplaintNumber = "COMP-2025-001"
    };

    // Act
    var content = new StringContent(
        JsonConvert.SerializeObject(dto),
        Encoding.UTF8,
        "application/json"
    );
    var response = await client.PutAsync(
        $"/api/case-requests/{requestId}/additional-info",
        content
    );

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var baseRecord = dbContext.AdditionalInfos
        .FirstOrDefault(ai => ai.CaseRegistrationRequestId == requestId);
    Assert.NotNull(baseRecord);

    // Type 1 should be null
    var type1 = dbContext.AdditionalInfoManagementDecisions
        .FirstOrDefault(md => md.AdditionalInfoId == baseRecord.Id);
    Assert.Null(type1);

    // Type 2 should still exist
    var type2 = dbContext.AdditionalInfoServiceRights
        .FirstOrDefault(sr => sr.AdditionalInfoId == baseRecord.Id);
    Assert.NotNull(type2);
}
```

#### Test 2.8: Invalid Request ID
```csharp
[Fact]
public async Task SaveAdditionalInfo_WithInvalidRequestId_Returns400()
{
    // Arrange
    var requestId = 0;
    var dto = new AdditionalInfoDTO { DecisionNumber = "DEC-001" };

    // Act
    var content = new StringContent(
        JsonConvert.SerializeObject(dto),
        Encoding.UTF8,
        "application/json"
    );
    var response = await client.PutAsync(
        $"/api/case-requests/{requestId}/additional-info",
        content
    );

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

#### Test 2.9: Non-existent Request ID
```csharp
[Fact]
public async Task SaveAdditionalInfo_WithNonexistentRequestId_Returns404()
{
    // Arrange
    var requestId = 9999;
    var dto = new AdditionalInfoDTO { DecisionNumber = "DEC-001" };

    // Act
    var content = new StringContent(
        JsonConvert.SerializeObject(dto),
        Encoding.UTF8,
        "application/json"
    );
    var response = await client.PutAsync(
        $"/api/case-requests/{requestId}/additional-info",
        content
    );

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

---

### 3. DELETE `/api/case-requests/{requestId}/additional-info`

**Purpose**: Soft delete additional information

**Test Cases**:

#### Test 3.1: Delete Existing Data
```csharp
[Fact]
public async Task DeleteAdditionalInfo_WithExistingData_MarksSoftDelete()
{
    // Arrange
    var requestId = 1;
    // Pre-seed with data

    // Act
    var response = await client.DeleteAsync(
        $"/api/case-requests/{requestId}/additional-info"
    );

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    // Verify soft delete
    var record = dbContext.AdditionalInfos
        .FirstOrDefault(ai => ai.CaseRegistrationRequestId == requestId);
    Assert.NotNull(record);
    Assert.True(record.IsDeleted);
}
```

#### Test 3.2: Delete Non-existent Data
```csharp
[Fact]
public async Task DeleteAdditionalInfo_WithNoData_Returns404()
{
    // Arrange
    var requestId = 9999;

    // Act
    var response = await client.DeleteAsync(
        $"/api/case-requests/{requestId}/additional-info"
    );

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

---

## Lookups API Tests

### GET `/api/lookups/notification-methods`

```csharp
[Fact]
public async Task GetNotificationMethods_ReturnsActiveRecords()
{
    // Act
    var response = await client.GetAsync("/api/lookups/notification-methods");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var data = await response.Content.ReadAsAsync<IEnumerable<object>>();
    Assert.NotEmpty(data);

    // Verify structure
    var method = data.FirstOrDefault() as dynamic;
    Assert.NotNull(method.id);
    Assert.NotNull(method.nameAr);
}
```

### GET `/api/lookups/government-entities`

```csharp
[Fact]
public async Task GetGovernmentEntities_ReturnsActiveRecords()
{
    // Act
    var response = await client.GetAsync("/api/lookups/government-entities");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var data = await response.Content.ReadAsAsync<IEnumerable<object>>();
    Assert.NotEmpty(data);

    // Verify structure
    var entity = data.FirstOrDefault() as dynamic;
    Assert.NotNull(entity.id);
    Assert.NotNull(entity.nameAr);
}
```

---

## Business Logic Tests

### AdditionalInfoBL.AddOrUpdateAsync

```csharp
[Fact]
public async Task AddOrUpdate_DetectsType1Data_SavesManagementDecision()
{
    // Arrange
    var requestId = 1;
    var dto = new AdditionalInfoDTO
    {
        DecisionNumber = "DEC-001",
        DecisionDate = DateTime.Now,
        NotificationDate = DateTime.Now,
        NotificationMethodId = 1,
        IssuingAuthorityId = 1
    };

    // Act
    var result = await additionalInfoBL.AddOrUpdateAsync(requestId, dto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("DEC-001", result.DecisionNumber);
    Assert.Null(result.ComplaintNumber); // Type 2 should be null
}
```

---

## Test Execution

### Run Backend Tests
```bash
cd src/Backend
dotnet test BOG.API.Tests --filter "AdditionalInfo"
```

### Run Specific Test Class
```bash
dotnet test BOG.API.Tests --filter "AdditionalInfoControllerTests"
```

### Run with Coverage
```bash
dotnet test BOG.API.Tests /p:CollectCoverage=true /p:CoverageFormat=opencover
```

---

## Success Criteria

- ✅ All three type records save independently
- ✅ Multiple types can coexist for single request
- ✅ Clearing type fields removes corresponding records
- ✅ Lookup data loads correctly from API
- ✅ Soft delete marks IsDeleted = 1
- ✅ 404 returns when no data exists
- ✅ 400 returns for invalid input
- ✅ API returns Arabic names (nameAr) for lookups
- ✅ Database maintains referential integrity
- ✅ No orphaned records after deletes

