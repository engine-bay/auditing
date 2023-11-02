namespace EngineBay.Auditing
{
    public abstract class AuditingConfiguration
    {
        public static bool IsAuditingEnabled()
        {
            var auditingEnabledEnvironmentVariable = Environment.GetEnvironmentVariable(EnvironmentVariableConstants.AUDITINGENABLED);

            if (string.IsNullOrEmpty(auditingEnabledEnvironmentVariable))
            {
                return true;
            }

            bool auditingEnabled;
            if (bool.TryParse(auditingEnabledEnvironmentVariable, out auditingEnabled))
            {
                if (!auditingEnabled)
                {
                    Console.WriteLine($"Warning: Auditing has been disabled by {EnvironmentVariableConstants.AUDITINGENABLED} configuration.");
                }

                return auditingEnabled;
            }

            throw new ArgumentException($"Invalid {EnvironmentVariableConstants.AUDITINGENABLED} configuration.");
        }
    }
}