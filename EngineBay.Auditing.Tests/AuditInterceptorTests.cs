namespace EngineBay.Auditing.Tests
{
    using EngineBay.Persistence;
    using Newtonsoft.Json;
    using Xunit;

    public class AuditInterceptorTests : BaseTestWithDbContext<AuditingWriteDbContext>
    {
        protected AuditInterceptorTests(string databaseName)
            : base(nameof(AuditInterceptorTests))
        {
            var path = Path.GetFullPath(@"./TestData/application-users.json");
            var applicationUsers = JsonConvert.DeserializeObject<List<ApplicationUser>>(File.ReadAllText(path));
            if (applicationUsers == null || this.DbContext.ApplicationUsers.Any())
            {
                return;
            }

            this.DbContext.AddRange(applicationUsers);
            this.DbContext.SaveChanges(this.ApplicationUser);
        }

        [Fact]
        public void WillAuditChangesOnSaveStart()
        {
            // var identity = new MockCurrentIdentity();

            // var options = Substitute.For<DbContextOptions<ModuleWriteDbContext>>();
            // var context = Substitute.For<AuditingWriteDbContext>(options);
            // var logOptions = Substitute.For<ILoggingOptions>();
            // var eventId = new EventId(1);
            // var eventDefinition = Substitute.For<EventDefinitionBase>(logOptions, eventId, LogLevel.Debug, "code");
            // var eventData = new DbContextEventData(eventDefinition, (definition, data) => "message", context);
            // var result = default(InterceptionResult<int>);

            // var userModel = new ApplicationUser()
            // {
            //    Id = Guid.NewGuid(),
            //    Username = "User",
            //    CreatedById = default(Guid),
            //    LastUpdatedById = default(Guid),
            //    CreatedAt = DateTime.Now,
            //    LastUpdatedAt = DateTime.Now,
            // };
            // var entity = new EntityEntry()
            // var entities = new IEnumerable<IEntit>

            // eventData.Context.ChangeTracker.Entries().Returns();

            // var interceptor = new DatabaseAuditingInterceptor(identity, this.DbContext);

            // interceptor.SavingChanges(eventData, result);
        }
    }
}
