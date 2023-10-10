namespace EngineBay.Auditing.Tests
{
    using EngineBay.Auditing.Tests.FakeAuditableModel;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Xunit;

    public class AuditInterceptorTests : BaseTestWithDbContext<AuditingWriteDbContext>
    {
        public AuditInterceptorTests()
            : base(nameof(AuditInterceptorTests))
        {
            var dbContextOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                    .UseInMemoryDatabase(nameof(AuditInterceptorTests))
                    .EnableSensitiveDataLogging()
                    .Options;

            var currentIdentity = new MockCurrentIdentity(this.ApplicationUser.Id, this.ApplicationUser.Username);
            var interceptor = new DatabaseAuditingInterceptor(currentIdentity, this.DbContext);

            var context = new FakeDbContext(dbContextOptions, interceptor);
            ArgumentNullException.ThrowIfNull(context);

            this.FakeDbContext = context;
            this.FakeDbContext.Database.EnsureDeleted();
            this.FakeDbContext.Database.EnsureCreated();

            var applicationUsersPath = Path.GetFullPath(@"./TestData/application-users.json");
            var applicationUsers = JsonConvert.DeserializeObject<List<ApplicationUser>>(File.ReadAllText(applicationUsersPath));
            if (applicationUsers == null || this.FakeDbContext.ApplicationUsers.Any())
            {
                return;
            }

            var fakeModelPath = Path.GetFullPath(@"./TestData/fake-models.json");
            var fakeModels = JsonConvert.DeserializeObject<List<FakeModel>>(File.ReadAllText(fakeModelPath));
            if (fakeModels == null || this.FakeDbContext.FakeModels.Any())
            {
                return;
            }

            this.FakeDbContext.AddRange(applicationUsers);
            this.FakeDbContext.SaveChanges(this.ApplicationUser);
            this.FakeDbContext.AddRange(fakeModels);
            this.FakeDbContext.SaveChanges(this.ApplicationUser);
        }

        protected FakeDbContext FakeDbContext { get; set; }

        [Fact]
        public void WillAuditChangesOnUpdate()
        {
            var identity = new MockCurrentIdentity();
            var numberOfAudits = this.DbContext.AuditEntries.Count();

            var idToTest = Guid.Parse("b9274cd3-c673-4656-b5d3-d8b20df41ad4");
            var modelToupdate = this.FakeDbContext.FakeModels.Find(idToTest);
            var oldDescription = modelToupdate.Description;

            var newDescription = modelToupdate.Description + " - updated";
            modelToupdate.Description = newDescription;

            this.FakeDbContext.SaveChanges();

            var newNumberOfAudits = this.DbContext.AuditEntries.Count();
            var newAudit = this.DbContext.AuditEntries.Single(x => x.EntityId == idToTest.ToString() && x.ActionType == "UPDATE");

            Assert.Equal(numberOfAudits + 1, newNumberOfAudits);
        }

        // [Fact]
        //        public async void CreatedAnAuditEntry()
        //        {
        //            if (this.auditedDbContext is null)
        //            {
        //                throw new ArgumentException();
        //            }
        //
        //            if (this.mockEntity is null)
        //            {
        //                throw new ArgumentException();
        //            }
        //
        //            var applicationUser = new MockApplicationUser();
        //
        //            this.auditedDbContext.MockEntities.Add(this.mockEntity);
        //
        //            await this.auditedDbContext.SaveChangesAsync(applicationUser).ConfigureAwait(false);
        //
        //            var auditEntry = this.auditedDbContext.AuditEntries.First();
        //
        //            Assert.NotNull(auditEntry);
        //        }
        //
        //        [Fact]
        //        public async void AuditsASeriesofChanges()
        //        {
        //            if (this.auditedDbContext is null)
        //            {
        //                throw new ArgumentException();
        //            }
        //
        //            if (this.mockEntity is null)
        //            {
        //                throw new ArgumentException();
        //            }
        //
        //            var applicationUser = new MockApplicationUser();
        //
        //            this.auditedDbContext.MockEntities.Add(this.mockEntity);
        //
        //            await this.auditedDbContext.SaveChangesAsync(applicationUser).ConfigureAwait(false);
        //
        //            var savedMockEntity = this.auditedDbContext.MockEntities.First();
        //
        //            savedMockEntity.Name = "Bye world!";
        //
        //            await this.auditedDbContext.SaveChangesAsync(applicationUser).ConfigureAwait(false);
        //
        //            this.auditedDbContext.MockEntities.Remove(savedMockEntity);
        //
        //            await this.auditedDbContext.SaveChangesAsync(applicationUser).ConfigureAwait(false);
        //
        //            var auditEntries = this.auditedDbContext.AuditEntries.
        //                Where(x => x.EntityId == savedMockEntity.Id.ToString())
        //                .OrderBy(x => x.CreatedAt)
        //                .ToList();
        //
        //            // we expect there to be three auditing entries
        //            Assert.Equal(3, auditEntries.Count);
        //
        //            var firstEntry = auditEntries[0];
        //            var secondEntry = auditEntries[1];
        //            var thirdEntry = auditEntries[2];
        //
        //            // with the following sequence of operations
        //            Assert.Equal(DatabaseOperationConstants.INSERT, firstEntry.ActionType);
        //            Assert.Equal(DatabaseOperationConstants.UPDATE, secondEntry.ActionType);
        //            Assert.Equal(DatabaseOperationConstants.DELETE, thirdEntry.ActionType);
        //        }
    }
}
