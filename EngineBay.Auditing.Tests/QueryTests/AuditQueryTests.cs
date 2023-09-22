using EngineBay.Core;
using EngineBay.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Security.Claims;
using Xunit;

namespace EngineBay.Auditing.Tests.AuditQueryTests
{
    public class AuditQueryTests : BaseAuditQueryTest
    {
        public AuditQueryTests()
          : base(nameof(AuditQueryTests))
        {
            var path = Path.GetFullPath(@"./TestData/audit-entries.json");
            var auditEntries = JsonConvert.DeserializeObject<List<AuditEntry>>(File.ReadAllText(path));
            if (auditEntries == null || this.AuditingDbContext.AuditEntries.Any())
            {
                return;
            }

            this.AuditingDbContext.AddRange(auditEntries);
            this.AuditingDbContext.SaveChanges(this.CurrentIdentity);
        }

        [Fact]
        public async Task AuditEntryCanBeReturnedByGuid()
        {
            var query = new GetAuditEntry(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);
            var getAuditEntryRequest = new GetAuditEntryRequest(this.ClaimsPrincipal, Guid.Parse("4c334609-b5c8-4652-8f4b-8cc9ca604392"));

            var dto = await query.Handle(getAuditEntryRequest, CancellationToken.None);

            Assert.Equal("ef57cf3c-fd93-4059-a080-a6e637d5ab74", dto.OrganisationId.ToString());
            Assert.Equal("consequat morbi a ipsum integer a nibh", dto.Changes);
        }

        [Fact]
        public async Task GettingAuditEntryThatDoesNotExistThrowsAnException()
        {
            var query = new GetAuditEntry(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);
            var getAuditEntryRequest = new GetAuditEntryRequest(this.ClaimsPrincipal, Guid.Parse("307fa9e8-a8f0-43cb-845a-38d7d916482f"));

            await Assert.ThrowsAsync<NotFoundException>(async () =>
              await query.Handle(getAuditEntryRequest, CancellationToken.None));
        }

        [Fact]
        public async Task EmptyPaginationParametersBringsDataAuditEntries()
        {
            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);

            Assert.Equal(6, dto.Data.Count());
        }

        [Fact]
        public async Task LimitingPaginationParametersShouldBringBackNoAuditEntries()
        {
            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                Limit = 0,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);

            Assert.Empty(dto.Data);
        }

        [Fact]
        public async Task LimitingPaginationParametersShouldBringBackNoAuditEntriesButTheTotalShouldStillBeThere()
        {
            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                Limit = 10,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);

            Assert.Equal(6, dto.Total);
        }

        [Fact]
        public async Task ThePageSizeOfPaginatedAuditEntriesCanBeControlled()
        {
            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                Limit = 2,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);

            Assert.Equal(2, dto.Data.Count());
        }

        [Theory]
        [InlineData("ActionType", "Thedrick")]
        [InlineData("EntityName", "Thedrick")]
        [InlineData("Id", "Audrie")]
        [InlineData("ApplicationUserId", "Ziavani")]
        public async Task PaginatedAuditEntriesCanBeSortedInReverse(string sortBy, string expectedFullName)
        {
            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                SortBy = sortBy,
                SortOrder = SortOrderType.Descending,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal(expectedFullName, first.ApplicationUserFullName);
        }

        [Theory]
        [InlineData("ActionType", "Teresina")]
        [InlineData("EntityName", "Ziavani")]
        [InlineData("Id", "Teresina")]
        [InlineData("ApplicationUserId", "Guthrie")]
        public async Task PaginatedAuditEntriesCanBeSorted(string sortBy, string expectedFullName)
        {
            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                SortBy = sortBy,
                SortOrder = SortOrderType.Ascending,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal(expectedFullName, first.ApplicationUserFullName);
        }

        [Fact]
        public async Task PaginatedAuditEntriesCanBeSortedButWithNoSpecifiedOrder()
        {
            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                SortBy = "ActionType",
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal("pede", first.ActionType);
        }

        [Fact]
        public async Task PaginatedAuditEntriesCanBeSortedButWithNoSpecifiedOrderingProperty()
        {
            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            queryAuditEntriesRequest.PaginationParameters = new PaginationParameters
            {
                SortOrder = SortOrderType.Descending,
            };

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal("Ziavani", first.ApplicationUserFullName);
        }

        [Fact]
        public async Task OrganisationIdNotMatchingAndThrowsException()
        {
            var claims = new List<Claim>()
      {
        new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, this.CurrentIdentity.Id.ToString() ?? "tests"),
        new Claim("selectedOrganisationId", "6b27a21d-255a-49bb-9d86-8dfabc34b955"),
        new Claim("selectedRoleId", "2da7fdb0-3240-4360-9b20-5c0d0e4ec19c"),
      };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            this.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var query = new GetAuditEntry(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);
            var getAuditEntryRequest = new GetAuditEntryRequest(this.ClaimsPrincipal, Guid.Parse("4c334609-b5c8-4652-8f4b-8cc9ca604392"));

            await Assert.ThrowsAsync<NotFoundException>(async () =>
              await query.Handle(getAuditEntryRequest, CancellationToken.None));
        }

        [Fact]
        public async Task PaginatedAuditEntriesCanBeFilteredByFirtsOrganisationId()
        {
            var claims = new List<Claim>()
      {
        new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, this.CurrentIdentity.Id.ToString() ?? "tests"),
        new Claim("selectedOrganisationId", "ef57cf3c-fd93-4059-a080-a6e637d5ab74"),
        new Claim("selectedRoleId", "9bad51f9-8b59-495f-a54d-79aac1c1f64b"),
      };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            this.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal("ef57cf3c-fd93-4059-a080-a6e637d5ab74", first.OrganisationId.ToString());
            Assert.Equal(6, dto.Data.Count());
        }

        [Fact]
        public async Task PaginatedAuditEntriesCanBeFilteredBySecondOrganisationId()
        {
            var claims = new List<Claim>()
      {
        new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, this.CurrentIdentity.Id.ToString() ?? "tests"),
        new Claim("selectedOrganisationId", "d61bcd8a-7302-4ade-b8d0-bb0638844d53"),
        new Claim("selectedRoleId", "471ab90b-33b9-41da-8587-de2b38e546cd"),
      };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            this.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var query = new QueryAuditEntries(this.AuditingDbContext, this.GetCurrentIdentityQuery, this.DataProtectorProvider);

            var queryAuditEntriesRequest = new QueryAuditEntriesRequest(this.ClaimsPrincipal, new PaginationParameters());

            var dto = await query.Handle(queryAuditEntriesRequest, CancellationToken.None);
            var first = dto.Data.First();
            Assert.Equal("d61bcd8a-7302-4ade-b8d0-bb0638844d53", first.OrganisationId.ToString());
            Assert.Equal(4, dto.Data.Count());
        }
    }
}
