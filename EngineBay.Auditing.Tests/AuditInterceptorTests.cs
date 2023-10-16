namespace EngineBay.Auditing.Tests
{
    using EngineBay.Auditing.Tests.FakeAuditableModel;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Xunit;

    public class AuditInterceptorTests : BaseTestWithDbContext<AuditingWriteDbContext>
    {
        private readonly List<FakeModel> fakeModels;
        private readonly List<ApplicationUser> applicationUsers;

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

            var tempApplicationUsers = JsonConvert.DeserializeObject<List<ApplicationUser>>(File.ReadAllText(applicationUsersPath));
            ArgumentNullException.ThrowIfNull(tempApplicationUsers);
            this.applicationUsers = tempApplicationUsers;

            var fakeModelPath = Path.GetFullPath(@"./TestData/fake-models.json");
            var tempFakeModels = JsonConvert.DeserializeObject<List<FakeModel>>(File.ReadAllText(fakeModelPath));
            ArgumentNullException.ThrowIfNull(tempFakeModels);
            this.fakeModels = tempFakeModels;
        }

        protected FakeDbContext FakeDbContext { get; set; }

        [Theory]
        [InlineData("6b29309a-f273-43f0-ad31-b88060e6d684", "one", "two")]
        [InlineData("b54636a8-26ba-4b09-bd4e-50bcffa50a1b", "three", "four")]
        [InlineData("b9d2a805-64cf-4719-ab83-568fca9dc820", "five", "six")]
        public void WillAuditChangesOnSave(string id, string name, string description)
        {
            this.ResetDbs();

            var modelToSave = new FakeModel
            {
                Id = Guid.Parse(id),
                Name = name,
                Description = description,
            };

            this.FakeDbContext.Add(modelToSave);
            this.FakeDbContext.SaveChanges();

            var audit = this.DbContext.AuditEntries.Single(x => x.EntityId == id.ToString());
            Assert.NotNull(audit);
            Assert.Contains(name, audit.Changes);
            Assert.Contains(description, audit.Changes);
        }

        [Theory]
        [InlineData("b9274cd3-c673-4656-b5d3-d8b20df41ad4")]
        [InlineData("7710a98b-a15e-4ba2-9a5e-8aba30a29033")]
        [InlineData("0407c397-af0a-475b-92a2-87570c6e6d02")]
        public void WillAuditChangesOnUpdate(string id)
        {
            this.ResetDbs();

            var idToTest = Guid.Parse(id);
            var modelToupdate = this.FakeDbContext.FakeModels.Find(idToTest);

            Assert.NotNull(modelToupdate);

            var newDescription = modelToupdate.Description + " - updated";
            modelToupdate.Description = newDescription;

            this.FakeDbContext.SaveChanges();

            var numberOfAudits = this.DbContext.AuditEntries.Count();
            var audit = this.DbContext.AuditEntries.Single(x => x.EntityId == idToTest.ToString());

            Assert.Equal(1, numberOfAudits);
            Assert.NotNull(audit);
            Assert.Contains(newDescription, audit.Changes);
        }

        private void ResetDbs()
        {
            this.FakeDbContext.AddRange(this.applicationUsers);
            this.FakeDbContext.SaveChanges(this.ApplicationUser);
            this.FakeDbContext.AddRange(this.fakeModels);
            this.FakeDbContext.SaveChanges(this.ApplicationUser);

            this.DbContext.AuditEntries.RemoveRange(this.DbContext.AuditEntries);
            this.DbContext.SaveChanges();
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
