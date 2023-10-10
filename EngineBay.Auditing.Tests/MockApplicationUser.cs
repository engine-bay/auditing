namespace EngineBay.Auditing.Tests
{
    using EngineBay.Persistence;

    public class MockApplicationUser : ApplicationUser
    {
        public MockApplicationUser()
        {
            var identifier = Guid.Parse("a06381d5-b12f-4033-bce2-203418e696d7");
            this.Id = identifier;
            this.Username = "User";
            this.CreatedById = default(Guid);
            this.LastUpdatedById = default(Guid);
            this.CreatedAt = DateTime.Now;
            this.LastUpdatedAt = DateTime.Now;
        }
    }
}
