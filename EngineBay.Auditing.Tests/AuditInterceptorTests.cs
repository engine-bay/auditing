namespace EngineBay.Auditing.Tests
{
    using EngineBay.Auditing.Tests.FakeAuditableModel;
    using EngineBay.Persistence;
    using Newtonsoft.Json;
    using Xunit;

    public class AuditInterceptorTests : BaseTestWithFullAuditedDb<FakeDbContext>
    {
        private readonly List<FakeModel> fakeModels;
        private readonly List<ApplicationUser> applicationUsers;

        public AuditInterceptorTests()
            : base(nameof(AuditInterceptorTests))
        {
            var applicationUsersPath = Path.GetFullPath(@"./TestData/application-users.json");

            var tempApplicationUsers = JsonConvert.DeserializeObject<List<ApplicationUser>>(File.ReadAllText(applicationUsersPath));
            ArgumentNullException.ThrowIfNull(tempApplicationUsers);
            this.applicationUsers = tempApplicationUsers;

            var fakeModelPath = Path.GetFullPath(@"./TestData/fake-models.json");
            var tempFakeModels = JsonConvert.DeserializeObject<List<FakeModel>>(File.ReadAllText(fakeModelPath));
            ArgumentNullException.ThrowIfNull(tempFakeModels);
            this.fakeModels = tempFakeModels;

            this.DbContext.RemoveRange(this.DbContext.ApplicationUsers);
            this.DbContext.AddRange(this.applicationUsers);
            this.DbContext.SaveChanges();
            this.DbContext.RemoveRange(this.DbContext.FakeModels);
            this.DbContext.AddRange(this.fakeModels);
            this.DbContext.SaveChanges();

            this.ResetAuditEntries();
        }

        [Theory]
        [InlineData("one", "two")]
        [InlineData("three", "four")]
        [InlineData("five", "six")]
        public void AuditChangesOnCreate(string name, string description)
        {
            var modelToSave = new FakeModel
            {
                Name = name,
                Description = description,
            };

            this.DbContext.Add(modelToSave);
            this.DbContext.SaveChanges();

            var audit = this.AuditDbContext.AuditEntries.Single(x => x.EntityId == modelToSave.Id.ToString() && x.ActionType == "INSERT");
            Assert.NotNull(audit);
            Assert.Contains(name, audit.Changes);
            Assert.Contains(description, audit.Changes);
            Assert.Equal(this.currentIdentity.UserId, modelToSave.CreatedById);
        }

        [Theory]
        [InlineData("b9274cd3-c673-4656-b5d3-d8b20df41ad4")]
        [InlineData("7710a98b-a15e-4ba2-9a5e-8aba30a29033")]
        [InlineData("0407c397-af0a-475b-92a2-87570c6e6d02")]
        public void AuditChangesOnUpdate(string id)
        {
            var idToTest = Guid.Parse(id);
            var modelToUpdate = this.DbContext.FakeModels.Find(idToTest);

            Assert.NotNull(modelToUpdate);

            var newDescription = modelToUpdate.Description + " - updated";
            modelToUpdate.Description = newDescription;

            this.DbContext.SaveChanges();

            var numberOfAudits = this.AuditDbContext.AuditEntries.Count();
            var audit = this.AuditDbContext.AuditEntries.Single(x => x.EntityId == idToTest.ToString() && x.ActionType == "UPDATE");

            Assert.Equal(1, numberOfAudits);
            Assert.NotNull(audit);
            Assert.Contains(newDescription, audit.Changes);
            Assert.Equal(this.currentIdentity.UserId, modelToUpdate.LastUpdatedById);
        }

        [Theory]
        [InlineData("b9274cd3-c673-4656-b5d3-d8b20df41ad4")]
        [InlineData("7710a98b-a15e-4ba2-9a5e-8aba30a29033")]
        [InlineData("0407c397-af0a-475b-92a2-87570c6e6d02")]
        public void AuditChangesOnDelete(string id)
        {
            var idToTest = Guid.Parse(id);
            var modelToDelete = this.DbContext.FakeModels.Find(idToTest);

            Assert.NotNull(modelToDelete);

            this.DbContext.FakeModels.Remove(modelToDelete);
            this.DbContext.SaveChanges();

            var numberOfAudits = this.AuditDbContext.AuditEntries.Count();
            var audit = this.AuditDbContext.AuditEntries.Single(x => x.EntityId == idToTest.ToString() && x.ActionType == "DELETE");

            Assert.Equal(1, numberOfAudits);
            Assert.NotNull(audit);
        }
    }
}
