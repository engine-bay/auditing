namespace EngineBay.Auditing
{
    using EngineBay.Core;

    public class FakeUserIdentity : ICurrentIdentity
    {
        public FakeUserIdentity()
        {
            this.UserId = Guid.Empty;
            this.Username = "FakeUser";
        }

        public Guid UserId { get; }

        public string? Username { get; }
    }
}
