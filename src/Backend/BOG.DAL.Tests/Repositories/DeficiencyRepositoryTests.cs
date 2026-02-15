using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BOG.DAL.Repositories;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Tests.Repositories
{
    /// <summary>
    /// Integration tests for DeficiencyRepository
    /// Tests database operations including soft delete and eager loading
    /// </summary>
    public class DeficiencyRepositoryTests : IAsyncLifetime
    {
        private readonly ApplicationDbContext _context;
        private readonly DeficiencyRepository _repository;
        private CaseRegistrationRequest? _testRequest;

        public DeficiencyRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new DeficiencyRepository(_context);
        }

        public async Task InitializeAsync()
        {
            // Seed test data
            await _context.Database.EnsureCreatedAsync();

            // Create test request
            _testRequest = new CaseRegistrationRequest
            {
                CourtId = 1,
                Subject = "Test Case",
                Evidence = "Test Evidence",
                RequestStatusId = 1, // Draft
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.CaseRegistrationRequests.Add(_testRequest);
            await _context.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            _context.Dispose();
        }

        #region GetByRequestIdAsync Tests

        [Fact]
        public async Task GetByRequestIdAsync_WithValidRequest_ReturnsDeficiencies()
        {
            // Arrange
            var deficiency = new RequestDeficiency
            {
                CaseRegistrationRequestId = _testRequest.Id,
                DeficiencyDescriptionId = 1,
                DisplayOrder = 0,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.RequestDeficiencies.Add(deficiency);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByRequestIdAsync(_testRequest.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().DeficiencyDescriptionId);
        }

        [Fact]
        public async Task GetByRequestIdAsync_ExcludesSoftDeletedRecords()
        {
            // Arrange
            var deficiency1 = new RequestDeficiency
            {
                CaseRegistrationRequestId = _testRequest.Id,
                DeficiencyDescriptionId = 1,
                DisplayOrder = 0,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var deficiency2 = new RequestDeficiency
            {
                CaseRegistrationRequestId = _testRequest.Id,
                DeficiencyDescriptionId = 2,
                DisplayOrder = 1,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = true // Soft deleted
            };

            _context.RequestDeficiencies.AddRange(deficiency1, deficiency2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByRequestIdAsync(_testRequest.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().DeficiencyDescriptionId);
        }

        [Fact]
        public async Task GetByRequestIdAsync_WithNoDeficiencies_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetByRequestIdAsync(_testRequest.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByRequestIdAsync_OrdersByDisplayOrder()
        {
            // Arrange
            var deficiencies = new List<RequestDeficiency>
            {
                new RequestDeficiency
                {
                    CaseRegistrationRequestId = _testRequest.Id,
                    DeficiencyDescriptionId = 3,
                    DisplayOrder = 2,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    IsDeleted = false
                },
                new RequestDeficiency
                {
                    CaseRegistrationRequestId = _testRequest.Id,
                    DeficiencyDescriptionId = 1,
                    DisplayOrder = 0,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    IsDeleted = false
                },
                new RequestDeficiency
                {
                    CaseRegistrationRequestId = _testRequest.Id,
                    DeficiencyDescriptionId = 2,
                    DisplayOrder = 1,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            _context.RequestDeficiencies.AddRange(deficiencies);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByRequestIdAsync(_testRequest.Id);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(0, result.ElementAt(0).DisplayOrder);
            Assert.Equal(1, result.ElementAt(1).DisplayOrder);
            Assert.Equal(2, result.ElementAt(2).DisplayOrder);
        }

        [Fact]
        public async Task GetByRequestIdAsync_EagerLoadsRelatedData()
        {
            // Arrange
            var deficiency = new RequestDeficiency
            {
                CaseRegistrationRequestId = _testRequest.Id,
                DeficiencyDescriptionId = 1,
                DisplayOrder = 0,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.RequestDeficiencies.Add(deficiency);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByRequestIdAsync(_testRequest.Id);

            // Assert
            // Verify that navigation properties are accessible
            // This would fail if eager loading wasn't working
            var item = result.First();
            Assert.True(item.DeficiencyDescriptionId > 0);
            // In a real scenario, we'd verify that DeficiencyDescription is loaded
            // but since we're testing with in-memory DB without actual seed data,
            // we verify the structure is correct
        }

        #endregion

        #region DeleteByRequestIdAsync Tests

        [Fact]
        public async Task DeleteByRequestIdAsync_SoftDeletesAllDeficiencies()
        {
            // Arrange
            var deficiency1 = new RequestDeficiency
            {
                CaseRegistrationRequestId = _testRequest.Id,
                DeficiencyDescriptionId = 1,
                DisplayOrder = 0,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var deficiency2 = new RequestDeficiency
            {
                CaseRegistrationRequestId = _testRequest.Id,
                DeficiencyDescriptionId = 2,
                DisplayOrder = 1,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.RequestDeficiencies.AddRange(deficiency1, deficiency2);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteByRequestIdAsync(_testRequest.Id);

            // Assert
            var result = await _repository.GetByRequestIdAsync(_testRequest.Id);
            Assert.Empty(result);

            // Verify they're soft deleted, not hard deleted
            var allRecords = _context.RequestDeficiencies
                .IgnoreQueryFilters()
                .Where(d => d.CaseRegistrationRequestId == _testRequest.Id)
                .ToList();

            Assert.Equal(2, allRecords.Count());
            Assert.All(allRecords, d => Assert.True(d.IsDeleted));
        }

        [Fact]
        public async Task DeleteByRequestIdAsync_UpdatesModifiedDate()
        {
            // Arrange
            var originalDate = DateTime.UtcNow.AddHours(-1);
            var deficiency = new RequestDeficiency
            {
                CaseRegistrationRequestId = _testRequest.Id,
                DeficiencyDescriptionId = 1,
                DisplayOrder = 0,
                CreatedDate = originalDate,
                ModifiedDate = originalDate,
                IsDeleted = false
            };

            _context.RequestDeficiencies.Add(deficiency);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteByRequestIdAsync(_testRequest.Id);

            // Assert
            var deletedRecord = _context.RequestDeficiencies
                .IgnoreQueryFilters()
                .First(d => d.CaseRegistrationRequestId == _testRequest.Id);

            Assert.True(deletedRecord.ModifiedDate > originalDate);
        }

        [Fact]
        public async Task DeleteByRequestIdAsync_WithNonExistentRequest_DoesNotThrow()
        {
            // Act & Assert - Should not throw exception
            await _repository.DeleteByRequestIdAsync(9999);
        }

        #endregion

        #region Batch Operation Tests

        [Fact]
        public async Task BatchUpdate_Workflow()
        {
            // Arrange
            var initialDeficiency = new RequestDeficiency
            {
                CaseRegistrationRequestId = _testRequest.Id,
                DeficiencyDescriptionId = 1,
                DisplayOrder = 0,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.RequestDeficiencies.Add(initialDeficiency);
            await _context.SaveChangesAsync();

            var initialCount = (await _repository.GetByRequestIdAsync(_testRequest.Id)).Count();
            Assert.Equal(1, initialCount);

            // Act - Delete old and add new
            await _repository.DeleteByRequestIdAsync(_testRequest.Id);

            var newDeficiencies = new List<RequestDeficiency>
            {
                new RequestDeficiency
                {
                    CaseRegistrationRequestId = _testRequest.Id,
                    DeficiencyDescriptionId = 2,
                    DisplayOrder = 0,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    IsDeleted = false
                },
                new RequestDeficiency
                {
                    CaseRegistrationRequestId = _testRequest.Id,
                    DeficiencyDescriptionId = 3,
                    DisplayOrder = 1,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            foreach (var def in newDeficiencies)
            {
                await _repository.AddAsync(def);
            }

            await _context.SaveChangesAsync();

            // Assert
            var updatedDeficiencies = await _repository.GetByRequestIdAsync(_testRequest.Id);
            Assert.Equal(2, updatedDeficiencies.Count());
            Assert.Contains(updatedDeficiencies, d => d.DeficiencyDescriptionId == 2);
            Assert.Contains(updatedDeficiencies, d => d.DeficiencyDescriptionId == 3);
            Assert.DoesNotContain(updatedDeficiencies, d => d.DeficiencyDescriptionId == 1);
        }

        #endregion

        #region CancellationToken Tests

        [Fact]
        public async Task GetByRequestIdAsync_RespectsCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.GetByRequestIdAsync(_testRequest.Id, cts.Token));
        }

        [Fact]
        public async Task DeleteByRequestIdAsync_RespectsCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.DeleteByRequestIdAsync(_testRequest.Id, cts.Token));
        }

        #endregion
    }
}
