namespace EngineBay.Auditing.Tests
{
    using EngineBay.Core;

    public class MockCurrentIdentity : ICurrentIdentity
    {
        public MockCurrentIdentity()
        {
            this.UserId = Guid.NewGuid();
            this.Username = "MockUser";
        }

        public MockCurrentIdentity(Guid userId, string? username)
        {
            this.UserId = userId;
            this.Username = username;
        }

        public Guid UserId { get; }

        public string? Username { get; }
    }
}
