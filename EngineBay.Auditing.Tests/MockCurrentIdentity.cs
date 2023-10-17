namespace EngineBay.Auditing.Tests
{
    using EngineBay.Core;

    public class MockCurrentIdentity : ICurrentIdentity
    {
        public MockCurrentIdentity()
        {
            this.UserId = Guid.Parse("a06381d5-b12f-4033-bce2-203418e696d7");
            this.Username = "MockUser";
        }

        public Guid UserId { get; }

        public string? Username { get; }
    }
}
