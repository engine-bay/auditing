namespace EngineBay.Auditing.Tests
{
    using EngineBay.Core;
    using Newtonsoft.Json;
    using Xunit;

    public class AuditQueryTests : BaseTestWithDbContext<AuditingDbContext>
    {
        public AuditQueryTests()
            : base(nameof(AuditQueryTests))
        {
            var path = Path.GetFullPath(@"./TestData/audit-entries.json");
            var auditEntries = JsonConvert.DeserializeObject<List<AuditEntry>>(File.ReadAllText(path));
            if (auditEntries == null || this.DbContext.AuditEntries.Any())
            {
                return;
            }

            this.DbContext.AddRange(auditEntries);
            this.DbContext.SaveChanges();
        }

        [Fact]
        public async Task AuditEntryCanBeReturnedByGuid()
        {
            var query = new GetAuditEntry(this.DbContext);
            var getAuditEntryRequest = new GetAuditEntryRequest(Guid.Parse("4c334609-b5c8-4652-8f4b-8cc9ca604392"));

            var dto = await query.Handle(getAuditEntryRequest, CancellationToken.None);

            Assert.Equal("consequat morbi a ipsum integer a nibh", dto.Changes);
        }

        [Fact]
        public async Task GettingAuditEntryThatDoesNotExistThrowsAnException()
        {
            var query = new GetAuditEntry(this.DbContext);
            var getAuditEntryRequest = new GetAuditEntryRequest(Guid.Parse("f8bd38e4-0778-4dc9-b092-09f590dfabf3"));

            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await query.Handle(getAuditEntryRequest, CancellationToken.None));
        }

        [Fact]
        public async Task EmptyPaginationParametersBringsDataAuditEntries()
        {
            var query = new QueryAuditEntries(this.DbContext);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(new PaginationParameters());

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);

            Assert.Equal(10, dto.Data.Count());
        }

        [Fact]
        public async Task LimitingPaginationParametersToZeroShouldBringBackNoAuditEntries()
        {
            var query = new QueryAuditEntries(this.DbContext);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                Limit = 0,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);

            Assert.Empty(dto.Data);
        }

        [Fact]
        public async Task IncreasingPaginationParametersBeyondExistingEntriesShouldBringBackOnlyExisting()
        {
            var query = new QueryAuditEntries(this.DbContext);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                Limit = 20,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);

            Assert.Equal(10, dto.Total);
        }

        [Fact]
        public async Task ThePageSizeOfPaginatedAuditEntriesCanBeControlled()
        {
            var query = new QueryAuditEntries(this.DbContext);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                Limit = 2,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);

            Assert.Equal(2, dto.Data.Count());
        }

        [Theory]
        [InlineData("ActionType", "Gwendolen")]
        [InlineData("EntityName", "Teresina")]
        [InlineData("Id", "Ryan")]
        [InlineData("ApplicationUserId", "Gwendolen")]
        public async Task PaginatedAuditEntriesCanBeSortedInReverse(string sortBy, string expectedFullName)
        {
            var query = new QueryAuditEntries(this.DbContext);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                SortBy = sortBy,
                SortOrder = SortOrderType.Descending,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal(expectedFullName, first.ApplicationUserName);
        }

        [Theory]
        [InlineData("ActionType", "Ryan")]
        [InlineData("EntityName", "Ziavani")]
        [InlineData("Id", "Gwendolen")]
        [InlineData("ApplicationUserId", "Guthrie")]
        public async Task PaginatedAuditEntriesCanBeSorted(string sortBy, string expectedFullName)
        {
            var query = new QueryAuditEntries(this.DbContext);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                SortBy = sortBy,
                SortOrder = SortOrderType.Ascending,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal(expectedFullName, first.ApplicationUserName);
        }

        [Fact]
        public async Task PaginatedAuditEntriesCanBeSortedButWithNoSpecifiedOrder()
        {
            var query = new QueryAuditEntries(this.DbContext);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                SortBy = "ActionType",
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal("orci", first.ActionType);
        }

        [Fact]
        public async Task PaginatedAuditEntriesCanBeSortedButWithNoSpecifiedOrderingProperty()
        {
            var query = new QueryAuditEntries(this.DbContext);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                SortOrder = SortOrderType.Descending,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal("Ziavani", first.ApplicationUserName);
        }
    }
}
