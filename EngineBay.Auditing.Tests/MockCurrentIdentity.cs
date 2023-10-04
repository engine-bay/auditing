namespace EngineBay.Auditing.Tests
{
    using EngineBay.Core;

    public class MockCurrentIdentity : ICurrentIdentity
    {
        public MockCurrentIdentity()
        {
            this.Username = "MockUser";
            this.UserId = Guid.NewGuid();
        }

        public string? Username { get; }

        public MockCurrentIdentity(string? username, Guid userId)
        {
            this.Username = username;
            this.UserId = userId;
        }

        public Guid UserId { get; }
    }
}
