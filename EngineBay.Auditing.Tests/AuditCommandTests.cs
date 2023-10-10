namespace EngineBay.Auditing.Tests
{
    using FluentValidation;
    using Newtonsoft.Json;
    using Xunit;

    public class AuditCommandTests : BaseTestWithDbContext<AuditingWriteDbContext>
    {
        private readonly IValidator<CreateAuditEntryRequest> validator;

        public AuditCommandTests()
            : base(nameof(AuditCommandTests))
        {
            this.validator = new CreateAuditEntryValidator();

            var path = Path.GetFullPath(@"./TestData/audit-entries.json");
            List<AuditEntry>? auditEntries = JsonConvert.DeserializeObject<List<AuditEntry>>(File.ReadAllText(path));
            var auditEntriesCount = this.DbContext.AuditEntries.Count();
            if (auditEntries is not null)
            {
                if (auditEntriesCount == 0)
                {
                    this.DbContext.AddRange(auditEntries);
                    this.DbContext.SaveChanges(this.ApplicationUser);
                }
            }
        }

        [Theory]
        [InlineData("48b99ae0-d854-40ec-a041-3864a6c6265d", "bob", "insert", "23637a89-0a83-4257-a66b-a0dc2aca0139", "someUser", "changes")]
        [InlineData("b1daf3e6-2f38-45c5-9f36-6785e7dd380e", "bob", "insert", "23637a89-0a83-4257-a66b-a0dc2aca0139", "", "changes")]
        [InlineData("48b99ae0-d854-40ec-a041-3864a6c6265d", "bob", "insert", "", "someUser", "changes")]
        public async void CreatedAnAuditEntryCommand(string applicationUserId, string applicationUserName, string actionType, string entityId, string entityName, string changes)
        {
            var command = new CreateAuditEntry(this.DbContext, this.validator);

            var auditEntry = new CreateAuditEntryRequest()
            {
                ApplicationUserId = new Guid(applicationUserId),
                ApplicationUserName = applicationUserName,
                ActionType = actionType,
                EntityId = entityId,
                EntityName = entityName,
                Changes = changes,
            };

            var dto = await command.Handle(auditEntry, this.ClaimsPrincipal, CancellationToken.None);

            Assert.NotNull(dto);
            Assert.Equal(dto.ApplicationUserId, new Guid(applicationUserId));
            Assert.Equal(dto.ApplicationUserName, applicationUserName);
            Assert.Equal(dto.ActionType, actionType);
            Assert.Equal(dto.EntityId, entityId);
            Assert.Equal(dto.EntityName, entityName);
            Assert.Equal(dto.Changes, changes);
        }

        [Theory]
        [InlineData("48b99ae0-d854-40ec-a041-3864a6c6265d", "bob", "insert", "23637a89-0a83-4257-a66b-a0dc2aca0139", "someUser", "changes")]
        [InlineData("b1daf3e6-2f38-45c5-9f36-6785e7dd380e", "bob", "insert", "23637a89-0a83-4257-a66b-a0dc2aca0139", "", "changes")]
        [InlineData("48b99ae0-d854-40ec-a041-3864a6c6265d", "bob", "insert", "", "someUser", "changes")]
        public async void CanQueryCreatedAuditEntryCommand(string applicationUserId, string applicationUserName, string actionType, string entityId, string entityName, string changes)
        {
            var command = new CreateAuditEntry(this.DbContext, this.validator);

            var createAuditEntryRequest = new CreateAuditEntryRequest()
            {
                ApplicationUserId = new Guid(applicationUserId),
                ApplicationUserName = applicationUserName,
                ActionType = actionType,
                EntityId = entityId,
                EntityName = entityName,
                Changes = changes,
            };

            var dto = await command.Handle(createAuditEntryRequest, this.ClaimsPrincipal, CancellationToken.None);

            Assert.NotNull(dto);
            Assert.Equal(dto.ApplicationUserId, new Guid(applicationUserId));
            Assert.Equal(dto.ApplicationUserName, applicationUserName);
            Assert.Equal(dto.ActionType, actionType);
            Assert.Equal(dto.EntityId, entityId);
            Assert.Equal(dto.EntityName, entityName);
            Assert.Equal(dto.Changes, changes);

            var query = new GetAuditEntry(this.DbContext);

            var getAuditEntryRequest = new GetAuditEntryRequest(this.ClaimsPrincipal, dto.Id);

            var queryDto = await query.Handle(getAuditEntryRequest, CancellationToken.None);

            Assert.NotNull(queryDto);
            Assert.Equal(queryDto.ApplicationUserId, new Guid(applicationUserId));
            Assert.Equal(queryDto.ApplicationUserName, applicationUserName);
            Assert.Equal(queryDto.ActionType, actionType);
            Assert.Equal(queryDto.EntityId, entityId);
            Assert.Equal(queryDto.EntityName, entityName);
            Assert.Equal(queryDto.Changes, changes);
        }

        [Theory]
        [InlineData("", "bob", "insert", "23637a89-0a83-4257-a66b-a0dc2aca0139", "someUser", "changes")]
        [InlineData("48b99ae0-d854-40ec-a041-3864a6c6265d", "", "insert", "23637a89-0a83-4257-a66b-a0dc2aca0139", "someUser", "changes")]
        [InlineData("48b99ae0-d854-40ec-a041-3864a6c6265d", "bob", "", "23637a89-0a83-4257-a66b-a0dc2aca0139", "someUser", "changes")]
        [InlineData("b688b38c-9ae1-41fa-8780-597d26490328", "bob", "insert", "23637a89-0a83-4257-a66b-a0dc2aca0139", "someUser", "")]
        public async void CreatedAnAuditEntryCommandShouldValidate(string applicationUserId, string applicationUserName, string actionType, string entityId, string entityName, string changes)
        {
            var command = new CreateAuditEntry(this.DbContext, this.validator);

            var auditEntry = new CreateAuditEntryRequest()
            {
                ApplicationUserId = string.IsNullOrWhiteSpace(applicationUserId) ? Guid.Empty : new Guid(applicationUserId),
                ApplicationUserName = applicationUserName,
                ActionType = actionType,
                EntityId = entityId,
                EntityName = entityName,
                Changes = changes,
            };

            await Assert.ThrowsAsync<ValidationException>(async () => await command.Handle(auditEntry, this.ClaimsPrincipal, CancellationToken.None));
        }
    }
}
