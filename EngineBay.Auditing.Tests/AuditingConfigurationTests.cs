namespace EngineBay.Auditing.Tests
{
    using Xunit;

    public class AuditingConfigurationTests
    {
        [Fact]
        public void IsAuditingEnabledAuditingEnabledEnvironmentVariableSetToTrueReturnsTrue()
        {
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.AUDITINGENABLED, "true");
            bool actual = AuditingConfiguration.IsAuditingEnabled();
            Assert.True(actual);
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.AUDITINGENABLED, null);
        }

        [Fact]
        public void IsAuditingEnabledAuditingEnabledEnvironmentVariableSetToFalseReturnsFalse()
        {
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.AUDITINGENABLED, "false");
            bool actual = AuditingConfiguration.IsAuditingEnabled();
            Assert.False(actual);
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.AUDITINGENABLED, null);
        }

        [Fact]
        public void IsAuditingEnabledAuditingEnabledEnvironmentVariableNotSetReturnsTrue()
        {
            bool actual = AuditingConfiguration.IsAuditingEnabled();
            Assert.True(actual);
        }

        [Fact]
        public void IsAuditingEnabledAuditingEnabledEnvironmentVariableInvalidValueThrowsArgumentException()
        {
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.AUDITINGENABLED, "invalid");
            Assert.Throws<ArgumentException>(() => AuditingConfiguration.IsAuditingEnabled());
            Environment.SetEnvironmentVariable(EnvironmentVariableConstants.AUDITINGENABLED, null);
        }
    }
}
