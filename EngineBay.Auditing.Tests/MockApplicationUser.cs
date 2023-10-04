namespace EngineBay.Auditing.Tests
{
    using EngineBay.Persistence;

    public class MockApplicationUser : ApplicationUser
    {
        public MockApplicationUser()
        {
            var identifier = Guid.NewGuid();
            this.Id = identifier;
            this.Username = "User";
            this.CreatedById = default(Guid);
            this.LastUpdatedById = default(Guid);
            this.CreatedAt = DateTime.Now;
            this.LastUpdatedAt = DateTime.Now;
        }
    }
}
